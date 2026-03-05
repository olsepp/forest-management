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
		if (!property) return;

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
			successMessage = 'Land property updated successfully.';
		} catch {
			errorMessage = 'Failed to save changes.';
		} finally {
			isSaving = false;
		}
	}

	onMount(loadProperty);
</script>

<h1>Land property details</h1>

<p class="breadcrumb">
	<a href={`/admin/${$page.params.CompanyId}/landproperty`}>← Back to properties</a>
</p>

{#if isLoading}
	<p>Loading property details...</p>
{:else if errorMessage && !property}
	<p class="error">{errorMessage}</p>
{:else if property}
	<section class="card">
		<h2>{property.name}</h2>
		<p><strong>ID:</strong> {property.id}</p>
		<p><strong>Company:</strong> {property.companyName}</p>

		<form onsubmit={saveProperty} class="form-grid">
			<label>
				<span>Name</span>
				<input type="text" bind:value={form.name} required />
			</label>

			<label>
				<span>Registration number</span>
				<input type="number" bind:value={form.registrationNumber} required />
			</label>

			<label>
				<span>County</span>
				<input type="text" bind:value={form.county} required />
			</label>

			<label>
				<span>Parish</span>
				<input type="text" bind:value={form.parish} />
			</label>

			<label>
				<span>Village</span>
				<input type="text" bind:value={form.village} />
			</label>

			<label>
				<span>Status</span>
				<select bind:value={form.status}>
					<option value="Active">Active</option>
					<option value="Inactive">Inactive</option>
					<option value="Sold">Sold</option>
				</select>
			</label>

			<label>
				<span>Bought date</span>
				<input type="date" bind:value={form.boughtDate} />
			</label>

			<label>
				<span>Sold date</span>
				<input type="date" bind:value={form.soldDate} />
			</label>

			<div class="actions">
				<button type="submit" disabled={isSaving}>{isSaving ? 'Saving...' : 'Save changes'}</button>
			</div>
		</form>

		{#if errorMessage}
			<p class="error">{errorMessage}</p>
		{/if}

		{#if successMessage}
			<p class="success">{successMessage}</p>
		{/if}
	</section>
{/if}

<style>
	.breadcrumb {
		margin-top: -0.25rem;
		margin-bottom: 1rem;
	}

	.breadcrumb a {
		color: #0f766e;
		text-decoration: none;
	}

	.breadcrumb a:hover {
		text-decoration: underline;
	}

	.card {
		padding: 1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.75rem;
		background: #fff;
	}

	.form-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(220px, 1fr));
		gap: 0.75rem 1rem;
		margin-top: 1rem;
	}

	label {
		display: flex;
		flex-direction: column;
		gap: 0.3rem;
	}

	input,
	select {
		padding: 0.5rem 0.6rem;
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
	}

	.actions {
		grid-column: 1 / -1;
		display: flex;
		justify-content: flex-end;
	}

	button {
		border: 1px solid #d1d5db;
		background: #fff;
		border-radius: 0.5rem;
		padding: 0.45rem 0.9rem;
		cursor: pointer;
	}

	button:disabled {
		opacity: 0.65;
		cursor: not-allowed;
	}

	.error {
		margin-top: 0.75rem;
		color: #b91c1c;
	}

	.success {
		margin-top: 0.75rem;
		color: #166534;
	}
</style>
