<script lang="ts">
	import './layout.css';
	import favicon from '$lib/assets/favicon.svg';
	import { isAuthenticated, user } from '$lib/stores/auth.store';
	import { getDefaultRouteForRole } from '$lib/services/auth';
	import { goto } from '$app/navigation';
	import { page } from '$app/stores';
	import { browser } from '$app/environment';

	let { children } = $props();

	// Client-side route protection
	// $isAuthenticated and $page are reactive — $effect re-runs automatically
	// when either changes, with no manual subscribe/unsubscribe needed.
	$effect(() => {
		if (!browser) return;

		const pathname: string = $page.url.pathname;
		const isPublicRoute = pathname === '/sign-in' || pathname === '/sign-up';

		if (!$isAuthenticated && !isPublicRoute) {
			// Not authenticated — redirect to sign-in
			goto('/sign-in');
		}
	});
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>
{@render children()}
