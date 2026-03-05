<script lang="ts">
	import { browser } from '$app/environment';
	import { goto } from '$app/navigation';
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
			goto(getDefaultRouteForRole(role));
		}
	});

	async function handleSignOut() {
		await authService.logout();
		goto('/sign-in');
	}
</script>

<div class="mx-auto flex w-full max-w-7xl gap-4 p-4">
	<AdminSidebar />
	<AdminContentArea>
		<div class="mb-4 flex justify-end">
			<button
				type="button"
				onclick={handleSignOut}
				class="rounded-lg border border-slate-300 px-3 py-2 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
			>
				Sign out
			</button>
		</div>
		{@render children()}
	</AdminContentArea>
</div>
