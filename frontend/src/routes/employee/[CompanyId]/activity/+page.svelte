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
		activityTypeName: string;
		activityTypeId: string;
		userName: string;
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
	let isUnauthorized = $state(false);
	let activities = $state<ActivityDto[]>([]);

	let companyId = $derived($page.params.CompanyId ?? '');

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
		const standNumber = item.forestStandNumber;
		if (Number.isFinite(standNumber) && standNumber > 0) return String(standNumber);
		return '—';
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

			const activitiesResponse = await fetch(`${apiBaseUrl}/api/activities/by-company/${companyId}/my`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!activitiesResponse.ok) {
				if (activitiesResponse.status === 401 || activitiesResponse.status === 403) {
					isUnauthorized = true;
					errorMessage = 'Unauthorized. Please sign in again.';
					return;
				}

				errorMessage = 'Failed to load activities.';
				return;
			}

			activities = (((await activitiesResponse.json()) as ActivityDto[]) ?? [])
				.filter((item) => Boolean(item?.id))
				.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
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
			{#each activities as activity (activity.id)}
				<article class="activity-card">
					<p class="activity-head">
						<strong>{activity.activityTypeName || 'Activity'}</strong>
						<span>{formatDate(activity.date)}</span>
					</p>
					<p>{activity.description || '—'}</p>
					<p><strong>Cadaster:</strong> {cadasterLabel(activity)}</p>
					<p><strong>Forest stand:</strong> {forestStandLabel(activity)}</p>
					<p><strong>Quantity:</strong> {formatQuantity(activity)}</p>
					<a
						href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
							CompanyId: companyId,
							ActivityId: activity.id
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
					{#each activities as activity (activity.id)}
						<tr>
							<td>{formatDate(activity.date)}</td>
							<td>{activity.activityTypeName || '—'}</td>
							<td>{activity.description || '—'}</td>
							<td>{cadasterLabel(activity)}</td>
							<td>{forestStandLabel(activity)}</td>
							<td>{formatQuantity(activity)}</td>
							<td>
								<a
									href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
										CompanyId: companyId,
										ActivityId: activity.id
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
