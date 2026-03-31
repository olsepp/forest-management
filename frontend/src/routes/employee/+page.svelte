<script lang="ts">
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import type { CompanyListDto } from '$lib/types/company';
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let companies = $state<CompanyListDto[]>([]);
	let isLoading = $state(true);
	let errorMessage = $state('');
	let currentUserId = $derived($user?.userId?.trim() ?? '');

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
	{#if currentUserId}
		<a class="profile-shortcut" href={resolve('/employee/user/[userId]', { userId: currentUserId })}>
			Minu profiil
		</a>
	{/if}
	<p class="kicker">Ettevõtte valik</p>
	<h1 class="employee-page-title">Vali ettevõte</h1>
	<!-- <p>Valige ettevõte, et avada oma töötajate tööruum.</p> -->
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
			</button>
		{/each}
	</div>
{/if}

<style>
	.hero {
		margin-bottom: 0.9rem;
		padding: 1rem;
		background: linear-gradient(180deg, #ffffff 0%, #f5f8fc 100%);
		border-color: #d3dde8;
	}

	.kicker {
		margin: 0;
		font-size: 0.72rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	.profile-shortcut {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		align-self: flex-start;
		min-height: 3rem;
		padding: 0.65rem 0.95rem;
		margin-bottom: 0.7rem;
		border-radius: 0.85rem;
		border: 1px solid #97b6a6;
		background: #eaf5ef;
		color: #123d2e;
		font-size: 0.95rem;
		font-weight: 700;
		text-decoration: none;
		box-shadow: 0 2px 8px rgba(15, 37, 28, 0.08);
	}

	.profile-shortcut:hover {
		background: #ddede4;
		border-color: #7ca48f;
	}

	.profile-shortcut:active {
		transform: translateY(1px);
	}

	.profile-shortcut:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.25);
	}

	p {
		margin: 0;
		color: #334155;
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
		border: 1px solid #cfd8e3;
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
		border-color: #aebed0;
		box-shadow: 0 6px 16px rgba(15, 23, 42, 0.12);
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
		color: #0f172a;
	}

	.company-meta {
		font-size: 0.88rem;
		color: #475569;
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
