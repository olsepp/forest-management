<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import CadastralMap from '$lib/components/shared/CadastralMap.svelte';
	import FscBadge from '$lib/components/shared/FscBadge.svelte';
	import type { GeoJSON } from 'leaflet';
	import type {
		CadasterDto,
		ForestStandListDto,
		ActivityListDto
	} from '$lib/dtos/cadaster/cadaster.dto';

	let {
		data
	}: {
		data: {
			cadaster: CadasterDto | null;
			forestStands: ForestStandListDto[];
			activities: ActivityListDto[];
		};
	} = $props();
	let cadaster = $derived(data.cadaster);
	let forestStands = $derived(data.forestStands ?? []);
	let activities = $derived(data.activities ?? []);
	let isLoading = $derived(!cadaster);

	let companyId = $derived($page.params.CompanyId ?? '');
	let showMap = $state(false);
	let showingLocation = $state(false);
	let locateError = $state<string | null>(null);
	let wazeUrl = $state<string | null>(null);
	let wazeLoading = $state(true);

	$effect(() => {
		if (!cadaster?.cadastralNumber) {
			wazeUrl = null;
			wazeLoading = false;
			return;
		}

		wazeLoading = true;
		let cancelled = false;

		const fetchCoordinates = async () => {
			try {
				const res = await fetch(`/api/cadastral-unit?tunnus=${encodeURIComponent(cadaster.cadastralNumber)}`);
				if (!res.ok) return;

				const geojson = await res.json();
				if (cancelled) return;

				if (geojson.features?.length) {
					const feature = geojson.features[0];
					const geom = feature.geometry;
					if (geom) {
						let lat: number | null = null;
						let lng: number | null = null;

						if (geom.type === 'Polygon') {
							const coords = (geom as GeoJSON.Polygon).coordinates;
							if (coords[0]?.[0]) {
								[lng, lat] = coords[0][0];
							}
						} else if (geom.type === 'MultiPolygon') {
							const coords = (geom as GeoJSON.MultiPolygon).coordinates;
							if (coords[0]?.[0]?.[0]) {
								[lng, lat] = coords[0][0][0];
							}
						}

						if (lat !== null && lng !== null) {
							wazeUrl = `https://waze.com/ul?ll=${lat},${lng}&navigate=yes`;
						}
					}
				}
			} catch {
				// Silently fail - button stays disabled
			} finally {
				if (!cancelled) {
					wazeLoading = false;
				}
			}
		};

		fetchCoordinates();

		return () => { cancelled = true; };
	});

	$effect(() => {
		if (locateError) {
			const timeout = setTimeout(() => { locateError = null; }, 5000);
			return () => clearTimeout(timeout);
		}
	});

	function formatDate(value: string | null): string {
		if (!value) return '—';
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleDateString();
	}

	function formatNumber(value: number | null | undefined): string {
		if (typeof value !== 'number' || Number.isNaN(value)) return '—';
		return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(value);
	}

	function formatActivityQuantity(activity: ActivityListDto): string {
		const quantity = Number.isFinite(activity.quantity) ? String(activity.quantity) : '—';
		return activity.unit ? `${quantity} ${activity.unit}` : quantity;
	}
</script>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse katastri andmeid… Halva ühenduse korral võib see veidi aega võtta.</div>
{:else if cadaster}
	<div class="page-header">
		<a
			class="back-link-button"
			href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
				CompanyId: companyId,
				LandPropertyId: cadaster.landPropertyId
			})}
		>
			<span aria-hidden="true">←</span>
			<span>Tagasi kinnistu juurde</span>
		</a>
		<a
			class="waze-link"
			class:disabled={wazeLoading || !wazeUrl}
			href={wazeUrl || '#'}
			tabindex={wazeLoading || !wazeUrl ? -1 : 0}
			aria-disabled={wazeLoading || !wazeUrl}
			aria-label="Ava katastri asukoht Waze'is"
			target="_blank"
			rel="noopener noreferrer"
		>
			{#if wazeLoading}
				<span class="waze-loading-text">Laetakse...</span>
			{:else}
				<svg class="waze-icon" viewBox="0 0 108 100" fill="white" xmlns="http://www.w3.org/2000/svg">
					<path fill="none" d="M58.9 83.8H49c-1.1-5.5-6-9.7-11.8-9.7-4.3 0-8 2.2-10.2 5.5v.1c-3.6-1.8-6.9-4.3-9.7-7.2-3.4-3.4-5.3-6.5-6.1-8.5 2.2-.5 4.2-1.7 5.8-3.4 2.1-2.2 3.2-5.2 3.2-8.2v-7.2c0-8.5 2.8-16.9 8.1-23.6 7.5-9.6 18.6-15 30.6-15 10.3 0 20 4 27.3 11.3 7.3 7.3 11.3 17 11.3 27.3s-4 20-11.3 27.3a38.84 38.84 0 0 1-27.3 11.3z"></path>
					<path d="M102.3 45.1c0-11.6-4.5-22.5-12.7-30.7A43.88 43.88 0 0 0 58.9 1.7c-13.3 0-25.7 6-34.2 16.7-6.1 7.7-9.3 17.2-9.3 27v7c0 3.6-2.5 7-7.5 7.3-1.2.1-2.2.9-2.2 2.1-.2 3.3 3.3 9.3 8.1 14.1 3.3 3.4 7.2 6.1 11.4 8.2a12.08 12.08 0 0 0 11.9 14.2c5.9 0 10.7-4.1 11.8-9.6H59c1.3 6.8 8.4 11.5 16 8.9 6.6-2.2 9.5-9.7 7.1-15.8 2.6-1.7 5.1-3.7 7.4-5.9a43.2 43.2 0 0 0 12.8-30.8zM58.9 83.8H49c-1.1-5.5-6-9.7-11.8-9.7-4.3 0-8 2.2-10.2 5.5v.1c-3.6-1.8-6.9-4.3-9.7-7.2-3.4-3.4-5.3-6.5-6.1-8.5 2.2-.5 4.2-1.7 5.8-3.4 2.1-2.2 3.2-5.2 3.2-8.2v-7.2c0-8.5 2.8-16.9 8.1-23.6 7.5-9.6 18.6-15 30.6-15 10.3 0 20 4 27.3 11.3 7.3 7.3 11.3 17 11.3 27.3s-4 20-11.3 27.3a38.84 38.84 0 0 1-27.3 11.3z"></path>
					<circle cx="78.2" cy="35.5" r="4.8"></circle>
					<circle cx="49.2" cy="35.5" r="4.8"></circle>
					<path d="M50.7 51.3c-.4-.8-1.3-1.4-2.2-1.4a2.4 2.4 0 0 0-2.2 3.4c3.1 6.5 9.7 11.1 17.5 11.1s14.4-4.5 17.5-11.1c.7-1.6-.4-3.4-2.2-3.4H79c-.9 0-1.7.5-2.1 1.4-2.3 4.9-7.3 8.3-13.1 8.3S53 56.2 50.7 51.3z"></path>
				</svg>
			{/if}
		</a>
	</div>

	<section class="employee-card summary">
		<div class="summary-head">
			<div>
				<h1>{cadaster.cadastralNumber}</h1>
			</div>
			<a
				class="log-activity-link"
				href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]/activity/new', {
					CompanyId: companyId,
					CadasterId: cadaster.id
				})}
			>
				Logi tegevus
			</a>
		</div>

		<div class="meta-grid">
			<p><strong>Kinnistu:</strong> {cadaster.landPropertyName || '—'} <FscBadge isFsc={cadaster.landPropertyIsFsc} /></p>
			<p><strong>Metsamaa pindala:</strong> {formatNumber(cadaster.forestArea)}</p>
			<p><strong>Haritav maa:</strong> {formatNumber(cadaster.arableArea)}</p>
			<p><strong>Rohumaa:</strong> {formatNumber(cadaster.grasslandArea)}</p>
			<p><strong>Õueala:</strong> {formatNumber(cadaster.yardArea)}</p>
			<p><strong>Ehitusala:</strong> {formatNumber(cadaster.buildingFootprintArea)}</p>
			<p><strong>Veealune maa:</strong> {formatNumber(cadaster.underwaterArea)}</p>
			<p><strong>Muu maa:</strong> {formatNumber(cadaster.otherArea)}</p>
			<p><strong>Boniteet:</strong> {formatNumber(cadaster.soilQualityIndex)}</p>
			<p><strong>Arvutatud tagavara:</strong> {formatNumber(cadaster.calculatedVolume)}</p>
			<p><strong>Mahukasv:</strong> {formatNumber(cadaster.volumeGrowth)}</p>
		</div>
	</section>

	<section class="employee-card">
		<h2>Eraldised</h2>
		{#if forestStands.length === 0}
			<div class="employee-state-block is-empty">Eraldisi ei leitud.</div>
		{:else}
			<div class="stand-button-grid" aria-label="Eraldised">
				{#each forestStands as stand (stand.id)}
					<a
						class="stand-button"
						href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
							CompanyId: companyId,
							ForestStandId: stand.id
						})}
						aria-label={`Ava eraldis ${stand.number}`}
						data-sveltekit-preload-data="tap"
					>
						#{stand.number}
					</a>
				{/each}
			</div>
		{/if}
	</section>

	<section class="employee-card">
		<div class="section-head">
			<h2>Sinu tegevused katastril</h2>
		</div>
		{#if activities.length === 0}
			<div class="employee-state-block is-empty">Ei leitud.</div>
		{:else}
			<div class="employee-stack-cards">
				{#each activities as activity (activity.id)}
					<article class="activity-card">
						<p class="activity-head">
							<strong>{activity.activityTypeName || 'Tegevus'}</strong>
							<span>{formatDate(activity.date)}</span>
						</p>
						<p>{activity.description || '—'}</p>
						<p><strong>Kogus:</strong> {formatActivityQuantity(activity)}</p>
						<p><strong>Eraldis:</strong> {activity.forestStandNumber || '—'}</p>
						<a
							class="activity-link"
							href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
								CompanyId: companyId,
								ActivityId: activity.id
							})}
							data-sveltekit-preload-data="tap"
						>
							Ava tegevus
						</a>
					</article>
				{/each}
			</div>
		{/if}
		<a
			class="log-activity-link is-secondary"
			href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]/activity/new', {
				CompanyId: companyId,
				CadasterId: cadaster.id
			})}
		>
			Logi uus tegevus
		</a>
	</section>

	<section class="employee-card">
		<div class="section-head">
			<h2>Katastriüksus kaardil</h2>
			<button
				type="button"
				class="map-toggle-btn"
				onclick={() => {
					showMap = !showMap;
					if (!showMap) showingLocation = false;
				}}
			>
				{showMap ? 'Peida kaart' : 'Näita kaarti'}
			</button>
			{#if showMap}
				<button
					type="button"
					class="location-btn"
					class:is-active={showingLocation}
					onclick={() => { showingLocation = !showingLocation; locateError = null; }}
				>
					<svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
						<line x1="12" y1="2" x2="12" y2="6" />
						<line x1="12" y1="18" x2="12" y2="22" />
						<line x1="2" y1="12" x2="6" y2="12" />
						<line x1="18" y1="12" x2="22" y2="12" />
						<circle cx="12" cy="12" r="4" />
					</svg>
					<span>{showingLocation ? 'Peida asukoht' : 'Minu asukoht'}</span>
				</button>
			{/if}
		</div>
		{#if locateError}
			<div class="location-error">{locateError}</div>
		{/if}
		{#if showMap}
			<CadastralMap
				tunnus={cadaster.cadastralNumber}
				showUserLocation={showingLocation}
				onLocationError={(msg: string) => { locateError = msg; }}
			/>
		{/if}
	</section>
{/if}

<style>
	.page-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.7rem;
		margin: 0 0 0.9rem;
	}

	.back-link-button {
		display: inline-flex;
		align-items: center;
		gap: 0.45rem;
		min-height: 3rem;
		padding: 0.65rem 0.95rem;
		border-radius: 0.85rem;
		border: 1px solid #c4d4cd;
		background: #ffffff;
		font-size: 0.97rem;
		font-weight: 700;
		text-decoration: none;
		color: #1f3f33;
		box-shadow: 0 2px 8px rgba(15, 37, 28, 0.06);
	}

	.back-link-button:hover {
		background: #f5f9f7;
		border-color: #afc6bb;
	}

	.waze-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3rem;
		min-width: 3rem;
		padding: 0.6rem 0.85rem;
		border: 1px solid #1f5a42;
		border-radius: 0.85rem;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		box-shadow: 0 6px 16px rgba(15, 42, 31, 0.22);
		cursor: pointer;
	}

	.waze-link:hover {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
	}

	.waze-link:active {
		transform: translateY(1px);
		box-shadow: 0 3px 10px rgba(15, 42, 31, 0.2);
	}

	.waze-icon {
		width: 1.5rem;
		height: 1.5rem;
	}

	.waze-loading-text {
		font-size: 0.85rem;
		font-weight: 700;
		color: #f3fbf7;
	}

	.waze-link.disabled {
		background: linear-gradient(180deg, #9ca3af 0%, #6b7280 100%);
		border-color: #9ca3af;
		box-shadow: none;
		cursor: not-allowed;
		pointer-events: none;
	}

	.waze-link.disabled:hover {
		background: linear-gradient(180deg, #9ca3af 0%, #6b7280 100%);
		border-color: #9ca3af;
		transform: none;
	}

	.summary {
		margin-bottom: 0.75rem;
	}

	.section-head {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.7rem;
		margin-bottom: 0.65rem;
	}

	.map-toggle-btn {
		min-height: 2.6rem;
		padding: 0.45rem 0.85rem;
		border: 1px solid #1f5a42;
		border-radius: 0.82rem;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		box-shadow: 0 6px 16px rgba(15, 42, 31, 0.22);
		color: #f3fbf7;
		font-size: 0.88rem;
		font-weight: 700;
		cursor: pointer;
	}

	.map-toggle-btn:hover {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
	}

	.map-toggle-btn:active {
		transform: translateY(1px);
		box-shadow: 0 3px 10px rgba(15, 42, 31, 0.2);
	}

	.location-btn {
		display: inline-flex;
		align-items: center;
		gap: 0.4rem;
		min-height: 2.6rem;
		padding: 0.45rem 0.85rem;
		border: 1px solid #1a73e8;
		border-radius: 0.82rem;
		background: #ffffff;
		color: #1a73e8;
		font-size: 0.88rem;
		font-weight: 700;
		cursor: pointer;
		box-shadow: 0 2px 8px rgba(26, 115, 232, 0.12);
	}

	.location-btn:hover {
		background: #e8f1ff;
		border-color: #1557b0;
	}

	.location-btn:active {
		transform: translateY(1px);
		box-shadow: 0 1px 4px rgba(26, 115, 232, 0.15);
	}

	.location-btn.is-active {
		background: #1a73e8;
		color: #ffffff;
	}

	.location-btn.is-active:hover {
		background: #1557b0;
		border-color: #0e4da4;
	}

	.location-error {
		margin-bottom: 0.5rem;
		padding: 0.5rem 0.75rem;
		border-radius: 0.62rem;
		background: #fef2f2;
		border: 1px solid #fecaca;
		color: #991b1b;
		font-size: 0.85rem;
		font-weight: 500;
	}

	.summary-head {
		display: flex;
		flex-wrap: wrap;
		justify-content: space-between;
		align-items: center;
		gap: 0.65rem;
		margin-bottom: 0.65rem;
	}

	.summary-head h1 {
		color: #1e553f;
		font-weight: bold;
		font-size: 1.8rem;
	}

	h1 {
		margin: 0.3rem 0;
		font-size: 1.2rem;
		line-height: 1.2;
		color: #17251e;
	}

	h2 {
		margin: 0;
		font-size: 1.05rem;
		color: #1f2937;
	}

	.log-activity-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3.5rem;
		padding: 0.6rem 1rem;
		border: 1px solid #1f5a42;
		border-radius: 0.85rem;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		box-shadow: 0 6px 16px rgba(15, 42, 31, 0.22);
		color: #f3fbf7;
		font-size: 1rem;
		font-weight: 700;
		text-decoration: none;
	}

	.log-activity-link:hover {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
	}

	.log-activity-link:active {
		transform: translateY(1px);
		box-shadow: 0 3px 10px rgba(15, 42, 31, 0.2);
	}

	.log-activity-link.is-secondary {
		background: linear-gradient(180deg, #3d7a5a 0%, #2d6148 100%);
		color: #ffffff;
		box-shadow: 0 4px 12px rgba(15, 42, 31, 0.15);
		min-height: 3.25rem;
		margin-top: 1rem;
	}

	.log-activity-link.is-secondary:hover {
		background: linear-gradient(180deg, #458664 0%, #356b52 100%);
	}

	.meta-grid {
		display: grid;
		gap: 0.5rem;
	}

	.meta-grid p {
		margin: 0;
		color: #334155;
	}

	.activity-card {
		border: 1px solid #d8e0dc;
		border-radius: 0.8rem;
		padding: 0.9rem;
		background: #ffffff;
		display: grid;
		gap: 0.42rem;
	}

	.stand-button-grid {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
		gap: 0.55rem;
	}

	.stand-button {
		text-decoration: none;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3rem;
		border: 1px solid #1f5a42;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		box-shadow: 0 6px 16px rgba(15, 42, 31, 0.22);
		color: #f3fbf7;
		border-radius: 0.82rem;
		font-size: 1rem;
		font-weight: 700;
	}

	.stand-button:hover {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
	}

	.stand-button:active {
		transform: translateY(1px);
		box-shadow: 0 3px 10px rgba(15, 42, 31, 0.2);
	}

	.activity-card p {
		margin: 0;
		color: #334155;
	}

	.activity-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		align-self: start;
		min-height: 2.75rem;
		margin-top: 0.2rem;
		padding: 0.45rem 0.8rem;
		border: 1px solid #bfd0c8;
		border-radius: 0.75rem;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		font-size: 0.95rem;
		font-weight: 700;
		color: white;
		text-decoration: none;
	}

	.activity-head {
		display: flex;
		justify-content: space-between;
		gap: 0.6rem;
	}

	@media (max-width: 420px) {
		.page-header {
			flex-direction: column;
			align-items: stretch;
		}

		.section-head {
			flex-direction: column;
			align-items: stretch;
		}

		.log-activity-link.is-secondary {
			width: 100%;
		}
	}
	@media (min-width: 768px) {
		.stand-button-grid {
			grid-template-columns: repeat(4, minmax(0, 1fr));
		}
	}
</style>
