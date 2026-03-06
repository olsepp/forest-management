<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import ActivityForm from '$lib/components/admin/ActivityForm.svelte';
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

	async function loadCadasterSummary() {
		try {
			errorMessage = '';
			isLoading = true;

			const cadasterId = $page.params.CadasterId;
			if (!cadasterId) {
				errorMessage = 'Missing cadaster id';
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
						? 'Cadaster not found.'
						: response.status === 401
							? 'Unauthorized. Please sign in again.'
							: 'Failed to load cadaster.';
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
	<a href={`/admin/${$page.params.CompanyId}/cadaster/${$page.params.CadasterId}`}>← Back to cadaster details</a>
</p>

{#if isLoading}
	<p>Loading cadaster...</p>
{:else if errorMessage}
	<p class="error">{errorMessage}</p>
{:else if cadaster}
	<section class="summary card">
		<h2>Cadaster context</h2>
		<p><strong>Cadastral number:</strong> {cadaster.cadastralNumber}</p>
		<p>
			<strong>Land property:</strong>
			<a href={`/admin/${$page.params.CompanyId}/landproperty/${cadaster.landPropertyId}`}
				>{cadaster.landPropertyName}</a
			>
		</p>
	</section>

	<ActivityForm
		companyId={$page.params.CompanyId}
		cadasterId={cadaster.id}
		cadasterLabel={cadaster.cadastralNumber}
		lockCadaster={true}
		cancelHref={`/admin/${$page.params.CompanyId}/cadaster/${cadaster.id}`}
		redirectHref={`/admin/${$page.params.CompanyId}/activity`}
		submitLabel="Log activity"
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
