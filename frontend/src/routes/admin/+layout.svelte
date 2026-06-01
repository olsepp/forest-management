<script lang="ts">
	import '$lib/styles/admin-theme.css';
	import { browser } from '$app/environment';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import AdminContentArea from '$lib/components/admin/AdminContentArea.svelte';
	import AdminSidebar from '$lib/components/admin/AdminSidebar.svelte';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import { getDefaultRouteForRole } from '$lib/services/auth';

	let { children } = $props();

	$effect(() => {
		if (!browser) return;

		const role = $user?.role?.trim().toLowerCase();
		if (role && role !== 'admin') {
			goto(resolve(getDefaultRouteForRole(role)));
		}
	});

	async function handleSignOut() {
		await authService.logout();
		goto(resolve('/sign-in'));
	}
</script>

<div class="admin-theme mx-auto flex w-full max-w-[1600px] gap-5 p-4">
	<AdminSidebar />
	<AdminContentArea>
		<div class="mb-4 flex justify-end">
			<button
				type="button"
				onclick={handleSignOut}
				class="admin-signout"
			>
				<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">
					<path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/>
					<polyline points="16 17 21 12 16 7"/>
					<line x1="21" y1="12" x2="9" y2="12"/>
				</svg>
				<span>Logi välja</span>
			</button>
		</div>
		{@render children()}
	</AdminContentArea>
</div>
