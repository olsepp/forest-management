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
						? 'Lubatud pääs puudub. Logige uuesti sisse.'
						: 'Ettevõtteid ei õnnestunud laadida.';
				return;
			}

			companies = (await response.json()) as CompanyListDto[];
		} catch {
			errorMessage = 'Ettevõtteid ei õnnestunud laadida.';
		} finally {
			isLoading = false;
		}
	});

	function openCompany(companyId: string) {
		goto(resolve('/employee/[CompanyId]', { CompanyId: companyId }));
	}
</script>

<section class="employee-card hero">
	<p class="kicker">Ettevõtte valik</p>
	<h1>Valige oma ettevõte</h1>
	<p>Valige ettevõte, et avada oma töötajate tööruum.</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse ettevõtteid…</div>
{:else if errorMessage}
	<div class="employee-state-block is-error">{errorMessage}</div>
{:else if companies.length === 0}
	<div class="employee-state-block is-empty">Ettevõtteid ei leitud.</div>
{:else}
	<div class="company-grid" role="list" aria-label="Saadaval olevad ettevõtted">
		{#each companies as company (company.id)}
			<button class="company-card" type="button" onclick={() => openCompany(company.id)}>
				<span class="company-name">{company.name}</span>
				<span class="company-meta">Reg. nr: {company.registrationNumber}</span>
				<span class="company-action">Avage ettevõte</span>
			</button>
		{/each}
	</div>
{/if}

<style>
	.hero {
		margin-bottom: 0.9rem;
		padding: 1rem;
		background: linear-gradient(180deg, #ffffff 0%, #f3f8f5 100%);
		border-color: #d2e1d8;
	}

	.kicker {
		margin: 0;
		font-size: 0.72rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	h1 {
		margin: 0.3rem 0 0.4rem;
		font-size: 1.15rem;
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
		gap: 0.75rem;
	}

	.company-card {
		display: flex;
		flex-direction: column;
		align-items: flex-start;
		gap: 0.4rem;
		width: 100%;
		min-height: 2.75rem;
		padding: 0.92rem;
		border: 1px solid #d2e0d8;
		border-radius: 1rem;
		background: #fff;
		text-align: left;
		cursor: pointer;
		transition:
			border-color 0.18s ease,
			box-shadow 0.18s ease,
			transform 0.18s ease;
	}

	.company-card:hover {
		border-color: #99b8a9;
		box-shadow: 0 6px 16px rgba(25, 53, 40, 0.12);
		transform: translateY(-1px);
	}

	.company-card:active {
		transform: translateY(1px);
	}

	.company-card:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.25);
	}

	.company-name {
		font-size: 1rem;
		font-weight: 700;
		color: #173b2d;
	}

	.company-meta {
		font-size: 0.88rem;
		color: #4a6356;
	}

	.company-action {
		margin-top: 0.28rem;
		font-size: 0.84rem;
		font-weight: 700;
		color: #1f5a42;
	}

	@media (min-width: 640px) {
		h1 {
			font-size: 1.25rem;
		}

		.company-grid {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}
	}
</style>
