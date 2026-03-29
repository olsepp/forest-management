<script lang="ts">
	import { onMount } from 'svelte';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';

	type ActivityTypeListDto = {
		id: string;
		activityTypeName: string;
	};

	type ActivityTypeDto = {
		id: string;
		activityTypeName: string;
		activityCount: number;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';
	const endpoint = `${apiBaseUrl}/api/activitytypes`;

	let activityTypes = $state<ActivityTypeListDto[]>([]);
	let activityTypeDetailsById = $state<Record<string, ActivityTypeDto>>({});
	let isLoading = $state(true);
	let isSubmitting = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');

	let createName = $state('');
	let editingId = $state<string | null>(null);
	let editName = $state('');

	function normalizeList(payload: unknown): ActivityTypeListDto[] {
		if (!Array.isArray(payload)) return [];

		return payload
			.filter((item): item is Record<string, unknown> => typeof item === 'object' && item !== null)
			.map((item) => ({
				id: String(item.id ?? ''),
				activityTypeName: String(item.activityTypeName ?? '')
			}))
			.filter((item) => item.id && item.activityTypeName.trim().length > 0);
	}

	function normalizeDetails(payload: unknown): ActivityTypeDto | null {
		if (typeof payload !== 'object' || payload === null) return null;
		const item = payload as Record<string, unknown>;

		return {
			id: String(item.id ?? ''),
			activityTypeName: String(item.activityTypeName ?? ''),
			activityCount: typeof item.activityCount === 'number' ? item.activityCount : 0
		};
	}

	async function authorizedFetch(url: string, init: RequestInit = {}) {
		const token = await authService.ensureValidToken();
		const headers = new Headers(init.headers);
		headers.set('Authorization', `Bearer ${token}`);

		if (init.body && !headers.has('Content-Type')) {
			headers.set('Content-Type', 'application/json');
		}

		return fetch(url, { ...init, headers });
	}

	async function loadActivityTypes() {
		try {
			isLoading = true;
			errorMessage = '';

			const response = await authorizedFetch(endpoint);
			if (!response.ok) {
				errorMessage = 'Tegevuse tüüpide laadimine ebaõnnestus.';
				activityTypes = [];
				activityTypeDetailsById = {};
				return;
			}

			const list = normalizeList(await response.json());
			activityTypes = list;

			const detailsEntries = await Promise.all(
				list.map(async (item) => {
					try {
						const detailsResponse = await authorizedFetch(`${endpoint}/${item.id}`);
						if (!detailsResponse.ok) {
							return [
								item.id,
								{ id: item.id, activityTypeName: item.activityTypeName, activityCount: 0 }
							] as const;
						}

						const details = normalizeDetails(await detailsResponse.json());
						return [
							item.id,
							details ?? { id: item.id, activityTypeName: item.activityTypeName, activityCount: 0 }
						] as const;
					} catch {
						return [
							item.id,
							{ id: item.id, activityTypeName: item.activityTypeName, activityCount: 0 }
						] as const;
					}
				})
			);

			activityTypeDetailsById = Object.fromEntries(detailsEntries);
		} catch {
			errorMessage = 'Tegevuse tüüpide laadimine ebaõnnestus.';
			activityTypes = [];
			activityTypeDetailsById = {};
		} finally {
			isLoading = false;
		}
	}

	function startEdit(item: ActivityTypeListDto) {
		editingId = item.id;
		editName = item.activityTypeName;
		errorMessage = '';
		successMessage = '';
	}

	function cancelEdit() {
		editingId = null;
		editName = '';
	}

	async function createActivityType() {
		const name = createName.trim();
		if (!name) {
			errorMessage = 'Nimi on kohustuslik.';
			return;
		}

		try {
			isSubmitting = true;
			errorMessage = '';
			successMessage = '';

			const response = await authorizedFetch(endpoint, {
				method: 'POST',
				body: JSON.stringify({ activityTypeName: name })
			});

			if (!response.ok) {
				errorMessage = 'Tegevuse tüübi loomine ebaõnnestus.';
				return;
			}

			createName = '';
			successMessage = 'Tegevuse tüüp loodi.';
			await loadActivityTypes();
		} catch {
			errorMessage = 'Tegevuse tüübi loomine ebaõnnestus.';
		} finally {
			isSubmitting = false;
		}
	}

	async function updateActivityType(id: string) {
		const name = editName.trim();
		if (!name) {
			errorMessage = 'Nimi on kohustuslik.';
			return;
		}

		try {
			isSubmitting = true;
			errorMessage = '';
			successMessage = '';

			const response = await authorizedFetch(`${endpoint}/${id}`, {
				method: 'PUT',
				body: JSON.stringify({ id, activityTypeName: name })
			});

			if (!response.ok) {
				errorMessage = 'Tegevuse tüübi uuendamine ebaõnnestus.';
				return;
			}

			cancelEdit();
			successMessage = 'Tegevuse tüüp uuendati.';
			await loadActivityTypes();
		} catch {
			errorMessage = 'Tegevuse tüübi uuendamine ebaõnnestus.';
		} finally {
			isSubmitting = false;
		}
	}

	async function deleteActivityType(id: string) {
		try {
			isSubmitting = true;
			errorMessage = '';
			successMessage = '';

			const response = await authorizedFetch(`${endpoint}/${id}`, {
				method: 'DELETE'
			});

			if (!response.ok) {
				errorMessage = 'Tegevuse tüübi kustutamine ebaõnnestus.';
				return;
			}

			if (editingId === id) cancelEdit();
			successMessage = 'Tegevuse tüüp kustutati.';
			await loadActivityTypes();
		} catch {
			errorMessage = 'Tegevuse tüübi kustutamine ebaõnnestus.';
		} finally {
			isSubmitting = false;
		}
	}

	onMount(loadActivityTypes);
</script>

<h1 class="mb-4 text-2xl font-semibold text-slate-900">Tegevuse tüübid</h1>

<section class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm mb-6">
	<h2 class="mb-3 text-lg font-semibold text-slate-800">Loo tegevuse tüüp</h2>
	<label class="text-sm text-slate-700 create-row">
		<span class="font-medium create-label">Nimi</span>
		<input
			type="text"
			bind:value={createName}
			placeholder="nt Istutamine"
			class="rounded-lg border border-slate-300 px-3 py-2 outline-none transition focus:border-emerald-500"
			class:create-input={true}
		/>
	</label>

	<div class="mt-5">
		<button
			type="button"
			onclick={createActivityType}
			disabled={isSubmitting}
			class="rounded-lg border border-slate-300 px-5 py-2.5 text-sm font-semibold transition disabled:cursor-not-allowed disabled:opacity-60"
		>
			Loo
		</button>
	</div>
</section>

{#if errorMessage}
	<p class="mb-4 rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">{errorMessage}</p>
{/if}

{#if successMessage}
	<p class="mb-4 rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700">
		{successMessage}
	</p>
{/if}

{#if isLoading}
	<p>Laetakse tegevuse tüüpe...</p>
{:else if activityTypes.length === 0}
	<p>Tegevuse tüüpe ei leitud.</p>
{:else}
	<div class="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
		<table class="min-w-full divide-y divide-slate-200 text-sm">
			<thead class="bg-slate-50">
				<tr>
					<th class="px-4 py-3 text-left font-semibold text-slate-700">Nimi</th>
					<th class="px-4 py-3 text-left font-semibold text-slate-700">Tegevuste arv</th>
					<th class="px-4 py-3 text-left font-semibold text-slate-700">Toimingud</th>
				</tr>
			</thead>
			<tbody class="divide-y divide-slate-100">
				{#each activityTypes as item (item.id)}
					<tr class="hover:bg-slate-50">
						{#if editingId === item.id}
							<td class="px-4 py-3">
								<input
									type="text"
									bind:value={editName}
									class="w-full rounded-lg border border-slate-300 px-2 py-1.5 outline-none transition focus:border-emerald-500"
								/>
							</td>
							<td class="px-4 py-3">{activityTypeDetailsById[item.id]?.activityCount ?? 0}</td>
							<td class="px-4 py-3">
								<div class="flex gap-2">
									<button
										type="button"
										onclick={() => updateActivityType(item.id)}
										disabled={isSubmitting}
									class="inline-flex items-center rounded-lg border border-emerald-300 px-3 py-1.5 text-sm font-medium text-emerald-700 hover:bg-emerald-50 disabled:opacity-60"
								>
									Salvesta
								</button>
								<button
									type="button"
									onclick={cancelEdit}
									class="inline-flex items-center rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
								>
									Tühista
								</button>
							</div>
						</td>
						{:else}
							<td class="px-4 py-3 text-slate-900">{item.activityTypeName}</td>
							<td class="px-4 py-3 text-slate-700">{activityTypeDetailsById[item.id]?.activityCount ?? 0}</td>
							<td class="px-4 py-3">
								<div class="flex gap-2">
									<button
										type="button"
										onclick={() => startEdit(item)}
									class="inline-flex items-center rounded-lg border border-slate-300 px-3 py-1.5 text-sm font-medium text-slate-700 hover:bg-slate-50"
								>
									Muuda
								</button>
								<button
									type="button"
									onclick={() => deleteActivityType(item.id)}
									disabled={isSubmitting}
									class="inline-flex items-center rounded-lg border border-red-300 px-3 py-1.5 text-sm font-medium text-red-700 hover:bg-red-50 disabled:opacity-60"
								>
									Kustuta
								</button>
							</div>
						</td>
						{/if}
					</tr>
				{/each}
			</tbody>
		</table>
	</div>
{/if}

<style>
	.create-row {
		display: flex;
		align-items: center;
		column-gap: 0.75rem;
	}

	.create-label {
		white-space: nowrap;
	}

	:global(.create-input) {
		flex: 1 1 auto;
	}
</style>
