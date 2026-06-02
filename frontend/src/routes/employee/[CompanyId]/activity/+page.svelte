<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import type { ActivityDto } from '$lib/dtos/activity/activity.dto';

	let { data }: { data: { activities: ActivityDto[] } } = $props();
	let activities = $derived(data.activities ?? []);
	let isLoading = $derived(activities.length === 0);

	let companyId = $derived($page.params.CompanyId ?? '');

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
		const standNumber = item.forestStandNumber;
		if (typeof standNumber === 'number' && Number.isFinite(standNumber) && standNumber > 0)
			return String(standNumber);
		return '—';
	}
</script>

<section class="employee-card summary">
	<h1>TEGEVUSED</h1>
	<p>Vaata kõiki tegevusi, mille oled selles ettevõttes sisestanud.</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse tegevusi… Halva ühenduse korral võib see veidi aega võtta.</div>
{:else if activities.length === 0}
	<div class="employee-state-block is-empty">Selles ettevõttes ei leitud sinu tegevusi.</div>
{:else}
	<section class="employee-card">
		<div class="employee-stack-cards activities-mobile">
			{#each activities as activity (activity.id)}
				<article class="activity-card">
					<p class="activity-head">
						<strong>{activity.activityTypeName || 'Tegevus'}</strong>
						<span>{formatDate(activity.date)}</span>
					</p>
					<p>{activity.description || '—'}</p>
					<p><strong>Kataster:</strong> {cadasterLabel(activity)}</p>
					<p><strong>Eraldis:</strong> {forestStandLabel(activity)}</p>
					<p><strong>Kogus:</strong> {formatQuantity(activity)}</p>
					<a
						class="activity-link"
						href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
							CompanyId: companyId,
							ActivityId: activity.id
						})}
						data-sveltekit-preload-data="tap"
					>
						Ava tegevus
					</a>
				</article>
			{/each}
		</div>

		<div class="employee-table-wrap activities-table">
			<table>
				<thead>
					<tr>
						<th>Kuupäev</th>
						<th>Tüüp</th>
						<th>Kirjeldus</th>
						<th>Kataster</th>
						<th>Eraldis</th>
						<th>Kogus</th>
						<th>Ava</th>
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
									data-sveltekit-preload-data="tap"
								>
									Ava
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
		text-decoration: none;
	}

	.activity-link:hover {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
	}

	.activity-link:active {
		transform: translateY(1px);
		box-shadow: 0 3px 10px rgba(15, 42, 31, 0.2);
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
	}
</style>
