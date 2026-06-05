<script lang="ts">
	import { page } from '$app/stores';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import FscBadge from '$lib/components/shared/FscBadge.svelte';
	import type {
		LandPropertyListDto,
		PropertyCadasterLinkDto
	} from '$lib/dtos/land-property/land-property-list.dto';

	let { data }: { data: { properties: LandPropertyListDto[]; total: number; skip: number; take: number; searchText: string; county: string; isFsc: boolean; counties: string[] } } = $props();

	let expandedPropertyIds = $state<string[]>([]);
	let searchQuery = $state(data.searchText ?? '');
	let selectedCounty = $state(data.county ?? '');
	let countyDropdownOpen = $state(false);
	let showFscOnly = $state(data.isFsc ?? false);

	const companyId = $derived($page.params.CompanyId ?? '');
	let properties = $derived(data.properties ?? []);
	let total = $derived(data.total ?? 0);
	let skip = $derived(data.skip ?? 0);
	let take = $derived(data.take ?? 20);
	let totalPages = $derived(Math.ceil(total / take));
	let currentPage = $derived(totalPages > 0 ? Math.floor(skip / take) + 1 : 0);

	let availableCounties = $derived(data.counties ?? []);

	function applyFilters() {
		const url = new URL($page.url);
		url.searchParams.set('skip', '0');
		if (searchQuery.trim()) url.searchParams.set('searchText', searchQuery.trim());
		else url.searchParams.delete('searchText');
		if (selectedCounty) url.searchParams.set('county', selectedCounty);
		else url.searchParams.delete('county');
		if (showFscOnly) url.searchParams.set('isFsc', 'true');
		else url.searchParams.delete('isFsc');
		goto(url.toString(), { replaceState: true });
	}

	function clearAllFilters() {
		searchQuery = '';
		selectedCounty = '';
		showFscOnly = false;
		const url = new URL($page.url);
		url.searchParams.delete('skip');
		url.searchParams.delete('searchText');
		url.searchParams.delete('county');
		url.searchParams.delete('isFsc');
		goto(url.toString(), { replaceState: true });
	}

	function goToPage(p: number) {
		const url = new URL($page.url);
		url.searchParams.set('skip', String((p - 1) * take));
		url.searchParams.set('take', String(take));
		goto(url.toString(), { replaceState: true });
	}

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
		if (fromDto.length > 0) return fromDto;
		const fromNumbers = Array.isArray(property.cadastralNumbers) ? property.cadastralNumbers : [];
		return fromNumbers.filter(Boolean).map((cadastralNumber) => ({ id: '', cadastralNumber }));
	}

	function toggleExpand(propertyId: string) {
		if (isExpanded(propertyId)) {
			expandedPropertyIds = expandedPropertyIds.filter((id) => id !== propertyId);
			return;
		}
		expandedPropertyIds = [...expandedPropertyIds, propertyId];
	}
</script>

<h1>Kinnistud</h1>

{#if data.properties.length === 0 && total === 0 && !searchQuery.trim() && !selectedCounty && !showFscOnly}
	<p>Selle ettevõtte jaoks kinnistuid ei leitud.</p>
{:else}
	<div class="search-row">
		<label class="search-input">
			<span class="sr-only">Otsi kinnistuid</span>
			<input
				type="search"
				bind:value={searchQuery}
				onkeydown={(e) => { if (e.key === 'Enter') applyFilters(); }}
				placeholder="Otsi kinnistu nime, registrinumbri või katastrinumbri järgi"
			/>
		</label>
		<label class="county-filter">
			<span class="sr-only">Filtreeri maakonna järgi</span>
			<div class="custom-dropdown">
				<button
					type="button"
					class="dropdown-trigger"
					onclick={() => (countyDropdownOpen = !countyDropdownOpen)}
					aria-expanded={countyDropdownOpen}
				>
					<span>{selectedCounty || 'Kõik maakonnad'}</span>
					<svg
						class="dropdown-arrow"
						class:open={countyDropdownOpen}
						viewBox="0 0 24 24"
						fill="none"
						stroke="currentColor"
						stroke-width="2.5"
						stroke-linecap="round"
						stroke-linejoin="round"
					>
						<path d="M6 9l6 6 6-6" />
					</svg>
				</button>
				{#if countyDropdownOpen}
					<div class="dropdown-menu">
						<button
							type="button"
							class="dropdown-option"
							class:selected={selectedCounty === ''}
							onclick={() => {
								selectedCounty = '';
								countyDropdownOpen = false;
								applyFilters();
							}}
						>
							Kõik maakonnad
						</button>
						{#each availableCounties as county (county)}
							<button
								type="button"
								class="dropdown-option"
								class:selected={selectedCounty === county}
								onclick={() => {
									selectedCounty = county;
									countyDropdownOpen = false;
									applyFilters();
								}}
							>
								{county}
							</button>
						{/each}
					</div>
				{/if}
			</div>
		</label>
		{#if searchQuery.trim()}
			<button type="button" class="clear-search" onclick={() => { searchQuery = ''; applyFilters(); }}
				>Tühjenda otsing</button
			>
		{/if}
		{#if selectedCounty}
			<button type="button" class="clear-search" onclick={() => { selectedCounty = ''; applyFilters(); }}
				>Tühjenda maakond</button
			>
		{/if}
		<button
			type="button"
			class="fsc-filter"
			class:active={showFscOnly}
			aria-pressed={showFscOnly}
			onclick={() => { showFscOnly = !showFscOnly; applyFilters(); }}
		>
			<span class="switch-track">
				<span class="switch-knob"></span>
			</span>
			<span>Ainult FSC kinnistud</span>
		</button>
		<button type="button" class="search-btn" onclick={applyFilters}>Otsi</button>
		{#if searchQuery.trim() || selectedCounty || showFscOnly}
			<button type="button" class="clear-all-btn" onclick={clearAllFilters}>Tühjenda filtrid</button>
		{/if}
	</div>

	{#if data.properties.length === 0}
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
						<th>FSC</th>
						<th>Katastrinumbrid</th>
						<th class="actions"></th>
					</tr>
				</thead>
				<tbody>
					{#each properties as property (property.id)}
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
							<td><FscBadge isFsc={property.isFsc} /></td>
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
								<td colspan="7">
									<div class="details-actions">
										<a
											href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
												CompanyId: companyId,
												LandPropertyId: property.id
											})}>Ava kinnistu</a
										>
									</div>
									<div class="details-grid">
										<p><strong>ID:</strong> {property.id}</p>
										<p><strong>Vald:</strong> {property.parish || '—'}</p>
										<p><strong>Küla:</strong> {property.village || '—'}</p>
										<p><strong>Ostukuupäev:</strong> {formatDate(property.boughtDate ?? null)}</p>
										<p><strong>Müügikuupäev:</strong> {formatDate(property.soldDate ?? null)}</p>
										<p><strong>Ettevõte:</strong> {property.companyName || '—'}</p>
										<p><strong>FSC:</strong> <FscBadge isFsc={property.isFsc} /></p>
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

	{#if totalPages > 1}
		<div class="pagination">
			<button class="pagination-btn" disabled={currentPage === 1} onclick={() => goToPage(currentPage - 1)}>
				Eelmine
			</button>
			{#each Array(totalPages) as _, i}
				<button
					class="pagination-btn"
					class:active={currentPage === i + 1}
					onclick={() => goToPage(i + 1)}
					aria-current={currentPage === i + 1 ? 'page' : undefined}
				>
					{i + 1}
				</button>
			{/each}
			<button class="pagination-btn" disabled={currentPage === totalPages} onclick={() => goToPage(currentPage + 1)}>
				Järgmine
			</button>
			<span class="pagination-info">Lehekülg {currentPage} / {totalPages}</span>
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
		flex-wrap: wrap;
	}

	.search-input {
		flex: 1;
		min-width: 200px;
	}

	.custom-dropdown {
		position: relative;
	}

	.dropdown-trigger {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.5rem;
		padding: 0.5rem 0.75rem;
		min-width: 160px;
		border: 1px solid #cad6cf;
		border-radius: 0.6rem;
		background: #fcfdfc;
		color: #1f2a24;
		font-size: 0.9rem;
		cursor: pointer;
		transition:
			border-color 0.15s ease,
			box-shadow 0.15s ease;
	}

	.dropdown-trigger:hover {
		border-color: #96b1a4;
	}

	.dropdown-trigger:focus {
		outline: none;
		border-color: #1f5a42;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.12);
	}

	.dropdown-arrow {
		width: 1rem;
		height: 1rem;
		color: #56645d;
		transition: transform 0.2s ease;
	}

	.dropdown-arrow.open {
		transform: rotate(180deg);
	}

	.dropdown-menu {
		position: absolute;
		top: calc(100% + 4px);
		left: 0;
		right: 0;
		z-index: 50;
		background: #fcfdfc !important;
		border: 1px solid #cad6cf;
		border-radius: 0.6rem;
		box-shadow: 0 4px 14px rgba(21, 41, 32, 0.12);
		max-height: 240px;
		overflow-y: auto;
	}

	.dropdown-option {
		display: block;
		width: 100%;
		padding: 0.6rem 0.75rem;
		border: none !important;
		background: transparent !important;
		color: #1f2a24;
		font-size: 0.9rem;
		text-align: left;
		cursor: pointer;
		transition: background 0.15s ease;
	}

	.dropdown-option:hover {
		background: #174834 !important;
		color: #ffffff !important;
	}

	.dropdown-option.selected {
		background: #1f5a42 !important;
		color: #ffffff !important;
		font-weight: 600;
	}

	.dropdown-option:first-child {
		border-radius: 0.6rem 0.6rem 0 0;
	}

	.dropdown-option:last-child {
		border-radius: 0 0 0.6rem 0.6rem;
	}

	.dropdown-menu:has(.dropdown-option:first-child:last-child) .dropdown-option {
		border-radius: 0.6rem;
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

	.search-btn {
		white-space: nowrap;
		background: #1f5a42;
		color: #fff;
		border-color: #1f5a42;
	}

	.search-btn:hover {
		background: #174834;
	}

	.clear-all-btn {
		white-space: nowrap;
		color: #991b1b;
		border-color: #fca5a5;
	}

	.clear-all-btn:hover {
		background: #fee2e2;
		border-color: #f87171;
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

	button:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.pagination {
		display: flex;
		justify-content: center;
		align-items: center;
		gap: 0.25rem;
		margin-top: 1rem;
		flex-wrap: wrap;
	}

	.pagination-btn {
		min-width: 2.2rem;
		height: 2.2rem;
		padding: 0 0.5rem;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		font-size: 0.85rem;
		background: #fff;
		color: #1f2a24;
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
		cursor: pointer;
		transition: background 0.15s ease, border-color 0.15s ease;
	}

	.pagination-btn:hover:not(:disabled) {
		background: #f9fafb;
	}

	.pagination-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.pagination-btn.active {
		background: #1f5a42 !important;
		color: #fff !important;
		border-color: #1f5a42 !important;
		font-weight: 700;
	}

	.pagination-btn.active:hover {
		background: #174834 !important;
	}

	.pagination-info {
		margin-left: 0.75rem;
		font-size: 0.85rem;
		color: #56645d;
		white-space: nowrap;
		align-self: center;
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
		background: #f4f7f5;
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
		background: #1f5a42;
		border: 1px solid #1f5a42;
		border-radius: 0.5rem;
		padding: 0.35rem 0.7rem;
		text-decoration: none;
		color: #ffffff;
		transition:
			background 0.2s ease,
			border-color 0.2s ease;
	}

	.details-actions a:hover {
		background: #174834;
		border-color: #174834;
		color: #ffffff;
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

	.fsc-filter {
		display: inline-flex;
		align-items: center;
		gap: 0.5rem;
		white-space: nowrap;
		cursor: pointer;
	}

	.switch-track {
		position: relative;
		display: inline-block;
		width: 2.4rem;
		height: 1.35rem;
		border-radius: 9999px;
		background: #cad6cf;
		border: 1px solid #96b1a4;
		transition: background 0.2s ease, border-color 0.2s ease;
		flex-shrink: 0;
	}

	.fsc-filter.active .switch-track {
		background: #1f5a42;
		border-color: #174834;
	}

	.switch-knob {
		position: absolute;
		top: 0.125rem;
		left: 0.125rem;
		width: 1rem;
		height: 1rem;
		border-radius: 50%;
		background: #ffffff;
		box-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
		transition: transform 0.2s ease;
	}

	.fsc-filter.active .switch-knob {
		transform: translateX(1.05rem);
	}
</style>
