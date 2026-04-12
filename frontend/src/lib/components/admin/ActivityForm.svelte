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

	type Props = {
		companyId?: string;
		cadasterId?: string | null;
		lockCadaster?: boolean;
		forestStandId?: string | null;
		redirectHref?: string;
		submitLabel?: string;
	};

	let {
		companyId = '',
		cadasterId = null,
		lockCadaster = false,
		forestStandId = null,
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

<form id="activity-form" class="detail-form" onsubmit={submit}>
	<section class="form-section">
		<h2>Tegevuse põhiandmed</h2>
		<div class="form-grid">
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
		</div>
	</section>

	<section class="form-section">
		<h2>Kirjeldus ja märkused</h2>
		<div class="form-grid">
			<label class="full-width">
				<span>Kirjeldus</span>
				<textarea bind:value={description} rows="4" required></textarea>
			</label>
			<label class="full-width">
				<span>Märkused</span>
				<textarea bind:value={notes} rows="4"></textarea>
			</label>
		</div>
	</section>

	<div class="form-actions">
		<button class="btn-save" type="submit" disabled={isSubmitting || isLoadingActivityTypes}>
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

<style>
	.detail-form {
		display: grid;
		gap: 1rem;
	}
	.form-section {
		padding: 1rem;
		border: 1px solid #cadbcf;
		border-radius: 0.85rem;
		background: #f9fcfa;
		box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.9);
	}
	h2 {
		margin: 0 0 0.8rem;
		font-size: 1.03rem;
	}
	.form-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.75rem 1rem;
	}
	label {
		display: flex;
		flex-direction: column;
		gap: 0.35rem;
	}
	.full-width {
		grid-column: 1 / -1;
	}
	.form-actions {
		display: flex;
		justify-content: flex-end;
		gap: 1rem;
	}
	.btn-save {
		padding: 0.62rem 1.1rem;
		background: #1f5a42;
		color: #f8fdfb;
		border: 1px solid #184835;
		font-weight: 700;
		border-radius: 0.65rem;
		box-shadow: 0 8px 16px rgba(31, 90, 66, 0.24);
	}
	.btn-save:hover {
		background: #174a35;
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
