<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
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

	type ForestStandListDto = {
		id: string;
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

	let property = $state<LandPropertyDto | null>(null);
	let cadasters = $state<CadasterLinkDto[]>([]);
	let activities = $state<ActivityListDto[]>([]);

	let companyId = $derived($page.params.CompanyId ?? '');
	let propertyId = $derived($page.params.LandPropertyId ?? '');
	let currentUsername = $derived(($user?.username ?? '').trim().toLowerCase());

	function statusLabel(status: LandPropertyDto['status']): string {
		if (typeof status === 'number') {
			if (status === 0) return 'Active';
			if (status === 2) return 'Sold';
			return 'Inactive';
		}

		const value = String(status ?? '').trim().toLowerCase();
		if (value === 'active') return 'Active';
		if (value === 'sold') return 'Sold';
		return 'Inactive';
	}

	function formatDate(value: string | null): string {
		if (!value) return '—';
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleDateString();
	}

	function formatActivityQuantity(activity: ActivityListDto): string {
		const quantity = Number.isFinite(activity.quantity) ? String(activity.quantity) : '—';
		return activity.unit ? `${quantity} ${activity.unit}` : quantity;
	}

	async function loadData() {
		if (!companyId || !propertyId) {
			errorMessage = 'Missing route parameters.';
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
					errorMessage = 'Unauthorized. Please sign in again.';
					return;
				}

				errorMessage = propertyResponse.status === 404 ? 'Property not found.' : 'Failed to load property.';
				return;
			}

			property = (await propertyResponse.json()) as LandPropertyDto;

			const cadastersResponse = await fetch(`${apiBaseUrl}/api/cadasters/by-land-property/${propertyId}`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			cadasters = cadastersResponse.ok
				? (((await cadastersResponse.json()) as CadasterLinkDto[]) ?? []).filter((item) => Boolean(item?.id))
				: [];

			const activityCollections = await Promise.all(
				cadasters.map(async (cadaster) => {
					const byCadasterResponse = await fetch(
						`${apiBaseUrl}/api/activities/by-cadaster/${cadaster.id}`,
						{ headers: { Authorization: `Bearer ${token}` } }
					);
					const byCadaster = byCadasterResponse.ok
						? (((await byCadasterResponse.json()) as ActivityListDto[]) ?? [])
						: [];

					const forestStandResponse = await fetch(
						`${apiBaseUrl}/api/foreststands/by-cadaster/${cadaster.id}`,
						{ headers: { Authorization: `Bearer ${token}` } }
					);
					const forestStands = forestStandResponse.ok
						? (((await forestStandResponse.json()) as ForestStandListDto[]) ?? []).filter((item) =>
							Boolean(item?.id)
						)
						: [];

					const forestStandActivityCollections = await Promise.all(
						forestStands.map(async (stand) => {
							const response = await fetch(
								`${apiBaseUrl}/api/activities/by-foreststand/${stand.id}`,
								{ headers: { Authorization: `Bearer ${token}` } }
							);
							return response.ok ? (((await response.json()) as ActivityListDto[]) ?? []) : [];
						})
					);

					return [...byCadaster, ...forestStandActivityCollections.flat()];
				})
			);

			const mergedActivities = activityCollections.flat();
			const uniqueById: Record<string, ActivityListDto> = {};
			for (const activity of mergedActivities) {
				if (!activity?.id) continue;
				if (!uniqueById[activity.id]) {
					uniqueById[activity.id] = activity;
				}
			}

			activities = Object.values(uniqueById)
				.filter((item) => (item.userName ?? '').trim().toLowerCase() === currentUsername)
				.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
		} catch {
			errorMessage = 'Failed to load property details.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadData);
</script>

{#if isLoading}
	<div class="employee-state-block is-loading">Loading property details…</div>
{:else if errorMessage && !property}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Your session may have expired.</span>
		{/if}
	</div>
{:else if property}
	<p class="back-link">
		<a href={resolve('/employee/[CompanyId]/landproperty', { CompanyId: companyId })}>← Back to properties</a>
	</p>

	<section class="employee-card summary">
		<p class="kicker">Property details</p>
		<h1>{property.name}</h1>
		<p class="status-line">Status: <strong>{statusLabel(property.status)}</strong></p>
		<div class="meta-grid">
			<p><strong>Registration:</strong> {property.registrationNumber}</p>
			<p><strong>County:</strong> {property.county || '—'}</p>
			<p><strong>Parish:</strong> {property.parish || '—'}</p>
			<p><strong>Village:</strong> {property.village || '—'}</p>
			<p><strong>Bought:</strong> {formatDate(property.boughtDate)}</p>
			<p><strong>Sold:</strong> {formatDate(property.soldDate)}</p>
		</div>
	</section>

	<section class="employee-card">
		<h2>Related cadasters</h2>
		{#if cadasters.length === 0}
			<div class="employee-state-block is-empty">No cadasters connected to this property.</div>
		{:else}
			<div class="cadaster-links">
				{#each cadasters as cadaster (cadaster.id)}
					<a
						href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
							CompanyId: companyId,
							CadasterId: cadaster.id
						})}
					>
						{cadaster.cadastralNumber || cadaster.id}
					</a>
				{/each}
			</div>
		{/if}
	</section>

	<section class="employee-card">
		<h2>Your activity history for this property</h2>
		{#if activities.length === 0}
			<div class="employee-state-block is-empty">No activities found for your account in this property.</div>
		{:else}
			<div class="employee-stack-cards">
				{#each activities as activity (activity.id)}
					<article class="activity-card">
						<p class="activity-head">
							<strong>{activity.activityTypeName || 'Activity'}</strong>
							<span>{formatDate(activity.date)}</span>
						</p>
						<p>{activity.description || '—'}</p>
						<p><strong>Quantity:</strong> {formatActivityQuantity(activity)}</p>
						<p>
							<strong>Target:</strong>
							{activity.cadasterCadastralNumber
								? `Cadaster ${activity.cadasterCadastralNumber}`
								: activity.forestStandNumber
									? `Stand ${activity.forestStandNumber}`
									: '—'}
						</p>
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

	.status-line {
		margin: 0 0 0.55rem;
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

	.cadaster-links {
		display: flex;
		flex-wrap: wrap;
		gap: 0.45rem;
	}

	.cadaster-links a {
		display: inline-flex;
		align-items: center;
		padding: 0.28rem 0.54rem;
		border-radius: 0.55rem;
		border: 1px solid #cde0d6;
		background: #f7fbf9;
		text-decoration: none;
		font-size: 0.85rem;
		color: #1f5a42;
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

	.activity-head {
		display: flex;
		justify-content: space-between;
		gap: 0.6rem;
	}
</style>
