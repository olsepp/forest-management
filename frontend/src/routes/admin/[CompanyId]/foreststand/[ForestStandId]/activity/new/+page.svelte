<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import ActivityForm from '$lib/components/admin/ActivityForm.svelte';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';
	import type { ForestStandSummaryDto } from '$lib/dtos/forest-stand/forest-stand.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let forestStand = $state<ForestStandSummaryDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');
	const companyId = $derived($page.params.CompanyId ?? '');
	const forestStandId = $derived($page.params.ForestStandId ?? '');

	async function loadForestStandSummary() {
		try {
			errorMessage = '';
			isLoading = true;

			const forestStandId = $page.params.ForestStandId;
			if (!forestStandId) {
				errorMessage = 'Puudub eraldise ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/foreststands/${forestStandId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 404
						? 'Eraldist ei leitud.'
						: response.status === 401
							? 'Ligipääs puudub. Logige uuesti sisse.'
							: 'Eraldise laadimine ebaõnnestus.';
				return;
			}

			const dto = (await response.json()) as ForestStandSummaryDto;
			forestStand = {
				id: dto.id,
				number: dto.number,
				cadasterId: dto.cadasterId,
				cadasterCadastralNumber: dto.cadasterCadastralNumber,
				landPropertyId: dto.landPropertyId,
				landPropertyName: dto.landPropertyName
			};
		} catch {
			errorMessage = 'Eraldise laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadForestStandSummary);
</script>

<h1>Logi tegevus eraldisele</h1>

<p class="breadcrumb">
	<a
		href={resolve('/admin/[CompanyId]/foreststand/[ForestStandId]', {
			CompanyId: companyId,
			ForestStandId: forestStandId
		})}>← Tagasi eraldise detailidesse</a
	>
</p>

{#if isLoading}
	<p>Laetakse eraldist...</p>
{:else if errorMessage}
	<p class="error">{errorMessage}</p>
{:else if forestStand}
	<section class="meta-grid">
		<article class="meta-card">
			<p class="meta-label">Eraldis</p>
			<p class="meta-value">Eraldis {forestStand.number}</p>
		</article>
		<article class="meta-card">
			<p class="meta-label">Kataster</p>
			<p class="meta-value">
				<a
					href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
						CompanyId: companyId,
						CadasterId: forestStand.cadasterId
					})}
				>
					{forestStand.cadasterCadastralNumber}
				</a>
			</p>
		</article>
		<article class="meta-card">
			<p class="meta-label">Kinnistu</p>
			<p class="meta-value">
				<a
					href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
						CompanyId: companyId,
						LandPropertyId: forestStand.landPropertyId!}
				)}>{forestStand.landPropertyName}</a
				>
			</p>
		</article>
	</section>

	<ActivityForm
		{companyId}
		cadasterId={forestStand.cadasterId}
		cadasterLabel={forestStand.cadasterCadastralNumber}
		forestStandId={forestStand.id}
		lockCadaster={true}
		cancelHref={`/admin/${companyId}/foreststand/${forestStand.id}`}
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
