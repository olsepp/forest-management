<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import type { CompanyDto } from '$lib/types/company';
	import { onMount } from 'svelte';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let company = $state<CompanyDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;
			const companyId = $page.params.CompanyId;
			if (!companyId) {
				errorMessage = 'Missing company id';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/companies/${companyId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage = response.status === 401 ? 'Unauthorized. Please sign in again.' : 'Failed to load company';
				return;
			}

			company = await response.json();
		} catch {
			errorMessage = 'Failed to load company';
		} finally {
			isLoading = false;
		}
	});
</script>

<h1>Admin company page</h1>

{#if isLoading}
	<p>Loading company...</p>
{:else if errorMessage}
	<p>{errorMessage}</p>
{:else if company}
	<p>Company ID: {company.id}</p>
	<p>Selected company: {company.name}</p>
{/if}
