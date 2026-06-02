<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import type { Map, GeoJSON, TileLayer, Marker } from 'leaflet';
  import 'leaflet/dist/leaflet.css';

  export let tunnus: string = '12301:001:0012';

  const RETRY_DELAY_MS = 5000;

  let mapEl: HTMLDivElement;
  let map: Map | null = null;
  let cadastralLayer: GeoJSON | null = null;
  let forestStandsLayer: GeoJSON | null = null;
  let forestStandLabels: Marker[] = [];
  let retryTimeout: ReturnType<typeof setTimeout> | null = null;
  let retryScheduled = false;

  function clearRetry(): void {
    if (retryTimeout) {
      clearTimeout(retryTimeout);
      retryTimeout = null;
    }
    retryScheduled = false;
  }

  function clearForestStandLabels(): void {
    for (const label of forestStandLabels) {
      label.remove();
    }
    forestStandLabels = [];
  }

  function getPolygonCentroid(feature: GeoJSON.Feature): [number, number] | null {
    const geom = feature.geometry;
    if (!geom) return null;

    let coords: number[][][] | number[][][][] = [];
    if (geom.type === 'Polygon') {
      coords = (geom as GeoJSON.Polygon).coordinates;
    } else if (geom.type === 'MultiPolygon') {
      coords = (geom as GeoJSON.MultiPolygon).coordinates;
    } else {
      return null;
    }

    let totalX = 0;
    let totalY = 0;
    let count = 0;

    const processRing = (ring: number[][]) => {
      for (const [x, y] of ring) {
        totalX += x;
        totalY += y;
        count++;
      }
    };

    if (geom.type === 'Polygon') {
      for (const ring of (coords as number[][][])) {
        processRing(ring);
      }
    } else {
      for (const polygon of (coords as number[][][][])) {
        for (const ring of polygon) {
          processRing(ring);
        }
      }
    }

    if (count === 0) return null;
    return [totalY / count, totalX / count];
  }

  async function loadForestStands(L: typeof import('leaflet'), katastriNr: string): Promise<void> {
    if (forestStandsLayer) {
      forestStandsLayer.remove();
      forestStandsLayer = null;
    }
    clearForestStandLabels();

    const res = await fetch(`/api/forest-stands?katastri_nr=${encodeURIComponent(katastriNr)}`);
    if (!res.ok) throw new Error(`Forest stands request failed: ${res.status}`);
    if (!map) return;

    const geojson: GeoJSON.FeatureCollection = await res.json();
    console.info('[CadastralMap] forest stands loaded', {
      featureCount: geojson.features?.length ?? 0,
      katastriNr
    });
    if (!geojson.features?.length) return;

    forestStandsLayer = L.geoJSON(geojson, {
      style: {
        color: '#f4a261',
        weight: 1.5,
        fillOpacity: 0.1
      },
      onEachFeature: (feature, layer) => {
        const eraldiseNr = feature.properties?.eraldise_nr;
        if (eraldiseNr !== undefined && eraldiseNr !== null) {
          const centroid = getPolygonCentroid(feature);
          if (centroid) {
            const icon = L.divIcon({
              className: 'forest-stand-label',
              html: `<span>${eraldiseNr}</span>`,
              iconSize: [30, 20],
              iconAnchor: [15, 10]
            });
            const label = L.marker(L.latLng(centroid[0], centroid[1]), {
              icon,
              interactive: false
            }).addTo(map!);
            forestStandLabels.push(label);
          }
        }
      }
    }).addTo(map!);
  }

  async function loadCadastralUnit(L: typeof import('leaflet'), tun: string): Promise<void> {
    if (cadastralLayer) {
      cadastralLayer.remove();
      cadastralLayer = null;
    }
    if (forestStandsLayer) {
      forestStandsLayer.remove();
      forestStandsLayer = null;
    }
    clearForestStandLabels();

    const res = await fetch(`/api/cadastral-unit?tunnus=${encodeURIComponent(tun)}`);
    if (!res.ok) throw new Error(`WFS request failed: ${res.status}`);
    if (!map) return;

    const geojson: GeoJSON.FeatureCollection = await res.json();
    console.info('[CadastralMap] cadastral unit loaded', {
      featureCount: geojson.features?.length ?? 0,
      tun
    });
    if (!geojson.features?.length) throw new Error(`No cadastral unit found for tunnus: ${tun}`);

    cadastralLayer = L.geoJSON(geojson, {
      style: {
        color: '#e63946',
        weight: 2,
        fillOpacity: 0.15
      }
    }).addTo(map!);

    map!.fitBounds(cadastralLayer.getBounds(), { padding: [40, 40] });

    await loadForestStands(L, tun);
    if (!map) return;
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
    clearForestStandLabels();
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

  :global(.forest-stand-label) {
    background: none;
    border: none;
    box-shadow: none;
  }

  :global(.forest-stand-label span) {
    display: block;
    color: #1d3557;
    font-weight: 700;
    font-size: 12px;
    text-align: center;
    line-height: 20px;
    text-shadow:
      -1px -1px 0 #fff,
       1px -1px 0 #fff,
      -1px  1px 0 #fff,
       1px  1px 0 #fff;
  }
</style>
