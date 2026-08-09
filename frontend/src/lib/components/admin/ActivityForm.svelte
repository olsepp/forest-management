<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import ActivityTypeSelect from '$lib/components/admin/ActivityTypeSelect.svelte';
	import Dropdown from '$lib/components/shared/Dropdown.svelte';
	import DatePicker from '$lib/components/DatePicker.svelte';
	import { activityService } from '$lib/services/activity';
	import { userService } from '$lib/services/user';
	import { user } from '$lib/stores/auth.store';
	import type { ActivityStatus } from '$lib/dtos/activity/activity.dto';

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

	let isSubmitting = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');

	let description = $state('');
	let quantity = $state('');
	let unit = $state('');
	let notes = $state('');
	let date = $state(new Date().toISOString().slice(0, 10));
	let activityTypeId = $state('');
	let selectedCadasterId = $state('');
	let applicationStatus = $state('');

	let userId = $state('');
	let userOptions = $state<{ value: string; label: string }[]>([]);
	let isLoadingUsers = $state(false);

	let isAdmin = $derived(($user?.role ?? '').toLowerCase() === 'admin');

	onMount(async () => {
		if (!isAdmin) return;
		isLoadingUsers = true;
		try {
			const users = await userService.getAll();
			userOptions = users.map((u) => ({
				value: u.id,
				label:
					`${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() ||
					u.username ||
					u.email
			}));
		} catch {
			// leave userOptions empty; dropdown shows placeholder
		} finally {
			isLoadingUsers = false;
		}
	});

	$effect(() => {
		if (isAdmin && $user && !userId) {
			userId = $user.userId;
		}
	});

	const unitOptions = [
		{ value: 'm3', label: 'm3' },
		{ value: 'ha', label: 'ha' },
		{ value: 'tk', label: 'tk' },
		{ value: 'h', label: 'h' }
	];

	const statusOptions = [
		{ value: 'Pending', label: 'Ootel' },
		{ value: 'Approved', label: 'Kinnitatud' },
		{ value: 'Rejected', label: 'Tagasi lükatud' }
	];

	$effect(() => {
		if (lockCadaster && cadasterId) {
			selectedCadasterId = cadasterId;
			return;
		}

		if (!lockCadaster && cadasterId && !selectedCadasterId) {
			selectedCadasterId = cadasterId;
		}
	});

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
			applicationStatus: (applicationStatus || null) as ActivityStatus | null,
			...(isAdmin && userId ? { userId } : {})
		};

		try {
			isSubmitting = true;
			await activityService.create(payload);
			successMessage = 'Tegevus logiti edukalt.';
			if (redirectHref) {
				await goto(resolve(redirectHref as unknown as '/'));
			}
		} catch (err) {
			const message = err instanceof Error ? err.message : '';
			errorMessage = message.includes('400')
				? 'Valideerimine ebaõnnestus. Kontrollige kohustuslikke välju.'
				: message.includes('401')
					? 'Ligipääs puudub. Logige uuesti sisse.'
					: 'Tegevuse loomine ebaõnnestus.';
			return;
		} finally {
			isSubmitting = false;
		}
	}

</script>

<form id="activity-form" class="detail-form" onsubmit={submit}>
	<section class="form-section">
		<h2>Tegevuse põhiandmed</h2>
		<div class="form-grid">
			<label>
				<span>Tegevuse tüüp</span>
				<ActivityTypeSelect bind:value={activityTypeId} />
			</label>

			<label>
				<DatePicker label="Kuupäev" bind:value={date} placeholder="Vali kuupäev" />
			</label>

			<label>
				<span>Kogus</span>
				<input type="number" step="any" bind:value={quantity} />
			</label>

		<label>
			<span>Ühik</span>
			<Dropdown bind:value={unit} options={unitOptions} placeholder="Vali ühik" />
		</label>

		<label>
			<span>Taotluse staatus</span>
			<Dropdown bind:value={applicationStatus} options={statusOptions} placeholder="—" />
		</label>
		</div>
	</section>

	{#if isAdmin}
		<section class="form-section">
			<h2>Logija</h2>
			<div class="form-grid">
				<label>
					<span>Logis kasutaja</span>
					<Dropdown
						bind:value={userId}
						options={userOptions}
						placeholder={isLoadingUsers ? 'Laadimine...' : 'Vali kasutaja'}
					/>
				</label>
			</div>
		</section>
	{/if}

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
		<button class="btn-save" type="submit" disabled={isSubmitting}>
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
