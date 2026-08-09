<script lang="ts">
	import { userService } from '$lib/services/user';
	import ActivityTypeSelect from '$lib/components/admin/ActivityTypeSelect.svelte';
	import Dropdown from '$lib/components/shared/Dropdown.svelte';
	import { workOrderService } from '$lib/services/workorder.service';
	import type { UserListDto } from '$lib/dtos/user/user.dto';
	import type { WorkOrderDto } from '$lib/dtos/workorder/workorder.dto';

	type Props = {
		open: boolean;
		cadasterId: string;
		forestStandId: string | null;
		workOrder: WorkOrderDto | null;
		onclose: () => void;
		oncreated: () => void;
	};

	let {
		open = false,
		cadasterId,
		forestStandId,
		workOrder = null,
		onclose,
		oncreated
	}: Props = $props();

	let isSubmitting = $state(false);
	let isLoadingUsers = $state(true);
	let errorMessage = $state('');
	let successMessage = $state('');

	let users = $state<UserListDto[]>([]);

	let assignedToId = $state('');
	let activityTypeId = $state('');
	let quantity = $state('');
	let unit = $state('');
	let notes = $state('');

	let userOptions = $derived(
		users.map((u) => ({
			value: u.id,
			label: `${u.firstName ?? ''} ${u.lastName ?? ''}`.trim() || u.username || u.email
		}))
	);

	const unitOptions = [
		{ value: 'm3', label: 'm³' },
		{ value: 'ha', label: 'ha' },
		{ value: 'tk', label: 'tk' },
		{ value: 'h', label: 'h' }
	];

	$effect(() => {
		if (open) {
			loadDropdowns();
		}
	});

	async function loadDropdowns() {
		errorMessage = '';
		successMessage = '';
		isLoadingUsers = true;

		try {
			const userList = await userService.getAll();
			users = userList.filter((u) => u.role !== 'Admin');

			if (workOrder) {
				assignedToId = workOrder.assignedToId ?? '';
				activityTypeId = workOrder.activityTypeId ?? '';
				quantity = workOrder.quantity ? String(workOrder.quantity) : '';
				unit = workOrder.unit ?? '';
				notes = workOrder.notes ?? '';
			}
		} catch {
			errorMessage = 'Andmete laadimine ebaõnnestus.';
		} finally {
			isLoadingUsers = false;
		}
	}

	async function submit(event: SubmitEvent) {
		event.preventDefault();

		errorMessage = '';

		if (!assignedToId) {
			errorMessage = 'Töötaja on kohustuslik.';
			return;
		}
		if (!activityTypeId) {
			errorMessage = 'Tegevuse tüüp on kohustuslik.';
			return;
		}

		const quantityNumber = quantity ? Number(quantity) : 0;
		if (!Number.isFinite(quantityNumber)) {
			errorMessage = 'Kogus peab olema number.';
			return;
		}

		try {
			isSubmitting = true;

			const effectiveCadasterId = workOrder?.cadasterId ?? cadasterId;
			const effectiveForestStandId = workOrder?.forestStandId ?? forestStandId;

			if (workOrder) {
				await workOrderService.update(workOrder.id, {
					id: workOrder.id,
					assignedToId,
					activityTypeId,
					cadasterId: effectiveCadasterId,
					forestStandId: effectiveForestStandId,
					quantity: quantityNumber,
					unit: unit || null,
					notes: notes || null
				});
			} else {
				await workOrderService.create({
					assignedToId,
					activityTypeId,
					cadasterId: effectiveCadasterId,
					forestStandId: effectiveForestStandId,
					quantity: quantityNumber,
					unit: unit || null,
					notes: notes || null
				});
			}
			successMessage = workOrder ? 'Töökäsk uuendatud.' : 'Töökäsk saadeti.';
			oncreated();
			if (workOrder) {
				onclose();
			} else {
				assignedToId = '';
				activityTypeId = '';
				quantity = '';
				unit = '';
				notes = '';
			}
		} catch (err) {
			const message = err instanceof Error ? err.message : '';
			errorMessage = message.includes('400')
				? 'Valideerimine ebaõnnestus. Kontrollige välju.'
				: message.includes('401')
					? 'Ligipääs puudub.'
					: (workOrder ? 'Töökäsu uuendamine ebaõnnestus.' : 'Töökäsu loomine ebaõnnestus.');
		} finally {
			isSubmitting = false;
		}
	}

	function handleBackdropClick(event: MouseEvent) {
		if (event.target === event.currentTarget) {
			onclose();
		}
	}

	function handleKeydown(event: KeyboardEvent) {
		if (event.key === 'Escape') {
			onclose();
		}
	}
</script>

<svelte:window onkeydown={handleKeydown} />

{#if open}
	<!-- svelte-ignore a11y_click_events_have_key_events -->
	<!-- svelte-ignore a11y_no_static_element_interactions -->
	<div class="modal-backdrop" onclick={handleBackdropClick}>
		<div class="modal" role="dialog" aria-modal="true" aria-label={workOrder ? 'Muuda töökäsku' : 'Saada töökäsk'}>
			<header class="modal-header">
				<h3>{workOrder ? 'Muuda töökäsku' : 'Saada töökäsk'}</h3>
				<button class="modal-close" onclick={onclose} aria-label="Sulge">&times;</button>
			</header>

			<form class="modal-body" onsubmit={submit}>
				<div class="form-grid">
					<label>
						<span>Töötaja</span>
						<Dropdown
							bind:value={assignedToId}
							options={userOptions}
							disabled={isLoadingUsers || isSubmitting}
							placeholder={isLoadingUsers ? 'Laadimine...' : 'Vali töötaja'}
						/>
					</label>

					<label>
						<span>Tegevuse tüüp</span>
						<ActivityTypeSelect
							bind:value={activityTypeId}
							disabled={isLoadingUsers || isSubmitting}
						/>
					</label>

					<label>
						<span>Kogus</span>
						<input type="number" step="any" bind:value={quantity} disabled={isSubmitting} />
					</label>

					<label>
						<span>Ühik</span>
						<Dropdown
							bind:value={unit}
							options={unitOptions}
							disabled={isSubmitting}
							placeholder="Vali ühik"
						/>
					</label>

					<label class="full-width">
						<span>Märkused</span>
						<textarea bind:value={notes} rows="3" disabled={isSubmitting}></textarea>
					</label>
				</div>

				{#if errorMessage}
					<p class="error">{errorMessage}</p>
				{/if}

				{#if successMessage}
					<p class="success">{successMessage}</p>
				{/if}

				<div class="modal-actions">
					<button type="button" class="btn-cancel" onclick={onclose} disabled={isSubmitting}>
						Tühista
					</button>
					<button
						type="submit"
						class="btn-save"
						style="background:#174834;color:#ffffff;border:1px solid #174834;border-radius:0.65rem;"
						disabled={isSubmitting || isLoadingUsers}
					>
						{isSubmitting ? (workOrder ? 'Salvestamine...' : 'Saatmine...') : (workOrder ? 'Salvesta' : 'Saada')}
					</button>
				</div>
			</form>
		</div>
	</div>
{/if}

<style>
	.modal-backdrop {
		position: fixed;
		inset: 0;
		background: rgba(0, 0, 0, 0.35);
		display: flex;
		align-items: center;
		justify-content: center;
		z-index: 1000;
		backdrop-filter: blur(2px);
	}
	.modal {
		background: #fff;
		border-radius: 0.85rem;
		box-shadow: 0 12px 40px rgba(0, 0, 0, 0.18);
		max-width: 540px;
		width: 92%;
		max-height: 90vh;
		overflow-y: auto;
	}
	.modal-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 1rem 1.25rem;
	}
	.modal-header h3 {
		margin: 0;
		font-size: 1.1rem;
	}
	.modal-close {
		background: none;
		border: none;
		width: 2rem;
		height: 2rem;
		display: flex;
		align-items: center;
		justify-content: center;
		border-radius: 0.4rem;
		font-size: 1.3rem;
		cursor: pointer;
		line-height: 1;
		color: #666;
	}
	.modal-close:hover {
		background: #e5e5e5;
	}
	.modal-body {
		padding: 1.25rem;
	}
	.form-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
		gap: 0.75rem 1rem;
	}
	.full-width {
		grid-column: 1 / -1;
	}
	label {
		display: flex;
		flex-direction: column;
		gap: 0.3rem;
		font-size: 0.9rem;
	}
	label span {
		font-weight: 600;
	}
	input,
	textarea {
		padding: 0.45rem 0.6rem;
		border: 1px solid #cadbcf;
		border-radius: 0.55rem;
		font-size: 0.9rem;
		font-family: inherit;
		background: #f9fcfa;
	}
	input:disabled,
	textarea:disabled {
		opacity: 0.6;
	}
	.modal-actions {
		display: flex;
		justify-content: flex-end;
		gap: 0.75rem;
		margin-top: 1rem;
		padding-top: 1rem;
		border-top: 1px solid #cadbcf;
	}
	.btn-save {
		padding: 0.5rem 1.2rem;
		font-weight: 700;
		cursor: pointer;
	}
	.btn-cancel {
		padding: 0.5rem 1.2rem;
		background: #fff !important;
		color: #333 !important;
		border: 1px solid #cadbcf !important;
		border-radius: 0.65rem !important;
		cursor: pointer;
	}
	.error {
		margin: 0.75rem 0 0;
		color: #b91c1c;
		font-size: 0.88rem;
	}
	.success {
		margin: 0.75rem 0 0;
		color: #166534;
		font-size: 0.88rem;
	}

	:global(.btn-save):hover:not(:disabled) {
		background: #235c44 !important;
		border-color: #235c44 !important;
	}
	:global(.btn-save):disabled {
		opacity: 0.6;
	}
	:global(.btn-cancel):hover:not(:disabled) {
		background: #f3f3f3 !important;
	}
</style>
