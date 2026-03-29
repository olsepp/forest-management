<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import ActivityForm from '$lib/components/admin/ActivityForm.svelte';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type ForestStandSummaryDto = {
		id: string;
		number: number;
		cadasterId: string;
		cadasterCadastralNumber: string;
		landPropertyId?: string;
		landPropertyName?: string;
	};

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
		})}
		>← Tagasi eraldise detailidesse</a
	>
</p>

{#if isLoading}
	<p>Laetakse eraldist...</p>
{:else if errorMessage}
	<p class="error">{errorMessage}</p>
{:else if forestStand}
	<section class="summary card">
		<h2>Eraldise kontekst</h2>
		<p><strong>Eraldis:</strong> Eraldis {forestStand.number}</p>
		<p>
			<strong>Kataster:</strong>
			<a
				href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
					CompanyId: companyId,
					CadasterId: forestStand.cadasterId
				})}
				>{forestStand.cadasterCadastralNumber}</a
			>
		</p>
		{#if forestStand.landPropertyId && forestStand.landPropertyName}
			<p>
				<strong>Kinnistu:</strong>
				<a
					href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
						CompanyId: companyId,
						LandPropertyId: forestStand.landPropertyId
					})}
					>{forestStand.landPropertyName}</a
				>
			</p>
		{/if}
	</section>

	<ActivityForm
		companyId={companyId}
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

	.card {
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.75rem;
		background: #fff;
	}

	.summary {
		margin-bottom: 1rem;
	}

	.error {
		margin-top: 0.75rem;
		color: #b91c1c;
	}
</style>
