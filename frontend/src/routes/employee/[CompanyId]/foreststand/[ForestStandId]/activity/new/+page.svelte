<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import ActivityForm from '$lib/components/employee/ActivityForm.svelte';
	import type { ForestStandSummaryDto } from '$lib/dtos/forest-stand/forest-stand.dto';

	let { data }: { data: { forestStand: ForestStandSummaryDto } } = $props();
	let forestStand = $derived(data.forestStand);
	let isLoading = $derived(!forestStand);

	const companyId = $derived($page.params.CompanyId ?? '');
	const forestStandId = $derived($page.params.ForestStandId ?? '');
</script>

<p class="employee-back-link">
	<a
		class="employee-back-link-button"
		href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
			CompanyId: companyId,
			ForestStandId: forestStandId
		})}
	>
		<span aria-hidden="true">←</span>
		<span>Tagasi eraldise juurde</span>
	</a>
</p>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse eraldist…</div>
{:else if forestStand}
	<section class="employee-card summary">
		<p><strong>Eraldis:</strong> Eraldis {forestStand.number}</p>
		<p>
			<strong>Kataster:</strong>
			<a
				href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
					CompanyId: companyId,
					CadasterId: forestStand.cadasterId
				})}>{forestStand.cadasterCadastralNumber}</a
			>
		</p>
		{#if forestStand.landPropertyId && forestStand.landPropertyName}
			<p>
				<strong>Kinnistu:</strong>
				<a
					href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
						CompanyId: companyId,
						LandPropertyId: forestStand.landPropertyId
					})}>{forestStand.landPropertyName}</a
				>
			</p>
		{/if}
	</section>

	<ActivityForm
		{companyId}
		cadasterId={forestStand.cadasterId}
		cadasterLabel={forestStand.cadasterCadastralNumber}
		forestStandId={forestStand.id}
		lockCadaster={true}
		cancelHref={`/employee/${companyId}/foreststand/${forestStand.id}`}
		redirectHref={`/employee/${companyId}/foreststand/${forestStand.id}`}
		submitLabel="Logi tegevus"
	/>
{/if}

<style>
	.summary {
		margin-bottom: 0.85rem;
	}

	.summary p {
		margin: 0.3rem 0;
		color: #334155;
	}

	.summary a {
		color: #1f5a42;
		font-weight: 700;
		text-decoration: none;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}
</style>
