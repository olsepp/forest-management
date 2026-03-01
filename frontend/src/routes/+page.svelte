<script lang="ts">
	import { user } from '$lib/stores/auth.store';
	import { authService } from '$lib/services/auth';
	import { goto } from '$app/navigation';
	import type { PageData } from './$types';

	interface Props {
		data: PageData;
	}

	let { data }: Props = $props();

	// The auth state is available from the server-side data
	// For client-side interactivity, we use the auth store
	let isLoggingOut = $state(false);

	/**
	 * Handle logout
	 */
	async function handleLogout() {
		isLoggingOut = true;
		try {
			await authService.logout();
			// Redirect to sign-in page using SvelteKit navigation
			goto('/sign-in');
		} catch (error) {
			console.error('Logout failed:', error);
			isLoggingOut = false;
		}
	}
</script>

<svelte:head>
	<title>Home - Forest Management</title>
</svelte:head>

<div class="min-h-screen bg-gray-100">
	<!-- Navigation Bar -->
	<nav class="bg-white shadow-sm">
		<div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
			<div class="flex justify-between h-16">
				<div class="flex">
					<div class="flex-shrink-0 flex items-center">
						<h1 class="text-xl font-bold text-gray-900">
							Forest Management
						</h1>
					</div>
				</div>
				<div class="flex items-center">
					{#if $user}
						<div class="flex items-center space-x-4">
							<span class="text-sm text-gray-700">
								{$user.username}
								<span class="text-gray-500">({$user.role})</span>
							</span>
							<button
								onclick={handleLogout}
								disabled={isLoggingOut}
								class="text-sm text-red-600 hover:text-red-800 disabled:opacity-50"
							>
								{isLoggingOut ? 'Signing out...' : 'Sign out'}
							</button>
						</div>
					{/if}
				</div>
			</div>
		</div>
	</nav>

	<!-- Main Content -->
	<main class="max-w-7xl mx-auto py-6 sm:px-6 lg:px-8">
		<div class="px-4 py-6 sm:px-0">
			<div class="border-4 border-dashed border-gray-200 rounded-lg h-96 p-8">
				<div class="text-center">
					<svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor">
						<path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
					</svg>
					<h3 class="mt-2 text-sm font-medium text-gray-900">
						Company Selection
					</h3>
					<p class="mt-1 text-sm text-gray-500">
						Select a company to manage its forest data
					</p>
					<div class="mt-6">
						<p class="text-sm text-gray-500">
							Company selection interface will be implemented here
						</p>
					</div>
				</div>
			</div>
		</div>
	</main>
</div>
