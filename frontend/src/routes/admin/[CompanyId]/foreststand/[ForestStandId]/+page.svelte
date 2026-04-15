<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';
	import type {
		RecentActivityDto,
		ForestStandDto,
		ForestStandUpdateDto,
		CadasterSummaryDto
	} from '$lib/dtos/forest-stand/forest-stand.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let isSaving = $state(false);
	let isEditMode = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
	let forestStand = $state<ForestStandDto | null>(null);
	let recentActivities = $state<RecentActivityDto[]>([]);
	let linkedLandPropertyId = $state('');
	let linkedLandPropertyName = $state('');
	const companyId = $derived($page.params.CompanyId ?? '');

	let form = $state({
		number: '',
		area: '',
		totalVolume: '',
		isActive: true,
		validFrom: '',
		validTo: ''
	});

	function toStringNumber(value: number | null | undefined): string {
		if (typeof value !== 'number' || Number.isNaN(value)) return '';
		return String(value);
	}

	function parseNumber(value: string): number {
		const parsed = Number(value);
		return Number.isFinite(parsed) ? parsed : 0;
	}

	function toDateInputValue(value: string | null): string {
		if (!value) return '';
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '';
		return date.toISOString().slice(0, 10);
	}

	function toApiDateTime(value: string): string | null {
		if (!value) return null;
		const date = new Date(`${value}T00:00:00`);
		if (Number.isNaN(date.getTime())) return null;
		return date.toISOString();
	}

	function formatDate(value: string | null): string {
		if (!value) return '—';
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleDateString();
	}

	function fillForm(detail: ForestStandDto): void {
		form = {
			number: toStringNumber(detail.number),
			area: toStringNumber(detail.area),
			totalVolume: toStringNumber(detail.totalVolume),
			isActive: !!detail.isActive,
			validFrom: toDateInputValue(detail.validFrom),
			validTo: toDateInputValue(detail.validTo)
		};
	}

	async function loadCadasterPropertyFallback(cadasterId: string, token: string): Promise<void> {
		const response = await fetch(`${apiBaseUrl}/api/cadasters/${cadasterId}`, {
			headers: {
				Authorization: `Bearer ${token}`
			}
		});

		if (!response.ok) return;

		const cadaster = (await response.json()) as CadasterSummaryDto;
		linkedLandPropertyId = cadaster.landPropertyId ?? linkedLandPropertyId;
		linkedLandPropertyName = cadaster.landPropertyName ?? linkedLandPropertyName;
	}

	async function loadForestStand() {
		try {
			errorMessage = '';
			successMessage = '';
			isLoading = true;

			const forestStandId = $page.params.ForestStandId;
			if (!forestStandId) {
				errorMessage = 'Puudub eraldise ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/foreststands/${forestStandId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 404
						? 'Eraldist ei leitud.'
						: response.status === 401
							? 'Ligipääs puudub. Logige uuesti sisse.'
							: 'Eraldise laadimine ebaõnnestus.';
				return;
			}

			const detail = (await response.json()) as ForestStandDto;
			forestStand = detail;
			recentActivities = Array.isArray(detail.recentActivities) ? detail.recentActivities : [];
			linkedLandPropertyId = detail.landPropertyId ?? '';
			linkedLandPropertyName = detail.landPropertyName ?? '';

			if ((!linkedLandPropertyId || !linkedLandPropertyName) && detail.cadasterId) {
				await loadCadasterPropertyFallback(detail.cadasterId, token);
			}

			fillForm(detail);
		} catch {
			errorMessage = 'Eraldise laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	async function saveForestStand(event: SubmitEvent) {
		event.preventDefault();
		if (!forestStand || !isEditMode) return;

		const payload: ForestStandUpdateDto = {
			id: forestStand.id,
			number: parseNumber(form.number),
			area: parseNumber(form.area),
			totalVolume: parseNumber(form.totalVolume),
			isActive: form.isActive,
			validFrom: toApiDateTime(form.validFrom) ?? forestStand.validFrom,
			validTo: toApiDateTime(form.validTo),
			cadasterId: forestStand.cadasterId
		};

		isSaving = true;
		errorMessage = '';
		successMessage = '';

		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/foreststands/${forestStand.id}`, {
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
							? 'Eraldist ei leitud.'
							: 'Muudatuste salvestamine ebaõnnestus.';
				return;
			}

			const updated = (await response.json()) as ForestStandDto;
			forestStand = updated;
			recentActivities = Array.isArray(updated.recentActivities) ? updated.recentActivities : [];
			linkedLandPropertyId = updated.landPropertyId ?? linkedLandPropertyId;
			linkedLandPropertyName = updated.landPropertyName ?? linkedLandPropertyName;
			fillForm(updated);
			isEditMode = false;
			successMessage = 'Eraldis uuendati edukalt.';
		} catch {
			errorMessage = 'Muudatuste salvestamine ebaõnnestus.';
		} finally {
			isSaving = false;
		}
	}

	onMount(loadForestStand);
</script>

{#if isLoading}
	<p>Laetakse eraldist...</p>
{:else if errorMessage && !forestStand}
	<p class="message error">{errorMessage}</p>
{:else if forestStand}
	<div class="detail-page">
		<p class="breadcrumb">
			<a
				href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
					CompanyId: companyId,
					CadasterId: forestStand.cadasterId
				})}>← Tagasi katastri juurde</a
			>
		</p>

		<header class="page-head">
			<div>
				<p class="eyebrow">Eraldis</p>
				<h1>Eraldis {forestStand.number}</h1>
			</div>
			<div class="head-actions">
				<a
					class="btn-log-activity"
					href={resolve('/admin/[CompanyId]/foreststand/[ForestStandId]/activity/new', {
						CompanyId: companyId,
						ForestStandId: forestStand.id
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
				<p class="meta-label">Eraldise ID</p>
				<p class="meta-value mono">{forestStand.id}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Kataster</p>
				<p class="meta-value">
					<a
						href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
							CompanyId: companyId,
							CadasterId: forestStand.cadasterId
						})}
					>
						{forestStand.cadasterCadastralNumber}
					</a>
				</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Kinnistu</p>
				<p class="meta-value">
					{#if linkedLandPropertyId && linkedLandPropertyName}
						<a
							href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
								CompanyId: companyId,
								LandPropertyId: linkedLandPropertyId
							})}
						>
							{linkedLandPropertyName}
						</a>
					{:else}
						—
					{/if}
				</p>
			</article>
		</section>

		<form id="foreststand-form" onsubmit={saveForestStand} class="detail-form">
			<section class="form-section">
				<h2>Detailid</h2>
				<div class="form-grid">
					<label
						><span>Eraldise nr</span><input
							type="number"
							min="0"
							bind:value={form.number}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Pindala</span><input
							type="number"
							step="any"
							bind:value={form.area}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Kogumaht</span><input
							type="number"
							bind:value={form.totalVolume}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Kehtib alates</span><input
							type="date"
							bind:value={form.validFrom}
							readonly={!isEditMode}
						/></label
					>
					<label
						><span>Kehtib kuni</span><input
							type="date"
							bind:value={form.validTo}
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
			<h2>Hiljutised tegevused</h2>
			{#if recentActivities.length === 0}
				<p>Ei leitud.</p>
			{:else}
				<div class="table-wrapper">
					<table>
						<thead>
							<tr>
								<th>Kuupäev</th>
								<th>Tüüp</th>
								<th>Kirjeldus</th>
								<th>Kogus</th>
								<th>Kasutaja</th>
								<th class="actions">Ava</th>
							</tr>
						</thead>
						<tbody>
							{#each recentActivities as activity (activity.id)}
								<tr>
									<td>{formatDate(activity.date)}</td>
									<td>{activity.activityTypeName}</td>
									<td>{activity.description}</td>
									<td>{activity.quantity}{activity.unit ? ` ${activity.unit}` : ''}</td>
									<td>{activity.userName}</td>
									<td class="actions">
										<a
											href={resolve('/admin/[CompanyId]/activity/[ActivityId]', {
												CompanyId: companyId,
												ActivityId: activity.id
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
	.checkbox-label {
		justify-content: flex-end;
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
	.error {
		background: #fdebec;
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
</style>
