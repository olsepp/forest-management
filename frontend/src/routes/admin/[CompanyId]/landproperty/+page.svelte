<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type LandPropertyListDto = {
		id: string;
		name: string;
		registrationNumber: number;
		county: string;
		status: 'Active' | 'Inactive' | 'Sold';
		cadastralNumbers: string[];
	};

	type LandPropertyDto = {
		id: string;
		name: string;
		registrationNumber: number;
		county: string;
		parish: string;
		village: string;
		boughtDate: string | null;
		soldDate: string | null;
		status: 'Active' | 'Inactive' | 'Sold';
		companyId: string;
		companyName: string;
		cadasters: {
			id: string;
			cadastralNumber: string;
			forestArea: number;
			forestStandCount: number;
		}[];
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let errorMessage = $state('');
	let properties = $state<LandPropertyListDto[]>([]);
	let expandedPropertyIds = $state<string[]>([]);
	let propertyDetailsById = $state<Record<string, LandPropertyDto>>({});
	let loadingDetailsById = $state<Record<string, boolean>>({});
	let detailsErrorById = $state<Record<string, string>>({});
	let cadastralNumbersByPropertyId = $state<Record<string, string[]>>({});

	function normalizeStatus(status: LandPropertyListDto['status'] | number | string | null | undefined): string {
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

	function statusLabel(status: LandPropertyListDto['status'] | number | string | null | undefined): string {
		const normalized = normalizeStatus(status);
		if (normalized === 'active') return 'Active';
		if (normalized === 'sold') return 'Sold';
		return 'Inactive';
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

	function tableCadastralNumbers(property: LandPropertyListDto): string {
		const direct = Array.isArray(property.cadastralNumbers) ? property.cadastralNumbers : [];
		const fallback = cadastralNumbersByPropertyId[property.id] ?? [];
		const numbers = direct.length > 0 ? direct : fallback;
		return numbers.length > 0 ? numbers.join(', ') : '—';
	}

	async function loadMissingCadastralNumbers(list: LandPropertyListDto[]) {
		const missing = list.filter(
			(item) => !Array.isArray(item.cadastralNumbers) || item.cadastralNumbers.length === 0
		);

		if (missing.length === 0) return;

		const token = await authService.ensureValidToken();

		const results = await Promise.all(
			missing.map(async (item) => {
				const response = await fetch(`${apiBaseUrl}/api/cadasters/by-land-property/${item.id}`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				});

				if (!response.ok) {
					return { propertyId: item.id, numbers: [] as string[] };
				}

				const cadasters = (await response.json()) as { cadastralNumber?: string }[];
				const numbers = Array.isArray(cadasters)
					? cadasters.map((c) => c.cadastralNumber).filter((n): n is string => Boolean(n))
					: [];

				return { propertyId: item.id, numbers };
			})
		);

		const nextMap = { ...cadastralNumbersByPropertyId };
		for (const item of results) {
			nextMap[item.propertyId] = item.numbers;
		}
		cadastralNumbersByPropertyId = nextMap;
	}

	async function loadPropertyDetails(propertyId: string) {
		if (propertyDetailsById[propertyId] || loadingDetailsById[propertyId]) {
			return;
		}

		loadingDetailsById = { ...loadingDetailsById, [propertyId]: true };
		detailsErrorById = { ...detailsErrorById, [propertyId]: '' };

		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/landproperties/${propertyId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				detailsErrorById = {
					...detailsErrorById,
					[propertyId]: 'Failed to load property details'
				};
				return;
			}

			const detail: LandPropertyDto = await response.json();

			// Some backends return land property details without populated `cadasters`.
			// Fallback to the dedicated endpoint to ensure cadasters are shown.
			if (!Array.isArray(detail.cadasters) || detail.cadasters.length === 0) {
				const cadastersResponse = await fetch(
					`${apiBaseUrl}/api/cadasters/by-land-property/${propertyId}`,
					{
						headers: {
							Authorization: `Bearer ${token}`
						}
					}
				);

				if (cadastersResponse.ok) {
					const cadasters = (await cadastersResponse.json()) as LandPropertyDto['cadasters'];
					detail.cadasters = Array.isArray(cadasters) ? cadasters : [];
				}
			}

			propertyDetailsById = { ...propertyDetailsById, [propertyId]: detail };
			cadastralNumbersByPropertyId = {
				...cadastralNumbersByPropertyId,
				[propertyId]: Array.isArray(detail.cadasters)
					? detail.cadasters
							.map((cadaster) => cadaster.cadastralNumber)
							.filter((n): n is string => Boolean(n))
					: []
			};
		} catch {
			detailsErrorById = { ...detailsErrorById, [propertyId]: 'Failed to load property details' };
		} finally {
			loadingDetailsById = { ...loadingDetailsById, [propertyId]: false };
		}
	}

	async function toggleExpand(propertyId: string) {
		if (isExpanded(propertyId)) {
			expandedPropertyIds = expandedPropertyIds.filter((id) => id !== propertyId);
			return;
		}

		expandedPropertyIds = [...expandedPropertyIds, propertyId];
		await loadPropertyDetails(propertyId);
	}

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;

			const companyId = $page.params.CompanyId;
			if (!companyId) {
				errorMessage = 'Missing company id';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/landproperties/search?companyId=${companyId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 401 ? 'Unauthorized. Please sign in again.' : 'Failed to load land properties';
				return;
			}

			const data = (await response.json()) as LandPropertyListDto[];
			properties = Array.isArray(data)
				? data.map((item) => ({
					...item,
					cadastralNumbers: Array.isArray(item.cadastralNumbers) ? item.cadastralNumbers : []
				}))
				: [];

			await loadMissingCadastralNumbers(properties);
		} catch {
			errorMessage = 'Failed to load land properties';
		} finally {
			isLoading = false;
		}
	});
</script>

<h1>Land properties</h1>

{#if isLoading}
	<p>Loading land properties...</p>
{:else if errorMessage}
	<p>{errorMessage}</p>
{:else if properties.length === 0}
	<p>No land properties found for this company.</p>
{:else}
	<div class="table-wrapper">
		<table>
			<thead>
				<tr>
					<th>Property</th>
					<th>Registration</th>
					<th>County</th>
					<th>Status</th>
					<th>Cadastral numbers</th>
					<th class="actions">Details</th>
				</tr>
			</thead>
			<tbody>
				{#each properties as property}
					<tr>
						<td>{property.name}</td>
						<td>{property.registrationNumber}</td>
						<td>{property.county}</td>
						<td>
							<span class={`status status-${normalizeStatus(property.status)}`}>{statusLabel(
								property.status
							)}</span>
						</td>
						<td>{tableCadastralNumbers(property)}</td>
						<td class="actions">
							<button type="button" onclick={() => toggleExpand(property.id)}>
								{isExpanded(property.id) ? 'Hide' : 'View'}
							</button>
						</td>
					</tr>

					{#if isExpanded(property.id)}
						<tr class="details-row">
							<td colspan="6">
								{#if loadingDetailsById[property.id]}
									<p>Loading details...</p>
								{:else if detailsErrorById[property.id]}
									<p>{detailsErrorById[property.id]}</p>
								{:else if propertyDetailsById[property.id]}
									{@const detail = propertyDetailsById[property.id]}
									<div class="details-actions">
										<a href={`/admin/${$page.params.CompanyId}/landproperty/${property.id}`}
											>Open property page</a
										>
									</div>
									<div class="details-grid">
										<p><strong>ID:</strong> {detail.id}</p>
										<p><strong>Parish:</strong> {detail.parish || '—'}</p>
										<p><strong>Village:</strong> {detail.village || '—'}</p>
										<p><strong>Bought date:</strong> {formatDate(detail.boughtDate)}</p>
										<p><strong>Sold date:</strong> {formatDate(detail.soldDate)}</p>
										<p><strong>Company:</strong> {detail.companyName}</p>
									</div>

									<h4>Cadasters</h4>
									{#if detail.cadasters.length === 0}
										<p>No cadasters found.</p>
									{:else}
										<ul>
											{#each detail.cadasters as cadaster}
												<li>{cadaster.cadastralNumber}</li>
											{/each}
										</ul>
									{/if}
								{/if}
							</td>
						</tr>
					{/if}
				{/each}
			</tbody>
		</table>
	</div>
{/if}

<style>
	.table-wrapper {
		overflow-x: auto;
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

	ul {
		margin: 0.25rem 0 0;
		padding-left: 1.2rem;
	}
</style>
