<script lang="ts">
	import { page, navigating } from '$app/stores';
	import { browser } from '$app/environment';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import { onMount } from 'svelte';
	import { get } from 'svelte/store';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import { getDefaultRouteForRole } from '$lib/services/auth';
	import ToastMessage from '$lib/components/shared/ToastMessage.svelte';
	import { toastStore } from '$lib/stores/toast.store';
	import TreeSpinner from '$lib/components/shared/TreeSpinner.svelte';
	import type { UserProfileDto } from '$lib/dtos/user/user.dto';

	type AppRoleRoute = '/' | '/admin' | '/employee' | '/sign-in';

	let { children } = $props();
	let pathname = $derived($page.url.pathname);
	let employeeDisplayName = $state('Metsandus');


	let currentCompanyId = $derived.by(() => {
		const fromParams = $page.params.CompanyId?.trim();
		if (fromParams) return fromParams;

		const matched = pathname.match(/^\/employee\/([^/]+)(?:\/|$)/i);
		if (!matched?.[1]) return null;

		const candidate = matched[1].trim();
		return candidate.toLowerCase() === 'user' ? null : candidate;
	});

	let navCompanyId = $derived(currentCompanyId ?? '');

	type NavIcon = 'overview' | 'property' | 'activity' | 'workorder' | 'companies';
	type NavItem = {
		key: 'overview' | 'landproperty' | 'activity' | 'workorder' | 'companies';
		label: string;
		route:
			| '/employee/[CompanyId]'
			| '/employee/[CompanyId]/landproperty'
			| '/employee/[CompanyId]/activity'
			| '/employee/[CompanyId]/workorder'
			| '/employee';
		isCompanyRoute: boolean;
		icon: NavIcon;
		isActive: boolean;
	};

	let navItems = $derived.by(() => {
		if (!navCompanyId) return [] as NavItem[];

		const companyRootPath = resolve('/employee/[CompanyId]', { CompanyId: navCompanyId });

		return [
			{
				key: 'overview',
				label: 'Ülevaade',
				route: '/employee/[CompanyId]',
				isCompanyRoute: true,
				icon: 'overview',
				isActive: pathname === companyRootPath
			},
			{
				key: 'landproperty',
				label: 'Kinnistud',
				route: '/employee/[CompanyId]/landproperty',
				isCompanyRoute: true,
				icon: 'property',
				isActive: pathname.includes(`/employee/${navCompanyId}/landproperty`)
			},
			{
				key: 'activity',
				label: 'Tegevused',
				route: '/employee/[CompanyId]/activity',
				isCompanyRoute: true,
				icon: 'activity',
				isActive: pathname.includes(`/employee/${navCompanyId}/activity`)
			},
			{
				key: 'workorder',
				label: 'Töökäsud',
				route: '/employee/[CompanyId]/workorder',
				isCompanyRoute: true,
				icon: 'workorder',
				isActive: pathname.includes(`/employee/${navCompanyId}/workorder`)
			},
			{
				key: 'companies',
				label: 'Ettevõtted',
				route: '/employee',
				isCompanyRoute: false,
				icon: 'companies',
				isActive: pathname === '/employee'
			}
		] satisfies NavItem[];
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
			const response = await fetch(`/api/users/profile`, {
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
		const role = get(user)?.role?.trim().toLowerCase();
		if (role && role !== 'employee') {
			goto(resolve(getDefaultRouteForRole(role) as AppRoleRoute));
			return;
		}

		loadEmployeeDisplayName();
	});
</script>

<div
	class="employee-layout mx-auto min-h-screen w-full max-w-6xl px-3 pt-0 sm:px-4 sm:pt-0 md:px-5"
>
	<header class="employee-appbar">
		<div>
			<p class="employee-kicker">Töötaja tööruum</p>
			<h1 class="employee-title">{employeeDisplayName}</h1>
		</div>

		<button type="button" onclick={handleSignOut} class="employee-signout">
			<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
				<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
				<polyline points="16 17 21 12 16 7"/>
				<line x1="21" y1="12" x2="9" y2="12"/>
			</svg>
			<span>Logi välja</span>
		</button>
	</header>

	{#if navItems.length > 0}
		<nav class="employee-top-nav" aria-label="Employee section navigation">
			{#each navItems as item (item.key)}
				{#if item.isCompanyRoute}
					<a
						href={resolve(
							item.route as
								| '/employee/[CompanyId]'
								| '/employee/[CompanyId]/landproperty'
								| '/employee/[CompanyId]/activity',
							{ CompanyId: navCompanyId }
						)}
						class="employee-nav-link"
						class:is-active={item.isActive}
						data-sveltekit-preload-data="tap"
					>
						<span class="employee-nav-icon" aria-hidden="true">
							{#if item.icon === 'overview'}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path
										d="M4 11.25 12 5l8 6.25V20a1 1 0 0 1-1 1h-4.5v-6h-5v6H5a1 1 0 0 1-1-1v-8.75Z"
									/>
								</svg>
							{:else if item.icon === 'property'}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path
										d="M4 7.5h16v12a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1v-12Zm2 2v9h12v-9H6Zm2-6h8l2 3H6l2-3Z"
									/>
								</svg>
							{:else if item.icon === 'activity'}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path d="M4 12.5h3.5l2-4 3.5 8 2.5-5H20" />
									<path d="M4 5h16v14H4z" fill="none" stroke-width="1.8" />
								</svg>
							{:else if item.icon === 'workorder'}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path
										d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2M9 5h6"
										fill="none"
										stroke-width="1.8"
									/>
									<path d="M9 12h2l1 6h-2l-1-6Z" />
									<path d="M10 12l1-2h2l1 2" />
								</svg>
							{:else}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path
										d="M3 7h18v11a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V7Zm4-3h10v2H7V4Zm2 6h2v2H9v-2Zm0 4h2v2H9v-2Zm4-4h6v2h-6v-2Zm0 4h6v2h-6v-2Z"
									/>
								</svg>
							{/if}
						</span>
						<span>{item.label}</span>
					</a>
				{:else}
					<a href={resolve('/employee')} class="employee-nav-link" class:is-active={item.isActive} data-sveltekit-preload-data="tap">
						<span class="employee-nav-icon" aria-hidden="true">
							<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
								<path
									d="M3 7h18v11a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V7Zm4-3h10v2H7V4Zm2 6h2v2H9v-2Zm0 4h2v2H9v-2Zm4-4h6v2h-6v-2Zm0 4h6v2h-6v-2Z"
								/>
							</svg>
						</span>
						<span>{item.label}</span>
					</a>
				{/if}
			{/each}
		</nav>
	{/if}

	<ToastMessage
		message={$toastStore.message}
		variant={$toastStore.variant}
		visible={$toastStore.visible}
	/>

	<main class="employee-main">
		<div class="employee-content-frame">
			<div class="employee-content">
				{@render children()}
			</div>
		</div>
	</main>

	{#if $navigating}
		<div class="employee-nav-overlay" aria-hidden="true">
			<TreeSpinner size={56} />
		</div>
	{/if}

	{#if navItems.length > 0}
		<nav class="employee-bottom-nav" aria-label="Employee mobile navigation">
			{#each navItems as item (item.key)}
				{#if item.isCompanyRoute}
					<a
						href={resolve(
							item.route as
								| '/employee/[CompanyId]'
								| '/employee/[CompanyId]/landproperty'
								| '/employee/[CompanyId]/activity'
								| '/employee/[CompanyId]/workorder',
							{ CompanyId: navCompanyId }
						)}
						class="employee-tab-link"
						class:is-active={item.isActive}
						data-sveltekit-preload-data="tap"
					>
						<span class="employee-tab-icon" aria-hidden="true">
							{#if item.icon === 'overview'}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path
										d="M4 11.25 12 5l8 6.25V20a1 1 0 0 1-1 1h-4.5v-6h-5v6H5a1 1 0 0 1-1-1v-8.75Z"
									/>
								</svg>
							{:else if item.icon === 'property'}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path
										d="M4 7.5h16v12a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1v-12Zm2 2v9h12v-9H6Zm2-6h8l2 3H6l2-3Z"
									/>
								</svg>
							{:else if item.icon === 'activity'}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path d="M4 12.5h3.5l2-4 3.5 8 2.5-5H20" />
									<path d="M4 5h16v14H4z" fill="none" stroke-width="1.8" />
								</svg>
							{:else if item.icon === 'workorder'}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path
										d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2M9 5a2 2 0 0 1 2-2h2a2 2 0 0 1 2 2M9 5h6"
										fill="none"
										stroke-width="1.8"
									/>
									<path d="M9 12h2l1 6h-2l-1-6Z" />
									<path d="M10 12l1-2h2l1 2" />
								</svg>
							{:else}
								<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
									<path
										d="M3 7h18v11a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V7Zm4-3h10v2H7V4Zm2 6h2v2H9v-2Zm0 4h2v2H9v-2Zm4-4h6v2h-6v-2Zm0 4h6v2h-6v-2Z"
									/>
								</svg>
							{/if}
						</span>
						<span>{item.label}</span>
					</a>
				{:else}
					<a href={resolve('/employee')} class="employee-tab-link" class:is-active={item.isActive} data-sveltekit-preload-data="tap">
						<span class="employee-tab-icon" aria-hidden="true">
							<svg viewBox="0 0 24 24" focusable="false" aria-hidden="true">
								<path
									d="M3 7h18v11a1 1 0 0 1-1 1H4a1 1 0 0 1-1-1V7Zm4-3h10v2H7V4Zm2 6h2v2H9v-2Zm0 4h2v2H9v-2Zm4-4h6v2h-6v-2Zm0 4h6v2h-6v-2Z"
								/>
							</svg>
						</span>
						<span>{item.label}</span>
					</a>
				{/if}
			{/each}
		</nav>
	{/if}
</div>

<style>
	:global(:root) {
		--employee-shell-bg: #edf2ef;
		--employee-surface: #ffffff;
		--employee-surface-alt: #f6faf7;
		--employee-accent: #1f5a42;
		--employee-accent-strong: #174632;
		--employee-border: #d6e2da;
		--employee-ink: #12221b;
		--employee-ink-soft: #42584c;
		--employee-focus-ring: rgba(31, 90, 66, 0.24);
		--employee-radius-md: 0.9rem;
		--employee-radius-lg: 1.1rem;
		--employee-shadow-soft: 0 4px 14px rgba(15, 30, 22, 0.08);
		--employee-shadow-strong: 0 10px 28px rgba(15, 30, 22, 0.16);
		--employee-bottom-nav-height: 4.4rem;
		--employee-safe-bottom: max(0.55rem, env(safe-area-inset-bottom));
		--employee-shell-pad-x: 0.75rem;
	}

	.employee-layout {
		color: var(--employee-ink);
		background: linear-gradient(180deg, #b6b5b5 0%, #cecfcf 100%);
		background-color: var(--employee-shell-bg);
		display: grid;
		grid-template-rows: auto auto 1fr;
		gap: 0.8rem;
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
		padding: 0.8rem 0.9rem;
		background: linear-gradient(180deg, #275f47 0%, #1f5a42 100%);
		border-radius: 0;
		box-shadow: var(--employee-shadow-strong);
		width: 100vw;
		margin-left: calc(50% - 50vw);
		margin-right: calc(50% - 50vw);
	}

	.employee-kicker {
		margin: 0;
		font-size: 0.72rem;
		font-weight: 700;
		letter-spacing: 0.03em;
		text-transform: uppercase;
		color: #d6e9df;
	}

	.employee-title {
		margin: 0.16rem 0 0;
		font-size: 1.05rem;
		line-height: 1.25;
		font-weight: 700;
		color: #f2faf6;
	}

	.employee-signout {
		display: inline-flex;
		align-items: center;
		gap: 0.45rem;
		border: 1px solid rgba(255, 255, 255, 0.25);
		background: rgba(255, 255, 255, 0.12);
		color: #f2faf6;
		border-radius: var(--employee-radius-md);
		min-height: 2.6rem;
		padding: 0.5rem 0.85rem;
		font-size: 0.84rem;
		font-weight: 600;
		line-height: 1;
		cursor: pointer;
		transition:
			background-color 0.18s ease,
			border-color 0.18s ease,
			transform 0.12s ease,
			box-shadow 0.18s ease;
	}

	.employee-signout svg {
		width: 1rem;
		height: 1rem;
		stroke-width: 2;
	}

	.employee-signout:hover {
		background: rgba(255, 255, 255, 0.22);
		border-color: rgba(255, 255, 255, 0.4);
	}

	.employee-signout:active {
		transform: translateY(1px);
	}

	.employee-signout:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px var(--employee-focus-ring);
	}

	.employee-top-nav {
		display: none;
	}

	.employee-nav-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		gap: 0.44rem;
		text-align: center;
		text-decoration: none;
		min-height: 3rem;
		padding: 0.62rem 0.7rem;
		border-radius: var(--employee-radius-md);
		border: 1px solid var(--employee-border);
		background: var(--employee-surface-alt);
		font-size: 0.9rem;
		font-weight: 600;
		color: var(--employee-ink-soft);
		transition:
			background-color 0.18s ease,
			border-color 0.18s ease,
			color 0.18s ease;
	}

	.employee-nav-icon {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		width: 1rem;
		height: 1rem;
	}

	.employee-nav-icon :global(svg) {
		width: 1rem;
		height: 1rem;
		fill: currentColor;
		stroke: currentColor;
		stroke-width: 0;
	}

	.employee-nav-link:hover {
		background: #eef5f1;
		border-color: #b8cdc1;
	}

	.employee-nav-link.is-active {
		background: linear-gradient(
			180deg,
			var(--employee-accent) 0%,
			var(--employee-accent-strong) 100%
		);
		border-color: var(--employee-accent-strong);
		color: #f3fbf7;
	}

	.employee-main {
		min-height: 0;
	}

	.employee-content-frame {
		background: transparent;
		padding: 0.85rem;
	}

	.employee-content {
		padding: 0;
		background: transparent;
		border: 0;
		border-radius: 0;
		box-shadow: none;
	}

	.employee-nav-overlay {
		position: fixed;
		inset: 0;
		z-index: 10000;
		display: flex;
		align-items: center;
		justify-content: center;
		background: rgba(237, 242, 239, 0.95);
		backdrop-filter: blur(4px);
		animation: overlay-fade-in 0.15s ease-out;
	}

	@keyframes overlay-fade-in {
		from {
			opacity: 0;
		}
		to {
			opacity: 1;
		}
	}

	.employee-bottom-nav {
		position: fixed;
		left: 0.55rem;
		right: 0.55rem;
		bottom: 0;
		z-index: 1200;
		display: flex;
		gap: 0.4rem;
		height: calc(var(--employee-bottom-nav-height) + var(--employee-safe-bottom));
		padding: 0.45rem 0.6rem calc(0.45rem + var(--employee-safe-bottom));
		background: rgba(251, 253, 252, 0.95);
		border: 1px solid #cfddd5;
		border-bottom: 0;
		border-top-left-radius: 1.15rem;
		border-top-right-radius: 1.15rem;
		box-shadow: 0 -8px 24px rgba(15, 34, 25, 0.16);
		backdrop-filter: blur(8px);
	}

	.employee-tab-link {
		flex: 1;
		display: inline-flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		gap: 0.2rem;
		text-align: center;
		text-decoration: none;
		min-height: 2.9rem;
		padding: 0.55rem 0.35rem;
		border-radius: var(--employee-radius-md);
		border: 1px solid transparent;
		background: transparent;
		font-size: 0.8rem;
		font-weight: 600;
		color: #496054;
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
		background: #e8f3ed;
		border-color: #b7cec1;
		color: #164c35;
	}

	.employee-tab-link:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px var(--employee-focus-ring);
	}

	.employee-tab-icon {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		width: 1rem;
		height: 1rem;
	}

	.employee-tab-icon :global(svg) {
		width: 1rem;
		height: 1rem;
		fill: currentColor;
		stroke: currentColor;
		stroke-width: 0;
	}

	:global(.employee-stack-cards) {
		display: grid;
		gap: 0.9rem;
	}

	:global(.employee-card) {
		border: 1px solid var(--employee-border);
		border-radius: var(--employee-radius-lg);
		background: linear-gradient(180deg, #ffffff 0%, #f8fbf9 100%);
		padding: 1rem;
		margin-bottom: 1rem;
		box-shadow: var(--employee-shadow-soft);
	}

	:global(.employee-card:is(.hero, .intro, .summary, .page-intro)) {
		position: relative;
		overflow: hidden;
	}

	/* :global(.employee-card:is(.hero, .intro, .summary, .page-intro)::before) {
		content: '';
		position: absolute;
		left: 0;
		top: 0;
		bottom: 0;
		width: 0.3rem;
		background: linear-gradient(180deg, #88b7a0 0%, #5e8f78 100%);
	} */

	:global(.employee-table-wrap) {
		overflow-x: auto;
		max-width: 100%;
		border: 1px solid var(--employee-border);
		border-radius: var(--employee-radius-md);
		background: #ffffff;
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

	:global(.employee-table-wrap th) {
		background: #edf4f0;
		color: #173728;
		font-weight: 700;
	}

	:global(.employee-table-wrap td) {
		color: #2c473b;
	}

	:global(.employee-state-block) {
		border-radius: var(--employee-radius-md);
		border: 1px solid #cfddd5;
		background: #f5faf7;
		padding: 0.85rem;
		font-size: 0.97rem;
		color: #2d4a3d;
	}

	:global(.employee-state-block.is-error) {
		border-color: #f0c2c2;
		background: #fff5f5;
		color: #8e2525;
	}

	:global(.employee-state-block.is-empty) {
		border-color: #d6e0ea;
		background: #f8fbff;
		color: #334155;
	}

	:global(.employee-state-block.is-loading) {
		border-color: #bfd3c8;
		background: #f1f7f4;
		color: #24543e;
		display: flex;
		align-items: center;
		gap: 0.65rem;
	}

	:global(.employee-state-block.is-loading::before) {
		content: '';
		flex-shrink: 0;
		width: 1.25rem;
		height: 1.25rem;
		border: 2.5px solid #bfd3c8;
		border-top-color: #1f5a42;
		border-radius: 50%;
		animation: employee-spin 0.7s linear infinite;
	}

	@keyframes employee-spin {
		to {
			transform: rotate(360deg);
		}
	}

	:global(.employee-content :is(button, a, input, select, textarea)) {
		min-height: 2.75rem;
	}

	:global(.employee-page-title) {
		margin: 0 0 0.8rem;
		font-size: 1.32rem;
		line-height: 1.25;
		color: #0f261c;
	}

	:global(.employee-back-link) {
		margin: 0 0 0.9rem;
	}

	:global(.employee-back-link-button) {
		display: inline-flex;
		align-items: center;
		gap: 0.45rem;
		margin-bottom: .7rem;
		min-height: 3rem;
		padding: 0.65rem 0.95rem;
		border-radius: 0.85rem;
		border: 1px solid #c8d3df;
		background: #ffffff;
		font-size: 0.97rem;
		font-weight: 700;
		text-decoration: none;
		color: #1e293b;
		box-shadow: 0 2px 8px rgba(15, 23, 42, 0.07);
	}

	:global(.employee-back-link-button:hover) {
		background: #f4f7fb;
		border-color: #b6c5d6;
	}

	:global(.employee-back-link-button:active) {
		transform: translateY(1px);
	}

	:global(.employee-back-link-button:focus-visible) {
		outline: none;
		box-shadow: 0 0 0 3px var(--employee-focus-ring);
	}

	:global(.employee-content :is(button, a):focus-visible),
	:global(.employee-content :is(input, select, textarea):focus-visible) {
		outline: none;
		box-shadow: 0 0 0 3px var(--employee-focus-ring);
	}

	:global(.employee-content :is(button, a):active) {
		transform: translateY(1px);
	}

	@media (min-width: 640px) {
		:global(:root) {
			--employee-shell-pad-x: 1rem;
		}

		.employee-layout {
			grid-template-rows: auto auto 1fr;
			padding-bottom: 2.25rem;
		}

		.employee-appbar {
			position: static;
			padding: 0.9rem 1rem;
			margin-bottom: 0;
		}

		.employee-title {
			font-size: 1.18rem;
		}

		.employee-top-nav {
			display: flex;
			align-items: center;
			gap: 0.55rem;
			padding: 0.5rem;
			background: #ffffff;
			border: 1px solid var(--employee-border);
			border-radius: calc(var(--employee-radius-lg) + 0.05rem);
			box-shadow: var(--employee-shadow-soft);
		}

		.employee-bottom-nav {
			display: none;
		}

		.employee-content {
			padding: 0;
		}

		.employee-content-frame {
			padding: 1.05rem;
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
			padding: 0;
		}

		.employee-content-frame {
			padding: 1.2rem;
		}

		:global(.employee-card) {
			padding: 1.05rem;
		}

		:global(.employee-page-title) {
			font-size: 1.45rem;
		}
	}
</style>
