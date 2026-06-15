<script lang="ts">
import { resolve } from '$app/paths';
import { goto } from '$app/navigation';
import { page } from '$app/stores';
import type { CompanyListDto } from '$lib/dtos/company/company.dto';
import { onMount } from 'svelte';
import { authService } from '$lib/services/auth';

	type MenuItem = {
		label: string;
		href: string;
	};

	const adminRootItems: MenuItem[] = [
		{ label: 'Ettevõtte valik', href: '/admin' },
		{ label: 'Kasutajad', href: '/admin/user' },
		{ label: 'Tegevuste tüübid', href: '/admin/activity-types' }
	];

	function getCompanyItems(companyId: string): MenuItem[] {
		return [
			{ label: 'Tegevused', href: `/admin/${companyId}/activity` },
			{ label: 'Töökäsud', href: `/admin/${companyId}/workorder` },
			{ label: 'Töölaud', href: `/admin/${companyId}/dashboard` },
			{ label: 'Kinnistud', href: `/admin/${companyId}/landproperty` },
			{ label: 'Müüdud kinnistud', href: `/admin/${companyId}/landproperty-sold` }
		];
	}

	const companyId = $derived.by(() => {
		const match = $page.url.pathname.match(/^\/admin\/([^/]+)(?:\/|$)/);
		if (!match) return null;

		const candidate = match[1];
		if (candidate === 'user' || candidate === 'activity-types') return null;

		return candidate;
	});

	let companies = $state<CompanyListDto[]>([]);

	onMount(async () => {
		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`/api/companies`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				companies = [];
				return;
			}

			companies = await response.json();
		} catch {
			companies = [];
		}
	});

	const companyName = $derived.by(() => {
		if (!companyId) return null;
		return companies.find((company) => company.id === companyId)?.name ?? companyId;
	});

	const menuItems = $derived(companyId ? getCompanyItems(companyId) : adminRootItems);

	function isActive(pathname: string, href: string): boolean {
		if (href === '/admin') {
			return pathname === '/admin';
		}

		return pathname === href || pathname.startsWith(`${href}/`);
	}

	async function handleSignOut() {
		await authService.logout();
		goto(resolve('/sign-in'));
	}
</script>

<aside class="admin-sidebar sticky top-4 h-[calc(100vh-2rem)] w-64 shrink-0 p-4 flex flex-col">
	<div class="mb-6">
		<p class="text-xs font-semibold tracking-wide text-slate-500 uppercase">Adminpaneel</p>
		<p class="mt-1 text-sm text-slate-700">
			{#if companyId}
				Ettevõtte vaade: {companyName}
			{:else}
				Üldvaade
			{/if}
		</p>
	</div>

	<nav class="space-y-1 flex-1 overflow-auto">
		<a
			href={resolve('/admin')}
			class={`home-link nav-item mb-3 block rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
				$page.url.pathname === '/admin'
					? 'nav-item-active border-slate-400 bg-slate-100 text-slate-800'
					: 'border-slate-200 text-slate-700 hover:bg-slate-100'
			}`}
		>
			Avaleht
		</a>

		{#each menuItems as item (item.href)}
			<a
				href={resolve(item.href as unknown as '/')}
				class={`nav-item block rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
					isActive($page.url.pathname, item.href)
						? 'nav-item-active bg-emerald-50 text-emerald-700'
						: 'text-slate-700 hover:bg-slate-100'
				}`}
			>
				{item.label}
			</a>
		{/each}
	</nav>

	<div class="pt-3 border-t border-slate-200">
		<button
			type="button"
			onclick={handleSignOut}
			class="admin-signout w-full"
		>
			<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
				<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
				<polyline points="16 17 21 12 16 7"/>
				<line x1="21" y1="12" x2="9" y2="12"/>
			</svg>
			<span>Logi välja</span>
		</button>
	</div>
</aside>
