<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import ActivityForm from '$lib/components/employee/ActivityForm.svelte';
	import type { CadasterSummaryDto } from '$lib/dtos/forest-stand/forest-stand.dto';

	let { data }: { data: { cadaster: CadasterSummaryDto } } = $props();
	let cadaster = $derived(data.cadaster);
	let isLoading = $derived(!cadaster);

	const companyId = $derived($page.params.CompanyId ?? '');
	const cadasterId = $derived($page.params.CadasterId ?? '');
</script>

<p class="employee-back-link">
	<a
		class="employee-back-link-button"
		href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
			CompanyId: companyId,
			CadasterId: cadasterId
		})}
	>
		<span aria-hidden="true">←</span>
		<span>Tagasi katastri juurde</span>
	</a>
</p>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse katastrit…</div>
{:else if cadaster}
	<section class="employee-card summary">
		<p><strong>Katastritunnus:</strong> {cadaster.cadastralNumber}</p>
		<p>
			<strong>Kinnistu:</strong>
			<a
				href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
					CompanyId: companyId,
					LandPropertyId: cadaster.landPropertyId
				})}>{cadaster.landPropertyName}</a
			>
		</p>
	</section>

	<ActivityForm
		{companyId}
		cadasterId={cadaster.id}
		cadasterLabel={cadaster.cadastralNumber}
		lockCadaster={true}
		cancelHref={`/employee/${companyId}/cadaster/${cadaster.id}`}
		redirectHref={`/employee/${companyId}/cadaster/${cadaster.id}`}
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
