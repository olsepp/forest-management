<script lang="ts">
	import { goto } from '$app/navigation';
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
		cadasterOptions = [],
		cancelHref = '',
		redirectHref = '',
		submitLabel = 'Log activity'
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
				errorMessage = 'Failed to load activity types.';
				activityTypes = [];
				return;
			}

			const data = (await response.json()) as ActivityTypeListDto[];
			activityTypes = Array.isArray(data) ? data : [];
			activityTypeId = activityTypes[0]?.id ?? '';
		} catch {
			errorMessage = 'Failed to load activity types.';
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
			errorMessage = 'Description is required.';
			return;
		}

		if (!activityTypeId) {
			errorMessage = 'Activity type is required.';
			return;
		}

		if (!selectedCadasterId) {
			errorMessage = 'Cadaster is required.';
			return;
		}

		const quantityRaw = String(quantity ?? '').trim();
		const quantityNumber = quantityRaw === '' ? 0 : Number(quantityRaw);
		if (!Number.isFinite(quantityNumber)) {
			errorMessage = 'Quantity must be a valid number.';
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
			forestStandId: null,
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
						? 'Validation failed. Please check required fields.'
						: response.status === 401
							? 'Unauthorized. Please sign in again.'
							: 'Failed to create activity.';
				return;
			}

			successMessage = 'Activity logged successfully.';
			if (redirectHref) {
				await goto(redirectHref);
			}
		} catch {
			errorMessage = 'Failed to create activity.';
		} finally {
			isSubmitting = false;
		}
	}

	onMount(loadActivityTypes);
</script>

<section class="card">
	<h2>New activity</h2>

	<form class="form-grid" onsubmit={submit}>
		<label>
			<span>Cadaster</span>
			{#if lockCadaster}
				<input type="text" value={cadasterLabel || selectedCadasterId} readonly />
			{:else}
				<select bind:value={selectedCadasterId} required>
					<option value="" disabled>Select cadaster</option>
					{#each cadasterOptions as option}
						<option value={option.id}>{option.label}</option>
					{/each}
				</select>
			{/if}
		</label>

		<label>
			<span>Activity type</span>
			<select bind:value={activityTypeId} disabled={isLoadingActivityTypes} required>
				<option value="" disabled>{isLoadingActivityTypes ? 'Loading...' : 'Select activity type'}</option>
				{#each activityTypes as type}
					<option value={type.id}>{type.activityTypeName}</option>
				{/each}
			</select>
		</label>

		<label>
			<span>Date</span>
			<input type="datetime-local" bind:value={date} required />
		</label>

		<label>
			<span>Description</span>
			<textarea bind:value={description} rows="3" required></textarea>
		</label>

		<label>
			<span>Quantity</span>
			<input type="number" step="any" bind:value={quantity} />
		</label>

		<label>
			<span>Unit</span>
			<input type="text" bind:value={unit} placeholder="e.g. m3, ha" />
		</label>

		<label class="notes">
			<span>Notes</span>
			<textarea bind:value={notes} rows="3"></textarea>
		</label>

		<div class="actions">
			{#if cancelHref}
				<a class="ghost" href={cancelHref}>Cancel</a>
			{/if}
			<button type="submit" disabled={isSubmitting || isLoadingActivityTypes}>
				{isSubmitting ? 'Saving...' : submitLabel}
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
