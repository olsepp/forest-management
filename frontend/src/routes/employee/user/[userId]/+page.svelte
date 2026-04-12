<script lang="ts">
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';
	import type { UserProfileDto } from '$lib/dtos/user/user.dto';

	type ProfileViewModel = {
		username: string;
		email: string;
		firstName: string;
		lastName: string;
		role: string;
		phoneNumber: string;
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

	function toProfileViewModel(data: UserProfileDto): ProfileViewModel {

		return {
			username: valueOrDash(data.username),
			email: valueOrDash(data.email),
			firstName: valueOrDash(data.firstName),
			lastName: valueOrDash(data.lastName),
			role: valueOrDash(data.role),
			phoneNumber: valueOrDash(data.phoneNumber)
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
				errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
				return;
			}

			const data = (await response.json()) as UserProfileDto;
			profile = toProfileViewModel(data);
		} catch {
			errorMessage = 'Profiili laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadProfile);
</script>

<p class="employee-back-link">
	<a class="employee-back-link-button" href={resolve('/employee')}>
		<span aria-hidden="true">←</span>
		<span>Tagasi töötaja avalehele</span>
	</a>
</p>

<section class="employee-card summary">
	<p class="kicker">Kasutajaprofiil</p>
	<h1 class="employee-page-title">Sinu konto</h1>
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
		<p><strong>Kasutajanimi:</strong> {profile.username}</p>
		<p><strong>Email:</strong> {profile.email}</p>
		<p><strong>Eesnimi:</strong> {profile.firstName}</p>
		<p><strong>Perekonnanimi:</strong> {profile.lastName}</p>
		<p><strong>Roll:</strong> {profile.role}</p>
		<p><strong>Telefon:</strong> {profile.phoneNumber}</p>
	</section>
{/if}

<style>
	.summary {
		margin-bottom: 0.75rem;
		padding: 1rem;
		background: linear-gradient(180deg, #ffffff 0%, #f5f8fc 100%);
		border-color: #d3dde8;
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
		margin: 0 0 0.4rem;
		font-size: 1.2rem;
		line-height: 1.2;
		color: #0f172a;
	}

	p {
		margin: 0;
		color: #334155;
	}

	.profile-grid {
		display: grid;
		gap: 0.52rem;
	}

	.profile-grid p {
		margin: 0;
		color: #334155;
		padding: 0.58rem 0.64rem;
		border: 1px solid #d7e0ea;
		border-radius: 0.8rem;
		background: #f8fbff;
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
