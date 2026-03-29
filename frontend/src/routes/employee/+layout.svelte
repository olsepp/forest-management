<script lang="ts">
	import { page } from '$app/stores';
	import { browser } from '$app/environment';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import { onMount } from 'svelte';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import { getDefaultRouteForRole } from '$lib/services/auth';

	type AppRoleRoute = '/' | '/admin' | '/employee' | '/sign-in';

	let { children } = $props();
	let pathname = $derived($page.url.pathname);
	let employeeDisplayName = $state('Metsandus');
	let currentUserId = $derived($user?.userId?.trim() ?? '');
	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	type UserProfileDto = {
		firstName?: string;
		lastName?: string;
	};

	let currentCompanyId = $derived.by(() => {
		const fromParams = $page.params.CompanyId?.trim();
		if (fromParams) return fromParams;

		const matched = pathname.match(/^\/employee\/([^/]+)(?:\/|$)/i);
		if (!matched?.[1]) return null;

		const candidate = matched[1].trim();
		return candidate.toLowerCase() === 'user' ? null : candidate;
	});

	let hasProfileId = $derived(currentUserId.length > 0);
	let navCompanyId = $derived(currentCompanyId ?? '');

	$effect(() => {
		if (!browser) return;

		const role = $user?.role?.trim().toLowerCase();
		if (role && role !== 'employee') {
			goto(resolve(getDefaultRouteForRole(role) as AppRoleRoute));
		}
	});

	async function handleSignOut() {
		await authService.logout();
		goto(resolve('/sign-in'));
	}

	function normalizeNamePart(value: unknown): string {
		if (typeof value !== 'string') return '';
		return value.trim();
	}

	async function loadEmployeeDisplayName() {
		if (!browser) return;

		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/users/profile`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!response.ok) return;

			const profile = (await response.json()) as UserProfileDto;
			const fullName = [normalizeNamePart(profile.firstName), normalizeNamePart(profile.lastName)]
				.filter(Boolean)
				.join(' ');

			if (fullName) {
				employeeDisplayName = fullName;
			}
		} catch {
			// Keep fallback title when profile endpoint is unavailable.
		}
	}

	onMount(() => {
		loadEmployeeDisplayName();
	});
</script>

<div class="employee-layout mx-auto min-h-screen w-full max-w-6xl px-3 pt-0 sm:px-4 sm:pt-0 md:px-5">
	<header class="employee-appbar">
		<div>
			<p class="employee-kicker">Töötajate tööruum</p>
			<h1 class="employee-title">{employeeDisplayName}</h1>
		</div>

		<button type="button" onclick={handleSignOut} class="employee-signout">Logi välja</button>
	</header>

	{#if navCompanyId}
		<nav class="employee-top-nav" aria-label="Employee section navigation">
			<a href={resolve('/employee')} class="employee-nav-link" class:is-active={pathname === '/employee'}>
				Avaleht
			</a>
			<a
				href={resolve('/employee/[CompanyId]/landproperty', { CompanyId: navCompanyId })}
				class="employee-nav-link"
				class:is-active={pathname.includes(`/employee/${navCompanyId}/landproperty`)}
			>
				Kinnistud
			</a>
			<a
				href={resolve('/employee/[CompanyId]/activity', { CompanyId: navCompanyId })}
				class="employee-nav-link"
				class:is-active={pathname.includes(`/employee/${navCompanyId}/activity`)}
			>
				Tegevused
			</a>

			{#if hasProfileId}
				<a
					href={resolve('/employee/user/[userId]', { userId: currentUserId })}
					class="employee-nav-link"
					class:is-active={pathname.startsWith('/employee/user/')}
				>
					Profiil
				</a>
			{:else}
				<a href={resolve('/employee')} class="employee-nav-link" class:is-active={pathname === '/employee'}>
					Profiil
				</a>
			{/if}
		</nav>
	{/if}

	<div class="employee-content">
		{@render children()}
	</div>

	{#if navCompanyId}
		<nav class="employee-bottom-nav" aria-label="Employee mobile navigation">
			<a href={resolve('/employee')} class="employee-tab-link" class:is-active={pathname === '/employee'}>
				Avaleht
			</a>
			<a
				href={resolve('/employee/[CompanyId]/landproperty', { CompanyId: navCompanyId })}
				class="employee-tab-link"
				class:is-active={pathname.includes(`/employee/${navCompanyId}/landproperty`)}
			>
				Kinnistud
			</a>
			<a
				href={resolve('/employee/[CompanyId]/activity', { CompanyId: navCompanyId })}
				class="employee-tab-link"
				class:is-active={pathname.includes(`/employee/${navCompanyId}/activity`)}
			>
				Tegevused
			</a>
			{#if hasProfileId}
				<a
					href={resolve('/employee/user/[userId]', { userId: currentUserId })}
					class="employee-tab-link"
					class:is-active={pathname.startsWith('/employee/user/')}
				>
					Profiil
				</a>
			{:else}
				<a href={resolve('/employee')} class="employee-tab-link" class:is-active={pathname === '/employee'}>
					Profiil
				</a>
			{/if}
		</nav>
	{/if}
</div>

<style>
	:global(:root) {
		--employee-shell-bg: #eef4f0;
		--employee-surface: #ffffff;
		--employee-accent: #1f5a42;
		--employee-border: #d7e2dc;
		--employee-radius-md: 0.9rem;
		--employee-radius-lg: 1rem;
		--employee-shadow-1: 0 6px 20px rgba(20, 41, 31, 0.08);
		--employee-shadow-2: 0 10px 26px rgba(20, 41, 31, 0.12);
		--employee-bottom-nav-height: 4.4rem;
		--employee-safe-bottom: max(0.55rem, env(safe-area-inset-bottom));
		--employee-shell-pad-x: 0.75rem;
	}

	.employee-layout {
		color: #1f2a24;
		padding-bottom: calc(var(--employee-bottom-nav-height) + var(--employee-safe-bottom) + 0.9rem);
	}

	.employee-appbar {
		position: sticky;
		top: 0;
		z-index: 1100;
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.75rem;
		margin-bottom: 0.75rem;
		padding: 0.78rem 0.82rem;
		background: linear-gradient(180deg, #265f48 0%, #1f5a42 100%);
		box-shadow: var(--employee-shadow-2);
		width: calc(100% + (var(--employee-shell-pad-x) * 2));
		margin-left: calc(var(--employee-shell-pad-x) * -1);
		margin-right: calc(var(--employee-shell-pad-x) * -1);
	}

	.employee-kicker {
		margin: 0;
		font-size: 0.7rem;
		font-weight: 700;
		letter-spacing: 0.03em;
		text-transform: uppercase;
		color: #d8e9df;
	}

	.employee-title {
		margin: 0.16rem 0 0;
		font-size: 1.02rem;
		line-height: 1.25;
		font-weight: 700;
		color: #eef6f1;
	}

	.employee-signout {
		border: 1px solid #c0d7cb;
		background: #f8fbf9;
		color: #1f352a;
		border-radius: var(--employee-radius-md);
		min-height: 2.75rem;
		padding: 0.68rem 0.92rem;
		font-size: 0.86rem;
		font-weight: 600;
		line-height: 1;
		text-align: center;
		cursor: pointer;
		transition:
			background-color 0.18s ease,
			border-color 0.18s ease,
			transform 0.12s ease,
			box-shadow 0.18s ease;
	}

	.employee-signout:hover {
		background: #f2f6f3;
		border-color: #9fbcad;
	}

	.employee-signout:active {
		transform: translateY(1px);
	}

	.employee-signout:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(41, 94, 69, 0.24);
	}

	.employee-top-nav {
		display: none;
	}

	.employee-nav-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		text-align: center;
		text-decoration: none;
		min-height: 2.8rem;
		padding: 0.62rem 0.7rem;
		border-radius: var(--employee-radius-md);
		border: 1px solid var(--employee-border);
		background: var(--employee-surface);
		font-size: 0.9rem;
		font-weight: 600;
		color: #1f4f39;
		transition:
			background-color 0.18s ease,
			border-color 0.18s ease,
			color 0.18s ease;
	}

	.employee-nav-link:hover {
		background: #f2f7f4;
		border-color: #b8cbc1;
	}

	.employee-nav-link.is-active {
		background: var(--employee-accent);
		border-color: var(--employee-accent);
		color: #f6fbf8;
	}

	.employee-content {
		border: 1px solid var(--employee-border);
		background: var(--employee-shell-bg);
		border-radius: var(--employee-radius-lg);
		padding: 0.82rem;
		box-shadow: var(--employee-shadow-1);
	}

	.employee-bottom-nav {
		position: fixed;
		left: 0;
		right: 0;
		bottom: 0;
		z-index: 1200;
		display: flex;
		gap: 0.4rem;
		height: calc(var(--employee-bottom-nav-height) + var(--employee-safe-bottom));
		padding: 0.45rem 0.6rem calc(0.45rem + var(--employee-safe-bottom));
		background: rgba(255, 255, 255, 0.96);
		border-top: 1px solid #d4e1db;
		box-shadow: 0 -8px 24px rgba(20, 40, 31, 0.15);
		backdrop-filter: blur(8px);
	}

	.employee-tab-link {
		flex: 1;
		display: inline-grid;
		align-items: center;
		justify-content: center;
		text-align: center;
		text-decoration: none;
		min-height: 2.75rem;
		padding: 0.55rem 0.35rem;
		border-radius: var(--employee-radius-md);
		border: 1px solid transparent;
		background: transparent;
		font-size: 0.8rem;
		font-weight: 600;
		color: #466055;
		transition:
			background-color 0.18s ease,
			border-color 0.18s ease,
			color 0.18s ease,
			transform 0.12s ease;
	}

	.employee-tab-link:active {
		transform: translateY(1px);
	}

	.employee-tab-link.is-active {
		background: #eaf3ee;
		border-color: #c4d7ce;
		color: #194934;
	}

	.employee-tab-link:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.24);
	}

	:global(.employee-stack-cards) {
		display: grid;
		gap: 0.75rem;
	}

	:global(.employee-card) {
		border: 1px solid var(--employee-border);
		border-radius: var(--employee-radius-lg);
		background: var(--employee-surface);
		padding: 0.9rem;
		margin-bottom: 1rem;
		box-shadow: 0 4px 14px rgba(18, 39, 29, 0.05);
	}

	:global(.employee-table-wrap) {
		overflow-x: auto;
		max-width: 100%;
		border: 1px solid var(--employee-border);
		border-radius: var(--employee-radius-md);
		background: #fff;
		-webkit-overflow-scrolling: touch;
	}

	:global(.employee-table-wrap table) {
		min-width: 44rem;
		width: 100%;
		border-collapse: collapse;
	}

	:global(.employee-table-wrap th),
	:global(.employee-table-wrap td) {
		padding: 0.72rem 0.8rem;
		border-bottom: 1px solid #e4ece8;
		text-align: left;
		vertical-align: top;
		white-space: nowrap;
	}

	:global(.employee-state-block) {
		border-radius: var(--employee-radius-md);
		border: 1px solid #d9e5df;
		background: #f7faf8;
		padding: 0.85rem;
		font-size: 0.95rem;
		color: #2d3e35;
	}

	:global(.employee-state-block.is-error) {
		border-color: #f0c2c2;
		background: #fff5f5;
		color: #8e2525;
	}

	:global(.employee-state-block.is-empty) {
		border-color: #d6e2db;
		background: #f8fbf9;
		color: #3d5347;
	}

	:global(.employee-state-block.is-loading) {
		border-color: #bfd3c8;
		background: #f1f7f4;
		color: #24543e;
	}

	:global(.employee-content :is(button, a, input, select, textarea)) {
		min-height: 2.75rem;
	}

	:global(.employee-content :is(button, a):focus-visible),
	:global(.employee-content :is(input, select, textarea):focus-visible) {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.22);
	}

	:global(.employee-content :is(button, a):active) {
		transform: translateY(1px);
	}

	@media (min-width: 640px) {
		:global(:root) {
			--employee-shell-pad-x: 1rem;
		}

		.employee-layout {
			padding-bottom: 2rem;
		}

		.employee-appbar {
			position: static;
			padding: 0.9rem 1rem;
		}

		.employee-title {
			font-size: 1.18rem;
		}

		.employee-top-nav {
			display: flex;
			align-items: center;
			gap: 0.55rem;
			margin-bottom: 0.85rem;
		}

		.employee-bottom-nav {
			display: none;
		}

		.employee-content {
			padding: 1rem;
		}

		:global(.employee-stack-cards) {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}
	}

	@media (min-width: 768px) {
		:global(:root) {
			--employee-shell-pad-x: 1.25rem;
		}

		.employee-content {
			padding: 1.1rem 1.2rem;
		}

		:global(.employee-card) {
			padding: 1rem;
		}
	}
</style>
