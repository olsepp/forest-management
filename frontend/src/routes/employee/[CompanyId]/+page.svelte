<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import type { CompanyDto } from '$lib/types/company';
	import { onMount } from 'svelte';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	type QuickAction = {
		label: string;
		description: string;
		kind: 'properties' | 'activities' | 'profile';
	};

	let company = $state<CompanyDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');
	let isUnauthorized = $state(false);

	let companyId = $derived($page.params.CompanyId ?? '');
	let currentUserId = $derived($user?.userId ?? '');

	let quickActions = $derived.by(() => {
		if (!companyId) return [] as QuickAction[];

		return [
			{
				label: 'Kinnistud',
				description: 'Sirvi aktiivseid kinnistuid ja ava katasteri detailid.',
				kind: 'properties'
			},
			{
				label: 'Tegevuste ajalugu',
				description: 'Vaata selle ettevõtte viimaseid töölogisid.',
				kind: 'activities'
			},
			{
				label: 'Profiil',
				description: 'Vaata oma konto ja kontaktandmeid.',
				kind: 'profile'
			}
		];
	});

	onMount(async () => {
		if (!companyId) {
			errorMessage = 'Puudub ettevõtte ID.';
			isLoading = false;
			return;
		}

		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/companies/${companyId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				if (response.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage = 'Ettevõtte andmeid ei õnnestunud laadida.';
				return;
			}

			company = (await response.json()) as CompanyDto;
		} catch {
			errorMessage = 'Ettevõtte andmeid ei õnnestunud laadida.';
		} finally {
			isLoading = false;
		}
	});
</script>

<section class="intro employee-card">
	<p class="kicker">Ettevõtte tööruum</p>
	<h1>{company?.name ?? 'Töötaja töölaud'}</h1>
	<p>
		{#if company}
			Valige valdkond, kus jätkata igapäevast tööd ettevõttes <strong>{company.name}</strong>.
		{:else}
			Valige valdkond, kus jätkata igapäevast tööd.
		{/if}
	</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse ettevõtte töölauda…</div>
{:else if errorMessage}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Teie sessioon võib olla aegunud.</span>
		{/if}
	</div>
{:else if quickActions.length === 0}
	<div class="employee-state-block is-empty">Toimingud puuduvad.</div>
	{:else}
	<section class="employee-stack-cards" aria-label="Töötaja kiirtoimingud">
		{#each quickActions as action (action.kind)}
			{#if action.kind === 'properties'}
				<a class="action-card" href={resolve('/employee/[CompanyId]/landproperty', { CompanyId: companyId })}>
					<h2>{action.label}</h2>
					<p>{action.description}</p>
					<span class="action-link">Ava</span>
				</a>
			{:else if action.kind === 'activities'}
				<a class="action-card" href={resolve('/employee/[CompanyId]/activity', { CompanyId: companyId })}>
					<h2>{action.label}</h2>
					<p>{action.description}</p>
					<span class="action-link">Ava</span>
				</a>
			{:else if currentUserId}
				<a class="action-card" href={resolve('/employee/user/[userId]', { userId: currentUserId })}>
					<h2>{action.label}</h2>
					<p>{action.description}</p>
					<span class="action-link">Ava</span>
				</a>
			{:else}
				<a class="action-card" href={resolve('/employee')}>
					<h2>{action.label}</h2>
					<p>{action.description}</p>
					<span class="action-link">Ava</span>
				</a>
			{/if}
		{/each}
	</section>
{/if}

<style>
	.intro {
		margin-bottom: 0.85rem;
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
		margin: 0.32rem 0 0.4rem;
		font-size: 1.16rem;
		line-height: 1.2;
		color: #17251e;
	}

	p {
		margin: 0;
		color: #40574a;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}

	.action-card {
		display: flex;
		flex-direction: column;
		gap: 0.45rem;
		min-height: 2.75rem;
		border: 1px solid #d2e0d8;
		border-radius: 1rem;
		padding: 0.92rem;
		background: #fff;
		text-decoration: none;
		transition:
			border-color 0.18s ease,
			box-shadow 0.18s ease,
			transform 0.18s ease;
	}

	.action-card:hover {
		border-color: #99b8a9;
		box-shadow: 0 6px 16px rgba(25, 53, 40, 0.12);
		transform: translateY(-1px);
	}

	.action-card:active {
		transform: translateY(1px);
	}

	.action-card:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.25);
	}

	h2 {
		margin: 0;
		font-size: 1.02rem;
		color: #173f2f;
	}

	.action-link {
		margin-top: 0.22rem;
		font-size: 0.84rem;
		font-weight: 700;
		color: #1f5a42;
	}

	@media (min-width: 640px) {
		h1 {
			font-size: 1.28rem;
		}
	}

	@media (min-width: 768px) {
		h1 {
			font-size: 1.45rem;
		}

		.action-card {
			padding: 1rem;
		}
	}
</style>
