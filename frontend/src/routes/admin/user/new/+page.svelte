<script lang="ts">
	import { resolve } from '$app/paths';
	import { authService } from '$lib/services/auth';

	let username = $state('');
	let email = $state('');
	let firstName = $state('');
	let lastName = $state('');
	let role = $state('Employee');
	let password = $state('');
	let showPassword = $state(false);

	let isSubmitting = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');

	function validatePassword(pw: string): string | null {
		if (pw.length < 6) return 'Parool peab olema vähemalt 6 märki pikk.';
		if (!/[A-Z]/.test(pw)) return 'Parool peab sisaldama vähemalt 1 suurtähte.';
		if (!/[0-9]/.test(pw)) return 'Parool peab sisaldama vähemalt 1 numbrit.';
		if (!/[^A-Za-z0-9]/.test(pw)) return 'Parool peab sisaldama vähemalt 1 sümbolit.';
		return null;
	}

	function validateUsername(uname: string): string | null {
		if (!uname.trim()) return 'Kasutajanimi on kohustuslik.';
		if (uname.trim().length < 6) return 'Kasutajanimi peab olema vähemalt 6 märki pikk.';
		if (!/^[A-Za-z0-9]+$/.test(uname.trim())) return 'Kasutajanimi võib sisaldada ainult tähti ja numbreid. Ei tohi sisaldada täpitähti ega sümboleid.';
		return null;
	}

	async function createUser(event: SubmitEvent) {
		event.preventDefault();
		errorMessage = '';
		successMessage = '';

		const usernameError = validateUsername(username);
		if (usernameError) {
			errorMessage = usernameError;
			return;
		}

		const passwordError = validatePassword(password);
		if (passwordError) {
			errorMessage = passwordError;
			return;
		}

		isSubmitting = true;

		try {
			const token = await authService.ensureValidToken();

			const response = await fetch(`/api/users`, {
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
				successMessage = 'Kasutajakonto loodi edukalt.';
				username = '';
				email = '';
				firstName = '';
				lastName = '';
				role = 'Employee';
				password = '';
				return;
			}

			if (response.status === 401) {
				errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
				return;
			}

			if (response.status === 403) {
				errorMessage = 'Keelatud. Vajalik on admini roll.';
				return;
			}

			if (response.status === 400) {
				const data = (await response.json().catch(() => null)) as { message?: string } | null;
				errorMessage = data?.message || 'Valideerimine ebaõnnestus. Kasutajanimi või e-post võib juba olemas olla.';
				return;
			}

			errorMessage = 'Kasutajakonto loomine ebaõnnestus.';
		} catch {
			errorMessage = 'Võrguviga. Proovige uuesti.';
		} finally {
			isSubmitting = false;
		}
	}
</script>

<div class="mb-4 flex items-center justify-between gap-3">
	<h1 class="text-2xl font-semibold text-slate-900">Loo kasutajakonto</h1>
	<a
		href={resolve('/admin/user')}
		class="inline-flex items-center rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm font-medium text-slate-700 hover:bg-slate-50"
	>
		Tagasi kasutajate juurde
	</a>
</div>

{#if errorMessage}
	<p class="mb-4 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{errorMessage}</p>
{/if}

{#if successMessage}
	<p class="mb-4 rounded-lg border border-green-200 bg-green-50 px-3 py-2 text-sm text-green-700">{successMessage}</p>
{/if}

<form onsubmit={createUser} class="max-w-2xl space-y-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
	<div class="grid grid-cols-1 gap-4 sm:grid-cols-2">
		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Kasutajanimi</span>
			<input bind:value={username} required class="rounded-lg border border-slate-300 px-3 py-2" />
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Email</span>
			<input bind:value={email} type="email" required class="rounded-lg border border-slate-300 px-3 py-2" />
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Eesnimi</span>
			<input bind:value={firstName} required class="rounded-lg border border-slate-300 px-3 py-2" />
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Perekonnanimi</span>
			<input bind:value={lastName} required class="rounded-lg border border-slate-300 px-3 py-2" />
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Roll</span>
			<select bind:value={role} class="rounded-lg border border-slate-300 px-3 py-2">
				<option value="Employee">Töötaja</option>
				<option value="Admin">Admin</option>
			</select>
		</label>

		<label class="flex flex-col gap-1 text-sm">
			<span class="font-medium text-slate-700">Parool</span>
			<div class="relative">
				<input
					bind:value={password}
					type={showPassword ? 'text' : 'password'}
					minlength="6"
					required
					class="w-full rounded-lg border border-slate-300 px-3 py-2 pr-10"
				/>
				<button
					type="button"
					onclick={() => (showPassword = !showPassword)}
					class="absolute right-0 top-0 flex h-full w-10 items-center justify-center text-slate-500 hover:text-slate-700"
					aria-label={showPassword ? 'Peida parool' : 'Näita parooli'}
				>
					{#if showPassword}
						<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="h-5 w-5">
							<path d="M10.733 5.076a10.744 10.744 0 0 1 11.205 6.575 1 1 0 0 1 0 .696 10.747 10.747 0 0 1-1.444 2.49"/>
							<path d="M14.084 14.158a3 3 0 0 1-4.242-4.242"/>
							<path d="M17.479 17.499a10.75 10.75 0 0 1-15.417-5.151 1 1 0 0 1 0-.696 10.75 10.75 0 0 1 4.446-5.143"/>
							<path d="M2 2l20 20"/>
						</svg>
					{:else}
						<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" class="h-5 w-5">
							<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/>
							<circle cx="12" cy="12" r="3"/>
						</svg>
					{/if}
				</button>
			</div>
			<span class="text-xs text-slate-500">Parool peab sisaldama vähemalt 1 sümbolit, 1 suurtähte ja 1 numbrit.</span>
		</label>
	</div>

	<div>
		<button
			type="submit"
			disabled={isSubmitting}
			class="inline-flex items-center rounded-lg bg-emerald-600 px-4 py-2 text-sm font-semibold text-white hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-60"
		>
			{isSubmitting ? 'Loomine...' : 'Loo konto'}
		</button>
	</div>
</form>
