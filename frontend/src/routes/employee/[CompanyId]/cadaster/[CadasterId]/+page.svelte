<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import CadastralMap from '$lib/components/shared/CadastralMap.svelte';
	import { user } from '$lib/stores/auth.store';
	import { onMount } from 'svelte';

	type CadasterDto = {
		id: string;
		cadastralNumber: string;
		forestArea: number;
		arableArea: number;
		grasslandArea: number;
		yardArea: number;
		buildingFootprintArea: number;
		underwaterArea: number;
		otherArea: number;
		soilQualityIndex: number;
		calculatedVolume: number;
		volumeGrowth: number;
		landPropertyId: string;
		landPropertyName: string;
	};

	type ForestStandListDto = {
		id: string;
		number: number;
		area: number;
		totalVolume: number;
		isActive: boolean;
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

	let cadaster = $state<CadasterDto | null>(null);
	let forestStands = $state<ForestStandListDto[]>([]);
	let activities = $state<ActivityListDto[]>([]);

	let companyId = $derived($page.params.CompanyId ?? '');
	let cadasterId = $derived($page.params.CadasterId ?? '');
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

	async function loadData() {
		if (!companyId || !cadasterId) {
			errorMessage = 'Marsruudi parameetrid puuduvad.';
			isLoading = false;
			return;
		}

		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const token = await authService.ensureValidToken();

			const [cadasterResponse, forestStandResponse, activityResponse] = await Promise.all([
				fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
					headers: { Authorization: `Bearer ${token}` }
				}),
				fetch(`${apiBaseUrl}/api/foreststands/by-cadaster/${cadasterId}`, {
					headers: { Authorization: `Bearer ${token}` }
				}),
				fetch(`${apiBaseUrl}/api/activities/by-cadaster/${cadasterId}`, {
					headers: { Authorization: `Bearer ${token}` }
				})
			]);

			if (!cadasterResponse.ok) {
				if (cadasterResponse.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage = cadasterResponse.status === 404 ? 'Katasterit ei leitud.' : 'Katastri laadimine ebaõnnestus.';
				return;
			}

			cadaster = (await cadasterResponse.json()) as CadasterDto;

			forestStands = forestStandResponse.ok
				? (((await forestStandResponse.json()) as ForestStandListDto[]) ?? [])
						.filter((item) => Boolean(item?.id))
						.sort((a, b) => a.number - b.number)
				: [];

			activities = activityResponse.ok
				? (((await activityResponse.json()) as ActivityListDto[]) ?? [])
						.filter((item) => (item.userName ?? '').trim().toLowerCase() === currentUsername)
						.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
				: [];
		} catch {
			errorMessage = 'Katastri laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadData);
</script>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse katastri detaile…</div>
{:else if errorMessage && !cadaster}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Teie sessioon võib olla aegunud.</span>
		{/if}
	</div>
{:else if cadaster}
	<p class="back-link">
		<a href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
			CompanyId: companyId,
			LandPropertyId: cadaster.landPropertyId
		})}>← Tagasi kinnistu juurde</a>
	</p>

	<section class="employee-card summary">
		<div class="summary-head">
			<div>
				<p class="kicker">Katastri detailid</p>
				<h1>{cadaster.cadastralNumber}</h1>
			</div>
			<a
				class="log-activity-link"
				href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]/activity/new', {
					CompanyId: companyId,
					CadasterId: cadaster.id
				})}
			>
				Logi tegevus
			</a>
		</div>

		<div class="meta-grid">
			<p><strong>Kinnistu:</strong> {cadaster.landPropertyName || '—'}</p>
			<p><strong>Metsamaa pindala:</strong> {formatNumber(cadaster.forestArea)}</p>
			<p><strong>Haritav maa:</strong> {formatNumber(cadaster.arableArea)}</p>
			<p><strong>Rohumaa:</strong> {formatNumber(cadaster.grasslandArea)}</p>
			<p><strong>Õueala:</strong> {formatNumber(cadaster.yardArea)}</p>
			<p><strong>Ehitusala:</strong> {formatNumber(cadaster.buildingFootprintArea)}</p>
			<p><strong>Veealune maa:</strong> {formatNumber(cadaster.underwaterArea)}</p>
			<p><strong>Muu maa:</strong> {formatNumber(cadaster.otherArea)}</p>
			<p><strong>Mullaviljakuse indeks:</strong> {formatNumber(cadaster.soilQualityIndex)}</p>
			<p><strong>Arvutuslik tagavara:</strong> {formatNumber(cadaster.calculatedVolume)}</p>
			<p><strong>Tagavara juurdekasv:</strong> {formatNumber(cadaster.volumeGrowth)}</p>
		</div>
	</section>

	<section class="employee-card">
		<h2>Eraldised selles katastris</h2>
		{#if forestStands.length === 0}
			<div class="employee-state-block is-empty">Eraldisi ei leitud.</div>
		{:else}
			<div class="employee-stack-cards stands-mobile">
				{#each forestStands as stand (stand.id)}
					<article class="stand-card">
						<p><strong>Eraldis:</strong> #{stand.number}</p>
						<p><strong>Pindala:</strong> {formatNumber(stand.area)}</p>
						<p><strong>Tagavara kokku:</strong> {formatNumber(stand.totalVolume)}</p>
						<p><strong>Olek:</strong> {stand.isActive ? 'Aktiivne' : 'Mitteaktiivne'}</p>
						<a
							href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
								CompanyId: companyId,
								ForestStandId: stand.id
							})}
						>
							Ava eraldis
						</a>
					</article>
				{/each}
			</div>

			<div class="employee-table-wrap stands-table">
				<table>
					<thead>
						<tr>
							<th>Eraldis</th>
							<th>Pindala</th>
							<th>Tagavara kokku</th>
							<th>Olek</th>
							<th>Ava</th>
						</tr>
					</thead>
					<tbody>
						{#each forestStands as stand (stand.id)}
							<tr>
								<td>#{stand.number}</td>
								<td>{formatNumber(stand.area)}</td>
								<td>{formatNumber(stand.totalVolume)}</td>
								<td>{stand.isActive ? 'Aktiivne' : 'Mitteaktiivne'}</td>
								<td>
									<a
										href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
											CompanyId: companyId,
											ForestStandId: stand.id
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

	<section class="employee-card">
		<h2>Sinu tegevused selles katastris</h2>
		{#if activities.length === 0}
			<div class="employee-state-block is-empty">Selles katastris ei leitud sinu konto tegevusi.</div>
		{:else}
			<div class="employee-stack-cards">
				{#each activities as activity (activity.id)}
					<article class="activity-card">
						<p class="activity-head">
							<strong>{activity.activityTypeName || 'Tegevus'}</strong>
							<span>{formatDate(activity.date)}</span>
						</p>
						<p>{activity.description || '—'}</p>
						<p><strong>Kogus:</strong> {formatActivityQuantity(activity)}</p>
						<p><strong>Eraldis:</strong> {activity.forestStandNumber || '—'}</p>
					</article>
				{/each}
			</div>
		{/if}
	</section>

	<section class="employee-card">
		<h2>Katastriüksus kaardil</h2>
		<CadastralMap tunnus={cadaster.cadastralNumber} />
	</section>

	{#if errorMessage}
		<div class="employee-state-block is-error">{errorMessage}</div>
	{/if}
{/if}

<style>
	.back-link {
		margin: 0 0 0.75rem;
	}

	.back-link a {
		font-size: 0.9rem;
		font-weight: 700;
		text-decoration: none;
		color: #1f5a42;
	}

	.summary {
		margin-bottom: 0.75rem;
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
		margin: 0 0 0.65rem;
		font-size: 1.05rem;
		color: #1a3228;
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
		min-height: 2.4rem;
		padding: 0.5rem 0.8rem;
		border: 1px solid #1f5a42;
		border-radius: 0.7rem;
		background: #1f5a42;
		color: #f6fbf8;
		font-size: 0.88rem;
		font-weight: 700;
		text-decoration: none;
	}

	.meta-grid {
		display: grid;
		gap: 0.45rem;
	}

	.stands-table {
		display: none;
	}

	.stand-card,
	.activity-card {
		border: 1px solid #d9e4de;
		border-radius: 0.8rem;
		padding: 0.8rem;
		background: #ffffff;
		display: grid;
		gap: 0.35rem;
	}

	.stand-card p,
	.activity-card p {
		margin: 0;
		color: #3f564a;
	}

	.stand-card a {
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
		.stands-mobile {
			display: none;
		}

		.stands-table {
			display: block;
		}
	}
</style>
