<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
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
		landPropertyId: string | null;
		landPropertyName: string | null;
		cadasterCadastralNumber: string | null;
		forestStandNumber: number;
	};

	type LandPropertyListDto = { id: string };
	type CadasterListDto = { id: string };
	type ForestStandListDto = { id: string };

	type ActivityRow = {
		base: ActivityListDto;
		details: ActivityDetailsDto | null;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let errorMessage = $state('');
	let isUnauthorized = $state(false);
	let activities = $state<ActivityRow[]>([]);

	let companyId = $derived($page.params.CompanyId ?? '');
	let currentUsername = $derived(($user?.username ?? '').trim().toLowerCase());

	function formatDate(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleString();
	}

	function formatQuantity(item: ActivityListDto): string {
		const quantity = typeof item.quantity === 'number' && Number.isFinite(item.quantity) ? item.quantity : 0;
		return item.unit ? `${quantity} ${item.unit}` : String(quantity);
	}

	function cadasterLabel(row: ActivityRow): string {
		return row.details?.cadasterCadastralNumber || row.base.cadasterCadastralNumber || '—';
	}

	function forestStandLabel(row: ActivityRow): string {
		const standNumber = row.details?.forestStandNumber || row.base.forestStandNumber;
		if (Number.isFinite(standNumber) && standNumber > 0) return String(standNumber);
		return '—';
	}

	async function loadActivityDetails(activityId: string, token: string): Promise<ActivityDetailsDto | null> {
		const response = await fetch(`${apiBaseUrl}/api/activities/${activityId}`, {
			headers: { Authorization: `Bearer ${token}` }
		});

		if (!response.ok) return null;

		const data = (await response.json()) as ActivityDetailsDto;
		return {
			id: data.id,
			cadasterId: data.cadasterId ?? null,
			forestStandId: data.forestStandId ?? null,
			landPropertyId: data.landPropertyId ?? null,
			landPropertyName: data.landPropertyName ?? null,
			cadasterCadastralNumber: data.cadasterCadastralNumber ?? null,
			forestStandNumber:
				typeof data.forestStandNumber === 'number' && Number.isFinite(data.forestStandNumber)
					? data.forestStandNumber
					: 0
		};
	}

	async function loadData() {
		if (!companyId) {
			errorMessage = 'Missing company id.';
			isLoading = false;
			return;
		}

		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const token = await authService.ensureValidToken();

			const [activitiesResponse, propertiesResponse] = await Promise.all([
				fetch(`${apiBaseUrl}/api/activities`, { headers: { Authorization: `Bearer ${token}` } }),
				fetch(`${apiBaseUrl}/api/landproperties/search?companyId=${companyId}`, {
					headers: { Authorization: `Bearer ${token}` }
				})
			]);

			if (!activitiesResponse.ok || !propertiesResponse.ok) {
				if (activitiesResponse.status === 401 || propertiesResponse.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Unauthorized. Please sign in again.';
					return;
				}

				errorMessage = 'Failed to load activities.';
				return;
			}

			const allActivities = ((await activitiesResponse.json()) as ActivityListDto[]) ?? [];
			const companyProperties = ((await propertiesResponse.json()) as LandPropertyListDto[]) ?? [];

			const propertyIds = new Set(companyProperties.map((item) => item.id).filter(Boolean));

			const cadastersByProperty = await Promise.all(
				Array.from(propertyIds).map(async (propertyId) => {
					const response = await fetch(`${apiBaseUrl}/api/cadasters/by-land-property/${propertyId}`, {
						headers: { Authorization: `Bearer ${token}` }
					});
					if (!response.ok) return [] as CadasterListDto[];
					const data = (await response.json()) as CadasterListDto[];
					return Array.isArray(data) ? data.filter((item) => Boolean(item?.id)) : [];
				})
			);

			const cadasterIds = new Set(cadastersByProperty.flat().map((item) => item.id).filter(Boolean));

			const forestStandsByCadaster = await Promise.all(
				Array.from(cadasterIds).map(async (cadasterId) => {
					const response = await fetch(`${apiBaseUrl}/api/foreststands/by-cadaster/${cadasterId}`, {
						headers: { Authorization: `Bearer ${token}` }
					});
					if (!response.ok) return [] as ForestStandListDto[];
					const data = (await response.json()) as ForestStandListDto[];
					return Array.isArray(data) ? data.filter((item) => Boolean(item?.id)) : [];
				})
			);

			const forestStandIds = new Set(
				forestStandsByCadaster
					.flat()
					.map((item) => item.id)
					.filter(Boolean)
			);

			const myActivities = allActivities
				.filter((item) => Boolean(item?.id))
				.filter((item) => (item.userName ?? '').trim().toLowerCase() === currentUsername);

			const rowsWithDetails = await Promise.all(
				myActivities.map(async (item) => {
					const details = await loadActivityDetails(item.id, token);
					return { base: item, details } as ActivityRow;
				})
			);

			activities = rowsWithDetails
				.filter((row) => {
					const details = row.details;
					if (!details) return false;

					if (details.landPropertyId && propertyIds.has(details.landPropertyId)) return true;
					if (details.cadasterId && cadasterIds.has(details.cadasterId)) return true;
					if (details.forestStandId && forestStandIds.has(details.forestStandId)) return true;

					return false;
				})
				.sort((a, b) => new Date(b.base.date).getTime() - new Date(a.base.date).getTime());
		} catch {
			errorMessage = 'Failed to load activities.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadData);
</script>

<section class="employee-card summary">
	<p class="kicker">Activity history</p>
	<h1>Your company activity history</h1>
	<p>Review all activities you have logged in this company.</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Loading activities…</div>
{:else if errorMessage}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Your session may have expired.</span>
		{/if}
	</div>
{:else if activities.length === 0}
	<div class="employee-state-block is-empty">No activities found for your account in this company.</div>
{:else}
	<section class="employee-card">
		<div class="employee-stack-cards activities-mobile">
			{#each activities as row (row.base.id)}
				<article class="activity-card">
					<p class="activity-head">
						<strong>{row.base.activityTypeName || 'Activity'}</strong>
						<span>{formatDate(row.base.date)}</span>
					</p>
					<p>{row.base.description || '—'}</p>
					<p><strong>Cadaster:</strong> {cadasterLabel(row)}</p>
					<p><strong>Forest stand:</strong> {forestStandLabel(row)}</p>
					<p><strong>Quantity:</strong> {formatQuantity(row.base)}</p>
					<a
						href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
							CompanyId: companyId,
							ActivityId: row.base.id
						})}
					>
						Open activity
					</a>
				</article>
			{/each}
		</div>

		<div class="employee-table-wrap activities-table">
			<table>
				<thead>
					<tr>
						<th>Date</th>
						<th>Type</th>
						<th>Description</th>
						<th>Cadaster</th>
						<th>Forest stand</th>
						<th>Quantity</th>
						<th>Open</th>
					</tr>
				</thead>
				<tbody>
					{#each activities as row (row.base.id)}
						<tr>
							<td>{formatDate(row.base.date)}</td>
							<td>{row.base.activityTypeName || '—'}</td>
							<td>{row.base.description || '—'}</td>
							<td>{cadasterLabel(row)}</td>
							<td>{forestStandLabel(row)}</td>
							<td>{formatQuantity(row.base)}</td>
							<td>
								<a
									href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
										CompanyId: companyId,
										ActivityId: row.base.id
									})}
								>
									Open
								</a>
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

	.kicker {
		margin: 0;
		font-size: 0.77rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	h1 {
		margin: 0.3rem 0 0.4rem;
		font-size: 1.2rem;
		line-height: 1.2;
		color: #17251e;
	}

	p {
		margin: 0;
		color: #40574a;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}

	.activities-table {
		display: none;
	}

	.activity-card {
		border: 1px solid #d9e4de;
		border-radius: 0.8rem;
		padding: 0.8rem;
		background: #ffffff;
		display: grid;
		gap: 0.35rem;
	}

	.activity-card p {
		margin: 0;
		color: #3f564a;
	}

	.activity-card a {
		font-size: 0.9rem;
		font-weight: 700;
		color: #1f5a42;
		text-decoration: none;
	}

	.activity-head {
		display: flex;
		justify-content: space-between;
		gap: 0.6rem;
	}

	@media (min-width: 768px) {
		h1 {
			font-size: 1.35rem;
		}

		.activities-mobile {
			display: none;
		}

		.activities-table {
			display: block;
		}
	}
</style>
