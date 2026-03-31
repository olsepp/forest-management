<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import { onMount } from 'svelte';

	type ForestStandDto = {
		id: string;
		number: number;
		area: number;
		totalVolume: number;
		isActive: boolean;
		validFrom: string;
		validTo: string | null;
		cadasterId: string;
		cadasterCadastralNumber: string;
		landPropertyId: string;
		landPropertyName: string;
	};

	type CadasterSummaryDto = {
		id: string;
		cadastralNumber: string;
		landPropertyId?: string;
		landPropertyName?: string;
	};

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
		locationDescription: string | null;
		applicationStatus: string | null;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let errorMessage = $state('');
	let isUnauthorized = $state(false);

	let forestStand = $state<ForestStandDto | null>(null);
	let activities = $state<ActivityListDto[]>([]);
	let linkedLandPropertyId = $state('');
	let linkedLandPropertyName = $state('');

	let companyId = $derived($page.params.CompanyId ?? '');
	let forestStandId = $derived($page.params.ForestStandId ?? '');
	let currentUsername = $derived(($user?.username ?? '').trim().toLowerCase());

	function formatDate(value: string | null): string {
		if (!value) return '—';
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleDateString();
	}

	function formatNumber(value: number | null | undefined): string {
		if (typeof value !== 'number' || Number.isNaN(value)) return '—';
		return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(value);
	}

	function formatActivityQuantity(activity: ActivityListDto): string {
		const quantity = Number.isFinite(activity.quantity) ? String(activity.quantity) : '—';
		return activity.unit ? `${quantity} ${activity.unit}` : quantity;
	}

	async function loadCadasterPropertyFallback(cadasterId: string, token: string): Promise<void> {
		const response = await fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
			headers: { Authorization: `Bearer ${token}` }
		});

		if (!response.ok) return;

		const cadaster = (await response.json()) as CadasterSummaryDto;
		linkedLandPropertyId = cadaster.landPropertyId ?? linkedLandPropertyId;
		linkedLandPropertyName = cadaster.landPropertyName ?? linkedLandPropertyName;
	}

	async function loadData() {
		if (!companyId || !forestStandId) {
			errorMessage = 'Marsruudi parameetrid puuduvad.';
			isLoading = false;
			return;
		}

		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const token = await authService.ensureValidToken();

			const [forestStandResponse, activityResponse] = await Promise.all([
				fetch(`${apiBaseUrl}/api/foreststands/${forestStandId}`, {
					headers: { Authorization: `Bearer ${token}` }
				}),
				fetch(`${apiBaseUrl}/api/activities/by-foreststand/${forestStandId}`, {
					headers: { Authorization: `Bearer ${token}` }
				})
			]);

			if (!forestStandResponse.ok) {
				if (forestStandResponse.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage =
					forestStandResponse.status === 404 ? 'Eraldist ei leitud.' : 'Eraldise laadimine ebaõnnestus.';
				return;
			}

			forestStand = (await forestStandResponse.json()) as ForestStandDto;
			linkedLandPropertyId = forestStand.landPropertyId ?? '';
			linkedLandPropertyName = forestStand.landPropertyName ?? '';

			if ((!linkedLandPropertyId || !linkedLandPropertyName) && forestStand.cadasterId) {
				await loadCadasterPropertyFallback(forestStand.cadasterId, token);
			}

			if (activityResponse.status === 401) {
				isUnauthorized = true;
				errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
				activities = [];
				return;
			}

			activities = activityResponse.ok
				? (((await activityResponse.json()) as ActivityListDto[]) ?? [])
						.filter((item) => (item.userName ?? '').trim().toLowerCase() === currentUsername)
						.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
				: [];
		} catch {
			errorMessage = 'Eraldise laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadData);
</script>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse eraldise detaile…</div>
{:else if errorMessage && !forestStand}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Teie sessioon võib olla aegunud.</span>
		{/if}
	</div>
{:else if forestStand}
	<p class="back-link">
		<a
			class="back-link-button"
			href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
				CompanyId: companyId,
				CadasterId: forestStand.cadasterId
			})}
		>
			<span aria-hidden="true">←</span>
			<span>Tagasi katastri juurde</span>
		</a>
	</p>

	<section class="employee-card summary">
		<div class="summary-head">
			<div>
				<p class="kicker">Eraldise detailid</p>
				<h1>Eraldis #{forestStand.number}</h1>
			</div>
			<a
				class="log-activity-link"
				href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]/activity/new', {
					CompanyId: companyId,
					ForestStandId: forestStand.id
				})}
			>
				Logi tegevus
			</a>
		</div>

		<div class="context-grid">
			<p>
				<strong>Kataster:</strong>
				<a
					href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
						CompanyId: companyId,
						CadasterId: forestStand.cadasterId
					})}
				>{forestStand.cadasterCadastralNumber || '—'}</a>
			</p>
			<p>
				<strong>Kinnistu:</strong>
				{#if linkedLandPropertyId && linkedLandPropertyName}
					<a
						href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
							CompanyId: companyId,
							LandPropertyId: linkedLandPropertyId
						})}
					>{linkedLandPropertyName}</a>
				{:else}
					—
				{/if}
			</p>
		</div>

		<div class="meta-grid">
			<p><strong>Olek:</strong> {forestStand.isActive ? 'Aktiivne' : 'Mitteaktiivne'}</p>
			<p><strong>Pindala:</strong> {formatNumber(forestStand.area)}</p>
			<p><strong>Tagavara kokku:</strong> {formatNumber(forestStand.totalVolume)}</p>
			<p><strong>Kehtiv alates:</strong> {formatDate(forestStand.validFrom)}</p>
			<p><strong>Kehtiv kuni:</strong> {formatDate(forestStand.validTo)}</p>
		</div>
	</section>

	<section class="employee-card">
		<div class="section-head">
			<h2>Sinu tegevused selles eraldises</h2>
			<a
				class="log-activity-link is-secondary"
				href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]/activity/new', {
					CompanyId: companyId,
					ForestStandId: forestStand.id
				})}
			>
				Logi uus tegevus
			</a>
		</div>
		{#if activities.length === 0}
			<div class="employee-state-block is-empty">Selles eraldises ei leitud sinu konto tegevusi.</div>
		{:else}
			<div class="employee-stack-cards activities-mobile">
				{#each activities as activity (activity.id)}
					<article class="activity-card">
						<p class="activity-head">
							<strong>{activity.activityTypeName || 'Tegevus'}</strong>
							<span>{formatDate(activity.date)}</span>
						</p>
						<p>{activity.description || '—'}</p>
						<p><strong>Kogus:</strong> {formatActivityQuantity(activity)}</p>
						<a
							href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
								CompanyId: companyId,
								ActivityId: activity.id
							})}
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
								<td>{formatActivityQuantity(activity)}</td>
								<td>
									<a
										href={resolve('/employee/[CompanyId]/activity/[ActivityId]', {
											CompanyId: companyId,
											ActivityId: activity.id
										})}
									>
									Ava
									</a>
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		{/if}
	</section>

	{#if errorMessage}
		<div class="employee-state-block is-error">{errorMessage}</div>
	{/if}
{/if}

<style>
	.back-link {
		margin: 0 0 0.9rem;
	}

	.back-link-button {
		display: inline-flex;
		align-items: center;
		gap: 0.45rem;
		min-height: 3rem;
		padding: 0.65rem 0.95rem;
		border-radius: 0.85rem;
		border: 1px solid #c4d4cd;
		background: #ffffff;
		font-size: 0.97rem;
		font-weight: 700;
		text-decoration: none;
		color: #1f3f33;
		box-shadow: 0 2px 8px rgba(15, 37, 28, 0.06);
	}

	.back-link-button:hover {
		background: #f5f9f7;
		border-color: #afc6bb;
	}

	.summary {
		margin-bottom: 0.75rem;
	}

	.section-head {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.7rem;
		margin-bottom: 0.65rem;
	}

	.summary-head {
		display: flex;
		flex-wrap: wrap;
		justify-content: space-between;
		align-items: center;
		gap: 0.65rem;
		margin-bottom: 0.65rem;
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
		margin: 0.3rem 0;
		font-size: 1.2rem;
		line-height: 1.2;
		color: #17251e;
	}

	h2 {
		margin: 0;
		font-size: 1.05rem;
		color: #1f2937;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}

	.log-activity-link {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3rem;
		padding: 0.6rem 1rem;
		border: 1px solid #1f5a42;
		border-radius: 0.85rem;
		background: #1f5a42;
		color: #f6fbf8;
		font-size: 0.96rem;
		font-weight: 700;
		text-decoration: none;
	}

	.log-activity-link.is-secondary {
		border-color: #b7cbc1;
		background: #f7fbf9;
		color: #184434;
	}

	.context-grid,
	.meta-grid {
		display: grid;
		gap: 0.5rem;
	}

	.context-grid {
		margin-bottom: 0.55rem;
	}

	.context-grid p,
	.meta-grid p {
		margin: 0;
		color: #334155;
	}

	.context-grid a {
		color: #1f5a42;
		font-weight: 700;
		text-decoration: none;
	}

	.activities-table {
		display: none;
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

	.activity-card a {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		align-self: start;
		min-height: 2.75rem;
		margin-top: 0.2rem;
		padding: 0.45rem 0.8rem;
		border: 1px solid #bfd0c8;
		border-radius: 0.75rem;
		background: #f8fbf9;
		font-size: 0.95rem;
		font-weight: 700;
		color: #184334;
		text-decoration: none;
	}

	.activity-head {
		display: flex;
		justify-content: space-between;
		gap: 0.6rem;
	}

	@media (max-width: 420px) {
		.section-head {
			flex-direction: column;
			align-items: stretch;
		}

		.log-activity-link.is-secondary {
			width: 100%;
		}
	}

	@media (min-width: 768px) {
		.activities-mobile {
			display: none;
		}

		.activities-table {
			display: block;
		}
	}
</style>
