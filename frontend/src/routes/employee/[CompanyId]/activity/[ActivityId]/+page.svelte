<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { user } from '$lib/stores/auth.store';
	import { activityService } from '$lib/services/activity';
	import { formatUserName } from '$lib/utils/format-user';
	import type {
		ActivityDto,
		ActivityTypeListDto,
		ActivityUpdateDto
	} from '$lib/dtos/activity/activity.dto';

	let { data }: { data: { activity: ActivityDto | null; activityTypes: ActivityTypeListDto[] } } =
		$props();
	let activity = $derived(data.activity);
	let activityTypes = $derived(data.activityTypes ?? []);
	let isLoading = $derived(!activity);
	let isSaving = $state(false);
	let isEditMode = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
	let isUnauthorized = $derived(activity && !isOwnActivity(activity));

	let companyId = $derived($page.params.CompanyId ?? '');
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
		if (!Number.isFinite(value) || value === 0) return '—';
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

	$effect(() => {
		if (activity) {
			fillForm(activity);
		}
	});

	function isOwnActivity(detail: ActivityDto): boolean {
		if (currentUserId && detail.userId) {
			return detail.userId === currentUserId;
		}
		return (detail.userName ?? '').trim().toLowerCase() === currentUsername;
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
			const updated = await activityService.update(activity.id, payload);
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
</script>

{#if isLoading}
	<div class="employee-state-block is-loading">Laetakse tegevuse andmeid… Halva ühenduse korral võib see veidi aega võtta.</div>
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
			<p><strong>Sisestas:</strong> {formatUserName(activity) || '—'}</p>
			<p><strong>Kuupäev:</strong> {formatDate(activity.date)}</p>
			<p><strong>Kogus:</strong> {formatQuantity(activity.quantity, activity.unit)}</p>
			{#if activity.landPropertyId}
				<p>
					<strong>Kinnistu:</strong>
					{activity.landPropertyName ?? activity.landPropertyId}
				</p>
			{/if}
			<p>
				<strong>Kataster:</strong>
				{#if activity.cadasterId}
					{activity.cadasterCadastralNumber ?? activity.cadasterId}
				{:else if activity.forestStandId}
					{activity.cadasterId || '—'}
				{:else}
					—
				{/if}
			</p>
			<p>
				<strong>Eraldis:</strong>
				{#if activity.forestStandId}
					{activity.forestStandNumber || activity.forestStandId}
				{:else if activity.cadasterId}
					—
				{:else}
					—
				{/if}
			</p>
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
					<select bind:value={form.unit} disabled={!isEditMode}>
						<option value="">Vali ühik</option>
						<option value="m3">m3</option>
						<option value="ha">ha</option>
						<option value="tk">tk</option>
						<option value="h">h</option>
					</select>
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

	.mode-btn:hover:not(:disabled) {
		cursor: pointer;
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

	.btn-save:hover:not(:disabled) {
		cursor: pointer;
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
