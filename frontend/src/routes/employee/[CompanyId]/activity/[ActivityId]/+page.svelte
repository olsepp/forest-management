<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import { onMount } from 'svelte';
	import type {
		ActivityStatus,
		ActivityDto as ActivityDtoType,
		ActivityTypeListDto as ActivityTypeListDtoType,
		ActivityUpdateDto as ActivityUpdateDtoType
	} from '$lib/dtos/activity/activity.dto';

	type ActivityStatus = 'Pending' | 'Approved' | 'Rejected';

	type ActivityDto = ActivityDtoType;

	type ActivityTypeListDto = ActivityTypeListDtoType;

	type ActivityUpdateDto = ActivityUpdateDtoType;

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let isSaving = $state(false);
	let isEditMode = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
	let isUnauthorized = $state(false);

	let activity = $state<ActivityDto | null>(null);
	let activityTypes = $state<ActivityTypeListDto[]>([]);

	let companyId = $derived($page.params.CompanyId ?? '');
	let activityId = $derived($page.params.ActivityId ?? '');
	let currentUserId = $derived($user?.userId ?? '');
	let currentUsername = $derived(($user?.username ?? '').trim().toLowerCase());

	let form = $state({
		description: '',
		quantity: '',
		unit: '',
		notes: '',
		date: '',
		activityTypeId: ''
	});

	function toDateInputValue(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '';
		return date.toISOString().slice(0, 16);
	}

	function formatDate(value: string | null): string {
		if (!value) return '—';
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleString();
	}

	function formatQuantity(value: number, unit: string | null): string {
		if (!Number.isFinite(value)) return '—';
		return unit ? `${value} ${unit}` : String(value);
	}

	function fillForm(detail: ActivityDto): void {
		form = {
			description: detail.description ?? '',
			quantity: typeof detail.quantity === 'number' ? String(detail.quantity) : '',
			unit: detail.unit ?? '',
			notes: detail.notes ?? '',
			date: toDateInputValue(detail.date),
			activityTypeId: detail.activityTypeId ?? ''
		};
	}

	function isOwnActivity(detail: ActivityDto): boolean {
		if (currentUserId && detail.userId) {
			return detail.userId === currentUserId;
		}

		return (detail.userName ?? '').trim().toLowerCase() === currentUsername;
	}

	async function loadActivityTypes() {
		try {
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activitytypes`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!response.ok) {
				activityTypes = [];
				return;
			}

			const data = (await response.json()) as ActivityTypeListDto[];
			activityTypes = Array.isArray(data) ? data.filter((type) => Boolean(type?.id)) : [];
		} catch {
			activityTypes = [];
		}
	}

	async function loadActivity() {
		if (!activityId) {
			errorMessage = 'Puudub tegevuse ID.';
			isLoading = false;
			return;
		}

		try {
			errorMessage = '';
			successMessage = '';
			isUnauthorized = false;
			isLoading = true;

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/activities/${activityId}`, {
				headers: { Authorization: `Bearer ${token}` }
			});

			if (!response.ok) {
				if (response.status === 401) {
					isUnauthorized = true;
					errorMessage = 'Ligipääs puudub. Logige uuesti sisse.';
					return;
				}

				errorMessage =
					response.status === 404 ? 'Tegevust ei leitud.' : 'Tegevuse laadimine ebaõnnestus.';
				return;
			}

			const detail = (await response.json()) as ActivityDto;

			if (!isOwnActivity(detail)) {
				isUnauthorized = true;
				errorMessage = 'Sul puudub sellele tegevusele ligipääs.';
				activity = null;
				return;
			}

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

		errorMessage = '';
		successMessage = '';

		if (!form.description.trim()) {
			errorMessage = 'Kirjeldus on kohustuslik.';
			return;
		}

		if (!form.activityTypeId) {
			errorMessage = 'Tegevuse tüüp on kohustuslik.';
			return;
		}

		const quantityRaw = String(form.quantity ?? '').trim();
		const quantity = quantityRaw === '' ? 0 : Number(quantityRaw);
		if (!Number.isFinite(quantity)) {
			errorMessage = 'Kogus peab olema korrektne number.';
			return;
		}

		const parsedDate = form.date ? new Date(form.date) : new Date();
		if (Number.isNaN(parsedDate.getTime())) {
			errorMessage = 'Kuupäev peab olema korrektne.';
			return;
		}

		const payload: ActivityUpdateDto = {
			id: activity.id,
			description: form.description.trim(),
			quantity,
			unit: form.unit.trim() || null,
			notes: form.notes.trim() || null,
			date: parsedDate.toISOString(),
			activityTypeId: form.activityTypeId,
			forestStandId: activity.forestStandId,
			cadasterId: activity.cadasterId,
			applicationStatus: activity.applicationStatus
		};

		isSaving = true;
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
							: response.status === 403
								? 'Sul ei ole õigust seda tegevust muuta.'
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
	<div class="employee-state-block is-loading">Laetakse tegevuse detaile…</div>
{:else if errorMessage && !activity}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Teie sessioon võib olla aegunud või ligipääs piiratud.</span>
		{/if}
	</div>
{:else if activity}
	<p class="employee-back-link">
		<a
			class="employee-back-link-button"
			href={resolve('/employee/[CompanyId]/activity', { CompanyId: companyId })}
		>
			<span aria-hidden="true">←</span>
			<span>Tagasi tegevuste ajalukku</span>
		</a>
	</p>

	<section class="employee-card summary">
		<div class="summary-head">
			<div>
				<p class="kicker">Tegevuse kirje</p>
				<h1 class="employee-page-title">{activity.activityTypeName || 'Tegevus'}</h1>
				<p class="subtitle">Vaata ja uuenda oma sisestatud tegevuse detaile.</p>
			</div>
			<button
				type="button"
				class="mode-btn"
				onclick={() => (isEditMode = !isEditMode)}
				disabled={isSaving}
			>
				{isEditMode ? 'Tühista muutmine' : 'Luba muutmine'}
			</button>
		</div>

		<div class="meta-grid">
			<p><strong>Sisestaja:</strong> {activity.userName || '—'}</p>
			<p><strong>Kuupäev:</strong> {formatDate(activity.date)}</p>
			<p><strong>Kogus:</strong> {formatQuantity(activity.quantity, activity.unit)}</p>
			<p><strong>Staatus:</strong> {activity.applicationStatus || '—'}</p>
			<p>
				<strong>Siht:</strong>
				{#if activity.cadasterId}
					<a
						href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
							CompanyId: companyId,
							CadasterId: activity.cadasterId
						})}
					>
						Kataster {activity.cadasterCadastralNumber ?? activity.cadasterId}
					</a>
				{:else if activity.forestStandId}
					<a
						href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
							CompanyId: companyId,
							ForestStandId: activity.forestStandId
						})}
					>
						Eraldis {activity.forestStandNumber || activity.forestStandId}
					</a>
				{:else}
					—
				{/if}
			</p>
			{#if activity.landPropertyId}
				<p>
					<strong>Kinnistu:</strong>
					<a
						href={resolve('/employee/[CompanyId]/landproperty/[LandPropertyId]', {
							CompanyId: companyId,
							LandPropertyId: activity.landPropertyId
						})}
					>
						{activity.landPropertyName ?? activity.landPropertyId}
					</a>
				</p>
			{/if}
		</div>
	</section>

	{#if isEditMode}
		<form onsubmit={saveActivity} class="employee-card detail-form">
			<h2>Muuda tegevust</h2>

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

				<label class="full-width">
					<span>Kirjeldus</span>
					<textarea bind:value={form.description} rows="4" readonly={!isEditMode}></textarea>
				</label>

				<label class="full-width">
					<span>Märkused</span>
					<textarea bind:value={form.notes} rows="4" readonly={!isEditMode}></textarea>
				</label>
			</div>

			<div class="form-actions">
				<button class="btn-save" type="submit" disabled={isSaving || !isEditMode}>
					{isSaving ? 'Salvestamine...' : 'Salvesta muudatused'}
				</button>
			</div>
		</form>
	{/if}

	{#if errorMessage}
		<div class="employee-state-block is-error">{errorMessage}</div>
	{/if}

	{#if successMessage}
		<div class="employee-state-block is-success">{successMessage}</div>
	{/if}
{/if}

<style>
	.summary {
		margin-bottom: 0.75rem;
	}

	.summary-head {
		display: flex;
		flex-wrap: wrap;
		justify-content: space-between;
		align-items: flex-start;
		gap: 0.75rem;
		margin-bottom: 0.65rem;
	}

	.kicker {
		margin: 0;
		font-size: 0.77rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	h1 {
		margin: 0;
		font-size: 1.28rem;
		line-height: 1.2;
		color: #0f172a;
	}

	h2 {
		margin: 0 0 0.75rem;
		font-size: 1.05rem;
		color: #1f2937;
	}

	.subtitle {
		margin: 0;
		color: #334155;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}

	.mode-btn {
		min-height: 2.8rem;
		padding: 0.55rem 0.95rem;
		background: #1f5a42;
		color: #f6fbf8;
		border: 1px solid #184835;
		border-radius: 0.75rem;
		font-size: 0.95rem;
		font-weight: 700;
	}

	.mode-btn:disabled {
		opacity: 0.65;
	}

	.meta-grid {
		display: grid;
		gap: 0.45rem;
	}

	.meta-grid p {
		margin: 0;
		color: #334155;
	}

	.meta-grid a {
		color: #1f5a42;
		font-weight: 700;
		text-decoration: none;
	}

	.detail-form {
		display: grid;
		gap: 0.8rem;
	}

	.form-grid {
		display: grid;
		grid-template-columns: 1fr;
		gap: 0.7rem;
	}

	label {
		display: flex;
		flex-direction: column;
		gap: 0.3rem;
	}

	label span {
		font-size: 0.86rem;
		font-weight: 700;
		color: #30483d;
	}

	input,
	select,
	textarea {
		padding: 0.6rem 0.65rem;
		border: 1px solid #d1dcd6;
		border-radius: 0.6rem;
		font: inherit;
		background: #fff;
	}

	textarea {
		resize: vertical;
	}

	.form-actions {
		display: flex;
		justify-content: flex-end;
	}

	.btn-save {
		min-height: 2.5rem;
		padding: 0.5rem 1rem;
		border: 1px solid #1f5a42;
		border-radius: 0.65rem;
		background: #1f5a42;
		color: #f6fbf8;
		font-size: 0.9rem;
		font-weight: 700;
	}

	.btn-save:disabled {
		opacity: 0.65;
		cursor: not-allowed;
	}

	@media (min-width: 768px) {
		h1 {
			font-size: 1.35rem;
		}

		.form-grid {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}

		.full-width,
		.form-actions {
			grid-column: 1 / -1;
		}
	}
</style>
