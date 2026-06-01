<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import CadastralMap from '$lib/components/shared/CadastralMap.svelte';
	import FscBadge from '$lib/components/shared/FscBadge.svelte';
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
	<div class="employee-state-block is-loading">Laetakse katastrit…</div>
{:else if cadaster}
	<p class="back-link">
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
	</p>

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
		<h2>Katastriüksus kaardil</h2>
		<CadastralMap tunnus={cadaster.cadastralNumber} />
	</section>
{/if}

<style>
	.back-link {
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
