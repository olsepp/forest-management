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
		path: string;
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
				label: 'Properties',
				description: 'Browse active properties and open cadastral details.',
				path: `/employee/${companyId}/landproperty`
			},
			{
				label: 'Activity history',
				description: 'Review your latest work logs for this company.',
				path: `/employee/${companyId}/activity`
			},
			{
				label: 'Profile',
				description: 'View your account details and contact data.',
				path: currentUserId ? `/employee/user/${currentUserId}` : '/employee'
			}
		];
	});

	onMount(async () => {
		if (!companyId) {
			errorMessage = 'Missing company id.';
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
					errorMessage = 'Unauthorized. Please sign in again.';
					return;
				}

				errorMessage = 'Unable to load company details.';
				return;
			}

			company = (await response.json()) as CompanyDto;
		} catch {
			errorMessage = 'Unable to load company details.';
		} finally {
			isLoading = false;
		}
	});
</script>

<section class="intro employee-card">
	<p class="kicker">Company workspace</p>
	<h1>{company?.name ?? 'Employee dashboard'}</h1>
	<p>
		{#if company}
			Choose an area to continue your daily work in <strong>{company.name}</strong>.
		{:else}
			Choose an area to continue your daily work.
		{/if}
	</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Loading company dashboard…</div>
{:else if errorMessage}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Your session may have expired.</span>
		{/if}
	</div>
{:else if quickActions.length === 0}
	<div class="employee-state-block is-empty">No actions available yet.</div>
{:else}
	<section class="employee-stack-cards" aria-label="Employee quick actions">
		{#each quickActions as action (action.path)}
			<a class="action-card" href={resolve(action.path)}>
				<h2>{action.label}</h2>
				<p>{action.description}</p>
				<span class="action-link">Open</span>
			</a>
		{/each}
	</section>
{/if}

<style>
	.intro {
		margin-bottom: 0.85rem;
	}

	.kicker {
		margin: 0;
		font-size: 0.77rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	h1 {
		margin: 0.32rem 0 0.4rem;
		font-size: 1.28rem;
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
		border: 1px solid #d5e2db;
		border-radius: 0.85rem;
		padding: 0.9rem;
		background: #fff;
		text-decoration: none;
		transition:
			border-color 0.18s ease,
			box-shadow 0.18s ease,
			transform 0.18s ease;
	}

	.action-card:hover {
		border-color: #9eb8ab;
		box-shadow: 0 6px 16px rgba(25, 53, 40, 0.12);
		transform: translateY(-1px);
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
		font-size: 0.88rem;
		font-weight: 700;
		color: #1f5a42;
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
