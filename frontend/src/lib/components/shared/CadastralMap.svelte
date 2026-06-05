<script lang="ts">
  import { onMount, onDestroy } from 'svelte';
  import type { Map, GeoJSON, TileLayer, Marker, Circle } from 'leaflet';
  import 'leaflet/dist/leaflet.css';

  export let tunnus: string = '12301:001:0012';
  export let showUserLocation = false;
  export let onLocationError: ((message: string) => void) | null = null;

  const RETRY_DELAY_MS = 5000;

  let mapEl: HTMLDivElement;
  let map: Map | null = null;
  let cadastralLayer: GeoJSON | null = null;
  let forestStandsLayer: GeoJSON | null = null;
  let forestStandLabels: Marker[] = [];
  let retryTimeout: ReturnType<typeof setTimeout> | null = null;
  let retryScheduled = false;
  let userMarker: Marker | null = null;
  let accuracyCircle: Circle | null = null;
  let isFirstLocate = true;
  let lastUserLocation: import('leaflet').LatLng | null = null;
  let recenterControl: import('leaflet').Control | null = null;
  let recenterAdded = false;
  let isLocating = false;

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

  function clearUserLocation(): void {
    if (userMarker) {
      userMarker.remove();
      userMarker = null;
    }
    if (accuracyCircle) {
      accuracyCircle.remove();
      accuracyCircle = null;
    }
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

  $: if (showUserLocation && map) {
    clearUserLocation();
    isFirstLocate = true;
    isLocating = true;
    map.locate({ watch: true, enableHighAccuracy: true });
    if (recenterControl && !recenterAdded) {
      recenterControl.addTo(map);
      recenterAdded = true;
    }
  } else if (!showUserLocation && map) {
    if (isLocating) {
      map.stopLocate();
      isLocating = false;
    }
    clearUserLocation();
    lastUserLocation = null;
    if (recenterControl && recenterAdded) {
      map.removeControl(recenterControl);
      recenterAdded = false;
    }
    if (cadastralLayer) {
      map.fitBounds(cadastralLayer.getBounds(), { padding: [40, 40] });
    }
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

    map.on('locationfound', (e: any) => {
      clearUserLocation();

      if (isFirstLocate) {
        isFirstLocate = false;
        map!.setView(e.latlng, 16);
      }

      lastUserLocation = e.latlng;

      const icon = L.divIcon({
        className: 'user-location-icon',
        html: '<div class="user-location-pulse"></div><div class="user-location-dot"></div>',
        iconSize: [30, 30],
        iconAnchor: [15, 15]
      });

      userMarker = L.marker(e.latlng, { icon })
        .bindPopup('Sinu asukoht')
        .addTo(map!);

      accuracyCircle = L.circle(e.latlng, {
        radius: e.accuracy,
        color: '#1a73e8',
        fillColor: '#1a73e8',
        fillOpacity: 0.1,
        weight: 1
      }).addTo(map!);
    });

    map.on('locationerror', (e: any) => {
      const messages: Record<number, string> = {
        1: 'Asukoha luba on keelatud',
        2: 'Asukohta ei leitud',
        3: 'Asukoha määramine aegus'
      };
      onLocationError?.(messages[e.code] ?? e.message);
    });

    const RecenterControl = L.Control.extend({
      onAdd: function() {
        const btn = L.DomUtil.create('button', 'recenter-control');
        btn.innerHTML = `<svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="#333" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
          <line x1="12" y1="2" x2="12" y2="6" />
          <line x1="12" y1="18" x2="12" y2="22" />
          <line x1="2" y1="12" x2="6" y2="12" />
          <line x1="18" y1="12" x2="22" y2="12" />
          <circle cx="12" cy="12" r="4" />
        </svg>`;
        btn.title = 'Keskendu asukohale';
        L.DomEvent.disableClickPropagation(btn);
        L.DomEvent.on(btn, 'click', () => {
          if (lastUserLocation && map) {
            map.setView(lastUserLocation, map.getZoom());
          }
        });
        return btn;
      }
    });
    recenterControl = new RecenterControl({ position: 'bottomright' });

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
    if (isLocating) {
      map?.stopLocate();
      isLocating = false;
    }
    clearUserLocation();
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

  :global(.user-location-icon) {
    background: none;
    border: none;
    box-shadow: none;
  }

  :global(.user-location-pulse) {
    position: absolute;
    top: 50%;
    left: 50%;
    width: 28px;
    height: 28px;
    margin-left: -14px;
    margin-top: -14px;
    border-radius: 50%;
    background: rgba(26, 115, 232, 0.3);
    animation: location-pulse 2s ease-out infinite;
  }

  :global(.user-location-dot) {
    position: absolute;
    top: 50%;
    left: 50%;
    width: 14px;
    height: 14px;
    margin-left: -7px;
    margin-top: -7px;
    border-radius: 50%;
    background: #1a73e8;
    border: 2px solid #ffffff;
    box-shadow: 0 0 6px rgba(0, 0, 0, 0.3);
    z-index: 2;
  }

  @keyframes location-pulse {
    0% {
      transform: scale(1);
      opacity: 1;
    }
    100% {
      transform: scale(2.5);
      opacity: 0;
    }
  }

  :global(.recenter-control) {
    width: 36px;
    height: 36px;
    border: 2px solid rgba(0, 0, 0, 0.2);
    border-radius: 4px;
    background: #ffffff;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    box-shadow: 0 2px 6px rgba(0, 0, 0, 0.3);
    padding: 0;
    font: inherit;
    line-height: 1;
  }

  :global(.recenter-control:hover) {
    background: #f4f4f4;
  }

  :global(.recenter-control svg) {
    display: block;
  }
</style>
