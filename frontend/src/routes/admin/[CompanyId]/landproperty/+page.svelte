<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { resolve } from '$app/paths';
	import { onMount } from 'svelte';
	import type {
		LandPropertyListDto,
		PropertyCadasterLinkDto
	} from '$lib/dtos/land-property/land-property-list.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let errorMessage = $state('');
	let properties = $state<LandPropertyListDto[]>([]);
	let expandedPropertyIds = $state<string[]>([]);
	let searchQuery = $state('');
	let selectedCounty = $state('');
	let companyId = $derived($page.params.CompanyId ?? '');
	let normalizedSearchQuery = $derived(searchQuery.trim().toLowerCase());
	let availableCounties = $derived.by(() => {
		const counties = properties
			.map((property) => property.county?.trim())
			.filter((county): county is string => Boolean(county));

		return [...new Set(counties)].sort((a, b) => a.localeCompare(b));
	});
	let filteredProperties = $derived.by(() => {
		return properties.filter((property) => {
			const matchesSearch =
				!normalizedSearchQuery || propertyMatchesSearch(property, normalizedSearchQuery);
			const matchesCounty = !selectedCounty || property.county === selectedCounty;

			return matchesSearch && matchesCounty;
		});
	});

	function normalizeStatus(
		status: LandPropertyListDto['status'] | number | string | null | undefined
	): string {
		if (typeof status === 'string') {
			return status.toLowerCase();
		}

		if (typeof status === 'number') {
			if (status === 0) return 'active';
			if (status === 1) return 'inactive';
			if (status === 2) return 'sold';
		}

		return 'inactive';
	}

	function statusLabel(
		status: LandPropertyListDto['status'] | number | string | null | undefined
	): string {
		const normalized = normalizeStatus(status);
		if (normalized === 'active') return 'Aktiivne';
		if (normalized === 'sold') return 'Müüdud';
		return 'Mitteaktiivne';
	}

	function isExpanded(propertyId: string): boolean {
		return expandedPropertyIds.includes(propertyId);
	}

	function formatDate(value: string | null): string {
		if (!value) return '—';

		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';

		return date.toLocaleDateString();
	}

	function tableCadasters(property: LandPropertyListDto): PropertyCadasterLinkDto[] {
		const fromDto = Array.isArray(property.cadasters)
			? property.cadasters.filter((item) => Boolean(item?.cadastralNumber))
			: [];
		if (fromDto.length > 0) {
			return fromDto;
		}

		const fromNumbers = Array.isArray(property.cadastralNumbers) ? property.cadastralNumbers : [];
		return fromNumbers.filter(Boolean).map((cadastralNumber) => ({ id: '', cadastralNumber }));
	}

	function propertySearchableCadastralNumbers(property: LandPropertyListDto): string[] {
		const fromTableMap = tableCadasters(property).map((item) => item.cadastralNumber);
		const fromListDto = Array.isArray(property.cadastralNumbers) ? property.cadastralNumbers : [];

		return [...new Set([...fromTableMap, ...fromListDto])];
	}

	function propertyMatchesSearch(property: LandPropertyListDto, query: string): boolean {
		if (!query) return true;

		const propertyName = property.name?.toLowerCase() ?? '';
		const registrationNumber = String(property.registrationNumber ?? '').toLowerCase();
		const cadastralNumbers = propertySearchableCadastralNumbers(property);

		if (propertyName.includes(query)) return true;
		if (registrationNumber.includes(query)) return true;

		return cadastralNumbers.some((number) => number.toLowerCase().includes(query));
	}

	function toggleExpand(propertyId: string) {
		if (isExpanded(propertyId)) {
			expandedPropertyIds = expandedPropertyIds.filter((id) => id !== propertyId);
			return;
		}

		expandedPropertyIds = [...expandedPropertyIds, propertyId];
	}

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;

			if (!companyId) {
				errorMessage = 'Puudub ettevõtte ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(
				`${apiBaseUrl}/api/landproperties/search?companyId=${companyId}`,
				{
					headers: {
						Authorization: `Bearer ${token}`
					}
				}
			);

			if (!response.ok) {
				errorMessage =
					response.status === 401
						? 'Ligipääs puudub. Logige uuesti sisse.'
						: 'Kinnistute laadimine ebaõnnestus.';
				return;
			}

			const data = (await response.json()) as LandPropertyListDto[];
			properties = Array.isArray(data)
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
		} catch {
			errorMessage = 'Kinnistute laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	});
</script>

<h1>Kinnistud</h1>

{#if isLoading}
	<p>Laetakse kinnistuid...</p>
{:else if errorMessage}
	<p>{errorMessage}</p>
{:else if properties.length === 0}
	<p>Selle ettevõtte jaoks kinnistuid ei leitud.</p>
{:else}
	<div class="search-row">
		<label class="search-input">
			<span class="sr-only">Otsi kinnistuid</span>
			<input
				type="search"
				bind:value={searchQuery}
				placeholder="Otsi kinnistu nime, registrinumbri või katastrinumbri järgi"
			/>
		</label>
		<label class="county-filter">
			<span class="sr-only">Filtreeri maakonna järgi</span>
			<select bind:value={selectedCounty}>
				<option value="">Kõik maakonnad</option>
				{#each availableCounties as county (county)}
					<option value={county}>{county}</option>
				{/each}
			</select>
		</label>
		{#if searchQuery.trim()}
			<button type="button" class="clear-search" onclick={() => (searchQuery = '')}
				>Tühjenda otsing</button
			>
		{/if}
		{#if selectedCounty}
			<button type="button" class="clear-search" onclick={() => (selectedCounty = '')}
				>Tühjenda maakond</button
			>
		{/if}
	</div>

	{#if filteredProperties.length === 0}
		<p>Praegusele otsingule vastavaid kinnistuid ei leitud.</p>
	{:else}
		<div class="table-wrapper">
			<table>
				<thead>
					<tr>
						<th>Kinnistu</th>
						<th>Registrinumber</th>
						<th>Maakond</th>
						<th>Olek</th>
						<th>Katastrinumbrid</th>
						<th class="actions">Detailid</th>
					</tr>
				</thead>
				<tbody>
					{#each filteredProperties as property (property.id)}
						<tr>
							<td>
								<a
									href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
										CompanyId: companyId,
										LandPropertyId: property.id
									})}>{property.name}</a
								>
							</td>
							<td>{property.registrationNumber}</td>
							<td>{property.county}</td>
							<td>
								<span class={`status status-${normalizeStatus(property.status)}`}
									>{statusLabel(property.status)}</span
								>
							</td>
							<td>
								{#if tableCadasters(property).length === 0}
									—
								{:else}
									<div class="cadaster-links">
										{#each tableCadasters(property) as cadaster (`${property.id}:${cadaster.id || cadaster.cadastralNumber}`)}
											{#if cadaster.id}
												<a
													href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
														CompanyId: companyId,
														CadasterId: cadaster.id
													})}>{cadaster.cadastralNumber}</a
												>
											{:else}
												<span>{cadaster.cadastralNumber}</span>
											{/if}
										{/each}
									</div>
								{/if}
							</td>
							<td class="actions">
								<button
									type="button"
									class="expand-toggle"
									onclick={() => toggleExpand(property.id)}
									aria-label={isExpanded(property.id) ? 'Peida detailid' : 'Näita detaile'}
									aria-expanded={isExpanded(property.id)}
								>
									<svg
										class={`expand-icon ${isExpanded(property.id) ? 'open' : ''}`}
										viewBox="0 0 24 24"
										fill="none"
										stroke="currentColor"
										stroke-width="2.75"
										stroke-linecap="round"
										stroke-linejoin="round"
										aria-hidden="true"
									>
										<path d="M6 9l6 6 6-6" />
									</svg>
								</button>
							</td>
						</tr>

						{#if isExpanded(property.id)}
							<tr class="details-row">
								<td colspan="6">
									<div class="details-actions">
										<a
											href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
												CompanyId: companyId,
												LandPropertyId: property.id
											})}>Ava kinnistu leht</a
										>
									</div>
									<div class="details-grid">
										<p><strong>ID:</strong> {property.id}</p>
										<p><strong>Vald:</strong> {property.parish || '—'}</p>
										<p><strong>Küla:</strong> {property.village || '—'}</p>
										<p><strong>Ostukuupäev:</strong> {formatDate(property.boughtDate ?? null)}</p>
										<p><strong>Müügikuupäev:</strong> {formatDate(property.soldDate ?? null)}</p>
										<p><strong>Ettevõte:</strong> {property.companyName || '—'}</p>
									</div>

									<h4>Katastrid</h4>
									{#if tableCadasters(property).length === 0}
										<p>Katastrid puuduvad.</p>
									{:else}
										<ul>
											{#each tableCadasters(property) as cadaster (`${property.id}:${cadaster.id || cadaster.cadastralNumber}`)}
												<li>
													{#if cadaster.id}
														<a
															href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
																CompanyId: companyId,
																CadasterId: cadaster.id
															})}>{cadaster.cadastralNumber}</a
														>
													{:else}
														<span>{cadaster.cadastralNumber}</span>
													{/if}
												</li>
											{/each}
										</ul>
									{/if}
								</td>
							</tr>
						{/if}
					{/each}
				</tbody>
			</table>
		</div>
	{/if}
{/if}

<style>
	.table-wrapper {
		overflow-x: auto;
	}

	.search-row {
		display: flex;
		gap: 0.5rem;
		align-items: center;
		margin-bottom: 0.75rem;
	}

	.search-input {
		flex: 1;
	}

	.county-filter select {
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
		background: #fff;
	}

	.county-filter select:focus {
		outline: 2px solid #99f6e4;
		outline-offset: 1px;
	}

	.search-input input {
		width: 100%;
		padding: 0.5rem 0.75rem;
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
	}

	.search-input input:focus {
		outline: 2px solid #99f6e4;
		outline-offset: 1px;
	}

	.clear-search {
		white-space: nowrap;
	}

	.sr-only {
		position: absolute;
		width: 1px;
		height: 1px;
		padding: 0;
		margin: -1px;
		overflow: hidden;
		clip: rect(0, 0, 0, 0);
		white-space: nowrap;
		border: 0;
	}

	table {
		width: 100%;
		border-collapse: collapse;
		background: #fff;
	}

	th,
	td {
		padding: 0.75rem;
		border-bottom: 1px solid #e5e7eb;
		text-align: left;
		vertical-align: top;
	}

	th.actions,
	td.actions {
		text-align: right;
		white-space: nowrap;
	}

	button {
		border: 1px solid #d1d5db;
		background: #fff;
		border-radius: 0.5rem;
		padding: 0.35rem 0.7rem;
		cursor: pointer;
	}

	button:hover {
		background: #f9fafb;
	}

	.expand-toggle {
		width: 2rem;
		height: 2rem;
		padding: 0;
		display: inline-flex;
		align-items: center;
		justify-content: center;
	}

	.arrow {
		display: inline-block;
		font-size: 1rem;
		line-height: 1;
		transition: transform 0.2s ease;
	}

	.arrow.open {
		transform: rotate(180deg);
	}

	.status {
		display: inline-block;
		padding: 0.2rem 0.5rem;
		border-radius: 9999px;
		font-size: 0.8rem;
		font-weight: 600;
	}

	.status-active {
		background: #dcfce7;
		color: #166534;
	}

	.status-inactive {
		background: #f3f4f6;
		color: #374151;
	}

	.status-sold {
		background: #fee2e2;
		color: #991b1b;
	}

	.details-row td {
		background: #f8fafc;
	}

	.details-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.4rem 1rem;
		margin-bottom: 0.75rem;
	}

	.details-actions {
		display: flex;
		justify-content: flex-end;
		margin-bottom: 0.75rem;
	}

	.details-actions a {
		display: inline-block;
		border: 1px solid #d1d5db;
		background: #fff;
		border-radius: 0.5rem;
		padding: 0.35rem 0.7rem;
		text-decoration: none;
		color: inherit;
	}

	.details-actions a:hover {
		background: #f9fafb;
	}

	.cadaster-links a {
		display: block;
		margin-bottom: 0.2rem;
		color: #0f766e;
		text-decoration: none;
	}

	.cadaster-links a:last-child {
		margin-bottom: 0;
	}

	.cadaster-links a:hover {
		text-decoration: underline;
	}

	ul {
		margin: 0.25rem 0 0;
		padding-left: 1.2rem;
	}
</style>
