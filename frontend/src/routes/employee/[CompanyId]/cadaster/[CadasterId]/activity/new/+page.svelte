<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import ActivityForm from '$lib/components/employee/ActivityForm.svelte';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';
	import type { CadasterSummaryDto } from '$lib/dtos/forest-stand/forest-stand.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let cadaster = $state<CadasterSummaryDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');
	let isUnauthorized = $state(false);
	const companyId = $derived($page.params.CompanyId ?? '');
	const cadasterId = $derived($page.params.CadasterId ?? '');

	async function loadCadasterSummary() {
		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const cadasterId = $page.params.CadasterId;
			if (!cadasterId) {
				errorMessage = 'Puudub katastri ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!response.ok) {
				if (response.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage =
					response.status === 404 ? 'Katasterit ei leitud.' : 'Katastri laadimine ebaõnnestus.';
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

<h1 class="employee-page-title">Logi tegevus katastri jaoks</h1>

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
{:else if errorMessage}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Teie sessioon võib olla aegunud.</span>
		{/if}
	</div>
{:else if cadaster}
	<section class="employee-card summary">
		<h2>Katastri kontekst</h2>
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
	h2 {
		margin: 0 0 0.6rem;
		font-size: 1.05rem;
		color: #1f2937;
	}

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
