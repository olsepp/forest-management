<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import { onMount } from 'svelte';

	type PropertyStatus = 'Active' | 'Inactive' | 'Sold';

	type LandPropertyDto = {
		id: string;
		name: string;
		registrationNumber: number;
		county: string;
		parish: string;
		village: string;
		boughtDate: string | null;
		soldDate: string | null;
		status: PropertyStatus | number | string;
		companyId: string;
		companyName: string;
	};

	type LandPropertyUpdateDto = {
		id: string;
		name: string;
		registrationNumber: number;
		county: string;
		parish: string;
		village: string;
		boughtDate: string | null;
		soldDate: string | null;
		status: PropertyStatus | number;
		companyId: string;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let isLoading = $state(true);
	let isSaving = $state(false);
	let isEditMode = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');
	let property = $state<LandPropertyDto | null>(null);

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
				typeof detail.registrationNumber === 'number'
					? String(detail.registrationNumber)
					: '',
			county: detail.county ?? '',
			parish: detail.parish ?? '',
			village: detail.village ?? '',
			boughtDate: toDateInputValue(detail.boughtDate),
			soldDate: toDateInputValue(detail.soldDate),
			status: normalizeStatus(detail.status)
		};
	}

	async function loadProperty() {
		try {
			errorMessage = '';
			successMessage = '';
			isLoading = true;

			const propertyId = $page.params.LandPropertyId;
			if (!propertyId) {
				errorMessage = 'Missing property id';
				return;
			}

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/landproperties/${propertyId}`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 404
						? 'Land property not found.'
						: response.status === 401
							? 'Unauthorized. Please sign in again.'
							: 'Failed to load land property.';
				return;
			}

			property = (await response.json()) as LandPropertyDto;
			fillForm(property);
		} catch {
			errorMessage = 'Failed to load land property.';
		} finally {
			isLoading = false;
		}
	}

	async function saveProperty(event: SubmitEvent) {
		event.preventDefault();
		if (!property || !isEditMode) return;

		const registrationNumber = Number(form.registrationNumber);
		if (!Number.isFinite(registrationNumber)) {
			errorMessage = 'Registration number must be a valid number.';
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
			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/landproperties/${property.id}`, {
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
							? 'Land property not found.'
							: 'Failed to save changes.';
				return;
			}

			const updated = (await response.json()) as LandPropertyDto;
			property = updated;
			fillForm(updated);
			isEditMode = false;
			successMessage = 'Land property updated successfully.';
		} catch {
			errorMessage = 'Failed to save changes.';
		} finally {
			isSaving = false;
		}
	}

	onMount(loadProperty);
</script>

{#if isLoading}
	<p>Loading property details...</p>
{:else if errorMessage && !property}
	<p class="message error">{errorMessage}</p>
{:else if property}
	<div class="detail-page">
		<p class="breadcrumb">
			<a href={`/admin/${$page.params.CompanyId}/landproperty`}>← Back to properties</a>
		</p>

		<header class="page-head">
			<div>
				<p class="eyebrow">Land property</p>
				<h1>{property.name}</h1>
				<p class="subtitle">Professional record and status management for this land asset.</p>
			</div>
			<button type="button" class="mode-btn" onclick={() => (isEditMode = !isEditMode)} disabled={isSaving}>
				{isEditMode ? 'Cancel editing' : 'Enable editing'}
			</button>
		</header>

		<section class="meta-grid">
			<article class="meta-card">
				<p class="meta-label">Property ID</p>
				<p class="meta-value mono">{property.id}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Company</p>
				<p class="meta-value">{property.companyName}</p>
			</article>
			<article class="meta-card">
				<p class="meta-label">Current status</p>
				<p class="meta-value">{form.status}</p>
			</article>
		</section>

		<form id="property-form" onsubmit={saveProperty} class="detail-form">
			<section class="form-section">
				<h2>Core information</h2>
				<div class="form-grid">
					<label>
						<span>Name</span>
						<input type="text" bind:value={form.name} required readonly={!isEditMode} />
					</label>
					<label>
						<span>Registration number</span>
						<input type="number" bind:value={form.registrationNumber} required readonly={!isEditMode} />
					</label>
					<label>
						<span>Status</span>
						<select bind:value={form.status} disabled={!isEditMode}>
							<option value="Active">Active</option>
							<option value="Inactive">Inactive</option>
							<option value="Sold">Sold</option>
						</select>
					</label>
				</div>
			</section>

			<section class="form-section">
				<h2>Location and timeline</h2>
				<div class="form-grid">
					<label>
						<span>County</span>
						<input type="text" bind:value={form.county} required readonly={!isEditMode} />
					</label>
					<label>
						<span>Parish</span>
						<input type="text" bind:value={form.parish} readonly={!isEditMode} />
					</label>
					<label>
						<span>Village</span>
						<input type="text" bind:value={form.village} readonly={!isEditMode} />
					</label>
					<label>
						<span>Bought date</span>
						<input type="date" bind:value={form.boughtDate} readonly={!isEditMode} />
					</label>
					<label>
						<span>Sold date</span>
						<input type="date" bind:value={form.soldDate} readonly={!isEditMode} />
					</label>
				</div>
			</section>

			<div class="form-actions">
				<button class="btn-save" type="submit" disabled={isSaving || !isEditMode}>
					{isSaving ? 'Saving...' : 'Save changes'}
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
		font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', monospace;
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

	.error {
		background: #fdebec;
	}

	.success {
		background: #e6f7ea;
	}
</style>
