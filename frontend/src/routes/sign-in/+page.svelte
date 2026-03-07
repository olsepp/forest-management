<script lang="ts">
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import { authService, AuthServiceError, getDefaultRouteForRole } from '$lib/services/auth';

	// Local state for form handling
	let isLoading = $state(false);
	let username = $state('');
	let password = $state('');
	let error = $state('');

	/**
	 * Handle login form submission entirely client-side.
	 * Tokens are never sent through the server — they go directly from the
	 * backend API to the browser's auth store and localStorage.
	 */
	async function handleLogin(e: SubmitEvent) {
		e.preventDefault();
		isLoading = true;
		error = '';

		try {
			const loginResponse = await authService.login({ username, password });
			const targetRoute = getDefaultRouteForRole(loginResponse.role);
			goto(resolve(targetRoute), { replaceState: true });
		} catch (err) {
			error = err instanceof AuthServiceError ? err.message : 'Login failed. Please try again.';
		} finally {
			isLoading = false;
		}
	}
</script>

<svelte:head>
	<title>Sign In - Forest Management</title>
</svelte:head>

<div class="min-h-screen flex items-center justify-center px-4 py-12 sm:px-6 lg:px-8">
	<div class="w-full max-w-md space-y-8 rounded-2xl border border-[#d8e1dc] bg-white/95 p-8 shadow-[0_14px_36px_rgba(20,41,31,0.12)] backdrop-blur-sm">
		<div class="text-center">
			<div class="mx-auto mb-4 inline-flex h-12 w-12 items-center justify-center rounded-xl border border-[#c8d5ce] bg-[#f4f8f5] text-[#1f5a42]">
				<svg class="h-6 w-6" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" aria-hidden="true">
					<path stroke-linecap="round" stroke-linejoin="round" d="M12 3v14m0-14L7 8m5-5 5 5M5 20h14" />
				</svg>
			</div>
			<h1 class="text-2xl font-bold tracking-tight text-[#1f2a24] sm:text-3xl">Sign in to your account</h1>
			<p class="mt-2 text-sm text-[#56645d]">Forest Management System</p>
		</div>

		<form
			onsubmit={handleLogin}
			class="mt-8 space-y-6"
		>
			{#if error}
				<div class="rounded-lg border border-[#e9c9c6] bg-[#fff4f3] p-4">
					<div class="flex">
						<div class="flex-shrink-0">
							<svg class="h-5 w-5 text-[#b1443d]" viewBox="0 0 20 20" fill="currentColor">
								<path fill-rule="evenodd" clip-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" />
							</svg>
						</div>
						<div class="ml-3">
							<h3 class="text-sm font-medium text-[#8f2c25]">
								{error}
							</h3>
						</div>
					</div>
				</div>
			{/if}

			<div class="space-y-4">
				<div>
					<label for="username" class="mb-1.5 block text-sm font-medium text-[#2d3a34]">Username</label>
					<input
						id="username"
						name="username"
						type="text"
						autocomplete="username"
						required
						bind:value={username}
						class="block w-full rounded-lg border border-[#cad6cf] bg-[#fcfdfc] px-3 py-2.5 text-sm text-[#1f2a24] placeholder:text-[#7a8a82] shadow-sm transition focus:border-[#1f5a42] focus:outline-none focus:ring-2 focus:ring-[#1f5a42]/20"
						placeholder="Username"
					/>
				</div>
				<div>
					<label for="password" class="mb-1.5 block text-sm font-medium text-[#2d3a34]">Password</label>
					<input
						id="password"
						name="password"
						type="password"
						autocomplete="current-password"
						required
						bind:value={password}
						class="block w-full rounded-lg border border-[#cad6cf] bg-[#fcfdfc] px-3 py-2.5 text-sm text-[#1f2a24] placeholder:text-[#7a8a82] shadow-sm transition focus:border-[#1f5a42] focus:outline-none focus:ring-2 focus:ring-[#1f5a42]/20"
						placeholder="Password"
					/>
				</div>
			</div>

			<div>
				<button
					type="submit"
					disabled={isLoading}
					class="group relative flex w-full items-center justify-center rounded-lg border border-[#1f5a42] bg-[#1f5a42] px-4 py-2.5 text-sm font-semibold text-white shadow-[0_6px_18px_rgba(31,90,66,0.28)] transition hover:bg-[#174834] hover:border-[#174834] focus:outline-none focus:ring-2 focus:ring-[#1f5a42]/30 focus:ring-offset-2 disabled:cursor-not-allowed disabled:opacity-60"
				>
					{#if isLoading}
						<svg class="-ml-1 mr-3 h-5 w-5 animate-spin text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
							<circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" />
							<path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z" />
						</svg>
						Signing in...
					{:else}
						Sign in
					{/if}
				</button>
			</div>
		</form>
	</div>
</div>
