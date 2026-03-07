<script lang="ts">
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let username = $state('');
	let email = $state('');
	let firstName = $state('');
	let lastName = $state('');
	let role = $state('Employee');
	let password = $state('');

	let isSubmitting = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');

	async function createUser() {
		errorMessage = '';
		successMessage = '';
		isSubmitting = true;

		try {
			const token = await authService.ensureValidToken();

			const response = await fetch(`${apiBaseUrl}/api/users`, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				},
				body: JSON.stringify({
					username,
					email,
					firstName,
					lastName,
					role,
					password
				})
			});

			if (response.status === 201) {
				successMessage = 'User account created successfully.';
				username = '';
				email = '';
				firstName = '';
				lastName = '';
				role = 'Employee';
				password = '';
				return;
			}

			if (response.status === 401) {
				errorMessage = 'Unauthorized. Please sign in again.';
				return;
			}

			if (response.status === 403) {
				errorMessage = 'Forbidden. Admin role is required.';
				return;
			}

			if (response.status === 400) {
				const data = (await response.json().catch(() => null)) as { message?: string } | null;
				errorMessage = data?.message || 'Validation failed. Username or email may already exist.';
				return;
			}

			errorMessage = 'Failed to create user account.';
		} catch {
			errorMessage = 'Network error. Please try again.';
		} finally {
			isSubmitting = false;
		}
	}
</script>

<div class="mb-4 flex items-center justify-between gap-3">
	<h1 class="text-2xl font-semibold text-slate-900">Create user account</h1>
	<a
		href={resolve('/admin/user')}
		class="inline-flex items-center rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
	>
		Back to users
	</a>
</div>

{#if errorMessage}
	<p class="mb-4 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{errorMessage}</p>
{/if}

{#if successMessage}
	<p class="mb-4 rounded-lg border border-green-200 bg-green-50 px-3 py-2 text-sm text-green-700">{successMessage}</p>
{/if}

<form on:submit|preventDefault={createUser} class="max-w-2xl space-y-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
	<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Username</span>
			<input bind:value={username} required class="rounded-lg border border-slate-300 px-3 py-2" />
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Email</span>
			<input bind:value={email} type="email" required class="rounded-lg border border-slate-300 px-3 py-2" />
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">First name</span>
			<input bind:value={firstName} required class="rounded-lg border border-slate-300 px-3 py-2" />
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Last name</span>
			<input bind:value={lastName} required class="rounded-lg border border-slate-300 px-3 py-2" />
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Role</span>
			<select bind:value={role} class="rounded-lg border border-slate-300 px-3 py-2">
				<option value="Employee">Employee</option>
				<option value="Admin">Admin</option>
			</select>
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Password</span>
			<input
				bind:value={password}
				type="password"
				minlength="6"
				required
				class="rounded-lg border border-slate-300 px-3 py-2"
			/>
			<span class="text-xs text-slate-500"
				>Password must include at least 1 symbol, 1 uppercase letter, and 1 number.</span
			>
		</label>
	</div>

	<div>
		<button
			type="submit"
			disabled={isSubmitting}
			class="inline-flex items-center rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-60"
		>
			{isSubmitting ? 'Creating...' : 'Create account'}
		</button>
	</div>
</form>
