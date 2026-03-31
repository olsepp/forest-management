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
		<a
			class="back-link-button"
			href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
				CompanyId: companyId,
				LandPropertyId: cadaster.landPropertyId
			})}
		>
			<span aria-hidden="true">←</span>
			<span>Tagasi kinnistu juurde</span>
		</a>
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
		<h2>Eraldised</h2>
		{#if forestStands.length === 0}
			<div class="employee-state-block is-empty">Eraldisi ei leitud.</div>
		{:else}
			<div class="stand-button-grid stands-mobile" aria-label="Eraldised">
				{#each forestStands as stand (stand.id)}
					<a
						class="stand-button"
						href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
							CompanyId: companyId,
							ForestStandId: stand.id
						})}
						aria-label={`Ava eraldis ${stand.number}`}
					>
						#{stand.number}
					</a>
				{/each}
			</div>

			
		{/if}
	</section>

	<section class="employee-card">
		<div class="section-head">
			<h2>Sinu tegevused selles katastris</h2>
			<a
				class="log-activity-link is-secondary"
				href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]/activity/new', {
					CompanyId: companyId,
					CadasterId: cadaster.id
				})}
			>
				Logi uus tegevus
			</a>
		</div>
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
						<a
							class="activity-link"
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

	.meta-grid {
		display: grid;
		gap: 0.5rem;
	}

	.meta-grid p {
		margin: 0;
		color: #334155;
	}

	.activity-card {
		border: 1px solid #d8e0dc;
		border-radius: 0.8rem;
		padding: 0.9rem;
		background: #ffffff;
		display: grid;
		gap: 0.42rem;
	}

	.stand-button-grid {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
		gap: 0.55rem;
	}

	.stand-button {
		text-decoration: none;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3rem;
		border: 1px solid #1f5a42;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		box-shadow: 0 6px 16px rgba(15, 42, 31, 0.22);
		color: #f3fbf7;
		border-radius: 0.82rem;
		font-size: 1rem;
		font-weight: 700;
	}

	.stand-button:hover {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
	}

	.stand-button:active {
		transform: translateY(1px);
		box-shadow: 0 3px 10px rgba(15, 42, 31, 0.2);
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
		.stands-mobile {
			display: none;
		}
	}
</style>
