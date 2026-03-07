<script lang="ts">
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import type { CompanyListDto } from '$lib/types/company';
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';

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
				errorMessage =
					response.status === 401
						? 'Unauthorized. Please sign in again.'
						: 'Failed to load companies.';
				return;
			}

			companies = (await response.json()) as CompanyListDto[];
		} catch {
			errorMessage = 'Failed to load companies.';
		} finally {
			isLoading = false;
		}
	});

	function openCompany(companyId: string) {
		goto(resolve('/employee/[CompanyId]', { CompanyId: companyId }));
	}
</script>

<section class="employee-card hero">
	<p class="kicker">Company selection</p>
	<h1>Choose your company</h1>
	<p>Select a company to open your employee workspace.</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Loading companies…</div>
{:else if errorMessage}
	<div class="employee-state-block is-error">{errorMessage}</div>
{:else if companies.length === 0}
	<div class="employee-state-block is-empty">No companies found.</div>
{:else}
	<div class="company-grid" role="list" aria-label="Available companies">
		{#each companies as company (company.id)}
			<button class="company-card" type="button" onclick={() => openCompany(company.id)}>
				<span class="company-name">{company.name}</span>
				<span class="company-meta">Reg. no: {company.registrationNumber}</span>
				<span class="company-action">Open company</span>
			</button>
		{/each}
	</div>
{/if}

<style>
	.hero {
		margin-bottom: 0.9rem;
	}

	.kicker {
		margin: 0;
		font-size: 0.78rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	h1 {
		margin: 0.3rem 0 0.4rem;
		font-size: 1.25rem;
		line-height: 1.2;
		color: #17251e;
	}

	p {
		margin: 0;
		color: #40574a;
	}

	.company-grid {
		display: grid;
		grid-template-columns: 1fr;
		gap: 0.7rem;
	}

	.company-card {
		display: flex;
		flex-direction: column;
		align-items: flex-start;
		gap: 0.35rem;
		width: 100%;
		padding: 0.9rem;
		border: 1px solid #d5e2db;
		border-radius: 0.85rem;
		background: #fff;
		text-align: left;
		cursor: pointer;
		transition:
			border-color 0.18s ease,
			box-shadow 0.18s ease,
			transform 0.18s ease;
	}

	.company-card:hover {
		border-color: #9eb8ab;
		box-shadow: 0 6px 16px rgba(25, 53, 40, 0.12);
		transform: translateY(-1px);
	}

	.company-card:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.25);
	}

	.company-name {
		font-size: 1rem;
		font-weight: 700;
		color: #173f2f;
	}

	.company-meta {
		font-size: 0.88rem;
		color: #4a6356;
	}

	.company-action {
		margin-top: 0.2rem;
		font-size: 0.88rem;
		font-weight: 700;
		color: #1f5a42;
	}

	@media (min-width: 640px) {
		.company-grid {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}
	}
</style>
