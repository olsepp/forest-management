<script lang="ts">
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import { onMount } from 'svelte';

	type UserProfileDto = {
		id?: string;
		userId?: string;
		username?: string;
		email?: string;
		firstName?: string;
		lastName?: string;
		role?: string;
		phoneNumber?: string;
		[key: string]: unknown;
	};

	type ProfileViewModel = {
		id: string;
		username: string;
		email: string;
		firstName: string;
		lastName: string;
		role: string;
		phoneNumber: string;
		source: 'api' | 'fallback';
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let errorMessage = $state('');
	let profile = $state<ProfileViewModel | null>(null);

	function valueOrDash(value: unknown): string {
		if (typeof value !== 'string') return '—';
		const trimmed = value.trim();
		return trimmed || '—';
	}

	function toProfileViewModel(data: UserProfileDto, source: 'api' | 'fallback'): ProfileViewModel {
		const id =
			typeof data.id === 'string' && data.id.trim()
				? data.id
				: typeof data.userId === 'string' && data.userId.trim()
					? data.userId
					: valueOrDash($user?.userId);

		return {
			id,
			username: valueOrDash(data.username),
			email: valueOrDash(data.email),
			firstName: valueOrDash(data.firstName),
			lastName: valueOrDash(data.lastName),
			role: valueOrDash(data.role),
			phoneNumber: valueOrDash(data.phoneNumber),
			source
		};
	}

	function fallbackFromStore(): ProfileViewModel | null {
		if (!$user) return null;

		return {
			id: valueOrDash($user.userId),
			username: valueOrDash($user.username),
			email: valueOrDash($user.email),
			firstName: '—',
			lastName: '—',
			role: valueOrDash($user.role),
			phoneNumber: '—',
			source: 'fallback'
		};
	}

	async function loadProfile() {
		try {
			errorMessage = '';
			isLoading = true;

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/users/profile`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!response.ok) {
				const fallback = fallbackFromStore();
				if (fallback) {
					profile = fallback;
					errorMessage =
						response.status === 401
							? 'Profile endpoint unavailable (unauthorized). Showing account data from your session.'
							: response.status === 403
								? 'Profile endpoint is restricted. Showing account data from your session.'
								: 'Profile endpoint unavailable. Showing account data from your session.';
					return;
				}

				errorMessage =
					response.status === 401
						? 'Unauthorized. Please sign in again.'
						: response.status === 403
							? 'Profile endpoint is restricted for your role.'
							: 'Failed to load profile.';
				return;
			}

			const data = (await response.json()) as UserProfileDto;
			profile = toProfileViewModel(data, 'api');
		} catch {
			const fallback = fallbackFromStore();
			if (fallback) {
				profile = fallback;
				errorMessage = 'Failed to load profile endpoint. Showing account data from your session.';
				return;
			}

			errorMessage = 'Failed to load profile.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadProfile);
</script>

<p class="back-link">
	<a href={resolve('/employee')}>← Back to employee home</a>
</p>

<section class="employee-card summary">
	<p class="kicker">User profile</p>
	<h1>Your account</h1>
	<p>Profile details for your employee account.</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Loading profile…</div>
{:else if !profile}
	<div class="employee-state-block is-error">{errorMessage || 'Profile is unavailable.'}</div>
{:else}
	{#if errorMessage}
		<div class="employee-state-block is-error">{errorMessage}</div>
	{/if}

	<section class="employee-card profile-grid">
		<p><strong>User ID:</strong> {profile.id}</p>
		<p><strong>Username:</strong> {profile.username}</p>
		<p><strong>Email:</strong> {profile.email}</p>
		<p><strong>First name:</strong> {profile.firstName}</p>
		<p><strong>Last name:</strong> {profile.lastName}</p>
		<p><strong>Role:</strong> {profile.role}</p>
		<p><strong>Phone:</strong> {profile.phoneNumber}</p>
		<p><strong>Data source:</strong> {profile.source === 'api' ? 'Profile endpoint' : 'Session fallback'}</p>
	</section>
{/if}

<style>
	.back-link {
		margin: 0 0 0.75rem;
	}

	.back-link a {
		font-size: 0.9rem;
		font-weight: 700;
		text-decoration: none;
		color: #1f5a42;
	}

	.summary {
		margin-bottom: 0.75rem;
	}

	.kicker {
		margin: 0;
		font-size: 0.77rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	h1 {
		margin: 0.3rem 0 0.4rem;
		font-size: 1.2rem;
		line-height: 1.2;
		color: #17251e;
	}

	p {
		margin: 0;
		color: #40574a;
	}

	.profile-grid {
		display: grid;
		gap: 0.45rem;
	}

	.profile-grid p {
		margin: 0;
		color: #3f564a;
	}

	@media (min-width: 768px) {
		h1 {
			font-size: 1.35rem;
		}

		.profile-grid {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}
	}
</style>

