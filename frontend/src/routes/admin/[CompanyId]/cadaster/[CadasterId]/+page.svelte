<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type ForestStandListDto = {
		id: string;
		number: number;
		area: number;
		totalVolume: number;
		isActive: boolean;
	};

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
		forestStands: ForestStandListDto[];
	};

	type CadasterUpdateDto = {
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
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let isSaving = $state(false);
	let isEditMode = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
	let cadaster = $state<CadasterDto | null>(null);
	let forestStands = $state<ForestStandListDto[]>([]);

	let form = $state({
		cadastralNumber: '',
		forestArea: '',
		arableArea: '',
		grasslandArea: '',
		yardArea: '',
		buildingFootprintArea: '',
		underwaterArea: '',
		otherArea: '',
		soilQualityIndex: '',
		calculatedVolume: '',
		volumeGrowth: ''
	});

	function toStringNumber(value: number | null | undefined): string {
		if (typeof value !== 'number' || Number.isNaN(value)) return '';
		return String(value);
	}

	function parseNumber(value: string): number {
		const parsed = Number(value);
		return Number.isFinite(parsed) ? parsed : 0;
	}

	function fillForm(detail: CadasterDto): void {
		form = {
			cadastralNumber: detail.cadastralNumber ?? '',
			forestArea: toStringNumber(detail.forestArea),
			arableArea: toStringNumber(detail.arableArea),
			grasslandArea: toStringNumber(detail.grasslandArea),
			yardArea: toStringNumber(detail.yardArea),
			buildingFootprintArea: toStringNumber(detail.buildingFootprintArea),
			underwaterArea: toStringNumber(detail.underwaterArea),
			otherArea: toStringNumber(detail.otherArea),
			soilQualityIndex: toStringNumber(detail.soilQualityIndex),
			calculatedVolume: toStringNumber(detail.calculatedVolume),
			volumeGrowth: toStringNumber(detail.volumeGrowth)
		};
	}

	async function loadCadaster() {
		try {
			errorMessage = '';
			successMessage = '';
			isLoading = true;

			const cadasterId = $page.params.CadasterId;
			if (!cadasterId) {
				errorMessage = 'Missing cadaster id';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 404
						? 'Cadaster not found.'
						: response.status === 401
							? 'Unauthorized. Please sign in again.'
							: 'Failed to load cadaster.';
				return;
			}

			const detail = (await response.json()) as CadasterDto;
			cadaster = detail;
			forestStands = Array.isArray(detail.forestStands) ? detail.forestStands : [];
			fillForm(detail);
		} catch {
			errorMessage = 'Failed to load cadaster.';
		} finally {
			isLoading = false;
		}
	}

	async function saveCadaster(event: SubmitEvent) {
		event.preventDefault();
		if (!cadaster || !isEditMode) return;

		isSaving = true;
		errorMessage = '';
		successMessage = '';

		const payload: CadasterUpdateDto = {
			id: cadaster.id,
			cadastralNumber: form.cadastralNumber.trim(),
			forestArea: parseNumber(form.forestArea),
			arableArea: parseNumber(form.arableArea),
			grasslandArea: parseNumber(form.grasslandArea),
			yardArea: parseNumber(form.yardArea),
			buildingFootprintArea: parseNumber(form.buildingFootprintArea),
			underwaterArea: parseNumber(form.underwaterArea),
			otherArea: parseNumber(form.otherArea),
			soilQualityIndex: parseNumber(form.soilQualityIndex),
			calculatedVolume: parseNumber(form.calculatedVolume),
			volumeGrowth: parseNumber(form.volumeGrowth),
			landPropertyId: cadaster.landPropertyId
		};

		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/cadasters/${cadaster.id}`, {
				method: 'PUT',
				headers: {
					Authorization: `Bearer ${token}`,
					'Content-Type': 'application/json'
				},
				body: JSON.stringify(payload)
			});

			if (!response.ok) {
				errorMessage =
					response.status === 400
						? 'Validation failed. Please check your values.'
						: response.status === 404
							? 'Cadaster not found.'
							: 'Failed to save changes.';
				return;
			}

			const updated = (await response.json()) as CadasterDto;
			cadaster = updated;
			forestStands = Array.isArray(updated.forestStands) ? updated.forestStands : [];
			fillForm(updated);
			isEditMode = false;
			successMessage = 'Cadaster updated successfully.';
		} catch {
			errorMessage = 'Failed to save changes.';
		} finally {
			isSaving = false;
		}
	}

	onMount(loadCadaster);
</script>

<h1>Cadaster details</h1>

<p class="breadcrumb">
	<a href={`/admin/${$page.params.CompanyId}/landproperty`}>← Back to properties</a>
</p>

{#if isLoading}
	<p>Loading cadaster details...</p>
{:else if errorMessage && !cadaster}
	<p class="error">{errorMessage}</p>
{:else if cadaster}
	<section class="card">
		<h2>{cadaster.cadastralNumber}</h2>
		<p><strong>ID:</strong> {cadaster.id}</p>
		<p>
			<strong>Land property:</strong>
			<a href={`/admin/${$page.params.CompanyId}/landproperty/${cadaster.landPropertyId}`}
				>{cadaster.landPropertyName}</a
			>
		</p>

		<form onsubmit={saveCadaster} class="form-grid">
			<div class="actions edit-actions">
				<button type="button" onclick={() => (isEditMode = !isEditMode)} disabled={isSaving}>
					{isEditMode ? 'Stop editing' : 'Enable editing'}
				</button>
			</div>

			<label>
				<span>Cadastral number</span>
				<input type="text" bind:value={form.cadastralNumber} required readonly={!isEditMode} />
			</label>

			<label>
				<span>Forest area</span>
				<input type="number" step="any" bind:value={form.forestArea} readonly={!isEditMode} />
			</label>

			<label>
				<span>Arable area</span>
				<input type="number" step="any" bind:value={form.arableArea} readonly={!isEditMode} />
			</label>

			<label>
				<span>Grassland area</span>
				<input type="number" step="any" bind:value={form.grasslandArea} readonly={!isEditMode} />
			</label>

			<label>
				<span>Yard area</span>
				<input type="number" step="any" bind:value={form.yardArea} readonly={!isEditMode} />
			</label>

			<label>
				<span>Building footprint area</span>
				<input type="number" step="any" bind:value={form.buildingFootprintArea} readonly={!isEditMode} />
			</label>

			<label>
				<span>Underwater area</span>
				<input type="number" step="any" bind:value={form.underwaterArea} readonly={!isEditMode} />
			</label>

			<label>
				<span>Other area</span>
				<input type="number" step="any" bind:value={form.otherArea} readonly={!isEditMode} />
			</label>

			<label>
				<span>Soil quality index</span>
				<input type="number" min="0" max="4" bind:value={form.soilQualityIndex} readonly={!isEditMode} />
			</label>

			<label>
				<span>Calculated volume</span>
				<input type="number" bind:value={form.calculatedVolume} readonly={!isEditMode} />
			</label>

			<label>
				<span>Volume growth</span>
				<input type="number" step="any" bind:value={form.volumeGrowth} readonly={!isEditMode} />
			</label>

			<div class="actions">
				<button type="submit" disabled={isSaving || !isEditMode}>
					{isSaving ? 'Saving...' : 'Save changes'}
				</button>
			</div>
		</form>

		{#if errorMessage}
			<p class="error">{errorMessage}</p>
		{/if}

		{#if successMessage}
			<p class="success">{successMessage}</p>
		{/if}
	</section>

	<section class="card forest-stands-card">
		<h3>Forest stands in this cadaster</h3>
		{#if forestStands.length === 0}
			<p>No forest stands found.</p>
		{:else}
			<div class="table-wrapper">
				<table>
					<thead>
						<tr>
							<th>Number</th>
							<th>Area</th>
							<th>Total volume</th>
							<th>Status</th>
							<th class="actions">Open</th>
						</tr>
					</thead>
					<tbody>
						{#each forestStands as stand}
							<tr>
								<td>{stand.number}</td>
								<td>{stand.area}</td>
								<td>{stand.totalVolume}</td>
								<td>{stand.isActive ? 'Active' : 'Inactive'}</td>
								<td class="actions">
									<a href={`/admin/${$page.params.CompanyId}/foreststand/${stand.id}`}>Open</a>
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
			</div>
		{/if}
	</section>
{/if}

<style>
	.breadcrumb {
		margin-top: -0.25rem;
		margin-bottom: 1rem;
	}

	.breadcrumb a {
		color: #0f766e;
		text-decoration: none;
	}

	.breadcrumb a:hover {
		text-decoration: underline;
	}

	.card {
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.75rem;
		background: #fff;
	}

	.forest-stands-card {
		margin-top: 1rem;
	}

	.form-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.75rem 1rem;
		margin-top: 1rem;
	}

	label {
		display: flex;
		flex-direction: column;
		gap: 0.3rem;
	}

	input {
		padding: 0.5rem 0.6rem;
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
	}

	.actions {
		grid-column: 1 / -1;
		display: flex;
		justify-content: flex-end;
	}

	.edit-actions {
		justify-content: flex-start;
	}

	button {
		border: 1px solid #d1d5db;
		background: #fff;
		border-radius: 0.5rem;
		padding: 0.45rem 0.9rem;
		cursor: pointer;
	}

	button:disabled {
		opacity: 0.65;
		cursor: not-allowed;
	}

	.error {
		margin-top: 0.75rem;
		color: #b91c1c;
	}

	.success {
		margin-top: 0.75rem;
		color: #166534;
	}

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

	td.actions a {
		color: #0f766e;
		text-decoration: none;
	}

	td.actions a:hover {
		text-decoration: underline;
	}
</style>
