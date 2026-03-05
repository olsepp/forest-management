<script lang="ts">
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { goto } from '$app/navigation';
	import type { CompanyListDto } from '$lib/types/company';
	import { onMount } from 'svelte';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let companies = $state<CompanyListDto[]>([]);
	let isLoading = $state(true);
	let errorMessage = $state('');

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/companies`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage = response.status === 401 ? 'Unauthorized. Please sign in again.' : 'Failed to load companies';
				return;
			}

			companies = await response.json();
		} catch {
			errorMessage = 'Failed to load companies';
		} finally {
			isLoading = false;
		}
	});

	function openCompany(companyId: string) {
		goto(`/admin/${companyId}`);
	}
</script>

<h1>Admin company selection</h1>

{#if isLoading}
	<p>Loading companies...</p>
{:else if errorMessage}
	<p>{errorMessage}</p>
{:else if companies.length === 0}
	<p>No companies found.</p>
{:else}
	<p class="intro-text">Choose a company to continue:</p>
	<div class="company-grid" role="list">
		{#each companies as company}
			<button class="company-card" type="button" onclick={() => openCompany(company.id)}>
				<span class="company-name">{company.name}</span>
				<span class="company-action">Open company</span>
			</button>
		{/each}
	</div>
{/if}

<style>
	.intro-text {
		margin: 0.5rem 0 1rem;
		color: #4b5563;
	}

	.company-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 1rem;
	}

	.company-card {
		display: flex;
		flex-direction: column;
		align-items: flex-start;
		justify-content: space-between;
		min-height: 130px;
		width: 100%;
		padding: 1.2rem;
		border: 1px solid #d1d5db;
		border-radius: 0.75rem;
		background: #ffffff;
		box-shadow: 0 1px 2px rgba(0, 0, 0, 0.06);
		cursor: pointer;
		text-align: left;
		transition:
			transform 0.16s ease,
			box-shadow 0.16s ease,
			border-color 0.16s ease;
	}

	.company-card:hover,
	.company-card:focus-visible {
		transform: translateY(-2px);
		border-color: #9ca3af;
		box-shadow: 0 8px 16px rgba(0, 0, 0, 0.08);
		outline: none;
	}

	.company-name {
		font-size: 1.05rem;
		font-weight: 600;
		color: #111827;
	}

	.company-action {
		margin-top: 0.75rem;
		font-size: 0.9rem;
		color: #2563eb;
	}
</style>
