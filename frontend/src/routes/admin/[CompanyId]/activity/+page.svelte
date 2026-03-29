<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type ActivityDto = {
		id: string;
		description: string;
		quantity: number;
		unit: string | null;
		notes: string | null;
		date: string;
		userId: string;
		userName: string;
		activityTypeId: string;
		activityTypeName: string;
		cadasterId: string | null;
		cadasterCadastralNumber: string | null;
		forestStandId: string | null;
		forestStandNumber: number | null;
		landPropertyId: string | null;
		landPropertyName: string | null;
		applicationStatus: number | null;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let errorMessage = $state('');
	let activities = $state<ActivityDto[]>([]);
	let expandedActivityIds = $state<string[]>([]);
	const companyId = $derived($page.params.CompanyId ?? '');

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
		const quantity = typeof item.quantity === 'number' && Number.isFinite(item.quantity) ? item.quantity : 0;
		return item.unit ? `${quantity} ${item.unit}` : String(quantity);
	}

	function cadasterLabel(item: ActivityDto): string {
		return item.cadasterCadastralNumber || '—';
	}

	function forestStandLabel(item: ActivityDto): string {
		if (typeof item.forestStandNumber === 'number' && Number.isFinite(item.forestStandNumber) && item.forestStandNumber > 0) {
			return String(item.forestStandNumber);
		}

		return '—';
	}

	function applicationStatusLabel(status: number | null): string {
		if (status === null || typeof status !== 'number') return '—';
		if (status === 0) return 'Ootel';
		if (status === 1) return 'Kinnitatud';
		if (status === 2) return 'Tagasi lükatud';
		return String(status);
	}

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;

			const companyId = $page.params.CompanyId;
			if (!companyId) {
				errorMessage = 'Puudub ettevõtte ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activities/by-company/${companyId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 401 || response.status === 403
						? 'Ligipääs puudub. Logige uuesti sisse.'
						: 'Tegevuste laadimine ebaõnnestus.';
				return;
			}

			activities = (((await response.json()) as ActivityDto[]) ?? [])
				.filter((item) => Boolean(item?.id))
				.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
		} catch {
			errorMessage = 'Tegevuste laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	});
</script>

<h1>Tegevused</h1>

{#if isLoading}
	<p>Laetakse tegevusi...</p>
{:else if errorMessage}
	<p class="error">{errorMessage}</p>
{:else if activities.length === 0}
	<p>Selle ettevõtte jaoks tegevusi ei leitud.</p>
{:else}
	<div class="table-wrapper">
		<table>
			<thead>
				<tr>
					<th>Kuupäev</th>
					<th>Tüüp</th>
					<th>Kataster</th>
					<th>Eraldis</th>
					<th>Kasutaja</th>
					<th class="actions">Toimingud</th>
				</tr>
			</thead>
			<tbody>
				{#each activities as item (item.id)}
					<tr>
						<td>{formatDate(item.date)}</td>
						<td>{item.activityTypeName}</td>
						<td>{cadasterLabel(item)}</td>
						<td>{forestStandLabel(item)}</td>
						<td>{item.userName}</td>
					<td class="actions">
								<button
								type="button"
								class="expand-toggle"
								onclick={() => toggleExpand(item.id)}
							aria-label={isExpanded(item.id) ? 'Peida tegevuse detailid' : 'Näita tegevuse detaile'}
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
						<td colspan="6">
							<div class="expanded-content">
								<div class="expanded-actions">
									<a
										class="details-open-btn"
										href={resolve('/admin/[CompanyId]/activity/[ActivityId]', {
										CompanyId: companyId,
										ActivityId: item.id
									})}
										>Ava tegevuse leht</a
									>
								</div>
								<p><strong>ID:</strong> {item.id}</p>
								<p><strong>Kuupäev:</strong> {formatDate(item.date)}</p>
								<p><strong>Tegevuse tüüp:</strong> {item.activityTypeName}</p>
								<p><strong>Kataster:</strong> {cadasterLabel(item)}</p>
									<p><strong>Eraldis:</strong> {forestStandLabel(item)}</p>
									<p><strong>Kinnistu:</strong> {item.landPropertyName || '—'}</p>
									<p><strong>Kasutaja:</strong> {item.userName}</p>
									<p><strong>Kirjeldus:</strong> {item.description || '—'}</p>
									<p><strong>Kogus:</strong> {formatQuantity(item)}</p>
									<p><strong>Ühik:</strong> {item.unit || '—'}</p>
									<p><strong>Staatus:</strong> {applicationStatusLabel(item.applicationStatus)}</p>
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
		background: #f8fafc;
	}

	.expanded-content {
		display: grid;
		gap: 0.35rem;
	}

	.expanded-actions {
		display: flex;
		justify-content: flex-end;
		margin-bottom: 0.5rem;
	}

	.details-open-btn {
		display: inline-flex;
		align-items: center;
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
		padding: 0.4rem 0.75rem;
		text-decoration: none;
		font-weight: 600;
	}

	.details-open-btn:hover {
		text-decoration: none;
	}

	.expanded-content p {
		margin: 0;
	}

	.error {
		margin-top: 0.75rem;
		color: #b91c1c;
	}
</style>
