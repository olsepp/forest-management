<script lang="ts">
	import { browser } from '$app/environment';
	import { goto } from '$app/navigation';
	import AdminContentArea from '$lib/components/admin/AdminContentArea.svelte';
	import AdminSidebar from '$lib/components/admin/AdminSidebar.svelte';
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
</script>

<div class="mx-auto flex w-full max-w-7xl gap-4 p-4">
	<AdminSidebar />
	<AdminContentArea>
		{@render children()}
	</AdminContentArea>
</div>
