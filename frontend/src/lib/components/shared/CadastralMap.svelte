<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import type { Map, GeoJSON, TileLayer } from 'leaflet';
  import 'leaflet/dist/leaflet.css';

  export let tunnus: string = '12301:001:0012';

  const RETRY_DELAY_MS = 5000;

  let mapEl: HTMLDivElement;
  let map: Map | null = null;
  let cadastralLayer: GeoJSON | null = null;
  let retryTimeout: ReturnType<typeof setTimeout> | null = null;
  let retryScheduled = false;

  function clearRetry(): void {
    if (retryTimeout) {
      clearTimeout(retryTimeout);
      retryTimeout = null;
    }
    retryScheduled = false;
  }

  async function loadCadastralUnit(L: typeof import('leaflet'), tun: string): Promise<void> {
    if (cadastralLayer) {
      cadastralLayer.remove();
      cadastralLayer = null;
    }

    const res = await fetch(`/api/cadastral-unit?tunnus=${encodeURIComponent(tun)}`);
    if (!res.ok) throw new Error(`WFS request failed: ${res.status}`);

    const geojson: GeoJSON.FeatureCollection = await res.json();
    if (!geojson.features?.length) throw new Error(`No cadastral unit found for tunnus: ${tun}`);

    cadastralLayer = L.geoJSON(geojson, {
      style: {
        color: '#e63946',
        weight: 2,
        fillOpacity: 0.15
      }
    }).addTo(map!);

    map!.fitBounds(cadastralLayer.getBounds(), { padding: [40, 40] });
  }

  function scheduleRetry(
    L: typeof import('leaflet'),
    tun: string,
    tileLayer: TileLayer.WMS
  ): void {
    if (retryScheduled) return;

    retryScheduled = true;
    retryTimeout = setTimeout(async () => {
      retryTimeout = null;

      try {
        tileLayer.setParams({ layers: 'of10000' }, true);
        await loadCadastralUnit(L, tun);
        retryScheduled = false;
      } catch (error) {
        console.error('Map reload failed, retrying in 5 seconds.', error);
        retryScheduled = false;
        scheduleRetry(L, tun, tileLayer);
      }
    }, RETRY_DELAY_MS);
  }

  onMount(async () => {
    const L = (await import('leaflet')).default;

    map = L.map(mapEl, { crs: L.CRS.EPSG4326 }).setView([58.5, 25.0], 7);

    const tileLayer = L.tileLayer.wms('/api/map-tiles?', {
    layers: 'of10000',
    format: 'image/jpeg',
    version: '1.3.0',
    crs: L.CRS.EPSG4326,
    attribution: '© Maa- ja Ruumiamet'
    }).addTo(map);

    tileLayer.on('tileerror', () => {
      if (tunnus) {
        scheduleRetry(L, tunnus, tileLayer);
      }
    });

    // Ensure Leaflet recalculates size after the element is fully laid out.
    setTimeout(() => map?.invalidateSize(), 0);

    if (tunnus) {
      try {
        await loadCadastralUnit(L, tunnus);
      } catch (error) {
        console.error('Map load failed, retrying in 5 seconds.', error);
        scheduleRetry(L, tunnus, tileLayer);
      }
    }
  });

  onDestroy(() => {
    clearRetry();
    map?.remove();
    map = null;
  });
</script>

<div bind:this={mapEl} class="map"></div>

<style>
  .map {
    width: 100%;
    height: 500px;
  }
</style>
