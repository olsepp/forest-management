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

{#if isLoading}
	<p>Loading cadaster details...</p>
{:else if errorMessage && !cadaster}
	<p class="message error">{errorMessage}</p>
{:else if cadaster}
	<div class="detail-page">
		<p class="breadcrumb">
			<a href={`/admin/${$page.params.CompanyId}/landproperty`}>← Back to properties</a>
		</p>

		<header class="page-head">
			<div>
				<p class="eyebrow">Cadaster</p>
				<h1>{cadaster.cadastralNumber}</h1>
				<p class="subtitle">Manage cadastral values and review connected forest stand records.</p>
			</div>
			<div class="head-actions">
				<a class="btn-log-activity" href={`/admin/${$page.params.CompanyId}/cadaster/${cadaster.id}/activity/new`}>Log activity</a>
				<button type="button" class="mode-btn" onclick={() => (isEditMode = !isEditMode)} disabled={isSaving}>
					{isEditMode ? 'Cancel editing' : 'Enable editing'}
				</button>
			</div>
		</header>

		<section class="meta-grid">
			<article class="meta-card">
				<p class="meta-label">Cadaster ID</p>
				<p class="meta-value mono">{cadaster.id}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Land property</p>
				<p class="meta-value"><a href={`/admin/${$page.params.CompanyId}/landproperty/${cadaster.landPropertyId}`}>{cadaster.landPropertyName}</a></p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Forest stand count</p>
				<p class="meta-value">{forestStands.length}</p>
			</article>
		</section>

		<form id="cadaster-form" onsubmit={saveCadaster} class="detail-form">
			<section class="form-section">
				<h2>General values</h2>
				<div class="form-grid">
					<label><span>Cadastral number</span><input type="text" bind:value={form.cadastralNumber} required readonly={!isEditMode} /></label>
					<label><span>Soil quality index</span><input type="number" min="0" max="4" bind:value={form.soilQualityIndex} readonly={!isEditMode} /></label>
					<label><span>Calculated volume</span><input type="number" bind:value={form.calculatedVolume} readonly={!isEditMode} /></label>
					<label><span>Volume growth</span><input type="number" step="any" bind:value={form.volumeGrowth} readonly={!isEditMode} /></label>
				</div>
			</section>

			<section class="form-section">
				<h2>Area breakdown</h2>
				<div class="form-grid">
					<label><span>Forest area</span><input type="number" step="any" bind:value={form.forestArea} readonly={!isEditMode} /></label>
					<label><span>Arable area</span><input type="number" step="any" bind:value={form.arableArea} readonly={!isEditMode} /></label>
					<label><span>Grassland area</span><input type="number" step="any" bind:value={form.grasslandArea} readonly={!isEditMode} /></label>
					<label><span>Yard area</span><input type="number" step="any" bind:value={form.yardArea} readonly={!isEditMode} /></label>
					<label><span>Building footprint area</span><input type="number" step="any" bind:value={form.buildingFootprintArea} readonly={!isEditMode} /></label>
					<label><span>Underwater area</span><input type="number" step="any" bind:value={form.underwaterArea} readonly={!isEditMode} /></label>
					<label><span>Other area</span><input type="number" step="any" bind:value={form.otherArea} readonly={!isEditMode} /></label>
				</div>
			</section>

			<div class="form-actions">
				<button class="btn-save" type="submit" disabled={isSaving || !isEditMode}>{isSaving ? 'Saving...' : 'Save changes'}</button>
			</div>
		</form>

		<section class="form-section">
			<h2>Forest stands in this cadaster</h2>
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
									<td class="actions"><a href={`/admin/${$page.params.CompanyId}/foreststand/${stand.id}`}>Open</a></td>
								</tr>
							{/each}
						</tbody>
					</table>
				</div>
			{/if}
		</section>

		{#if errorMessage}
			<p class="message error">{errorMessage}</p>
		{/if}
		{#if successMessage}
			<p class="message success">{successMessage}</p>
		{/if}
	</div>
{/if}

<style>
	.detail-page {
		display: grid;
		gap: 1rem;
		padding: 0.9rem;
		border: 1px solid #d7e3dc;
		border-radius: 1rem;
		background: #eef5f1;
	}
	.breadcrumb { margin: 0; }
	.page-head { display: flex; justify-content: space-between; align-items: flex-start; gap: 1rem; }
	.eyebrow { margin: 0; font-size: 0.78rem; text-transform: uppercase; letter-spacing: 0.08em; font-weight: 700; }
	h1 { margin: 0.2rem 0 0.35rem; font-size: 1.6rem; }
	.subtitle { margin: 0; }
	.head-actions { display: flex; gap: 0.6rem; align-items: center; }
	.btn-log-activity {
		white-space: nowrap;
		padding: 0.58rem 1rem;
		background: #1f5a42;
		color: #f7fcf9;
		border: 1px solid #184835;
		border-radius: 0.65rem;
		font-weight: 700;
		box-shadow: 0 8px 16px rgba(31, 90, 66, 0.24);
		text-decoration: none;
	}
	.btn-log-activity:hover { background: #174a35; color: #ffffff; text-decoration: none; }
	.mode-btn {
		padding: 0.58rem 1rem;
		background: #2f5f49;
		color: #f6fbf8;
		border: 1px solid #264735;
		border-radius: 0.65rem;
		box-shadow: 0 6px 14px rgba(29, 61, 46, 0.2);
	}
	.mode-btn:hover { background: #274f3d; }
	.meta-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 0.8rem; }
	.meta-card { padding: 0.9rem; border: 1px solid #c9dace; border-radius: 0.75rem; background: #f4faf6; }
	.meta-label { margin: 0; font-size: 0.75rem; text-transform: uppercase; letter-spacing: 0.08em; }
	.meta-value { margin: 0.35rem 0 0; font-size: 1rem; font-weight: 600; }
	.mono { font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', monospace; font-size: 0.88rem; }
	.detail-form { display: grid; gap: 1rem; }
	.form-section {
		padding: 1rem;
		border: 1px solid #cadbcf;
		border-radius: 0.85rem;
		background: #f9fcfa;
		box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.9);
	}
	h2 { margin: 0 0 0.8rem; font-size: 1.03rem; }
	.form-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); gap: 0.75rem 1rem; }
	label { display: flex; flex-direction: column; gap: 0.35rem; }
	.form-actions { display: flex; justify-content: flex-end; }
	.btn-save {
		padding: 0.62rem 1.1rem;
		background: #1f5a42;
		color: #f8fdfb;
		border: 1px solid #184835;
		font-weight: 700;
		border-radius: 0.65rem;
		box-shadow: 0 8px 16px rgba(31, 90, 66, 0.24);
	}
	.btn-save:hover { background: #174a35; }
	.message { margin: 0; padding: 0.7rem 0.9rem; border-radius: 0.65rem; }
	.error { background: #fdebec; }
	.success { background: #e6f7ea; }
	.table-wrapper { overflow-x: auto; }
	table { width: 100%; border-collapse: collapse; background: #fff; }
	th, td { padding: 0.75rem; border-bottom: 1px solid #e5e7eb; text-align: left; vertical-align: top; }
	th.actions, td.actions { text-align: right; white-space: nowrap; }
</style>
