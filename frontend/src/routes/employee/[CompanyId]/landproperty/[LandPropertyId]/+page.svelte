<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type LandPropertyDto = {
		id: string;
		name: string;
		registrationNumber: number;
		county: string;
		parish: string;
		village: string;
		boughtDate: string | null;
		soldDate: string | null;
		status: 'Active' | 'Inactive' | 'Sold' | number | string;
		companyId: string;
		companyName: string;
	};

	type CadasterLinkDto = {
		id: string;
		cadastralNumber: string;
		forestArea?: number;
		forestStandCount?: number;
	};

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

	let property = $state<LandPropertyDto | null>(null);
	let cadasters = $state<CadasterLinkDto[]>([]);
	let activities = $state<ActivityDto[]>([]);

	let companyId = $derived($page.params.CompanyId ?? '');
	let propertyId = $derived($page.params.LandPropertyId ?? '');

	function statusLabel(status: LandPropertyDto['status']): string {
		if (typeof status === 'number') {
			if (status === 0) return 'Aktiivne';
			if (status === 2) return 'Müüdud';
			return 'Mitteaktiivne';
		}

		const value = String(status ?? '').trim().toLowerCase();
		if (value === 'active') return 'Aktiivne';
		if (value === 'sold') return 'Müüdud';
		return 'Mitteaktiivne';
	}

	function formatDate(value: string | null): string {
		if (!value) return '—';
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleDateString();
	}

	function formatActivityQuantity(activity: ActivityDto): string {
		const quantity = Number.isFinite(activity.quantity) ? String(activity.quantity) : '—';
		return activity.unit ? `${quantity} ${activity.unit}` : quantity;
	}

	async function loadData() {
		if (!companyId || !propertyId) {
			errorMessage = 'Marsruudi parameetrid puuduvad.';
			isLoading = false;
			return;
		}

		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const token = await authService.ensureValidToken();

			const propertyResponse = await fetch(`${apiBaseUrl}/api/landproperties/${propertyId}`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!propertyResponse.ok) {
				if (propertyResponse.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage = propertyResponse.status === 404 ? 'Kinnistut ei leitud.' : 'Kinnistu laadimine ebaõnnestus.';
				return;
			}

			property = (await propertyResponse.json()) as LandPropertyDto;

			const cadastersResponse = await fetch(`${apiBaseUrl}/api/cadasters/by-land-property/${propertyId}`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			cadasters = cadastersResponse.ok
				? (((await cadastersResponse.json()) as CadasterLinkDto[]) ?? []).filter((item) => Boolean(item?.id))
				: [];

			const activitiesResponse = await fetch(`${apiBaseUrl}/api/activities/by-property/${propertyId}/my`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!activitiesResponse.ok) {
				if (activitiesResponse.status === 401 || activitiesResponse.status === 403) {
					isUnauthorized = true;
					errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage = 'Kinnistu detailide laadimine ebaõnnestus.';
				return;
			}

			activities = (((await activitiesResponse.json()) as ActivityDto[]) ?? [])
				.filter((item) => Boolean(item?.id))
				.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
		} catch {
			errorMessage = 'Kinnistu detailide laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadData);
</script>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse kinnistu detaile…</div>
{:else if errorMessage && !property}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Teie sessioon võib olla aegunud.</span>
		{/if}
	</div>
{:else if property}
	<p class="employee-back-link">
		<a class="employee-back-link-button" href={resolve('/employee/[CompanyId]/landproperty', { CompanyId: companyId })}>
			<span aria-hidden="true">←</span>
			<span>Tagasi kinnistute juurde</span>
		</a>
	</p>

	<section class="employee-card summary">
		<p class="kicker">Kinnistu</p>
		<h1 class="employee-page-title">{property.name}</h1>
		<p class="status-line">Olek: <strong>{statusLabel(property.status)}</strong></p>
		<div class="meta-grid">
			<p><strong>Registrinumber:</strong> {property.registrationNumber}</p>
			<p><strong>Maakond:</strong> {property.county || '—'}</p>
			<p><strong>Vald:</strong> {property.parish || '—'}</p>
			<p><strong>Küla:</strong> {property.village || '—'}</p>
			<p><strong>Ostetud:</strong> {formatDate(property.boughtDate)}</p>
			<p><strong>Müüdud:</strong> {formatDate(property.soldDate)}</p>
		</div>
	</section>

	<section class="employee-card">
		<h2>Katastrid</h2>
		{#if cadasters.length === 0}
			<div class="employee-state-block is-empty">Ei leitud.</div>
		{:else}
			<div class="cadaster-list" role="list" aria-label="Seotud katastrid">
				{#each cadasters as cadaster (cadaster.id)}
					<a
						class="cadaster-row"
						href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
							CompanyId: companyId,
							CadasterId: cadaster.id
						})}
					>
						<div class="cadaster-row-main">
							<p class="cadaster-row-kicker">Kataster</p>
							<p class="cadaster-row-number">{cadaster.cadastralNumber || cadaster.id}</p>
							<div class="cadaster-row-meta">
								{#if typeof cadaster.forestArea === 'number'}
									<span>Metsamaa: {cadaster.forestArea}</span>
								{/if}
							</div>
						</div>
					</a>
				{/each}
			</div>
		{/if}
	</section>

	<section class="employee-card">
		<h2>Sinu tegevused sellel kinnistul</h2>
		{#if activities.length === 0}
			<div class="employee-state-block is-empty">Ei leitud.</div>
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
						<p>
							<strong>Siht:</strong>
							{activity.cadasterCadastralNumber
								? `Kataster ${activity.cadasterCadastralNumber}`
								: activity.forestStandNumber
									? `Eraldis ${activity.forestStandNumber}`
									: '—'}
						</p>
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

	{#if errorMessage}
		<div class="employee-state-block is-error">{errorMessage}</div>
	{/if}
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
		margin: 0;
		font-size: 1.28rem;
		line-height: 1.2;
		color: #0f172a;
	}

	h2 {
		margin: 0 0 0.65rem;
		font-size: 1.05rem;
		color: #1f2937;
	}

	.status-line {
		margin: 0 0 0.55rem;
		color: #334155;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}

	.meta-grid {
		display: grid;
		gap: 0.45rem;
	}

	.meta-grid p {
		margin: 0;
		color: #334155;
	}

	.cadaster-list {
		display: grid;
		gap: 0.55rem;
	}

	.cadaster-row {
		display: flex;
		align-items: center;
		justify-content: flex-start;
		gap: 0.65rem;
		min-height: 3.15rem;
		padding: 0.8rem 0.9rem;
		border: 1px solid #9ec6b0;
		border-radius: 0.95rem;
		background: linear-gradient(180deg, #eef8f2 0%, #e6f4ec 100%);
		text-decoration: none;
		color: #173328;
		box-shadow: 0 4px 14px rgba(15, 40, 30, 0.12);
		position: relative;
		overflow: hidden;
	}

	.cadaster-row::before {
		content: '';
		position: absolute;
		left: 0;
		top: 0;
		bottom: 0;
		width: 0.34rem;
		background: linear-gradient(180deg, #4f8b70 0%, #2d6b4f 100%);
	}

	.cadaster-row-main {
		display: grid;
		gap: 0.14rem;
		min-width: 0;
		padding-left: 0.3rem;
	}

	.cadaster-row-kicker {
		margin: 0;
		font-size: 0.72rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #537666;
	}

	.cadaster-row-number {
		margin: 0;
		font-size: 1rem;
		font-weight: 700;
		line-height: 1.2;
		color: #173328;
	}

	.cadaster-row-meta {
		display: flex;
		flex-wrap: wrap;
		gap: 0.35rem;
	}

	.cadaster-row-meta span {
		font-size: 0.83rem;
		font-weight: 600;
		color: #456657;
	}

	.cadaster-row:active {
		transform: translateY(1px);
		background: #edf6f1;
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
</style>
