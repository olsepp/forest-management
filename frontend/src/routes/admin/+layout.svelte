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
				Logi välja
			</button>
		</div>
		{@render children()}
	</AdminContentArea>
</div>
