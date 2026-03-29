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
							? 'Profiili teenus pole saadaval (ligipääs puudub). Kuvatakse sinu seansi kontoandmed.'
							: response.status === 403
								? 'Profiili teenus on piiratud. Kuvatakse sinu seansi kontoandmed.'
								: 'Profiili teenus pole saadaval. Kuvatakse sinu seansi kontoandmed.';
					return;
				}

				errorMessage =
					response.status === 401
						? 'Ligipääs puudub. Logige uuesti sisse.'
						: response.status === 403
							? 'Profiili teenus on sinu rollile piiratud.'
							: 'Profiili laadimine ebaõnnestus.';
				return;
			}

			const data = (await response.json()) as UserProfileDto;
			profile = toProfileViewModel(data, 'api');
		} catch {
			const fallback = fallbackFromStore();
			if (fallback) {
				profile = fallback;
				errorMessage = 'Profiili teenuse laadimine ebaõnnestus. Kuvatakse sinu seansi kontoandmed.';
				return;
			}

			errorMessage = 'Profiili laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadProfile);
</script>

<p class="back-link">
	<a href={resolve('/employee')}>← Tagasi töötaja avalehele</a>
</p>

<section class="employee-card summary">
	<p class="kicker">Kasutajaprofiil</p>
	<h1>Sinu konto</h1>
	<p>Profiiliandmed sinu töötaja kontole.</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse profiili…</div>
{:else if !profile}
	<div class="employee-state-block is-error">{errorMessage || 'Profiil pole saadaval.'}</div>
{:else}
	{#if errorMessage}
		<div class="employee-state-block is-error">{errorMessage}</div>
	{/if}

	<section class="employee-card profile-grid">
		<p><strong>Kasutaja ID:</strong> {profile.id}</p>
		<p><strong>Kasutajanimi:</strong> {profile.username}</p>
		<p><strong>Email:</strong> {profile.email}</p>
		<p><strong>Eesnimi:</strong> {profile.firstName}</p>
		<p><strong>Perekonnanimi:</strong> {profile.lastName}</p>
		<p><strong>Roll:</strong> {profile.role}</p>
		<p><strong>Telefon:</strong> {profile.phoneNumber}</p>
		<p><strong>Andmeallikas:</strong> {profile.source === 'api' ? 'Profiili teenus' : 'Seansi varuandmed'}</p>
	</section>
{/if}

<style>
	.back-link {
		margin: 0 0 0.75rem;
	}

	.back-link a {
		display: inline-flex;
		align-items: center;
		min-height: 2.75rem;
		padding: 0.25rem 0.5rem;
		border-radius: 0.75rem;
		font-size: 0.86rem;
		font-weight: 700;
		text-decoration: none;
		color: #1f5a42;
		transition:
			background-color 0.18s ease,
			color 0.18s ease,
			transform 0.12s ease;
	}

	.back-link a:hover {
		background: #eef6f2;
	}

	.back-link a:active {
		transform: translateY(1px);
	}

	.back-link a:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.22);
	}

	.summary {
		margin-bottom: 0.75rem;
		padding: 1rem;
		background: linear-gradient(180deg, #ffffff 0%, #f3f8f5 100%);
		border-color: #d2e1d8;
	}

	.kicker {
		margin: 0;
		font-size: 0.72rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	h1 {
		margin: 0.3rem 0 0.4rem;
		font-size: 1.14rem;
		line-height: 1.2;
		color: #17251e;
	}

	p {
		margin: 0;
		color: #40574a;
	}

	.profile-grid {
		display: grid;
		gap: 0.52rem;
	}

	.profile-grid p {
		margin: 0;
		color: #3f564a;
		padding: 0.58rem 0.64rem;
		border: 1px solid #d9e5df;
		border-radius: 0.8rem;
		background: #f8fbf9;
	}

	@media (min-width: 640px) {
		h1 {
			font-size: 1.22rem;
		}
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

