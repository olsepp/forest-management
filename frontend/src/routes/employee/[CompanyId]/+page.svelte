<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import type { CompanyDto } from '$lib/dtos/company/company.dto';
	import type { ActivityDto } from '$lib/dtos/activity/activity.dto';

	type QuickAction = {
		label: string;
		description: string;
		kind: 'properties' | 'activities';
	};

	let { data }: { data: { company: CompanyDto | null; activities: ActivityDto[] } } = $props();
	let company = $derived(data.company);
	let activities = $derived(data.activities ?? []);
	let isLoading = $derived(!company);

	let companyId = $derived($page.params.CompanyId ?? '');

	let quickActions = $derived.by(() => {
		if (!companyId) return [] as QuickAction[];

		return [
			{
				label: 'Kinnistud',
				description: 'Otsi aktiivseid kinnistuid.',
				kind: 'properties'
			},
			{
				label: 'Tegevuste ajalugu',
				description: 'Vaata enda tehtud tegevuste ajalugu.',
				kind: 'activities'
			}
		];
	});

	function formatDate(dateStr: string): string {
		const date = new Date(dateStr);
		return date.toLocaleDateString('et-EE', { day: 'numeric', month: 'short', year: 'numeric' });
	}
</script>

<section class="intro employee-card">
	<p class="kicker">Ettevõtte tööruum</p>
	<h1 class="employee-page-title">{company?.name ?? 'Töötaja töölaud'}</h1>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse ettevõtte töölauda…</div>
{:else if quickActions.length === 0}
	<div class="employee-state-block is-empty">Toimingud puuduvad.</div>
{:else}
	<section class="employee-stack-cards" aria-label="Töötaja kiirtoimingud">
		{#each quickActions as action (action.kind)}
			{#if action.kind === 'properties'}
				<a
					class="action-card"
					href={resolve('/employee/[CompanyId]/landproperty', { CompanyId: companyId })}
				>
					<h2>{action.label}</h2>
					<p>{action.description}</p>
					<span class="action-link">Ava</span>
				</a>
			{:else}
				<a
					class="action-card"
					href={resolve('/employee/[CompanyId]/activity', { CompanyId: companyId })}
				>
					<h2>{action.label}</h2>
					<p>{action.description}</p>
					<span class="action-link">Ava</span>
				</a>
			{/if}
		{/each}
	</section>
{/if}
{#if activities.length > 0}
	<ul class="activities-list">
		{#each activities as activity (activity.id)}
			<li>
				<a
					href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
						CompanyId: companyId,
						ActivityId: activity.id
					})}
					class="activity-item"
				>
					<div class="activity-header">
						<span class="activity-type">{activity.activityTypeName}</span>
						<span class="activity-date">{formatDate(activity.date)}</span>
					</div>
					<p class="activity-description">{activity.description}</p>
					<div class="activity-meta">
						{#if activity.forestStandNumber !== null}
							<span class="activity-location"
								>{activity.cadasterCadastralNumber} / Eraldis {activity.forestStandNumber}</span
							>
						{:else if activity.cadasterCadastralNumber}
							<span class="activity-location">{activity.cadasterCadastralNumber}</span>
						{/if}
						<span class="activity-quantity">{activity.quantity} {activity.unit}</span>
					</div>
				</a>
			</li>
		{/each}
	</ul>
{:else}
	<p class="activities-empty">Ei leitud.</p>
{/if}

<style>
	.intro {
		margin-bottom: 0.85rem;
		padding: 1rem;
		background: linear-gradient(180deg, #ffffff 0%, #f5f8fc 100%);
		border-color: #d3dde8;
	}

	.kicker {
		margin: 0;
		font-size: 0.72rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	p {
		margin: 0;
		color: #334155;
	}

	.action-card {
		display: flex;
		flex-direction: column;
		gap: 0.45rem;
		min-height: 2.75rem;
		border: 1px solid #cfd8e3;
		border-radius: 1rem;
		padding: 0.92rem;
		background: #fff;
		text-decoration: none;
		transition:
			border-color 0.18s ease,
			box-shadow 0.18s ease,
			transform 0.18s ease;
	}

	.action-card:hover {
		border-color: #aebed0;
		box-shadow: 0 6px 16px rgba(15, 23, 42, 0.12);
		transform: translateY(-1px);
	}

	.action-card:active {
		transform: translateY(1px);
	}

	.action-card:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.25);
	}

	h2 {
		margin: 0;
		font-size: 1.02rem;
		color: #0f172a;
	}

	.action-link {
		margin-top: 0.28rem;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 2.6rem;
		padding: 0.45rem 0.85rem;
		font-size: 0.9rem;
		font-weight: 700;
		color: #ffffff;
		background: #174834;
		border-radius: 0.72rem;
	}

	@media (min-width: 640px) {
		h1 {
			font-size: 1.28rem;
		}
	}

	@media (min-width: 768px) {
		h1 {
			font-size: 1.45rem;
		}

		.action-card {
			padding: 1rem;
		}
	}

	.activities-list {
		list-style: none;
		margin: 0;
		padding: 0;
		display: flex;
		flex-direction: column;
		gap: 0.65rem;
	}

	.activity-item {
		display: block;
		padding: 0.85rem;
		background: #fff;
		border: 1px solid #cfd8e3;
		border-radius: 0.85rem;
		text-decoration: none;
		transition:
			border-color 0.18s ease,
			box-shadow 0.18s ease;
	}

	.activity-item:hover {
		border-color: #aebed0;
		box-shadow: 0 4px 12px rgba(15, 23, 42, 0.1);
	}

	.activity-item:active {
		transform: translateY(1px);
	}

	.activity-header {
		display: flex;
		justify-content: space-between;
		align-items: center;
		margin-bottom: 0.35rem;
	}

	.activity-type {
		font-size: 0.95rem;
		font-weight: 600;
		color: #184334;
	}

	.activity-date {
		font-size: 0.82rem;
		color: #64748b;
	}

	.activity-description {
		margin: 0 0 0.5rem;
		font-size: 0.95rem;
		color: #334155;
		line-height: 1.4;
	}

	.activity-meta {
		display: flex;
		justify-content: space-between;
		font-size: 0.82rem;
		color: #64748b;
	}

	.activity-location {
		font-weight: 500;
	}

	.activity-quantity {
		font-weight: 500;
		color: #475569;
	}

	.activities-empty {
		margin: 0;
		padding: 1rem;
		text-align: center;
		font-size: 0.95rem;
		color: #64748b;
		background: #f8fafc;
		border-radius: 0.85rem;
	}

	@media (min-width: 768px) {

		.activity-item {
			padding: 1rem;
		}

		.activity-description {
			font-size: 1rem;
		}
	}
</style>
