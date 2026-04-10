<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import ActivityForm from '$lib/components/admin/ActivityForm.svelte';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';
	import type { CadasterSummaryDto } from '$lib/dtos/forest-stand/forest-stand.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let cadaster = $state<CadasterSummaryDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');
	const companyId = $derived($page.params.CompanyId ?? '');
	const cadasterId = $derived($page.params.CadasterId ?? '');

	async function loadCadasterSummary() {
		try {
			errorMessage = '';
			isLoading = true;

			const cadasterId = $page.params.CadasterId;
			if (!cadasterId) {
				errorMessage = 'Puudub katastri ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 404
						? 'Katastrit ei leitud.'
						: response.status === 401
							? 'Ligipääs puudub. Logige uuesti sisse.'
							: 'Katastri laadimine ebaõnnestus.';
				return;
			}

			const dto = (await response.json()) as CadasterSummaryDto;
			cadaster = {
				id: dto.id,
				cadastralNumber: dto.cadastralNumber,
				landPropertyId: dto.landPropertyId,
				landPropertyName: dto.landPropertyName
			};
		} catch {
			errorMessage = 'Katastri laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadCadasterSummary);
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
{:else if errorMessage}
	<p class="error">{errorMessage}</p>
{:else if cadaster}
	<section class="summary card">
		<h2>Katastri kontekst</h2>
		<p><strong>Katastrinumber:</strong> {cadaster.cadastralNumber}</p>
		<p>
			<strong>Kinnistu:</strong>
			<a
				href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
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
		cancelHref={`/admin/${companyId}/cadaster/${cadaster.id}`}
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
