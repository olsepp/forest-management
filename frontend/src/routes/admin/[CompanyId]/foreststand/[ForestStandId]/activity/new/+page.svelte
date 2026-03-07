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

	async function loadForestStandSummary() {
		try {
			errorMessage = '';
			isLoading = true;

			const forestStandId = $page.params.ForestStandId;
			if (!forestStandId) {
				errorMessage = 'Missing forest stand id';
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
						? 'Forest stand not found.'
						: response.status === 401
							? 'Unauthorized. Please sign in again.'
							: 'Failed to load forest stand.';
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
			errorMessage = 'Failed to load forest stand.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadForestStandSummary);
</script>

<h1>Log activity for forest stand</h1>

<p class="breadcrumb">
	<a
		href={resolve('/admin/[CompanyId]/foreststand/[ForestStandId]', {
			CompanyId: $page.params.CompanyId,
			ForestStandId: $page.params.ForestStandId
		})}
		>← Back to forest stand details</a
	>
</p>

{#if isLoading}
	<p>Loading forest stand...</p>
{:else if errorMessage}
	<p class="error">{errorMessage}</p>
{:else if forestStand}
	<section class="summary card">
		<h2>Forest stand context</h2>
		<p><strong>Forest stand:</strong> Stand {forestStand.number}</p>
		<p>
			<strong>Cadaster:</strong>
			<a
				href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
					CompanyId: $page.params.CompanyId,
					CadasterId: forestStand.cadasterId
				})}
				>{forestStand.cadasterCadastralNumber}</a
			>
		</p>
		{#if forestStand.landPropertyId && forestStand.landPropertyName}
			<p>
				<strong>Land property:</strong>
				<a
					href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
						CompanyId: $page.params.CompanyId,
						LandPropertyId: forestStand.landPropertyId
					})}
					>{forestStand.landPropertyName}</a
				>
			</p>
		{/if}
	</section>

	<ActivityForm
		companyId={$page.params.CompanyId}
		cadasterId={forestStand.cadasterId}
		cadasterLabel={forestStand.cadasterCadastralNumber}
		forestStandId={forestStand.id}
		lockCadaster={true}
		cancelHref={`/admin/${$page.params.CompanyId}/foreststand/${forestStand.id}`}
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
