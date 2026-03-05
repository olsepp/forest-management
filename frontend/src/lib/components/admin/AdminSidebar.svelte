<script lang="ts">
	import { page } from '$app/stores';

	type MenuItem = {
		label: string;
		href: string;
	};

	const adminRootItems: MenuItem[] = [
		{ label: 'Company selection', href: '/admin' },
		{ label: 'Users', href: '/admin/user' }
	];

	function getCompanyItems(companyId: string): MenuItem[] {
		return [
			{ label: 'Activities', href: `/admin/${companyId}/activity` },
			{ label: 'Dashboard', href: `/admin/${companyId}/dashboard` },
			{ label: 'Land properties', href: `/admin/${companyId}/landproperty` }
		];
	}

	const companyId = $derived.by(() => {
		const match = $page.url.pathname.match(/^\/admin\/([^/]+)(?:\/|$)/);
		if (!match) return null;

		const candidate = match[1];
		if (candidate === 'user') return null;

		return candidate;
	});

	const menuItems = $derived(companyId ? getCompanyItems(companyId) : adminRootItems);

	function isActive(pathname: string, href: string): boolean {
		if (href === '/admin') {
			return pathname === '/admin';
		}

		return pathname === href || pathname.startsWith(`${href}/`);
	}
</script>

<aside
	class="sticky top-4 h-[calc(100vh-2rem)] w-64 shrink-0 rounded-2xl border border-slate-200 bg-white p-4 shadow-sm"
>
	<div class="mb-6">
		<p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Admin panel</p>
		<p class="mt-1 text-sm text-slate-700">
			{#if companyId}
				Company scope: {companyId}
			{:else}
				Global scope
			{/if}
		</p>
	</div>

	<nav class="space-y-1">
		<a
			href="/admin"
			class={`mb-3 block rounded-lg border px-3 py-2 text-sm font-medium transition-colors ${
				$page.url.pathname === '/admin'
					? 'border-slate-400 bg-slate-100 text-slate-800'
					: 'border-slate-200 text-slate-700 hover:bg-slate-100'
			}`}
		>
			Home
		</a>

		{#each menuItems as item}
			<a
				href={item.href}
				class={`block rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
					isActive($page.url.pathname, item.href)
						? 'bg-emerald-50 text-emerald-700'
						: 'text-slate-700 hover:bg-slate-100'
				}`}
			>
				{item.label}
			</a>
		{/each}
	</nav>
</aside>
