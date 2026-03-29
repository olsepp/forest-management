<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import CadastralMap from '$lib/components/shared/CadastralMap.svelte';
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
	const companyId = $derived($page.params.CompanyId ?? '');

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

	function sortForestStandsByNumber(items: ForestStandListDto[]): ForestStandListDto[] {
		return [...items].sort((a, b) => a.number - b.number);
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
				errorMessage = 'Puudub katastri ID.';
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
						? 'Katastrit ei leitud.'
						: response.status === 401
							? 'Ligipääs puudub. Logige uuesti sisse.'
							: 'Katastri laadimine ebaõnnestus.';
				return;
			}

			const detail = (await response.json()) as CadasterDto;
			cadaster = detail;
			forestStands = Array.isArray(detail.forestStands)
				? sortForestStandsByNumber(detail.forestStands)
				: [];
			fillForm(detail);
		} catch {
			errorMessage = 'Katastri laadimine ebaõnnestus.';
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
						? 'Valideerimine ebaõnnestus. Kontrollige sisestatud väärtusi.'
						: response.status === 404
							? 'Katastrit ei leitud.'
							: 'Muudatuste salvestamine ebaõnnestus.';
				return;
			}

			const updated = (await response.json()) as CadasterDto;
			cadaster = updated;
			forestStands = Array.isArray(updated.forestStands)
				? sortForestStandsByNumber(updated.forestStands)
				: [];
			fillForm(updated);
			isEditMode = false;
			successMessage = 'Kataster uuendati edukalt.';
		} catch {
			errorMessage = 'Muudatuste salvestamine ebaõnnestus.';
		} finally {
			isSaving = false;
		}
	}

	onMount(loadCadaster);
</script>

{#if isLoading}
	<p>Laetakse katastri detaile...</p>
{:else if errorMessage && !cadaster}
	<p class="message error">{errorMessage}</p>
{:else if cadaster}
	<div class="detail-page">
		<p class="breadcrumb">
			<a href={resolve('/admin/[CompanyId]/landproperty', { CompanyId: companyId })}
				>← Tagasi kinnistute juurde</a
			>
		</p>

		<header class="page-head">
			<div>
				<p class="eyebrow">Kataster</p>
				<h1>{cadaster.cadastralNumber}</h1>
				<p class="subtitle">Halda katastri väärtusi ja vaata seotud eraldise kirjeid.</p>
			</div>
			<div class="head-actions">
				<a
					class="btn-log-activity"
					href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]/activity/new', {
						CompanyId: companyId,
						CadasterId: cadaster.id
					})}>Logi tegevus</a
				>
				<button type="button" class="mode-btn" onclick={() => (isEditMode = !isEditMode)} disabled={isSaving}>
					{isEditMode ? 'Tühista muutmine' : 'Luba muutmine'}
				</button>
			</div>
		</header>

		<section class="meta-grid">
			<article class="meta-card">
				<p class="meta-label">Katastri ID</p>
				<p class="meta-value mono">{cadaster.id}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Kinnistu</p>
				<p class="meta-value">
					<a
						href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
							CompanyId: companyId,
							LandPropertyId: cadaster.landPropertyId
						})}>{cadaster.landPropertyName}</a
					>
				</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Eraldiste arv</p>
				<p class="meta-value">{forestStands.length}</p>
			</article>
		</section>

		
		<form id="cadaster-form" onsubmit={saveCadaster} class="detail-form">
			<section class="form-section">
				<h2>Üldised väärtused</h2>
				<div class="form-grid">
					<label><span>Katastrinumber</span><input type="text" bind:value={form.cadastralNumber} required readonly={!isEditMode} /></label>
					<label><span>Mullaviljakuse indeks</span><input type="number" min="0" max="4" bind:value={form.soilQualityIndex} readonly={!isEditMode} /></label>
					<label><span>Arvutatud maht</span><input type="number" bind:value={form.calculatedVolume} readonly={!isEditMode} /></label>
					<label><span>Mahukasv</span><input type="number" step="any" bind:value={form.volumeGrowth} readonly={!isEditMode} /></label>
				</div>
			</section>

			<section class="form-section">
				<h2>Pindalade jaotus</h2>
				<div class="form-grid">
					<label><span>Metsamaa pindala</span><input type="number" step="any" bind:value={form.forestArea} readonly={!isEditMode} /></label>
					<label><span>Haritava maa pindala</span><input type="number" step="any" bind:value={form.arableArea} readonly={!isEditMode} /></label>
					<label><span>Rohumaa pindala</span><input type="number" step="any" bind:value={form.grasslandArea} readonly={!isEditMode} /></label>
					<label><span>Õueala pindala</span><input type="number" step="any" bind:value={form.yardArea} readonly={!isEditMode} /></label>
					<label><span>Hoonete alune pindala</span><input type="number" step="any" bind:value={form.buildingFootprintArea} readonly={!isEditMode} /></label>
					<label><span>Veealune pindala</span><input type="number" step="any" bind:value={form.underwaterArea} readonly={!isEditMode} /></label>
					<label><span>Muu pindala</span><input type="number" step="any" bind:value={form.otherArea} readonly={!isEditMode} /></label>
				</div>
			</section>

			<div class="form-actions">
				<button class="btn-save" type="submit" disabled={isSaving || !isEditMode}>{isSaving ? 'Salvestamine...' : 'Salvesta muudatused'}</button>
			</div>
		</form>

		<section class="form-section">
			<h2>Selle katastri eraldised</h2>
			{#if forestStands.length === 0}
				<p>Eraldisi ei leitud.</p>
			{:else}
				<div class="table-wrapper">
					<table>
						<thead>
							<tr>
								<th>Eraldise nr</th>
								<th>Pindala</th>
								<th>Kogumaht</th>
								<th>Staatus</th>
								<th class="actions">Ava</th>
							</tr>
						</thead>
						<tbody>
							{#each forestStands as stand (stand.id)}
								<tr>
									<td>{stand.number}</td>
									<td>{stand.area}</td>
									<td>{stand.totalVolume}</td>
									<td>{stand.isActive ? 'Aktiivne' : 'Mitteaktiivne'}</td>
									<td class="actions">
										<a
											href={resolve('/admin/[CompanyId]/foreststand/[ForestStandId]', {
												CompanyId: companyId,
												ForestStandId: stand.id
											})}>Ava</a
										>
									</td>
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

		<section class="form-section">
			<h2>Katastriüksus kaardil</h2>
			<CadastralMap tunnus={cadaster.cadastralNumber} />
		</section>

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
