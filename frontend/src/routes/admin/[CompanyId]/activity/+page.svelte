<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type ActivityListDto = {
		id: string;
		description: string;
		quantity: number;
		unit: string | null;
		date: string;
		activityTypeName: string;
		userName: string;
		cadasterCadastralNumber: string | null;
		forestStandNumber: number;
		applicationStatus: 'Pending' | 'Approved' | 'Rejected' | null;
	};

	type ActivityDetailsDto = {
		id: string;
		cadasterId: string | null;
		forestStandId: string | null;
		landPropertyName: string | null;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let errorMessage = $state('');
	let activities = $state<ActivityListDto[]>([]);
	let expandedActivityIds = $state<string[]>([]);
	let activityDetailsById = $state<Record<string, ActivityDetailsDto>>({});

	function isExpanded(activityId: string): boolean {
		return expandedActivityIds.includes(activityId);
	}

	async function toggleExpand(activityId: string): Promise<void> {
		if (isExpanded(activityId)) {
			expandedActivityIds = expandedActivityIds.filter((id) => id !== activityId);
			return;
		}

		expandedActivityIds = [...expandedActivityIds, activityId];
		await loadActivityDetails(activityId);
	}

	function landPropertyLabel(activityId: string): string {
		return activityDetailsById[activityId]?.landPropertyName || '—';
	}

	async function loadActivityDetails(activityId: string): Promise<void> {
		if (activityDetailsById[activityId]) return;

		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activities/${activityId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) return;

			const data = (await response.json()) as ActivityDetailsDto;
			activityDetailsById = {
				...activityDetailsById,
				[activityId]: {
					id: data.id,
					cadasterId: data.cadasterId ?? null,
					forestStandId: data.forestStandId ?? null,
					landPropertyName: data.landPropertyName ?? null
				}
			};

			if (activityDetailsById[activityId]?.landPropertyName) return;

			const cadasterId = data.cadasterId ?? null;
			const forestStandId = data.forestStandId ?? null;

			if (cadasterId) {
				const cadasterResponse = await fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				});

				if (cadasterResponse.ok) {
					const cadasterData = (await cadasterResponse.json()) as { landPropertyName?: string | null };
					activityDetailsById = {
						...activityDetailsById,
						[activityId]: {
							...activityDetailsById[activityId],
							landPropertyName: cadasterData.landPropertyName ?? null
						}
					};
					return;
				}
			}

			if (forestStandId) {
				const forestStandResponse = await fetch(`${apiBaseUrl}/api/foreststands/${forestStandId}`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				});

				if (forestStandResponse.ok) {
					const forestStandData = (await forestStandResponse.json()) as {
						landPropertyName?: string | null;
					};
					activityDetailsById = {
						...activityDetailsById,
						[activityId]: {
							...activityDetailsById[activityId],
							landPropertyName: forestStandData.landPropertyName ?? null
						}
					};
				}
			}
		} catch {
			// keep UI functional even if details request fails
		}
	}

	function formatDate(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleString();
	}

	function formatQuantity(item: ActivityListDto): string {
		const quantity = typeof item.quantity === 'number' && Number.isFinite(item.quantity) ? item.quantity : 0;
		return item.unit ? `${quantity} ${item.unit}` : String(quantity);
	}

	function cadasterLabel(item: ActivityListDto): string {
		return item.cadasterCadastralNumber || '—';
	}

	function forestStandLabel(item: ActivityListDto): string {
		if (Number.isFinite(item.forestStandNumber) && item.forestStandNumber > 0) {
			return String(item.forestStandNumber);
		}

		return '—';
	}

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;

			const companyId = $page.params.CompanyId;
			if (!companyId) {
				errorMessage = 'Missing company id';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activities`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 401
						? 'Unauthorized. Please sign in again.'
						: 'Failed to load activities.';
				return;
			}

			const data = (await response.json()) as ActivityListDto[];
			activities = Array.isArray(data) ? data : [];
		} catch {
			errorMessage = 'Failed to load activities.';
		} finally {
			isLoading = false;
		}
	});
</script>

<h1>Activities</h1>

{#if isLoading}
	<p>Loading activities...</p>
{:else if errorMessage}
	<p class="error">{errorMessage}</p>
{:else if activities.length === 0}
	<p>No activities found for this company.</p>
{:else}
	<div class="table-wrapper">
		<table>
			<thead>
				<tr>
					<th>Date</th>
					<th>Type</th>
					<th>Cadaster</th>
					<th>Forest stand</th>
					<th>User</th>
					<th class="actions">Actions</th>
				</tr>
			</thead>
			<tbody>
				{#each activities as item}
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
							aria-label={isExpanded(item.id) ? 'Collapse activity details' : 'Expand activity details'}
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
									<a class="details-open-btn" href={`/admin/${$page.params.CompanyId}/activity/${item.id}`}
										>Open single activity page</a
									>
								</div>
								<p><strong>ID:</strong> {item.id}</p>
								<p><strong>Date:</strong> {formatDate(item.date)}</p>
								<p><strong>Activity type:</strong> {item.activityTypeName}</p>
								<p><strong>Cadaster:</strong> {cadasterLabel(item)}</p>
									<p><strong>Forest stand:</strong> {forestStandLabel(item)}</p>
									<p><strong>Land property:</strong> {landPropertyLabel(item.id)}</p>
									<p><strong>User:</strong> {item.userName}</p>
									<p><strong>Description:</strong> {item.description || '—'}</p>
									<p><strong>Quantity:</strong> {formatQuantity(item)}</p>
									<p><strong>Unit:</strong> {item.unit || '—'}</p>
									<p><strong>Status:</strong> {item.applicationStatus ?? '—'}</p>
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
