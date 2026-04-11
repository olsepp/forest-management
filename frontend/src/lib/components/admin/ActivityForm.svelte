<script lang="ts">
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type ActivityTypeListDto = {
		id: string;
		activityTypeName: string;
	};

	type CadasterOption = {
		id: string;
		label: string;
	};

	type Props = {
		companyId?: string;
		cadasterId?: string | null;
		cadasterLabel?: string;
		lockCadaster?: boolean;
		forestStandId?: string | null;
		cadasterOptions?: CadasterOption[];
		cancelHref?: string;
		redirectHref?: string;
		submitLabel?: string;
	};

	let {
		companyId = '',
		cadasterId = null,
		cadasterLabel = '',
		lockCadaster = false,
		forestStandId = null,
		cadasterOptions = [],
		cancelHref = '',
		redirectHref = '',
		submitLabel = 'Logi tegevus'
	}: Props = $props();

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isSubmitting = $state(false);
	let isLoadingActivityTypes = $state(true);
	let errorMessage = $state('');
	let successMessage = $state('');

	let activityTypes = $state<ActivityTypeListDto[]>([]);

	let description = $state('');
	let quantity = $state('');
	let unit = $state('');
	let notes = $state('');
	let date = $state(new Date().toISOString().slice(0, 16));
	let activityTypeId = $state('');
	let selectedCadasterId = $state('');
	let applicationStatus = $state('');

	$effect(() => {
		if (lockCadaster && cadasterId) {
			selectedCadasterId = cadasterId;
			return;
		}

		if (!lockCadaster && cadasterId && !selectedCadasterId) {
			selectedCadasterId = cadasterId;
		}
	});

	async function loadActivityTypes() {
		try {
			errorMessage = '';
			isLoadingActivityTypes = true;
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activitytypes`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage = 'Tegevuse tüüpe ei õnnestunud laadida.';
				activityTypes = [];
				return;
			}

			const data = (await response.json()) as ActivityTypeListDto[];
			activityTypes = Array.isArray(data) ? data : [];
			activityTypeId = activityTypes[0]?.id ?? '';
		} catch {
			errorMessage = 'Tegevuse tüüpe ei õnnestunud laadida.';
			activityTypes = [];
		} finally {
			isLoadingActivityTypes = false;
		}
	}

	async function submit(event: SubmitEvent) {
		event.preventDefault();
		if (!companyId) return;

		errorMessage = '';
		successMessage = '';

		if (!description.trim()) {
			errorMessage = 'Kirjeldus on kohustuslik.';
			return;
		}

		if (!activityTypeId) {
			errorMessage = 'Tegevuse tüüp on kohustuslik.';
			return;
		}

		if (!selectedCadasterId) {
			errorMessage = 'Kataster on kohustuslik.';
			return;
		}

		const quantityRaw = String(quantity ?? '').trim();
		const quantityNumber = quantityRaw === '' ? 0 : Number(quantityRaw);
		if (!Number.isFinite(quantityNumber)) {
			errorMessage = 'Kogus peab olema number!';
			return;
		}

		const payload = {
			description: description.trim(),
			quantity: quantityNumber,
			unit: unit.trim() || null,
			notes: notes.trim() || null,
			date: date ? new Date(date).toISOString() : new Date().toISOString(),
			activityTypeId,
			cadasterId: selectedCadasterId,
			forestStandId: forestStandId ?? null,
			applicationStatus: applicationStatus || null
		};

		try {
			isSubmitting = true;
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activities`, {
				method: 'POST',
				headers: {
					Authorization: `Bearer ${token}`,
					'Content-Type': 'application/json'
				},
				body: JSON.stringify(payload)
			});

			if (!response.ok) {
				errorMessage =
					response.status === 400
						? 'Valideerimine ebaõnnestus. Kontrollige kohustuslikke välju.'
						: response.status === 401
							? 'Ligipääs puudub. Logige uuesti sisse.'
							: 'Tegevuse loomine ebaõnnestus.';
				return;
			}

			successMessage = 'Tegevus logiti edukalt.';
			if (redirectHref) {
				await goto(resolve(redirectHref as unknown as '/'));
			}
		} catch {
			errorMessage = 'Tegevuse loomine ebaõnnestus.';
		} finally {
			isSubmitting = false;
		}
	}

	onMount(loadActivityTypes);
</script>

<section class="card">
	<h2>Uus tegevus</h2>

	<form class="form-grid" onsubmit={submit}>
		<label>
			<span>Kataster</span>
			{#if lockCadaster}
				<input type="text" value={cadasterLabel || selectedCadasterId} readonly />
			{:else}
				<select bind:value={selectedCadasterId} required>
					<option value="" disabled>Vali kataster</option>
					{#each cadasterOptions as option (option.id)}
						<option value={option.id}>{option.label}</option>
					{/each}
				</select>
			{/if}
		</label>

		<label>
			<span>Tegevuse tüüp</span>
			<select bind:value={activityTypeId} disabled={isLoadingActivityTypes} required>
				<option value="" disabled
					>{isLoadingActivityTypes ? 'Laadimine...' : 'Vali tegevuse tüüp'}</option
				>
				{#each activityTypes as type (type.id)}
					<option value={type.id}>{type.activityTypeName}</option>
				{/each}
			</select>
		</label>

		<label>
			<span>Kuupäev</span>
			<input type="datetime-local" bind:value={date} required />
		</label>

		<label>
			<span>Kirjeldus</span>
			<textarea bind:value={description} rows="3" required></textarea>
		</label>

		<label>
			<span>Kogus</span>
			<input type="number" step="any" bind:value={quantity} />
		</label>

		<label>
			<span>Ühik</span>
			<input type="text" bind:value={unit} placeholder="nt m3, ha" />
		</label>

		<label>
			<span>Taotluse staatus</span>
			<select bind:value={applicationStatus}>
				<option value=""></option>
				<option value="Pending">Ootel</option>
				<option value="Approved">Kinnitatud</option>
				<option value="Rejected">Tagasi lükatud</option>
			</select>
		</label>

		<label class="notes">
			<span>Märkused</span>
			<textarea bind:value={notes} rows="3"></textarea>
		</label>

		<div class="actions">
			{#if cancelHref}
				<a class="ghost" href={resolve(cancelHref as unknown as '/')}>Tühista</a>
			{/if}
			<button type="submit" disabled={isSubmitting || isLoadingActivityTypes}>
				{isSubmitting ? 'Salvestamine...' : submitLabel}
			</button>
		</div>
	</form>

	{#if errorMessage}
		<p class="error">{errorMessage}</p>
	{/if}

	{#if successMessage}
		<p class="success">{successMessage}</p>
	{/if}
</section>

<style>
	.card {
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.75rem;
		background: #fff;
	}

	.form-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.75rem 1rem;
		margin-top: 1rem;
	}

	label {
		display: flex;
		flex-direction: column;
		gap: 0.3rem;
	}

	label.notes {
		grid-column: 1 / -1;
	}

	input,
	select,
	textarea {
		padding: 0.5rem 0.6rem;
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
		font: inherit;
	}

	.actions {
		grid-column: 1 / -1;
		display: flex;
		justify-content: flex-end;
		gap: 0.5rem;
	}

	button,
	.ghost {
		border: 1px solid #d1d5db;
		background: #fff;
		border-radius: 0.5rem;
		padding: 0.45rem 0.9rem;
		cursor: pointer;
		text-decoration: none;
		color: inherit;
	}

	button:disabled {
		opacity: 0.65;
		cursor: not-allowed;
	}

	.error {
		margin-top: 0.75rem;
		color: #b91c1c;
	}

	.success {
		margin-top: 0.75rem;
		color: #166534;
	}
</style>
