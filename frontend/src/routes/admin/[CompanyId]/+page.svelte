<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
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
				errorMessage = 'Puudub ettevõtte ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/companies/${companyId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 401 ? 'Ligipääs puudub. Logige uuesti sisse.' : 'Ettevõtte laadimine ebaõnnestus.';
				return;
			}

			company = await response.json();
		} catch {
			errorMessage = 'Ettevõtte laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	});

	function openSection(path: string) {
		goto(resolve(path as unknown as '/'));
	}

	const companyActions = $derived.by(() => {
		if (!company) return [] as { label: string; path: string }[];

		return [
			{ label: 'Ava töölaud', path: `/admin/${company.id}/dashboard` },
			{ label: 'Ava kinnistud', path: `/admin/${company.id}/landproperty` },
			{ label: 'Ava tegevused', path: `/admin/${company.id}/activity` }
		];
	});
</script>

<h1>Ettevõtte tööruum</h1>

{#if isLoading}
	<p>Laetakse ettevõtet...</p>
{:else if errorMessage}
	<p class="error">{errorMessage}</p>
{:else if company}
	<section class="card">
		<p class="meta">Valitud ettevõte</p>
		<h2>{company.name}</h2>
		<p><strong>Ettevõtte ID:</strong> {company.id}</p>

		<div class="actions">
			{#each companyActions as action (action.path)}
				<button type="button" onclick={() => openSection(action.path)}>{action.label}</button>
			{/each}
		</div>
	</section>
{/if}

<style>
	.card {
		padding: 1.1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.75rem;
		background: #fff;
	}

	.meta {
		margin: 0;
		font-size: 0.78rem;
		text-transform: uppercase;
		letter-spacing: 0.08em;
		font-weight: 700;
	}

	h2 {
		margin: 0.35rem 0 0.85rem;
		font-size: 1.2rem;
	}

	.actions {
		margin-top: 1rem;
		display: flex;
		flex-wrap: wrap;
		gap: 0.6rem;
	}

	button {
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
		background: #fff;
		padding: 0.5rem 0.9rem;
		font: inherit;
		cursor: pointer;
	}

	.error {
		color: #b91c1c;
	}
</style>
