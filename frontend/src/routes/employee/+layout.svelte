<script lang="ts">
	import { browser } from '$app/environment';
	import { goto } from '$app/navigation';
	import { user } from '$lib/stores/auth.store';
	import { getDefaultRouteForRole } from '$lib/services/auth';

	let { children } = $props();

	$effect(() => {
		if (!browser) return;

		const role = $user?.role?.trim().toLowerCase();
		if (role && role !== 'employee') {
			goto(getDefaultRouteForRole(role));
		}
	});
</script>

{@render children()}
