<script lang="ts">
	import { user } from '$lib/stores/auth.store';
	import { getDefaultRouteForRole } from '$lib/services/auth';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import { browser } from '$app/environment';

	$effect(() => {
		if (!browser) return;

		const role = $user?.role?.trim().toLowerCase();
		if (role) {
			goto(resolve(getDefaultRouteForRole(role)));
		}
	});
</script>

<svelte:head>
	<title>Avaleht - Metsandussüsteem</title>
</svelte:head>

<div class="flex min-h-screen items-center justify-center bg-gray-100">
	<div class="flex flex-col items-center gap-4">
		<div
			class="h-10 w-10 animate-spin rounded-full border-4 border-gray-300 border-t-gray-600"
		></div>
		<p class="text-sm text-gray-500">Suunatakse ümber...</p>
	</div>
</div>
