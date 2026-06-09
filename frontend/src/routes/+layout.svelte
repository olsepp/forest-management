<script lang="ts">
	import './layout.css';
	import favicon from '$lib/assets/icon.svg';
	import { isAuthenticated } from '$lib/stores/auth.store';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
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
			goto(resolve('/sign-in'));
		}
	});

	$effect(() => {
		if (!browser) return;

		const pathname = $page.url.pathname;
		const shouldUseAdminBackground = pathname.startsWith('/admin') || pathname === '/sign-in';
		document.body.classList.toggle('admin-background', shouldUseAdminBackground);

		return () => {
			document.body.classList.remove('admin-background');
		};
	});
</script>

<svelte:head><link rel="icon" href={favicon} /></svelte:head>
{@render children()}
