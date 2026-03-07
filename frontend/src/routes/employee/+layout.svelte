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

	let { children } = $props();
	let isMenuOpen = $state(false);
	let pathname = $derived($page.url.pathname);
	let employeeDisplayName = $state('Forest management');
	let currentUserId = $derived($user?.userId?.trim() ?? '');
	let profilePath = $derived(currentUserId ? `/employee/user/${currentUserId}` : '/employee');
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
	let navItems = $derived.by(() => {
		const items: { path: string; label: string; activeWhen: (path: string) => boolean }[] = [];

		if (currentCompanyId) {
			items.push(
				{
					path: '/employee',
					label: 'Home',
					activeWhen: (path) => path === '/employee'
				},	
				{
					path: `/employee/${currentCompanyId}/landproperty`,
					label: 'Properties',
					activeWhen: (path) => path.includes(`/employee/${currentCompanyId}/landproperty`)
				},
				{
					path: `/employee/${currentCompanyId}/activity`,
					label: 'Activities',
					activeWhen: (path) => path.includes(`/employee/${currentCompanyId}/activity`)
				}
			);
		} else {
			items.push({
				path: '/employee',
				label: 'Home',
				activeWhen: (path) => path === '/employee'
			});
		}

		return items;
	});

	$effect(() => {
		if (!browser) return;

		const role = $user?.role?.trim().toLowerCase();
		if (role && role !== 'employee') {
			goto(resolve(getDefaultRouteForRole(role)));
		}
	});

	async function handleSignOut() {
		await authService.logout();
		goto(resolve('/sign-in'));
	}

	function toggleMenu() {
		isMenuOpen = !isMenuOpen;
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

	$effect(() => {
		if (pathname) {
			isMenuOpen = false;
			return;
		}

		isMenuOpen = false;
	});
</script>


<div class="employee-layout mx-auto min-h-screen w-full max-w-6xl px-3 pb-6 pt-3 sm:px-4 sm:pt-4 md:px-5 md:pb-8">
	<header class="employee-header mb-3 sm:mb-4">
		<div class="employee-title-wrap">
			<button
				type="button"
				onclick={toggleMenu}
				class="employee-menu-toggle"
				aria-expanded={isMenuOpen}
				aria-controls="employee-main-nav"
				aria-label="Open navigation menu"
			>
				<span aria-hidden="true" class="hamburger-icon">☰</span>
			</button>

			<div>
			<p class="employee-kicker">Employee workspace</p>
			<h1 class="employee-title">{employeeDisplayName}</h1>
			</div>
		</div>
	</header>

	{#if navItems.length > 0}
		{#if isMenuOpen}
			<button
				type="button"
				class="employee-menu-backdrop"
				onclick={toggleMenu}
				aria-label="Close navigation menu"
			></button>
		{/if}

		<nav
			id="employee-main-nav"
			class="employee-nav mb-4"
			class:is-open={isMenuOpen}
			aria-label="Employee section navigation"
		>
			{#each navItems as item (item.path)}
				<a
					href={resolve(item.path)}
					class="employee-nav-link"
					class:is-active={item.activeWhen(pathname)}
					onclick={() => (isMenuOpen = false)}
				>
					{item.label}
				</a>
			{/each}

			<a
				href={resolve(profilePath)}
				class="employee-nav-link employee-profile-link"
				class:is-active={pathname.startsWith('/employee/user/')}
				onclick={() => (isMenuOpen = false)}
			>
				Profile
			</a>

			<button type="button" onclick={handleSignOut} class="employee-signout">Sign out</button>
		</nav>
	{/if}

	<div class="employee-content">
		{@render children()}
	</div>
</div>

<style>
	.employee-layout {
		color: #1f2a24;
	}

	.employee-title-wrap {
		display: flex;
		align-items: center;
		gap: 0.55rem;
	}

	.employee-header {
		display: flex;
		align-items: flex-start;
		justify-content: space-between;
		gap: 0.8rem;
		border: 1px solid #dbe4df;
		border-radius: 0.9rem;
		padding: 0.85rem 0.85rem 0.95rem;
		background: linear-gradient(180deg, #f9fcfa 0%, #f2f7f4 100%);
	}

	.employee-kicker {
		margin: 0;
		font-size: 0.74rem;
		font-weight: 700;
		letter-spacing: 0.03em;
		text-transform: uppercase;
		color: #3d5447;
	}

	.employee-title {
		margin: 0.2rem 0 0;
		font-size: 1.1rem;
		line-height: 1.25;
		font-weight: 700;
		color: #17231d;
	}

	.employee-signout {
		border: 1px solid #bfcec5;
		background: #f7faf8;
		color: #24332b;
		border-radius: 0.75rem;
		min-height: 2.75rem;
		padding: 0.65rem 0.95rem;
		font-size: 0.9rem;
		font-weight: 600;
		line-height: 1;
		text-align: center;
		cursor: pointer;
		transition:
			background-color 0.18s ease,
			border-color 0.18s ease,
			box-shadow 0.18s ease;
	}

	.employee-menu-toggle {
		border: 1px solid #bfcec5;
		background: #f9fcfa;
		color: #24332b;
		border-radius: 0.75rem;
		min-height: 2.75rem;
		min-width: 2.75rem;
		padding: 0.65rem;
		font-size: 0.9rem;
		font-weight: 600;
		line-height: 1;
	}

	.hamburger-icon {
		font-size: 1.12rem;
		line-height: 1;
	}

	.employee-signout:hover {
		background: #f2f6f3;
		border-color: #a7beb1;
	}

	.employee-signout:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(41, 94, 69, 0.24);
	}

	.employee-nav {
		position: fixed;
		top: 0;
		left: 0;
		z-index: 30;
		display: flex;
		flex-direction: column;
		align-content: start;
		gap: 0.55rem;
		width: min(84vw, 18rem);
		height: 100dvh;
		padding: 1rem 0.8rem 1.2rem;
		background: #ffffff;
		border-right: 1px solid #d2ded8;
		box-shadow: 0 16px 32px rgba(17, 37, 28, 0.18);
		transform: translateX(-105%);
		transition: transform 0.22s ease;
	}

	.employee-nav.is-open {
		transform: translateX(0);
	}

	.employee-menu-backdrop {
		position: fixed;
		inset: 0;
		z-index: 20;
		background: rgba(19, 34, 27, 0.34);
		border: 0;
		padding: 0;
	}

	.employee-nav-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		text-align: center;
		text-decoration: none;
		min-height: 2.8rem;
		padding: 0.62rem 0.7rem;
		border-radius: 0.75rem;
		border: 1px solid #d2ded8;
		background: #ffffff;
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
		background: #1f5a42;
		border-color: #1f5a42;
		color: #f6fbf8;
	}

	.employee-profile-link {
		margin-top: auto;
	}

	.employee-content {
		border: 1px solid #dce6e0;
		background: #ffffff;
		border-radius: 0.95rem;
		padding: 0.85rem;
		box-shadow: 0 8px 24px rgba(20, 41, 31, 0.06);
	}

	:global(.employee-stack-cards) {
		display: grid;
		gap: 0.7rem;
	}

	:global(.employee-card) {
		border: 1px solid #d9e4de;
		border-radius: 0.85rem;
		background: #fff;
		padding: 0.85rem;
		box-shadow: 0 3px 10px rgba(18, 39, 29, 0.04);
	}

	:global(.employee-table-wrap) {
		overflow-x: auto;
		max-width: 100%;
		border: 1px solid #d9e3de;
		border-radius: 0.8rem;
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
		border-radius: 0.8rem;
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

	@media (min-width: 640px) {
		.employee-menu-toggle {
			display: none;
		}

		.employee-header {
			align-items: center;
			padding: 1rem 1.05rem;
		}

		.employee-title {
			font-size: 1.25rem;
		}

		.employee-nav {
			position: static;
			transform: none;
			width: auto;
			height: auto;
			padding: 0;
			background: transparent;
			border-right: 0;
			box-shadow: none;
			display: flex;
			flex-direction: row;
			align-items: center;
		}

		.employee-signout {
			margin-top: 0;
			margin-left: auto;
		}

		.employee-content {
			padding: 1rem;
		}

		:global(.employee-stack-cards) {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}
	}

	@media (min-width: 768px) {
		.employee-content {
			padding: 1.1rem 1.2rem;
		}

		:global(.employee-card) {
			padding: 1rem;
		}
	}
</style>
