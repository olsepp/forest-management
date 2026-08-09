<script lang="ts">
	import { page } from '$app/stores';
	import { onMount } from 'svelte';
	import { workOrderService } from '$lib/services/workorder.service';
	import { formatUserName } from '$lib/utils/format-user';
	import type { WorkOrderDto } from '$lib/dtos/workorder/workorder.dto';
	import WorkOrderModal from '$lib/components/admin/WorkOrderModal.svelte';

	const companyId = $derived($page.params.CompanyId ?? '');

	let workOrders = $state<WorkOrderDto[]>([]);
	let isLoading = $state(true);
	let errorMessage = $state('');

	let statusFilter = $state('');
	let isDeleting = $state(false);
	let deleteTargetId = $state<string | null>(null);
	let isDeleteSubmitting = $state(false);
	let showWorkOrderModal = $state(false);
	let editingWorkOrder = $state<WorkOrderDto | null>(null);

	let filteredOrders = $derived(
		statusFilter
			? workOrders.filter((w) => w.status.toString() === statusFilter)
			: workOrders
	);

	function formatDate(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleString('et-EE', {
			day: '2-digit',
			month: '2-digit',
			year: 'numeric',
			hour: '2-digit',
			minute: '2-digit'
		});
	}

	function formatQuantity(quantity: number, unit: string | null): string {
		const q = Number.isFinite(quantity) ? quantity : 0;
		return unit ? `${q} ${unit}` : String(q);
	}

	function statusLabel(status: string): string {
		return status === 'Sent' ? 'Saadetud' : 'Tehtud';
	}

	function statusClass(status: string): string {
		return status === 'Sent' ? 'status-sent' : 'status-completed';
	}

	function forestStandLabel(item: WorkOrderDto): string {
		if (item.forestStandNumber != null && item.forestStandNumber > 0) {
			return String(item.forestStandNumber);
		}
		return '—';
	}

	function startDelete(id: string) {
		deleteTargetId = id;
		isDeleting = true;
		errorMessage = '';
	}

	function cancelDelete() {
		isDeleting = false;
		deleteTargetId = null;
	}

	async function confirmDelete() {
		if (!deleteTargetId) return;
		isDeleteSubmitting = true;
		errorMessage = '';
		try {
			await workOrderService.delete(deleteTargetId);
			workOrders = workOrders.filter((w) => w.id !== deleteTargetId);
			isDeleting = false;
			deleteTargetId = null;
		} catch {
			errorMessage = 'Kustutamine ebaõnnestus.';
		} finally {
			isDeleteSubmitting = false;
		}
	}

	onMount(async () => {
		try {
			workOrders = await workOrderService.getByCompany(companyId);
		} catch {
			errorMessage = 'Töökäskude laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	});
</script>

<h1>Töökäsud</h1>

{#if errorMessage}
	<p class="error">{errorMessage}</p>
{/if}

<div class="filter-bar">
	<select bind:value={statusFilter}>
		<option value="">Kõik staatused</option>
		<option value="Sent">Saadetud</option>
		<option value="Completed">Tehtud</option>
	</select>
</div>

{#if isLoading}
	<p>Laadimine...</p>
{:else if workOrders.length === 0}
	<p>Töökäske ei leitud.</p>
{:else}
	<div class="table-wrapper">
		<table>
			<thead>
				<tr>
					<th>Töötaja</th>
					<th>Tüüp</th>
					<th>Kataster</th>
					<th>Eraldis</th>
					<th>Kogus</th>
					<th>Staatus</th>
					<th>Loodud</th>
					<th class="actions"></th>
				</tr>
			</thead>
			<tbody>
				{#each filteredOrders as item (item.id)}
					<tr>
						<td>{formatUserName({ userFirstName: item.assignedToUserFirstName, userLastName: item.assignedToUserLastName, userName: item.assignedToUserName })}</td>
						<td>{item.activityTypeName}</td>
						<td>{item.cadasterCadastralNumber}</td>
						<td>{forestStandLabel(item)}</td>
						<td>{formatQuantity(item.quantity, item.unit)}</td>
						<td><span class={statusClass(item.status)}>{statusLabel(item.status)}</span></td>
						<td>{formatDate(item.createdAt)}</td>
						<td class="actions">
							<button
								class="btn-edit"
								onclick={() => {
									editingWorkOrder = item;
									showWorkOrderModal = true;
								}}
								disabled={isDeleteSubmitting}
							>
								Muuda
							</button>
							<button
								class="btn-delete"
								onclick={() => startDelete(item.id)}
								disabled={isDeleteSubmitting}
							>
								Kustuta
							</button>
						</td>
					</tr>
				{/each}
			</tbody>
		</table>
	</div>
{/if}

{#if isDeleting}
	<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
		<div class="w-full max-w-md rounded-xl border-2 border-red-500 bg-red-50 p-6 shadow-xl">
			<h2 class="mb-4 text-xl font-semibold text-red-800">Kustuta töökäsk</h2>
			<p class="mb-6 text-base text-red-700">
				Kas olete kindel, et soovite selle töökäsu kustutada? Seda toimingut ei saa tagasi võtta.
			</p>
			<div class="flex gap-3">
				<button
					type="button"
					onclick={confirmDelete}
					disabled={isDeleteSubmitting}
					class="!cursor-pointer !rounded-lg !border !border-red-600 !bg-red-600 !px-4 !py-2 !text-sm !font-semibold !text-white hover:!bg-red-700 disabled:!opacity-60"
				>
					{isDeleteSubmitting ? 'Kustutamisel...' : 'Kustuta'}
				</button>
				<button
					type="button"
					onclick={cancelDelete}
					class="!cursor-pointer !rounded-lg !border !border-[#174834] !bg-[#174834] !px-4 !py-2 !text-sm !font-semibold !text-white hover:!bg-[#235c44]"
				>
					Tühista
				</button>
			</div>
		</div>
	</div>
{/if}

<style>
	.filter-bar {
		margin-bottom: 1rem;
	}

	.filter-bar select {
		padding: 0.5rem 0.75rem;
		border: 1px solid #e2e8f0;
		border-radius: 0.5rem;
		font-size: 0.95rem;
		background: #fff;
		min-width: 180px;
	}

	.table-wrapper {
		overflow-x: auto;
	}

	table {
		width: 100%;
		border-collapse: collapse;
		background: #fff;
	}

	th,
	td {
		padding: 0.75rem;
		border-bottom: 1px solid #e5e7eb;
		text-align: left;
		vertical-align: middle;
	}

	th.actions,
	td.actions {
		text-align: center;
		width: 10rem;
		white-space: nowrap;
	}

	.status-sent {
		display: inline-block;
		padding: 0.15rem 0.55rem;
		border-radius: 0.4rem;
		background: #fef3c7;
		color: #92400e;
		font-size: 0.85rem;
		font-weight: 600;
	}

	.status-completed {
		display: inline-block;
		padding: 0.15rem 0.55rem;
		border-radius: 0.4rem;
		background: #d1fae5;
		color: #065f46;
		font-size: 0.85rem;
		font-weight: 600;
	}

	.btn-edit {
		padding: 0.3rem 0.7rem;
		border: 1px solid #174834 !important;
		border-radius: 0.5rem;
		background: #174834 !important;
		color: #fff !important;
		font-size: 0.85rem;
		font-weight: 600;
		cursor: pointer;
		font-family: inherit;
	}
	.btn-edit:hover:not(:disabled) {
		background: #235c44 !important;
	}
	.btn-edit:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.btn-delete {
		padding: 0.3rem 0.7rem;
		border: 1px solid #dc2626 !important;
		border-radius: 0.5rem;
		background: #dc2626 !important;
		color: #fff !important;
		font-size: 0.85rem;
		font-weight: 600;
		cursor: pointer;
		font-family: inherit;
	}
	.btn-delete:hover:not(:disabled) {
		background: #b91c1c !important;
	}
	.btn-delete:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.error {
		color: #b91c1c;
		margin-bottom: 0.75rem;
	}
</style>
<WorkOrderModal
	open={showWorkOrderModal}
	cadasterId={editingWorkOrder?.cadasterId ?? ''}
	forestStandId={editingWorkOrder?.forestStandId ?? null}
	workOrder={editingWorkOrder}
	onclose={() => {
		showWorkOrderModal = false;
		editingWorkOrder = null;
	}}
	oncreated={async () => {
		workOrders = await workOrderService.getByCompany(companyId);
	}}
/>
