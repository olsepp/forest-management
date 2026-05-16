<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { goto } from '$app/navigation';
	import type { ActivityDto } from '$lib/dtos/activity/activity.dto';
	import DatePicker from '$lib/components/DatePicker.svelte';
	import { activityService } from '$lib/services/activity';

	let { data }: { data: { activities: ActivityDto[] } } = $props();

	const companyId = $derived($page.params.CompanyId ?? '');
	let expandedActivityIds = $state<string[]>([]);
	let formStartDate = $state('');
	let formEndDate = $state('');
	let isExporting = $state(false);

	// Initialize form values from URL params
	if ($page.url.searchParams.get('startDate')) {
		formStartDate = $page.url.searchParams.get('startDate')?.split('T')[0] ?? '';
	}
	if ($page.url.searchParams.get('endDate')) {
		formEndDate = $page.url.searchParams.get('endDate')?.split('T')[0] ?? '';
	}

	function isExpanded(activityId: string): boolean {
		return expandedActivityIds.includes(activityId);
	}

	function toggleExpand(activityId: string): void {
		if (isExpanded(activityId)) {
			expandedActivityIds = expandedActivityIds.filter((id) => id !== activityId);
			return;
		}

		expandedActivityIds = [...expandedActivityIds, activityId];
	}

	function formatDate(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleString();
	}

	function formatQuantity(item: ActivityDto): string {
		const quantity =
			typeof item.quantity === 'number' && Number.isFinite(item.quantity) ? item.quantity : 0;
		return item.unit ? `${quantity} ${item.unit}` : String(quantity);
	}

	function cadasterLabel(item: ActivityDto): string {
		return item.cadasterCadastralNumber || '—';
	}

	function forestStandLabel(item: ActivityDto): string {
		if (
			typeof item.forestStandNumber === 'number' &&
			Number.isFinite(item.forestStandNumber) &&
			item.forestStandNumber > 0
		) {
			return String(item.forestStandNumber);
		}

		return '—';
	}

	function applicationStatusLabel(status: string | null): string {
		if (status === null) return '—';
		if (status === 'Pending') return 'Ootel';
		if (status === 'Approved') return 'Kinnitatud';
		if (status === 'Rejected') return 'Tagasi lükatud';
		return String(status);
	}

	function handleSubmit() {
		const url = new URL($page.url);
		if (formStartDate) {
			url.searchParams.set('startDate', formStartDate);
		} else {
			url.searchParams.delete('startDate');
		}
		if (formEndDate) {
			url.searchParams.set('endDate', `${formEndDate}T23:59:59.999`);
		} else {
			url.searchParams.delete('endDate');
		}
		goto(url.toString(), { replaceState: true });
	}

	function handleReset() {
		formStartDate = '';
		formEndDate = '';
		goto($page.url.pathname, { replaceState: true });
	}

	const canExport = $derived(formStartDate !== '' && formEndDate !== '');

	async function handleExport() {
		if (!canExport || !companyId) return;

		isExporting = true;
		try {
			const blob = await activityService.exportToExcel(companyId, formStartDate, formEndDate);
			const url = window.URL.createObjectURL(blob);
			const a = document.createElement('a');
			a.href = url;
			a.download = `tegevused_${formStartDate}_${formEndDate}.xlsx`;
			document.body.appendChild(a);
			a.click();
			a.remove();
			window.URL.revokeObjectURL(url);
		} catch (error) {
			console.error('Failed to export activities:', error);
		} finally {
			isExporting = false;
		}
	}
</script>

<h1>Tegevused</h1>

<div class="date-range-filter">
	<DatePicker
		label="Alates"
		bind:value={formStartDate}
		placeholder="Vali alguskuupäev"
	/>
	<DatePicker
		label="Kuni"
		bind:value={formEndDate}
		placeholder="Vali lõppkuupäev"
	/>
	<button class="filter-btn" onclick={handleSubmit}>Filtreeri</button>
	<button class="reset-btn" onclick={handleReset}>Lähtesta</button>
	<button class="export-btn" disabled={!canExport || isExporting} onclick={handleExport}>
		{isExporting ? 'Laen...' : 'Lae alla'}
	</button>
</div>

{#if data.activities.length === 0}
	<p>Tegevusi ei leitud.</p>
{:else}
	<div class="table-wrapper">
		<table>
			<thead>
				<tr>
					<th>Kuupäev</th>
					<th>Tüüp</th>
					<th>Kinnistu</th>
					<th>Kataster</th>
					<th>Eraldis</th>
					<th>Kasutaja</th>
					<th class="actions"></th>
				</tr>
			</thead>
			<tbody>
				{#each data.activities as item (item.id)}
					<tr>
						<td>{formatDate(item.date)}</td>
						<td>{item.activityTypeName}</td>
						<td>{item.landPropertyName}</td>
						<td>{cadasterLabel(item)}</td>
						<td>{forestStandLabel(item)}</td>
						<td>{item.userName}</td>
						<td class="actions">
							<button
								type="button"
								class="expand-toggle"
								onclick={() => toggleExpand(item.id)}
								aria-label={isExpanded(item.id)
									? 'Peida tegevuse detailid'
									: 'Näita tegevuse detaile'}
								aria-expanded={isExpanded(item.id)}
							>
								<svg
									class={`expand-icon ${isExpanded(item.id) ? 'open' : ''}`}
									viewBox="0 0 24 24"
									fill="none"
									stroke="currentColor"
									stroke-width="2.75"
									stroke-linecap="round"
									stroke-linejoin="round"
									aria-hidden="true"
								>
									<path d="M6 9l6 6 6-6" />
								</svg>
							</button>
						</td>
					</tr>
					{#if isExpanded(item.id)}
						<tr class="expanded-row">
							<td colspan="7">
								<div class="expanded-content">
									<div class="expanded-actions">
										<a
											class="details-open-btn"
											href={resolve('/admin/[CompanyId]/activity/[ActivityId]', {
												CompanyId: companyId,
												ActivityId: item.id
											})}
										>
											Ava tegevus
										</a>
									</div>
									<div class="details-grid">
										<div class="detail-item">
											<span class="detail-label">ID</span>
											<span class="detail-value">{item.id}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Kuupäev</span>
											<span class="detail-value">{formatDate(item.date)}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Tegevuse tüüp</span>
											<span class="detail-value">{item.activityTypeName}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Kataster</span>
											<span class="detail-value">{cadasterLabel(item)}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Eraldis</span>
											<span class="detail-value">{forestStandLabel(item)}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Kinnistu</span>
											<span class="detail-value">{item.landPropertyName || '—'}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Kasutaja</span>
											<span class="detail-value">{item.userName}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Kirjeldus</span>
											<span class="detail-value">{item.description || '—'}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Kogus</span>
											<span class="detail-value">{formatQuantity(item)}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Ühik</span>
											<span class="detail-value">{item.unit || '—'}</span>
										</div>
										<div class="detail-item">
											<span class="detail-label">Staatus</span>
											<span class="detail-value"
												>{applicationStatusLabel(item.applicationStatus)}</span
											>
										</div>
									</div>
								</div>
							</td>
						</tr>
					{/if}
				{/each}
			</tbody>
		</table>
	</div>
{/if}

<style>
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
		vertical-align: top;
	}

	th.actions,
	td.actions {
		text-align: right;
		white-space: nowrap;
	}

	.link-button {
		border: none;
		background: transparent;
		padding: 0;
		color: #0f766e;
		cursor: pointer;
		font: inherit;
		text-decoration: none;
	}

	.link-button:hover {
		text-decoration: underline;
	}

	.separator {
		margin: 0 0.4rem;
		color: #94a3b8;
	}

	.expanded-row td {
		background: #f4f7f5;
	}

	.expanded-content {
		padding: 0.5rem 0;
	}

	.expanded-actions {
		display: flex;
		justify-content: flex-end;
		margin-bottom: 0.75rem;
	}

	.details-open-btn {
		display: inline-flex;
		align-items: center;
		background: #1f5a42;
		border: 1px solid #1f5a42;
		border-radius: 0.5rem;
		padding: 0.4rem 0.75rem;
		text-decoration: none;
		font-weight: 600;
		color: #ffffff;
		transition:
			background 0.2s ease,
			border-color 0.2s ease;
	}

	.details-open-btn:hover {
		background: #174834;
		border-color: #174834;
		color: #ffffff;
		text-decoration: none;
	}

	.details-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
		gap: 0.75rem;
	}

	.detail-item {
		display: flex;
		flex-direction: column;
		gap: 0.2rem;
	}

	.detail-label {
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: var(--admin-text-muted);
	}

	.detail-value {
		font-weight: 500;
		color: var(--admin-text);
	}

	.error {
		margin-top: 0.75rem;
		color: #b91c1c;
	}

	/* Date range filter styles */
	.date-range-filter {
		display: flex;
		flex-wrap: wrap;
		gap: 0.75rem;
		align-items: flex-end;
		margin-bottom: 1.5rem;
		padding: 0.75rem;
		background: #f8fafc;
		border-radius: 0.5rem;
		border: 1px solid #e2e8f0;
	}

	.date-range-filter :global(.date-picker-container) {
		min-width: 200px;
	}

	.date-range-filter button {
		margin-top: 1.5rem;
		padding: 0.75rem 1.5rem;
		border: none !important;
		border-radius: 0.6rem;
		font-size: 1rem;
		font-weight: 600;
		cursor: pointer;
		transition: background 0.2s ease;
		background: #1f5a42 !important;
		color: white !important;
	}

	.date-range-filter button:hover:not(:disabled) {
		background: #174834 !important;
	}

	.date-range-filter button:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}
</style>
