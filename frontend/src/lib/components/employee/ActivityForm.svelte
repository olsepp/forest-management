<script lang="ts">
	import { resolve } from '$app/paths';
	import { goto } from '$app/navigation';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { toastStore } from '$lib/stores/toast.store';
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

	let activityTypes = $state<ActivityTypeListDto[]>([]);

	let description = $state('');
	let quantity = $state('');
	let unit = $state('');
	let notes = $state('');
	let date = $state(new Date().toISOString().slice(0, 16));
	let activityTypeId = $state('');
	let selectedCadasterId = $state('');

	async function loadActivityTypes() {
		try {
			errorMessage = '';
			isLoadingActivityTypes = true;
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activitytypes`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!response.ok) {
				errorMessage = 'Tegevuste tüüpe ei õnnestunud laadida.';
				activityTypes = [];
				return;
			}

			const data = (await response.json()) as ActivityTypeListDto[];
			activityTypes = Array.isArray(data) ? data : [];
			activityTypeId = activityTypes[0]?.id ?? '';
		} catch {
			errorMessage = 'Tegevuste tüüpe ei õnnestunud laadida.';
			activityTypes = [];
		} finally {
			isLoadingActivityTypes = false;
		}
	}

	async function submit(event: SubmitEvent) {
		event.preventDefault();
		if (!companyId) return;

		errorMessage = '';

		if (!description.trim()) {
			errorMessage = 'Kirjeldus on kohustuslik.';
			toastStore.showToast(errorMessage, 'error');
			return;
		}

		if (!activityTypeId) {
			errorMessage = 'Tegevuse tüüp on kohustuslik.';
			toastStore.showToast(errorMessage, 'error');
			return;
		}

		if (!selectedCadasterId) {
			errorMessage = 'Kataster on kohustuslik.';
			toastStore.showToast(errorMessage, 'error');
			return;
		}

		const quantityRaw = String(quantity ?? '').trim();
		const quantityNumber = quantityRaw === '' ? 0 : Number(quantityRaw);
		if (!Number.isFinite(quantityNumber)) {
			errorMessage = 'Kogus peab olema korrektne number.';
			toastStore.showToast(errorMessage, 'error');
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
			applicationStatus: null
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
				toastStore.showToast(errorMessage, 'error');
				return;
			}

			toastStore.showToast('Tegevus logiti edukalt.', 'success');

			if (redirectHref) {
				await goto(resolve(redirectHref as unknown as '/'));
			}
		} catch {
			errorMessage = 'Tegevuse loomine ebaõnnestus.';
			toastStore.showToast(errorMessage, 'error');
		} finally {
			isSubmitting = false;
		}
	}

	onMount(() => {
		if (cadasterId) {
			selectedCadasterId = cadasterId;
		}

		loadActivityTypes();
	});
</script>

<section class="employee-card activity-form-card">
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
				<option value="" disabled>{isLoadingActivityTypes ? 'Laadimine...' : 'Vali tegevuse tüüp'}</option>
				{#each activityTypes as type (type.id)}
					<option value={type.id}>{type.activityTypeName}</option>
				{/each}
			</select>
		</label>

		<label>
			<span>Kuupäev</span>
			<input type="datetime-local" bind:value={date} required />
		</label>

		<label class="full-width">
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

		<label class="full-width">
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
		<div class="employee-state-block is-error">{errorMessage}</div>
	{/if}
</section>

<style>
	.activity-form-card h2 {
		margin: 0 0 0.75rem;
		font-size: 1.05rem;
		color: #1a3228;
	}

	.form-grid {
		display: grid;
		grid-template-columns: 1fr;
		gap: 0.75rem;
	}

	label {
		display: flex;
		flex-direction: column;
		gap: 0.3rem;
	}

	label span {
		font-size: 0.86rem;
		font-weight: 700;
		color: #30483d;
	}

	input,
	select,
	textarea {
		padding: 0.6rem 0.65rem;
		border: 1px solid #d1dcd6;
		border-radius: 0.6rem;
		font: inherit;
		background: #fff;
	}

	textarea {
		resize: vertical;
	}

	.actions {
		display: flex;
		justify-content: flex-end;
		gap: 0.55rem;
	}

	button,
	.ghost {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 2.5rem;
		border-radius: 0.65rem;
		padding: 0.5rem 0.95rem;
		font-size: 0.9rem;
		font-weight: 700;
		text-decoration: none;
		cursor: pointer;
	}

	button {
		border: 1px solid #1f5a42;
		background: #1f5a42;
		color: #f6fbf8;
	}

	.ghost {
		border: 1px solid #cad8d1;
		background: #fff;
		color: #1f4f39;
	}

	button:disabled {
		opacity: 0.65;
		cursor: not-allowed;
	}

	@media (min-width: 768px) {
		.form-grid {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}

		.full-width,
		.actions {
			grid-column: 1 / -1;
		}
	}
</style>
