	<script lang="ts">
		import { page } from '$app/stores';
		import { resolve } from '$app/paths';
		import ActivityForm from '$lib/components/admin/ActivityForm.svelte';
		import FscBadge from '$lib/components/shared/FscBadge.svelte';
		import type { CadasterSummaryDto } from '$lib/dtos/forest-stand/forest-stand.dto';

	let { data }: { data: { cadaster: CadasterSummaryDto } } = $props();
	let cadaster = $derived(data.cadaster);
	let isLoading = $derived(!cadaster);

	const companyId = $derived($page.params.CompanyId ?? '');
	const cadasterId = $derived($page.params.CadasterId ?? '');
</script>

<h1>Logi tegevus katastrile</h1>

<p class="breadcrumb">
	<a
		href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
			CompanyId: companyId,
			CadasterId: cadasterId
		})}>← Tagasi katastri juurde</a
	>
</p>

{#if isLoading}
	<p>Laetakse katastrit...</p>
{:else if cadaster}
	<section class="meta-grid">
		<article class="meta-card">
			<p class="meta-label">Eraldis</p>
			<p class="meta-value"></p>
		</article>
		<article class="meta-card">
			<p class="meta-label">Kataster</p>
			<p class="meta-value">
				<a
					href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
						CompanyId: companyId,
						CadasterId: cadaster.id
					})}
				>
					{cadaster.cadastralNumber}
				</a>
			</p>
		</article>
		<article class="meta-card">
			<p class="meta-label">Kinnistu</p>
			<p class="meta-value">
				<a
					href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
						CompanyId: companyId,
						LandPropertyId: cadaster.landPropertyId
					})}>{cadaster.landPropertyName}</a
				>
				<FscBadge isFsc={cadaster.landPropertyIsFsc} />
			</p>
		</article>
	</section>

	<ActivityForm
		{companyId}
		cadasterId={cadaster.id}
		lockCadaster={true}
		redirectHref={`/admin/${companyId}/activity`}
		submitLabel="Logi tegevus"
	/>
{/if}

<style>
	.breadcrumb {
		margin-top: -0.25rem;
		margin-bottom: 1rem;
	}

	.breadcrumb a {
		color: #0f766e;
		text-decoration: none;
	}

	.breadcrumb a:hover {
		text-decoration: underline;
	}

	.error {
		margin-top: 0.75rem;
		color: #b91c1c;
	}

	.meta-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.8rem;
		margin-bottom: 1rem;
	}

	.meta-card {
		padding: 0.9rem;
		border: 1px solid #c9dace;
		border-radius: 0.75rem;
		background: #f4faf6;
	}

	.meta-label {
		margin: 0;
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.08em;
	}

	.meta-value {
		margin: 0.35rem 0 0;
		font-size: 1rem;
		font-weight: 600;
	}
</style>
