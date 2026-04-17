<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { cadasterService } from '$lib/services/cadaster';
	import CadastralMap from '$lib/components/shared/CadastralMap.svelte';
import type {
		ForestStandListDto,
		CadasterDto,
		CadasterUpdateDto,
		ActivityListDto
	} from '$lib/dtos/cadaster/cadaster.dto';

	let {
		data
	}: {
		data: {
			cadaster: CadasterDto | null;
			forestStands: ForestStandListDto[];
			activities: ActivityListDto[];
		};
	} = $props();

	let cadaster = $derived(data.cadaster);
	let forestStands = $derived(data.forestStands ?? []);
	let activities = $derived(data.activities ?? []);
	let isLoading = $derived(!cadaster);
	let isSaving = $state(false);
	let isEditMode = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
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

	function formatDateTime(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleString();
	}

	function formatActivityQuantity(item: ActivityListDto): string {
		const quantity =
			typeof item.quantity === 'number' && Number.isFinite(item.quantity) ? item.quantity : 0;
		return item.unit ? `${quantity} ${item.unit}` : String(quantity);
	}

	function forestStandLabel(item: ActivityListDto): string {
		if (
			typeof item.forestStandNumber === 'number' &&
			Number.isFinite(item.forestStandNumber) &&
			item.forestStandNumber > 0
		) {
			return String(item.forestStandNumber);
		}
		return '—';
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
			const updated = await cadasterService.update(cadaster.id, payload);
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

	$effect(() => {
		if (cadaster) {
			fillForm(cadaster);
		}
	});
</script>

{#if isLoading}
	<p>Laetakse katastrit...</p>
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
			</div>
			<div class="head-actions">
				<a
					class="btn-log-activity"
					href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]/activity/new', {
						CompanyId: companyId,
						CadasterId: cadaster.id
					})}>Logi tegevus</a
				>
				<button
					type="button"
					class="mode-btn"
					onclick={() => (isEditMode = !isEditMode)}
					disabled={isSaving}
				>
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
				<p class="meta-label">Katastrinumber</p>
				<p class="meta-value">{cadaster.cadastralNumber}</p>
			</article>
		</section>

		<form id="cadaster-form" onsubmit={saveCadaster} class="detail-form">
			<section class="form-section">
				<h2>Detailid</h2>
				<div class="form-grid">
					<label
						><span>Boniteet</span><input
							type="number"
							min="0"
							max="4"
							bind:value={form.soilQualityIndex}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Arvutatud maht (tm)</span><input
							type="number"
							bind:value={form.calculatedVolume}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Mahukasv (tm/a)</span><input
							type="number"
							step="any"
							bind:value={form.volumeGrowth}
							readonly={!isEditMode}
						/></label
					>
				</div>
			</section>

			<section class="form-section">
				<h2>Pindala jaotus</h2>
				<div class="form-grid">
					<label
						><span>Metsamaa pindala</span><input
							type="number"
							step="any"
							bind:value={form.forestArea}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Haritava maa pindala</span><input
							type="number"
							step="any"
							bind:value={form.arableArea}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Rohumaa pindala</span><input
							type="number"
							step="any"
							bind:value={form.grasslandArea}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Õueala pindala</span><input
							type="number"
							step="any"
							bind:value={form.yardArea}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Hoonete alune pindala</span><input
							type="number"
							step="any"
							bind:value={form.buildingFootprintArea}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Veealune pindala</span><input
							type="number"
							step="any"
							bind:value={form.underwaterArea}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Muu pindala</span><input
							type="number"
							step="any"
							bind:value={form.otherArea}
							readonly={!isEditMode}
						/></label
					>
				</div>
			</section>

			<div class="form-actions">
				<button class="btn-save" type="submit" disabled={isSaving || !isEditMode}
					>{isSaving ? 'Salvestamine...' : 'Salvesta muudatused'}</button
				>
			</div>
		</form>

		<section class="form-section">
			<h2>Eraldised</h2>
			{#if forestStands.length === 0}
				<p class="message">Eraldisi ei leitud.</p>
			{:else}
				<div class="stand-button-grid stands-mobile" aria-label="Eraldised">
					{#each forestStands as stand (stand.id)}
						<a
							class="stand-button"
							href={resolve('/admin/[CompanyId]/foreststand/[ForestStandId]', {
								CompanyId: companyId,
								ForestStandId: stand.id
							})}
							aria-label={`Ava eraldis ${stand.number}`}
						>
							#{stand.number}
						</a>
					{/each}
				</div>
			{/if}
		</section>

		<section class="form-section">
			<h2>Tegevused</h2>
			{#if activities.length === 0}
				<p class="message">Ei leitud.</p>
			{:else}
				<div class="table-wrapper">
					<table>
						<thead>
							<tr>
								<th>Kuupäev</th>
								<th>Tüüp</th>
								<th>Eraldis</th>
								<th>Kasutaja</th>
								<th>Kogus</th>
								<th>Kirjeldus</th>
							</tr>
						</thead>
						<tbody>
							{#each activities as activity (activity.id)}
								<tr>
									<td>{formatDateTime(activity.date)}</td>
									<td>{activity.activityTypeName || '—'}</td>
									<td>{forestStandLabel(activity)}</td>
									<td>{activity.userName || '—'}</td>
									<td>{formatActivityQuantity(activity)}</td>
									<td>{activity.description || '—'}</td>
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
	.breadcrumb {
		margin: 0;
	}
	.page-head {
		display: flex;
		justify-content: space-between;
		align-items: flex-start;
		gap: 1rem;
	}
	.eyebrow {
		margin: 0;
		font-size: 0.78rem;
		text-transform: uppercase;
		letter-spacing: 0.08em;
		font-weight: 700;
	}
	h1 {
		margin: 0.2rem 0 0.35rem;
		font-size: 1.6rem;
	}

	.head-actions {
		display: flex;
		gap: 0.6rem;
		align-items: center;
	}
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
	.btn-log-activity:hover {
		background: #174a35;
		color: #ffffff;
		text-decoration: none;
	}
	.mode-btn {
		padding: 0.58rem 1rem;
		background: #2f5f49;
		color: #f6fbf8;
		border: 1px solid #264735;
		border-radius: 0.65rem;
		box-shadow: 0 6px 14px rgba(29, 61, 46, 0.2);
	}
	.mode-btn:hover {
		background: #274f3d;
		cursor: pointer;
	}
	.meta-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.8rem;
	}
	.meta-card {
		padding: 0.9rem;
		border: 1px solid #c9dace;
		border-radius: 0.75rem;
		background: #f4faf6;
	}
	.meta-label {
		margin: 0;
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.08em;
	}
	.meta-value {
		margin: 0.35rem 0 0;
		font-size: 1rem;
		font-weight: 600;
	}
	.mono {
		font-family:
			ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', monospace;
		font-size: 0.88rem;
	}
	.detail-form {
		display: grid;
		gap: 1rem;
	}
	.form-section {
		padding: 1rem;
		border: 1px solid #cadbcf;
		border-radius: 0.85rem;
		background: #f9fcfa;
		box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.9);
	}
	h2 {
		margin: 0 0 0.8rem;
		font-size: 1.03rem;
	}
	.form-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.75rem 1rem;
	}
	label {
		display: flex;
		flex-direction: column;
		gap: 0.35rem;
	}
	.form-actions {
		display: flex;
		justify-content: flex-end;
	}
	.btn-save {
		padding: 0.62rem 1.1rem;
		background: #1f5a42;
		color: #f8fdfb;
		border: 1px solid #184835;
		font-weight: 700;
		border-radius: 0.65rem;
		box-shadow: 0 8px 16px rgba(31, 90, 66, 0.24);
	}
	.btn-save:hover {
		background: #174a35;
	}
	.message {
		margin: 0;
		padding: 0.7rem 0.9rem;
		border-radius: 0.65rem;
	}
	.stand-button-grid {
		display: grid;
		grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
		gap: 0.55rem;
	}

	.stand-button {
		text-decoration: none;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3rem;
		padding: 0 0.5rem;
		border: 1px solid #1f5a42;
		background: linear-gradient(180deg, #2a6b4f 0%, #1f5a42 100%);
		box-shadow: 0 6px 16px rgba(15, 42, 31, 0.22);
		color: #f3fbf7;
		border-radius: 0.82rem;
		font-size: 0.95rem;
		font-weight: 700;
		white-space: normal;
		text-align: center;
	}

	.stand-button:hover {
		background: linear-gradient(180deg, #2f7657 0%, #245f46 100%);
		border-color: #184736;
		color: #ffffff !important;
		text-shadow: 0 1px 3px rgba(0, 0, 0, 0.2);
		text-decoration: none;
	}

	.stand-button:active {
		transform: translateY(1px);
		box-shadow: 0 3px 10px rgba(15, 42, 31, 0.2);
	}
	.success {
		background: #e6f7ea;
	}
	.table-wrapper {
		overflow-x: auto;
	}

	table {
		width: 100%;
		border-collapse: collapse;
		background: #f9fcfa;
		border: 1px solid #d8e5dd;
		border-radius: 0.75rem;
		overflow: hidden;
	}

	th,
	td {
		padding: 0.65rem 0.75rem;
		text-align: left;
		border-bottom: 1px solid #e3ece7;
		white-space: nowrap;
	}

	tbody tr:last-child td {
		border-bottom: none;
	}
</style>
