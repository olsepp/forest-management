<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
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

	function openPropertyDetails(propertyId: string): void {
		void goto(
			resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
				CompanyId: companyId,
				LandPropertyId: propertyId
			})
		);
	}

	function handleRowKeydown(event: KeyboardEvent, propertyId: string): void {
		if (event.key === 'Enter' || event.key === ' ') {
			event.preventDefault();
			openPropertyDetails(propertyId);
		}
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
					errorMessage = 'Unauthorized. Please sign in again.';
					return;
				}

				errorMessage = 'Failed to load properties.';
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
			errorMessage = 'Failed to load properties.';
		} finally {
			isLoading = false;
		}
	}

	onMount(loadData);
</script>

<section class="employee-card page-intro">
	<p class="kicker">Properties</p>
	<h1>Active land properties</h1>
	<p>Search by name, registration number, or cadastral number.</p>
</section>

{#if isLoading}
	<div class="employee-state-block is-loading">Loading properties…</div>
{:else if errorMessage}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Your session may have expired.</span>
		{/if}
	</div>
{:else}
	<section class="filters employee-card" aria-label="Property filters">
		<label for="property-search" class="filter-label">Search</label>
		<input
			id="property-search"
			type="search"
			bind:value={searchQuery}
			placeholder="Name, registration number, cadastral number"
		/>
	</section>

	{#if filteredProperties.length === 0}
		<div class="employee-state-block is-empty">No properties match the current search.</div>
	{:else}
		<section class="mobile-list employee-stack-cards" aria-label="Property list cards">
			{#each filteredProperties as property (property.id)}
				<article
					class="employee-card property-card"
					tabindex="0"
					role="link"
					onclick={() => openPropertyDetails(property.id)}
					onkeydown={(event) => handleRowKeydown(event, property.id)}
					aria-label={`Open property ${property.name}`}
				>
					<div class="card-top">
						<h2>{property.name}</h2>
						<span class={`status ${statusClass(property.status)}`}>{statusLabel(property.status)}</span>
					</div>
					<p><strong>Registration:</strong> {property.registrationNumber}</p>
					<p><strong>County:</strong> {property.county || '—'}</p>
					<p><strong>Cadasters:</strong></p>
					{#if cadastersForProperty(property).length === 0}
						<p class="muted">No cadasters found.</p>
					{:else}
						<div class="cadaster-links">
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

		<div class="desktop-table employee-table-wrap" aria-label="Property list table">
			<table>
				<thead>
					<tr>
						<th>Property</th>
						<th>Registration</th>
						<th>County</th>
						<th>Status</th>
						<th>Cadastral numbers</th>
					</tr>
				</thead>
				<tbody>
					{#each filteredProperties as property (property.id)}
						<tr
							class="clickable-row"
							tabindex="0"
							role="link"
							onclick={() => openPropertyDetails(property.id)}
							onkeydown={(event) => handleRowKeydown(event, property.id)}
							aria-label={`Open property ${property.name}`}
						>
							<td>
								<a
									href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
										CompanyId: companyId,
										LandPropertyId: property.id
									})}
								>
									{property.name}
								</a>
							</td>
							<td>{property.registrationNumber}</td>
							<td>{property.county || '—'}</td>
							<td>
							<span class={`status ${statusClass(property.status)}`}>{statusLabel(property.status)}</span>
							</td>
							<td>
								{#if cadastersForProperty(property).length === 0}
									—
								{:else}
									<div class="cadaster-links">
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
							</td>
						</tr>
					{/each}
				</tbody>
			</table>
		</div>
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
		gap: 0.45rem;
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

	.cadaster-links {
		display: flex;
		flex-direction: column;
		align-items: flex-start;
		gap: 0.38rem;
	}

	.cadaster-links a {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 48px;
		min-width: 48px;
		padding: 0.5rem 1.5rem;
		border: 2px solid #1f5a42;
		border-radius: 0.75rem;
		background: #1f5a42;
		text-decoration: none;
		color: #ffffff;
		font-size: 1.1rem;
		font-weight: 600;
		letter-spacing: 0.02em;
		box-shadow: 0 4px 12px rgba(31, 90, 66, 0.3);
		cursor: pointer;

		/* Touch-specific */
		touch-action: manipulation;
		-webkit-tap-highlight-color: transparent;
		user-select: none;
	}

	.cadaster-links a:active {
		background: #174d38;
		box-shadow: 0 2px 6px rgba(31, 90, 66, 0.2);
		transform: scale(0.97);
	}

	.desktop-table {
		display: none;
	}

	:global(.employee-table-wrap table) {
		width: 100%;
		border-collapse: separate;
		border-spacing: 0 0.55rem;
	}

	:global(.employee-table-wrap table thead th) {
		padding: 0.2rem 0.75rem 0.35rem;
	}

	:global(.employee-table-wrap table tbody td) {
		background: #ffffff;
		padding: 0.78rem 0.75rem;
		border-top: 2px solid #a9c8b9;
		border-bottom: 2px solid #a9c8b9;
	}

	:global(.employee-table-wrap table tbody tr td:first-child) {
		border-left: 2px solid #a9c8b9;
		border-radius: 0.8rem 0 0 0.8rem;
	}

	:global(.employee-table-wrap table tbody tr td:last-child) {
		border-right: 2px solid #a9c8b9;
		border-radius: 0 0.8rem 0.8rem 0;
	}

	:global(.employee-table-wrap table tbody tr.clickable-row) {
		cursor: pointer;
		transition:
			background-color 0.18s ease,
			box-shadow 0.18s ease,
			transform 0.18s ease;
	}

	:global(.employee-table-wrap table tbody tr.clickable-row:active td) {
		background: #edf7f2;
		border-top-color: #4f8b70;
		border-bottom-color: #4f8b70;
	}

	:global(.employee-table-wrap table tbody tr.clickable-row:active td:first-child) {
		border-left-color: #4f8b70;
		box-shadow: inset 4px 0 0 #2e6d52;
	}

	:global(.employee-table-wrap table tbody tr.clickable-row:active td:last-child) {
		border-right-color: #4f8b70;
	}

	:global(.employee-table-wrap table tbody tr.clickable-row:focus-visible) {
		outline: none;
	}

	:global(.employee-table-wrap table tbody tr.clickable-row:focus-visible td) {
		background: #edf7f2;
		border-top-color: #9fc5b2;
		border-bottom-color: #9fc5b2;
	}

	:global(.employee-table-wrap table tbody tr.clickable-row:focus-visible td:first-child) {
		border-left-color: #9fc5b2;
		box-shadow:
			inset 4px 0 0 #2e6d52,
			0 0 0 2px rgba(31, 90, 66, 0.22);
	}

	:global(.employee-table-wrap table tbody tr.clickable-row:focus-visible td:last-child) {
		border-right-color: #9fc5b2;
		box-shadow: 0 0 0 2px rgba(31, 90, 66, 0.22);
	}

	@media (min-width: 768px) {
		.mobile-list {
			display: none;
		}

		.desktop-table {
			display: block;
		}

		h1 {
			font-size: 1.38rem;
		}
	}
</style>
