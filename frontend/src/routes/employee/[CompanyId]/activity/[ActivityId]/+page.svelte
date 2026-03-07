<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { user } from '$lib/stores/auth.store';
	import { onMount } from 'svelte';

	type ActivityStatus = 'Pending' | 'Approved' | 'Rejected';

	type ActivityDto = {
		id: string;
		description: string;
		quantity: number;
		unit: string | null;
		notes: string | null;
		date: string;
		userId: string;
		userName: string;
		activityTypeId: string;
		activityTypeName: string;
		cadasterId: string | null;
		cadasterCadastralNumber: string | null;
		forestStandId: string | null;
		forestStandNumber: number;
		landPropertyId: string | null;
		landPropertyName: string | null;
		applicationStatus: ActivityStatus | null;
	};

	type ActivityTypeListDto = {
		id: string;
		activityTypeName: string;
	};

	type ActivityUpdateDto = {
		id: string;
		description: string;
		quantity: number;
		unit: string | null;
		notes: string | null;
		date: string;
		activityTypeId: string;
		forestStandId: string | null;
		cadasterId: string | null;
		applicationStatus: ActivityStatus | null;
	};

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
			errorMessage = 'Missing activity id.';
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
					errorMessage = 'Unauthorized. Please sign in again.';
					return;
				}

				errorMessage = response.status === 404 ? 'Activity not found.' : 'Failed to load activity.';
				return;
			}

			const detail = (await response.json()) as ActivityDto;

			if (!isOwnActivity(detail)) {
				isUnauthorized = true;
				errorMessage = 'You do not have access to this activity.';
				activity = null;
				return;
			}

			activity = detail;
			fillForm(detail);
		} catch {
			errorMessage = 'Failed to load activity.';
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
			errorMessage = 'Description is required.';
			return;
		}

		if (!form.activityTypeId) {
			errorMessage = 'Activity type is required.';
			return;
		}

		const quantityRaw = String(form.quantity ?? '').trim();
		const quantity = quantityRaw === '' ? 0 : Number(quantityRaw);
		if (!Number.isFinite(quantity)) {
			errorMessage = 'Quantity must be a valid number.';
			return;
		}

		const parsedDate = form.date ? new Date(form.date) : new Date();
		if (Number.isNaN(parsedDate.getTime())) {
			errorMessage = 'Date must be valid.';
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
						? 'Validation failed. Please check your values.'
						: response.status === 404
							? 'Activity not found.'
							: response.status === 403
								? 'You are not allowed to edit this activity.'
								: 'Failed to save changes.';
				return;
			}

			const updated = (await response.json()) as ActivityDto;
			activity = updated;
			fillForm(updated);
			isEditMode = false;
			successMessage = 'Activity updated successfully.';
		} catch {
			errorMessage = 'Failed to save changes.';
		} finally {
			isSaving = false;
		}
	}

	onMount(async () => {
		await Promise.all([loadActivityTypes(), loadActivity()]);
	});
</script>

{#if isLoading}
	<div class="employee-state-block is-loading">Loading activity details…</div>
{:else if errorMessage && !activity}
	<div class="employee-state-block is-error">
		{errorMessage}
		{#if isUnauthorized}
			<span class="inline-note">Your session may have expired or access is restricted.</span>
		{/if}
	</div>
{:else if activity}
	<p class="back-link">
		<a href={resolve('/employee/[CompanyId]/activity', { CompanyId: companyId })}>← Back to activity history</a>
	</p>

	<section class="employee-card summary">
		<div class="summary-head">
			<div>
				<p class="kicker">Activity record</p>
				<h1>{activity.activityTypeName || 'Activity'}</h1>
				<p class="subtitle">Review and update your logged activity details.</p>
			</div>
			<button type="button" class="mode-btn" onclick={() => (isEditMode = !isEditMode)} disabled={isSaving}>
				{isEditMode ? 'Cancel editing' : 'Enable editing'}
			</button>
		</div>

		<div class="meta-grid">
			<p><strong>Logged by:</strong> {activity.userName || '—'}</p>
			<p><strong>Date:</strong> {formatDate(activity.date)}</p>
			<p><strong>Quantity:</strong> {formatQuantity(activity.quantity, activity.unit)}</p>
			<p><strong>Status:</strong> {activity.applicationStatus || '—'}</p>
			<p>
				<strong>Target:</strong>
				{#if activity.cadasterId}
					<a
						href={resolve('/employee/[CompanyId]/cadaster/[CadasterId]', {
							CompanyId: companyId,
							CadasterId: activity.cadasterId
						})}
					>
						Cadaster {activity.cadasterCadastralNumber ?? activity.cadasterId}
					</a>
				{:else if activity.forestStandId}
					<a
						href={resolve('/employee/[CompanyId]/foreststand/[ForestStandId]', {
							CompanyId: companyId,
							ForestStandId: activity.forestStandId
						})}
					>
						Stand {activity.forestStandNumber || activity.forestStandId}
					</a>
				{:else}
					—
				{/if}
			</p>
			{#if activity.landPropertyId}
				<p>
					<strong>Land property:</strong>
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

	<form onsubmit={saveActivity} class="employee-card detail-form">
		<h2>Edit activity</h2>

		<div class="form-grid">
			<label>
				<span>Activity type</span>
				<select bind:value={form.activityTypeId} disabled={!isEditMode}>
					{#each activityTypes as type (type.id)}
						<option value={type.id}>{type.activityTypeName}</option>
					{/each}
				</select>
			</label>

			<label>
				<span>Date</span>
				<input type="datetime-local" bind:value={form.date} readonly={!isEditMode} />
			</label>

			<label>
				<span>Quantity</span>
				<input type="number" step="any" bind:value={form.quantity} readonly={!isEditMode} />
			</label>

			<label>
				<span>Unit</span>
				<input type="text" bind:value={form.unit} readonly={!isEditMode} />
			</label>

			<label class="full-width">
				<span>Description</span>
				<textarea bind:value={form.description} rows="4" readonly={!isEditMode}></textarea>
			</label>

			<label class="full-width">
				<span>Notes</span>
				<textarea bind:value={form.notes} rows="4" readonly={!isEditMode}></textarea>
			</label>
		</div>

		<div class="form-actions">
			<button class="btn-save" type="submit" disabled={isSaving || !isEditMode}>
				{isSaving ? 'Saving...' : 'Save changes'}
			</button>
		</div>
	</form>

	{#if errorMessage}
		<div class="employee-state-block is-error">{errorMessage}</div>
	{/if}

	{#if successMessage}
		<div class="employee-state-block is-success">{successMessage}</div>
	{/if}
{/if}

<style>
	.back-link {
		margin: 0 0 0.75rem;
	}

	.back-link a {
		font-size: 0.9rem;
		font-weight: 700;
		text-decoration: none;
		color: #1f5a42;
	}

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
		margin: 0.3rem 0 0.2rem;
		font-size: 1.2rem;
		line-height: 1.2;
		color: #17251e;
	}

	h2 {
		margin: 0 0 0.75rem;
		font-size: 1.05rem;
		color: #1a3228;
	}

	.subtitle {
		margin: 0;
		color: #415a4d;
	}

	.inline-note {
		display: block;
		margin-top: 0.35rem;
		font-size: 0.88rem;
	}

	.mode-btn {
		min-height: 2.45rem;
		padding: 0.5rem 0.9rem;
		background: #1f5a42;
		color: #f6fbf8;
		border: 1px solid #184835;
		border-radius: 0.65rem;
		font-size: 0.9rem;
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
		color: #3f564a;
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
