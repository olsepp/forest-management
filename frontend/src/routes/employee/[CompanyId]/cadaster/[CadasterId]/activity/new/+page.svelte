<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import ActivityForm from '$lib/components/employee/ActivityForm.svelte';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type CadasterSummaryDto = {
		id: string;
		cadastralNumber: string;
		landPropertyId: string;
		landPropertyName: string;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let cadaster = $state<CadasterSummaryDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');
	let isUnauthorized = $state(false);

	async function loadCadasterSummary() {
		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const cadasterId = $page.params.CadasterId;
			if (!cadasterId) {
				errorMessage = 'Missing cadaster id.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!response.ok) {
				if (response.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Unauthorized. Please sign in again.';
					return;
				}

				errorMessage = response.status === 404 ? 'Cadaster not found.' : 'Failed to load cadaster.';
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
			errorMessage = 'Failed to load cadaster.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadCadasterSummary);
</script>

<h1>Log activity for cadaster</h1>

<p class="breadcrumb">
	<a
		href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
			CompanyId: $page.params.CompanyId,
			CadasterId: $page.params.CadasterId
		})}>← Back to cadaster details</a
	>
</p>

{#if isLoading}
	<div class="employee-state-block is-loading">Loading cadaster…</div>
{:else if errorMessage}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Your session may have expired.</span>
		{/if}
	</div>
{:else if cadaster}
	<section class="employee-card summary">
		<h2>Cadaster context</h2>
		<p><strong>Cadastral number:</strong> {cadaster.cadastralNumber}</p>
		<p>
			<strong>Land property:</strong>
			<a
				href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
					CompanyId: $page.params.CompanyId,
					LandPropertyId: cadaster.landPropertyId
				})}
				>{cadaster.landPropertyName}</a
			>
		</p>
	</section>

	<ActivityForm
		companyId={$page.params.CompanyId}
		cadasterId={cadaster.id}
		cadasterLabel={cadaster.cadastralNumber}
		lockCadaster={true}
		cancelHref={`/employee/${$page.params.CompanyId}/cadaster/${cadaster.id}`}
		redirectHref={`/employee/${$page.params.CompanyId}/cadaster/${cadaster.id}`}
		submitLabel="Log activity"
	/>
{/if}

<style>
	.breadcrumb {
		margin-top: -0.25rem;
		margin-bottom: 0.9rem;
	}

	.breadcrumb a {
		font-size: 0.9rem;
		font-weight: 700;
		text-decoration: none;
		color: #1f5a42;
	}

	h1 {
		margin: 0 0 0.7rem;
		font-size: 1.2rem;
		color: #17251e;
	}

	h2 {
		margin: 0 0 0.6rem;
		font-size: 1.05rem;
		color: #1a3228;
	}

	.summary {
		margin-bottom: 0.85rem;
	}

	.summary p {
		margin: 0.3rem 0;
		color: #3f564a;
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

