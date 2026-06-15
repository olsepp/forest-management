<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { onMount } from 'svelte';
	import { workOrderService } from '$lib/services/workorder.service';
	import type { WorkOrderListDto } from '$lib/dtos/workorder/workorder.dto';

	const companyId = $derived($page.params.CompanyId ?? '');

	let workOrders = $state<WorkOrderListDto[]>([]);
	let isLoading = $state(true);
	let errorMessage = $state('');

	let completingId = $state<string | null>(null);
	let revertingId = $state<string | null>(null);
	let successMessage = $state('');

	let statusFilter = $state('');

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

	function statusLabel(status: number): string {
		return status === 0 ? 'Saadetud' : 'Tehtud';
	}

	function statusClass(status: number): string {
		return status === 0 ? 'status-sent' : 'status-completed';
	}

	function detailLink(item: WorkOrderListDto): string {
		if (item.forestStandId && item.forestStandNumber != null) {
			return resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
				CompanyId: companyId,
				ForestStandId: item.forestStandId
			});
		}
		if (item.cadasterId) {
			return resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
				CompanyId: companyId,
				CadasterId: item.cadasterId
			});
		}
		return '#';
	}

	function forestStandLabel(item: WorkOrderListDto): string {
		if (item.forestStandNumber != null && item.forestStandNumber > 0) {
			return String(item.forestStandNumber);
		}
		return '—';
	}

	async function handleComplete(id: string) {
		completingId = id;
		errorMessage = '';
		successMessage = '';
		try {
			await workOrderService.complete(id);
			workOrders = workOrders.map((w) =>
				w.id === id ? { ...w, status: 1 as const } : w
			);
			successMessage = 'Töökäsk märgiti tehtuks.';
			setTimeout(() => (successMessage = ''), 3000);
		} catch {
			errorMessage = 'Töökäsu lõpetamine ebaõnnestus.';
		} finally {
			completingId = null;
		}
	}

	async function handleRevert(id: string) {
		revertingId = id;
		errorMessage = '';
		successMessage = '';
		try {
			await workOrderService.revert(id);
			workOrders = workOrders.map((w) =>
				w.id === id ? { ...w, status: 0 as const } : w
			);
			successMessage = 'Töökäsk võeti tagasi.';
			setTimeout(() => (successMessage = ''), 3000);
		} catch {
			errorMessage = 'Töökäsu tagasivõtmine ebaõnnestus.';
		} finally {
			revertingId = null;
		}
	}

	onMount(async () => {
		try {
			workOrders = await workOrderService.getMyByCompany(companyId);
		} catch {
			errorMessage = 'Töökäskude laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	});
</script>

<section class="employee-card summary">
	<h1>TÖÖKÄSUD</h1>
	<p>Sulle määratud töökäsud selles ettevõttes.</p>
</section>

{#if errorMessage}
	<div class="employee-state-block is-error">{errorMessage}</div>
{/if}

{#if successMessage}
	<div class="employee-state-block is-success">{successMessage}</div>
{/if}

<div class="filter-bar">
	<select bind:value={statusFilter}>
		<option value="">Kõik staatused</option>
		<option value="0">Saadetud</option>
		<option value="1">Tehtud</option>
	</select>
</div>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse töökäske…</div>
{:else if workOrders.length === 0}
	<div class="employee-state-block is-empty">Sulle ei ole töökäske määratud.</div>
{:else}
	<section class="employee-card">
		<div class="employee-stack-cards activities-mobile">
			{#each filteredOrders as item (item.id)}
				<article class="activity-card">
					<p class="activity-head">
						<strong>{item.activityTypeName}</strong>
						<span class={statusClass(item.status)}>{statusLabel(item.status)}</span>
					</p>
					<p><strong>Kataster:</strong> {item.cadasterCadastralNumber}</p>
					<p><strong>Eraldis:</strong> {forestStandLabel(item)}</p>
					<p><strong>Kogus:</strong> {formatQuantity(item.quantity, item.unit)}</p>
					<p class="activity-date">{formatDate(item.createdAt)}</p>
					{#if item.status === 0}
						<div class="card-actions">
							<button
								class="activity-link"
								onclick={() => handleComplete(item.id)}
								disabled={completingId === item.id}
							>
								{completingId === item.id ? '...' : 'Märgi tehtuks'}
							</button>
							<a class="btn-log-activity" href={detailLink(item)} data-sveltekit-preload-data="tap">
								Logi tegevus
							</a>
						</div>
					{:else if item.status === 1}
						<div class="card-actions">
							<button
								class="activity-link revert"
								onclick={() => handleRevert(item.id)}
								disabled={revertingId === item.id}
							>
								{revertingId === item.id ? '...' : 'Tagasi'}
							</button>
						</div>
					{/if}
				</article>
			{/each}
		</div>

		<div class="employee-table-wrap activities-table">
			<table>
				<thead>
					<tr>
						<th>Tüüp</th>
						<th>Kataster</th>
						<th>Eraldis</th>
						<th>Kogus</th>
						<th>Staatus</th>
						<th>Loodud</th>
						<th>Tegevus</th>
					</tr>
				</thead>
				<tbody>
					{#each filteredOrders as item (item.id)}
						<tr>
							<td>{item.activityTypeName}</td>
							<td>{item.cadasterCadastralNumber}</td>
							<td>{forestStandLabel(item)}</td>
							<td>{formatQuantity(item.quantity, item.unit)}</td>
							<td><span class={statusClass(item.status)}>{statusLabel(item.status)}</span></td>
							<td>{formatDate(item.createdAt)}</td>
							<td class="actions-cell">
								{#if item.status === 0}
									<a class="btn-log-table" href={detailLink(item)} data-sveltekit-preload-data="tap">
										Logi tegevus
									</a>
									<button
										class="btn-complete"
										onclick={() => handleComplete(item.id)}
										disabled={completingId === item.id}
									>
										{completingId === item.id ? '...' : 'Märgi tehtuks'}
									</button>
								{:else if item.status === 1}
									<button
										class="btn-revert"
										onclick={() => handleRevert(item.id)}
										disabled={revertingId === item.id}
									>
										{revertingId === item.id ? '...' : 'Tagasi'}
									</button>
								{/if}
							</td>
						</tr>
					{/each}
				</tbody>
			</table>
		</div>
	</section>
{/if}

<style>
	.summary {
		margin-bottom: 0.75rem;
	}

	h1 {
		margin: 0;
		font-size: 1.2rem;
		line-height: 1.2;
		color: #17251e;
		text-transform: uppercase;
		letter-spacing: 0.03em;
	}

	p {
		margin: 0.4rem 0 0;
		color: #334155;
	}

	.filter-bar {
		margin-bottom: 0.75rem;
	}

	.filter-bar select {
		padding: 0.5rem 0.75rem;
		border: 1px solid #d8e0dc;
		border-radius: 0.5rem;
		font-size: 0.95rem;
		background: #fff;
		min-width: 180px;
	}

	.activities-table {
		display: none !important;
	}

	.activity-card {
		border: 1px solid #d8e0dc;
		border-radius: 0.8rem;
		padding: 0.9rem;
		background: #ffffff;
		display: grid;
		gap: 0.42rem;
	}

	.activity-card p {
		margin: 0;
		color: #334155;
	}

	.activity-date {
		font-size: 0.85rem;
		color: #64748b;
	}

	.activity-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		align-self: start;
		min-height: 3rem;
		margin-top: 0.2rem;
		padding: 0.5rem 0.9rem;
		border: 1px solid #1f5a42;
		border-radius: 0.82rem;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		box-shadow: 0 6px 16px rgba(15, 42, 31, 0.22);
		font-size: 0.95rem;
		font-weight: 700;
		color: #f3fbf7;
		cursor: pointer;
		font-family: inherit;
	}

	.activity-link:hover:not(:disabled) {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
	}

	.activity-link:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.activity-link.revert {
		border-color: #b45309;
		background: linear-gradient(180deg, #d97706 0%, #b45309 100%);
		box-shadow: 0 6px 16px rgba(146, 64, 14, 0.22);
		color: #fffbeb;
	}
	.activity-link.revert:hover:not(:disabled) {
		background: linear-gradient(180deg, #e08b1a 0%, #c4630b 100%);
		border-color: #92400e;
	}

	.activity-head {
		display: flex;
		justify-content: space-between;
		gap: 0.6rem;
	}

	.status-sent {
		display: inline-block;
		padding: 0.1rem 0.45rem;
		border-radius: 0.35rem;
		background: #fef3c7;
		color: #92400e;
		font-size: 0.8rem;
		font-weight: 600;
	}

	.status-completed {
		display: inline-block;
		padding: 0.1rem 0.45rem;
		border-radius: 0.35rem;
		background: #d1fae5;
		color: #065f46;
		font-size: 0.8rem;
		font-weight: 600;
	}

	.btn-complete {
		padding: 0.3rem 0.7rem;
		border: 1px solid #1f5a42;
		border-radius: 0.5rem;
		background: #1f5a42;
		color: #fff;
		font-size: 0.85rem;
		font-weight: 600;
		cursor: pointer;
		font-family: inherit;
	}

	.btn-complete:hover:not(:disabled) {
		background: #174a35;
	}

	.btn-complete:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.btn-revert {
		padding: 0.3rem 0.7rem;
		border: 1px solid #b45309;
		border-radius: 0.5rem;
		background: #b45309;
		color: #fff;
		font-size: 0.85rem;
		font-weight: 600;
		cursor: pointer;
		font-family: inherit;
	}

	.btn-revert:hover:not(:disabled) {
		background: #92400e;
	}

	.btn-revert:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.card-actions {
		display: flex;
		gap: 0.5rem;
		margin-top: 0.2rem;
	}

	.btn-log-activity {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		align-self: start;
		min-height: 3rem;
		padding: 0.5rem 0.9rem;
		border: 1px solid #1f5a42;
		border-radius: 0.82rem;
		background: #fff;
		font-size: 0.95rem;
		font-weight: 700;
		color: #1f5a42;
		text-decoration: none;
	}

	.btn-log-activity:hover {
		background: #eef5f1;
	}

	.actions-cell {
		display: flex;
		gap: 0.4rem;
		align-items: center;
	}

	.btn-log-table {
		padding: 0.3rem 0.7rem;
		border: 1px solid #1f5a42;
		border-radius: 0.5rem;
		background: #fff;
		color: #1f5a42;
		font-size: 0.85rem;
		font-weight: 600;
		cursor: pointer;
		font-family: inherit;
		text-decoration: none;
	}

	.btn-log-table:hover {
		background: #eef5f1;
	}

	@media (min-width: 768px) {
		h1 {
			font-size: 1.35rem;
		}
	}
</style>
