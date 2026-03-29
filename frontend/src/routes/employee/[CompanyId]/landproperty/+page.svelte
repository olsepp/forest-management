<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type LandPropertyListDto = {
		id: string;
		name: string;
		registrationNumber: number;
		county: string;
		status: 'Active' | 'Inactive' | 'Sold' | number | string;
		cadastralNumbers?: string[];
		cadasters?: PropertyCadasterLinkDto[];
	};

	type PropertyCadasterLinkDto = {
		id: string;
		cadastralNumber: string;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let properties = $state<LandPropertyListDto[]>([]);
	let isLoading = $state(true);
	let errorMessage = $state('');
	let isUnauthorized = $state(false);
	let searchQuery = $state('');

	let companyId = $derived($page.params.CompanyId ?? '');
	let normalizedSearchQuery = $derived(searchQuery.trim().toLowerCase());

	let filteredProperties = $derived.by(() => {
		if (!normalizedSearchQuery) return properties;

		return properties.filter((property) => {
			const name = property.name?.toLowerCase() ?? '';
			const registrationNumber = String(property.registrationNumber ?? '').toLowerCase();
			const cadastralNumbers = propertySearchableCadastralNumbers(property).map((item) =>
				item.toLowerCase()
			);

			return (
				name.includes(normalizedSearchQuery) ||
				registrationNumber.includes(normalizedSearchQuery) ||
				cadastralNumbers.some((number) => number.includes(normalizedSearchQuery))
			);
		});
	});

	function statusLabel(status: LandPropertyListDto['status']): string {
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

	function statusClass(status: LandPropertyListDto['status']): string {
		return statusLabel(status).toLowerCase();
	}

	function isActiveStatus(status: LandPropertyListDto['status']): boolean {
		return statusLabel(status) === 'Active';
	}

	function statusText(status: LandPropertyListDto['status']): string {
		const normalized = statusLabel(status);
		if (normalized === 'Active') return 'Aktiivne';
		if (normalized === 'Sold') return 'Müüdud';
		return 'Mitteaktiivne';
	}

	function cadastersForProperty(property: LandPropertyListDto): PropertyCadasterLinkDto[] {
		const fromDto = Array.isArray(property.cadasters)
			? property.cadasters.filter((item) => Boolean(item?.cadastralNumber))
			: [];
		if (fromDto.length > 0) return fromDto;

		const fromNumbers = Array.isArray(property.cadastralNumbers) ? property.cadastralNumbers : [];
		return fromNumbers
			.filter(Boolean)
			.map((cadastralNumber) => ({ id: '', cadastralNumber }));
	}

	function propertySearchableCadastralNumbers(property: LandPropertyListDto): string[] {
		const fromDto = Array.isArray(property.cadastralNumbers) ? property.cadastralNumbers : [];
		const fromLookup = cadastersForProperty(property).map((item) => item.cadastralNumber);
		return [...new Set([...fromDto, ...fromLookup].filter(Boolean))];
	}

	async function loadData() {
		if (!companyId) {
			errorMessage = 'Puudub ettevõtte ID.';
			isLoading = false;
			return;
		}

		try {
			errorMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const token = await authService.ensureValidToken();
			const response = await fetch(
				`${apiBaseUrl}/api/landproperties/search?companyId=${encodeURIComponent(companyId)}&status=0`,
				{
					headers: {
						Authorization: `Bearer ${token}`
					}
				}
			);

			if (!response.ok) {
				if (response.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Lubatud pääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage = 'Ettevõtteid ei õnnestunud laadida.';
				return;
			}

			const data = (await response.json()) as LandPropertyListDto[];
			const receivedProperties = Array.isArray(data)
				? data.map((item) => ({
					...item,
					cadasters: Array.isArray(item.cadasters)
						? item.cadasters.filter(
								(cadaster) => Boolean(cadaster?.id) && Boolean(cadaster?.cadastralNumber)
							)
						: [],
					cadastralNumbers: Array.isArray(item.cadastralNumbers) ? item.cadastralNumbers : []
				}))
				: [];

			// Safety filter: some backends may ignore `activeOnly=true`.
			// Enforce employee visibility for active properties only on client side too.
			properties = receivedProperties.filter((item) => isActiveStatus(item.status));
		} catch {
			errorMessage = 'Ettevõtteid ei õnnestunud laadida.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadData);
</script>

<section class="employee-card page-intro">
	<p class="kicker">Kinnistud</p>
	<h1>Aktiivsed kinnistud</h1>
	<p>Otsi nime, reg. nr või katastri nr järgi.</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse kinnistu…</div>
{:else if errorMessage}
		<div class="employee-state-block is-error">
			{errorMessage}
			{#if isUnauthorized}
				<span class="inline-note">Sessioon võib olla lõppenud.</span>
			{/if}
		</div>
{:else}
	<section class="filters employee-card" aria-label="Kinnistute filtrid">
		<label for="property-search" class="filter-label">Otsi</label>
		<input
			id="property-search"
			type="search"
			bind:value={searchQuery}
			placeholder="Nimi, reg. nr, katasteri nr"
		/>
	</section>

	{#if filteredProperties.length === 0}
		<div class="employee-state-block is-empty">Praeguse otsingu järgi kinnistuid ei leitud.</div>
	{:else}
		<section class="mobile-list employee-stack-cards" aria-label="Kinnistute kaardid">
			{#each filteredProperties as property (property.id)}
				<article class="employee-card property-card" aria-label={`Kinnistu ${property.name}`}>
					<div class="card-top">
						<h2>
							<a
								href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
									CompanyId: companyId,
									LandPropertyId: property.id
								})}
							>
								{property.name}
							</a>
						</h2>
						<span class={`status ${statusClass(property.status)}`}>{statusText(property.status)}</span>
					</div>
					<div class="property-meta" aria-label="Kinnistu detailid">
						<p class="meta-item">
							<span class="meta-label">Registrinumber</span>
							<span class="meta-value">{property.registrationNumber}</span>
						</p>
						<p class="meta-item">
							<span class="meta-label">Maakond</span>
							<span class="meta-value">{property.county || '—'}</span>
						</p>
					</div>
					{#if cadastersForProperty(property).length === 0}
						<p class="muted kataster-empty">Katastrid puuduvad.</p>
					{:else}
						<div class="kataster-links card-kataster-links" aria-label="Katastriüksused">
							{#each cadastersForProperty(property) as cadaster (`${property.id}:${cadaster.id || cadaster.cadastralNumber}`)}
								{#if cadaster.id}
									<a
										onclick={(event) => event.stopPropagation()}
										href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
											CompanyId: companyId,
											CadasterId: cadaster.id
										})}
									>
										{cadaster.cadastralNumber}
									</a>
								{:else}
									<span>{cadaster.cadastralNumber}</span>
								{/if}
							{/each}
						</div>
					{/if}

				</article>
			{/each}
		</section>

	{/if}
{/if}

<style>
	.page-intro {
		margin-bottom: 0.85rem;
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

	h2 {
		margin: 0;
		font-size: 1.03rem;
		color: #173f2f;
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

	.filters {
		display: grid;
		gap: 0.35rem;
		margin-bottom: 0.85rem;
	}

	.filter-label {
		font-size: 0.84rem;
		font-weight: 700;
		color: #2b4639;
	}

	input[type='search'] {
		width: 100%;
		min-height: 2.65rem;
		border: 1px solid #c8d8cf;
		border-radius: 0.7rem;
		padding: 0.55rem 0.72rem;
		font: inherit;
		background: #fff;
	}

	input[type='search']:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.23);
	}

	.mobile-list {
		display: grid;
		margin-bottom: 0.85rem;
	}

	.property-card {
		display: grid;
		gap: 0.55rem;
		border: 2px solid #4f8b70;
		border-radius: 0.9rem;
		background: #ffffff;
		box-shadow: 0 2px 10px rgba(20, 53, 40, 0.1);
		transition:
			border-color 0.18s ease,
			box-shadow 0.18s ease,
			transform 0.18s ease;
	}

	.property-card:focus-visible {
		outline: none;
		border-color: #2f6f53;
		box-shadow: 0 0 0 3px rgba(47, 111, 83, 0.26);
	}

	.property-card:active {
		transform: scale(0.992);
		border-color: #2f6f53;
		background: #edf7f2;
		box-shadow: 0 1px 6px rgba(20, 53, 40, 0.18);
	}

	.card-top {
		display: flex;
		justify-content: space-between;
		align-items: center;
		gap: 0.5rem;
	}

	.property-meta {
		display: grid;
		grid-template-columns: repeat(2, minmax(0, 1fr));
		gap: 0.5rem;
	}

	.meta-item {
		display: grid;
		gap: 0.1rem;
		padding: 0.45rem 0.55rem;
		background: #f3f8f5;
		border: 1px solid #d4e4dc;
		border-radius: 0.6rem;
	}

	.meta-label {
		font-size: 0.73rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #507061;
	}

	.meta-value {
		font-size: 0.95rem;
		font-weight: 600;
		color: #1f382d;
	}

	.status {
		display: inline-flex;
		align-items: center;
		padding: 0.2rem 0.52rem;
		border-radius: 999px;
		font-size: 0.78rem;
		font-weight: 700;
		border: 1px solid transparent;
	}

	.status.active {
		color: #1a5a3f;
		background: #e9f8ef;
		border-color: #bde8cd;
	}

	.status.inactive {
		color: #5a4a23;
		background: #fbf5e6;
		border-color: #e8d6aa;
	}

	.status.sold {
		color: #6b1e1e;
		background: #fdecec;
		border-color: #f1c0c0;
	}

	.muted {
		font-size: 0.9rem;
		color: #587265;
	}

	.kataster-empty {
		padding: 0.35rem 0.15rem 0;
	}

	.kataster-links {
		display: flex;
		flex-direction: column;
		align-items: center;
		gap: 0.38rem;
	}

	.kataster-links a {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 48px;
		min-width: 48px;
		padding: 0.45rem 0.85rem;
		border: 2px solid #1f5a42;
		border-radius: 12px;
		background: #1f5a42;
		text-decoration: none;
		color: #ffffff;
	}
</style>
