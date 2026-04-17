<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import type {
		LandPropertyDto,
		CadasterLinkDto,
		ActivityDto
	} from '$lib/dtos/land-property/land-property.dto';

	let {
		data
	}: {
		data: {
			property: LandPropertyDto | null;
			cadasters: CadasterLinkDto[];
			activities: ActivityDto[];
		};
	} = $props();
	let property = $derived(data.property);
	let cadasters = $derived(data.cadasters ?? []);
	let activities = $derived(data.activities ?? []);
	let isLoading = $derived(!property);

	let companyId = $derived($page.params.CompanyId ?? '');

	function formatDate(value: string | null): string {
		if (!value) return '—';
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleDateString();
	}

	function formatActivityQuantity(activity: ActivityDto): string {
		const quantity = Number.isFinite(activity.quantity) ? String(activity.quantity) : '—';
		return activity.unit ? `${quantity} ${activity.unit}` : quantity;
	}
</script>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse kinnistut…</div>
{:else if property}
	<p class="employee-back-link">
		<a
			class="employee-back-link-button"
			href={resolve('/employee/[CompanyId]/landproperty', { CompanyId: companyId })}
		>
			<span aria-hidden="true">←</span>
			<span>Tagasi kinnistute juurde</span>
		</a>
	</p>

	<section class="employee-card summary">
		<h1 class="employee-page-title">{property.name}</h1>
		<div class="meta-grid">
			<p><strong>Registrinumber:</strong> {property.registrationNumber}</p>
			<p><strong>Maakond:</strong> {property.county || '—'}</p>
			<p><strong>Vald:</strong> {property.parish || '—'}</p>
			<p><strong>Küla:</strong> {property.village || '—'}</p>
		</div>
	</section>

	<section class="employee-card">
		<h2>Katastrid</h2>
		{#if cadasters.length === 0}
			<div class="employee-state-block is-empty">Ei leitud.</div>
		{:else}
			<div class="cadaster-list" role="list" aria-label="Seotud katastrid">
				{#each cadasters as cadaster (cadaster.id)}
					<a
						class="cadaster-row"
						href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
							CompanyId: companyId,
							CadasterId: cadaster.id
						})}
					>
						{cadaster.cadastralNumber || cadaster.id}
					</a>
				{/each}
			</div>
		{/if}
	</section>

	<section class="employee-card">
		<h2>Sinu tegevused sellel kinnistul</h2>
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
						<p>
							<strong>Siht:</strong>
							{activity.cadasterCadastralNumber
								? `Kataster ${activity.cadasterCadastralNumber}`
								: activity.forestStandNumber
									? `Eraldis ${activity.forestStandNumber}`
									: '—'}
						</p>
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
	</section>
{/if}

<style>
	.summary {
		margin-bottom: 0.75rem;
	}

	.employee-page-title {
		margin: 0;
		color: #1e553f;
		font-weight: bold;
		margin-bottom: 1rem;
	}

	h1 {
		margin: 0;
		font-size: 1.28rem;
		line-height: 1.2;
		color: #0f172a;
	}

	h2 {
		margin: 0 0 0.65rem;
		font-size: 1.05rem;
		color: #1f2937;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}

	.meta-grid {
		display: grid;
		gap: 0.45rem;
	}

	.meta-grid p {
		margin: 0;
		color: #334155;
	}

	.cadaster-list {
		display: grid;
		gap: 0.55rem;
	}

	@media (min-width: 480px) {
		.cadaster-list {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}
	}

	.cadaster-row {
		text-decoration: none;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3.5rem;
		border: 1px solid #1f5a42;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		box-shadow: 0 6px 16px rgba(15, 42, 31, 0.22);
		color: #f3fbf7;
		border-radius: 0.82rem;
		font-size: 1.15rem;
		font-weight: 700;
	}

	.cadaster-row:hover {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
	}

	.cadaster-row:active {
		transform: translateY(1px);
		box-shadow: 0 3px 10px rgba(15, 42, 31, 0.2);
	}

	@media (min-width: 768px) {
		.cadaster-list {
			grid-template-columns: repeat(4, minmax(0, 1fr));
		}
	}

	.activity-card {
		border: 1px solid #d8e0dc;
		border-radius: 0.8rem;
		padding: 0.9rem;
		background: #ffffff;
		display: grid;
		gap: 0.42rem;
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
</style>
