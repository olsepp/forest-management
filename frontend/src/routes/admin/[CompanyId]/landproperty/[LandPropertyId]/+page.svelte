<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { landPropertyService } from '$lib/services/land-property';
	import type {
		PropertyStatus,
		LandPropertyDto,
		LandPropertyUpdateDto,
		CadasterLinkDto,
		ActivityDto
	} from '$lib/dtos/land-property/land-property.dto';

	let {
		data
	}: {
		data: {
			property: LandPropertyDto | null;
			cadasters: CadasterLinkDto[];
			activities: ActivityDto[];
		};
	} = $props();

	let property = $derived(data.property);
	let cadasters = $derived(data.cadasters ?? []);
	let activities = $derived(data.activities ?? []);
	let isEditMode = $state(false);
	let isSaving = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
	let cadasterErrorMessage = $state('');
	let activityErrorMessage = $state('');
	let companyId = $derived($page.params.CompanyId ?? '');

	let form = $state({
		name: '',
		registrationNumber: '',
		county: '',
		parish: '',
		village: '',
		boughtDate: '',
		soldDate: '',
		status: 'Inactive' as PropertyStatus
	});

	function normalizeStatus(status: LandPropertyDto['status']): PropertyStatus {
		if (typeof status === 'string') {
			const s = status.toLowerCase();
			if (s === 'active') return 'Active';
			if (s === 'sold') return 'Sold';
			return 'Inactive';
		}

		if (typeof status === 'number') {
			if (status === 0) return 'Active';
			if (status === 2) return 'Sold';
			return 'Inactive';
		}

		return 'Inactive';
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

	function toApiStatus(status: PropertyStatus): number {
		if (status === 'Active') return 0;
		if (status === 'Sold') return 2;
		return 1;
	}

	function fillForm(detail: LandPropertyDto): void {
		form = {
			name: detail.name ?? '',
			registrationNumber:
				typeof detail.registrationNumber === 'number' ? String(detail.registrationNumber) : '',
			county: detail.county ?? '',
			parish: detail.parish ?? '',
			village: detail.village ?? '',
			boughtDate: toDateInputValue(detail.boughtDate),
			soldDate: toDateInputValue(detail.soldDate),
			status: normalizeStatus(detail.status)
		};
	}

	$effect(() => {
		if (property) {
			fillForm(property);
		}
	});

	function formatDateTime(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleString();
	}

	function formatActivityQuantity(item: ActivityDto): string {
		const quantity =
			typeof item.quantity === 'number' && Number.isFinite(item.quantity) ? item.quantity : 0;
		return item.unit ? `${quantity} ${item.unit}` : String(quantity);
	}

	function forestStandLabel(item: ActivityDto): string {
		if (
			typeof item.forestStandNumber === 'number' &&
			Number.isFinite(item.forestStandNumber) &&
			item.forestStandNumber > 0
		) {
			return String(item.forestStandNumber);
		}

		return '—';
	}

	function applicationStatusLabel(status: number | null): string {
		if (status === null || typeof status !== 'number') return '—';
		if (status === 0) return 'Ootel';
		if (status === 1) return 'Kinnitatud';
		if (status === 2) return 'Tagasi lükatud';
		return String(status);
	}

	async function saveProperty(event: SubmitEvent) {
		event.preventDefault();
		if (!property || !isEditMode) return;

		const registrationNumber = Number(form.registrationNumber);
		if (!Number.isFinite(registrationNumber)) {
			errorMessage = 'Registrinumber peab olema korrektne number.';
			return;
		}

		const payload: LandPropertyUpdateDto = {
			id: property.id,
			name: form.name.trim(),
			registrationNumber,
			county: form.county.trim(),
			parish: form.parish.trim(),
			village: form.village.trim(),
			boughtDate: toApiDateTime(form.boughtDate),
			soldDate: toApiDateTime(form.soldDate),
			status: toApiStatus(form.status),
			companyId: property.companyId
		};

		isSaving = true;
		errorMessage = '';
		successMessage = '';

		try {
			const updated = await landPropertyService.update(property.id, payload);
			property = updated;
			fillForm(updated);
			isEditMode = false;
			successMessage = 'Kinnistu uuendati edukalt.';
		} catch (e) {
			const err = e as Error;
			if (err.message?.includes('404')) {
				errorMessage = 'Kinnistut ei leitud.';
			} else if (err.message?.includes('400')) {
				errorMessage = 'Valideerimine ebaõnnestus. Kontrollige sisestatud väärtusi.';
			} else {
				errorMessage = 'Muudatuste salvestamine ebaõnnestus.';
			}
		} finally {
			isSaving = false;
		}
	}
</script>

{#if !property}
	<p>Laetakse kinnistut...</p>
{:else}
	<div class="detail-page">
		<p class="breadcrumb">
			<a href={resolve('/admin/[CompanyId]/landproperty', { CompanyId: companyId })}
				>← Tagasi kinnistute juurde</a
			>
		</p>

		<header class="page-head">
			<div>
				<p class="eyebrow">Kinnistu</p>
				<h1>{property.name}</h1>
			</div>
			<button
				type="button"
				class="mode-btn"
				onclick={() => (isEditMode = !isEditMode)}
				disabled={isSaving}
			>
				{isEditMode ? 'Tühista muutmine' : 'Luba muutmine'}
			</button>
		</header>

		<section class="meta-grid">
			<article class="meta-card">
				<p class="meta-label">Kinnistu ID</p>
				<p class="meta-value mono">{property.id}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Ettevõte</p>
				<p class="meta-value">{property.companyName}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Nimi</p>
				<p class="meta-value">{property.name}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Registrinumber</p>
				<p class="meta-value mono">{property.registrationNumber}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Maakond</p>
				<p class="meta-value">{property.county}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Vald</p>
				<p class="meta-value">{property.parish}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Küla</p>
				<p class="meta-value">{property.village}</p>
			</article>
		</section>

		<form id="property-form" onsubmit={saveProperty} class="detail-form">
			<section class="form-section">
				<h2>Detailid</h2>
				<div class="form-grid">
					<label>
						<span>Olek</span>
						<div class="select-wrapper">
							<select bind:value={form.status} disabled={!isEditMode}>
								<option value="Active">Aktiivne</option>
								<option value="Inactive">Mitteaktiivne</option>
								<option value="Sold">Müüdud</option>
							</select>
							<div class="select-arrow"></div>
						</div>
					</label>
					<label>
						<span>Ostukuupäev</span>
						<input type="date" bind:value={form.boughtDate} disabled={!isEditMode} />
					</label>
					<label>
						<span>Müügikuupäev</span>
						<input type="date" bind:value={form.soldDate} disabled={!isEditMode} />
					</label>
				</div>
			</section>

			<div class="form-actions">
				<button class="btn-save" type="submit" disabled={isSaving || !isEditMode}>
					{isSaving ? 'Salvestamine...' : 'Salvesta muudatused'}
				</button>
			</div>
		</form>

		<section class="activity-section">
			<h2>Katastriüksused</h2>
			{#if cadasterErrorMessage}
				<p class="message error">{cadasterErrorMessage}</p>
			{:else if cadasters.length === 0}
				<p class="message">Ei leitud.</p>
			{:else}
				<div class="cadaster-links">
					{#each cadasters as cadaster (cadaster.id)}
						<a
							href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
								CompanyId: companyId,
								CadasterId: cadaster.id
							})}
						>
							{cadaster.cadastralNumber || cadaster.id}
						</a>
					{/each}
				</div>
			{/if}
		</section>

		<section class="activity-section">
			<h2>Tegevused</h2>
			{#if activityErrorMessage}
				<p class="message error">{activityErrorMessage}</p>
			{:else if activities.length === 0}
				<p class="message">Selle kinnistu tegevusi ei leitud.</p>
			{:else}
				<div class="table-wrapper">
					<table>
						<thead>
							<tr>
								<th>Kuupäev</th>
								<th>Tüüp</th>
								<th>Kataster</th>
								<th>Eraldis</th>
								<th>Kasutaja</th>
								<th>Kogus</th>
								<th>Staatus</th>
							</tr>
						</thead>
						<tbody>
							{#each activities as activity (activity.id)}
								<tr>
									<td>{formatDateTime(activity.date)}</td>
									<td>{activity.activityTypeName || '—'}</td>
									<td>{activity.cadasterCadastralNumber || '—'}</td>
									<td>{forestStandLabel(activity)}</td>
									<td>{activity.userName || '—'}</td>
									<td>{formatActivityQuantity(activity)}</td>
									<td>{applicationStatusLabel(activity.applicationStatus)}</td>
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
		white-space: nowrap;
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

	.select-wrapper {
		position: relative;
		display: flex;
		align-items: center;
	}

	select {
		-webkit-appearance: none;
		-moz-appearance: none;
		appearance: none;
		padding-right: 2rem;
		flex: 1;
	}

	select:disabled {
		padding-right: 1rem;
	}

	.select-arrow {
		position: absolute;
		right: 0.75rem;
		width: 0;
		height: 0;
		pointer-events: none;
		border-left: 5px solid transparent;
		border-right: 5px solid transparent;
		border-top: 5px solid #2f5f49;
	}

	select:disabled + .select-arrow {
		display: none;
	}

	.detail-form {
		display: grid;
		gap: 1rem;
	}

	.activity-section {
		display: grid;
		gap: 0.65rem;
	}

	.cadaster-links {
		display: flex;
		flex-wrap: wrap;
		gap: 0.45rem;
	}

	.cadaster-links a {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		padding: 0.9rem 2rem;
		border: 2px solid #1f5a42;
		border-radius: 0.75rem;
		background: #1f5a42;
		text-decoration: none;
		color: #ffffff;
		font-size: 1.1rem;
		font-weight: 600;
		letter-spacing: 0.02em;
		box-shadow: 0 4px 12px rgba(31, 90, 66, 0.3);
		transition:
			background 0.2s,
			box-shadow 0.2s,
			transform 0.1s;
		cursor: pointer;
	}

	.cadaster-links a:hover {
		background: #174d38;
		box-shadow: 0 6px 18px rgba(31, 90, 66, 0.4);
		transform: translateY(-1px);
	}

	.cadaster-links a:active {
		transform: translateY(0);
		box-shadow: 0 2px 6px rgba(31, 90, 66, 0.3);
	}

	.form-section {
		padding: 1rem;
		border: 1px solid #cadbcf;
		border-radius: 0.85rem;
		background: #f9fcfa;
		box-shadow: inset 0 1px 0 rgba(255, 255, 255, 0.95);
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
		padding-inline: 1.05rem;
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

	.error {
		background: #fdebec;
	}

	.success {
		background: #e6f7ea;
	}
</style>
