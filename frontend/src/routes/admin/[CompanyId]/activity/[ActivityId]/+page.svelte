<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';
	import type {
		ActivityStatus,
		ActivityDto,
		ActivityTypeListDto,
		ActivityUpdateDto
	} from '$lib/dtos/activity/activity.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let isSaving = $state(false);
	let isEditMode = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
	let activity = $state<ActivityDto | null>(null);
	let activityTypes = $state<ActivityTypeListDto[]>([]);
	const companyId = $derived($page.params.CompanyId ?? '');

	let form = $state({
		description: '',
		quantity: '',
		unit: '',
		notes: '',
		date: '',
		activityTypeId: '',
		applicationStatus: '' as '' | ActivityStatus
	});

	function toDateInputValue(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '';
		return date.toISOString().slice(0, 16);
	}

	function fillForm(detail: ActivityDto): void {
		form = {
			description: detail.description ?? '',
			quantity: typeof detail.quantity === 'number' ? String(detail.quantity) : '',
			unit: detail.unit ?? '',
			notes: detail.notes ?? '',
			date: toDateInputValue(detail.date),
			activityTypeId: detail.activityTypeId ?? '',
			applicationStatus: detail.applicationStatus ?? ''
		};
	}

	async function loadActivityTypes() {
		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activitytypes`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				activityTypes = [];
				return;
			}

			const data = (await response.json()) as ActivityTypeListDto[];
			activityTypes = Array.isArray(data) ? data : [];
		} catch {
			activityTypes = [];
		}
	}

	async function loadActivity() {
		try {
			errorMessage = '';
			successMessage = '';
			isLoading = true;

			const activityId = $page.params.ActivityId;
			if (!activityId) {
				errorMessage = 'Puudub tegevuse ID.';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activities/${activityId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 404
						? 'Tegevust ei leitud.'
						: response.status === 401
							? 'Ligipääs puudub. Logige uuesti sisse.'
							: 'Tegevuse laadimine ebaõnnestus.';
				return;
			}

			const detail = (await response.json()) as ActivityDto;
			activity = detail;
			fillForm(detail);
		} catch {
			errorMessage = 'Tegevuse laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	}

	async function saveActivity(event: SubmitEvent) {
		event.preventDefault();
		if (!activity || !isEditMode) return;

		const quantityRaw = String(form.quantity ?? '').trim();
		const quantity = quantityRaw === '' ? 0 : Number(quantityRaw);
		if (!Number.isFinite(quantity)) {
			errorMessage = 'Kogus peab olema korrektne number.';
			return;
		}

		if (!form.description.trim()) {
			errorMessage = 'Kirjeldus on kohustuslik.';
			return;
		}

		if (!form.activityTypeId) {
			errorMessage = 'Tegevuse tüüp on kohustuslik.';
			return;
		}

		const payload: ActivityUpdateDto = {
			id: activity.id,
			description: form.description.trim(),
			quantity,
			unit: form.unit.trim() || null,
			notes: form.notes.trim() || null,
			date: form.date ? new Date(form.date).toISOString() : new Date().toISOString(),
			activityTypeId: form.activityTypeId,
			forestStandId: activity.forestStandId,
			cadasterId: activity.cadasterId,
			applicationStatus: form.applicationStatus || null
		};

		isSaving = true;
		errorMessage = '';
		successMessage = '';

		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activities/${activity.id}`, {
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
							? 'Tegevust ei leitud.'
							: 'Muudatuste salvestamine ebaõnnestus.';
				return;
			}

			const updated = (await response.json()) as ActivityDto;
			activity = updated;
			fillForm(updated);
			isEditMode = false;
			successMessage = 'Tegevus uuendati edukalt.';
		} catch {
			errorMessage = 'Muudatuste salvestamine ebaõnnestus.';
		} finally {
			isSaving = false;
		}
	}

	onMount(async () => {
		await Promise.all([loadActivityTypes(), loadActivity()]);
	});
</script>

{#if isLoading}
	<p>Laetakse tegevuse detaile...</p>
{:else if errorMessage && !activity}
	<p class="message error">{errorMessage}</p>
{:else if activity}
	<div class="detail-page">
		<p class="breadcrumb">
			<a href={resolve('/admin/[CompanyId]/activity', { CompanyId: companyId })}
				>← Tagasi tegevuste juurde</a
			>
		</p>

		<header class="page-head">
			<div>
				<p class="eyebrow">Tegevuse kirje</p>
				<h1>{activity.activityTypeName}</h1>
				<p class="subtitle">
					Vaata ja uuenda tegevuse metaandmeid, väärtusi ja staatust ühes töölauas.
				</p>
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
				<p class="meta-label">Tegevuse ID</p>
				<p class="meta-value mono">{activity.id}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Logis</p>
				<p class="meta-value">{activity.userName}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Sihtobjekt</p>
				<p class="meta-value">
					{#if activity.forestStandId}
						<a
							href={resolve('/admin/[CompanyId]/foreststand/[ForestStandId]', {
								CompanyId: companyId,
								ForestStandId: activity.forestStandId
							})}
						>
							Eraldis {activity.forestStandNumber || activity.forestStandId}
						</a>
					{:else if activity.cadasterId}
						<a
							href={resolve('/admin/[CompanyId]/cadaster/[CadasterId]', {
								CompanyId: companyId,
								CadasterId: activity.cadasterId
							})}
						>
							Kataster {activity.cadasterCadastralNumber ?? activity.cadasterId}
						</a>
					{:else}
						—
					{/if}
				</p>
			</article>
			{#if activity.landPropertyId}
				<article class="meta-card">
					<p class="meta-label">Kinnistu</p>
					<p class="meta-value">
						<a
							href={resolve('/admin/[CompanyId]/landproperty/[LandPropertyId]', {
								CompanyId: companyId,
								LandPropertyId: activity.landPropertyId
							})}
						>
							{activity.landPropertyName ?? activity.landPropertyId}
						</a>
					</p>
				</article>
			{/if}
		</section>

		<form id="activity-form" onsubmit={saveActivity} class="detail-form">
			<section class="form-section">
				<h2>Tegevuse põhiandmed</h2>
				<div class="form-grid">
					<label>
						<span>Tegevuse tüüp</span>
						<select bind:value={form.activityTypeId} disabled={!isEditMode}>
							{#each activityTypes as type (type.id)}
								<option value={type.id}>{type.activityTypeName}</option>
							{/each}
						</select>
					</label>
					<label>
						<span>Kuupäev</span>
						<input type="datetime-local" bind:value={form.date} readonly={!isEditMode} />
					</label>
					<label>
						<span>Kogus</span>
						<input type="number" step="any" bind:value={form.quantity} readonly={!isEditMode} />
					</label>
					<label>
						<span>Ühik</span>
						<input type="text" bind:value={form.unit} readonly={!isEditMode} />
					</label>
					<label>
						<span>Taotluse staatus</span>
						<select bind:value={form.applicationStatus} disabled={!isEditMode}>
							<option value=""></option>
							<option value="Pending">Ootel</option>
							<option value="Approved">Kinnitatud</option>
							<option value="Rejected">Tagasi lükatud</option>
						</select>
					</label>
				</div>
			</section>

			<section class="form-section">
				<h2>Kirjeldus ja märkused</h2>
				<div class="form-grid">
					<label class="full-width">
						<span>Kirjeldus</span>
						<textarea bind:value={form.description} rows="4" readonly={!isEditMode}></textarea>
					</label>
					<label class="full-width">
						<span>Märkused</span>
						<textarea bind:value={form.notes} rows="4" readonly={!isEditMode}></textarea>
					</label>
				</div>
			</section>

			<div class="form-actions">
				<button class="btn-save" type="submit" disabled={isSaving || !isEditMode}>
					{isSaving ? 'Salvestamine...' : 'Salvesta muudatused'}
				</button>
			</div>
		</form>

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
	.subtitle {
		margin: 0;
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
	.full-width {
		grid-column: 1 / -1;
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
</style>
