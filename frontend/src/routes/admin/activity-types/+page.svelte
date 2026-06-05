<script lang="ts">
	import { PUBLIC_API_URL } from '$env/static/public';
	import { invalidateAll } from '$app/navigation';
	import { authService } from '$lib/services/auth';
	import type { ActivityTypeListDto } from '$lib/dtos/activity-type/activity-type.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';
	const endpoint = `${apiBaseUrl}/api/activitytypes`;

	let { data }: { data: { activityTypes: ActivityTypeListDto[] } } = $props();

	let activityTypes = $derived(data.activityTypes);
	let isLoading = $derived(data.activityTypes.length === 0);
	let isSubmitting = $state(false);
	let errorMessage = $state('');
	let successMessage = $state('');

	function dismissError() {
		errorMessage = '';
	}

	function dismissSuccess() {
		successMessage = '';
	}

	let createName = $state('');
	let editingId = $state<string | null>(null);
	let editName = $state('');
	let deletingId = $state<string | null>(null);

	async function authorizedFetch(url: string, init: RequestInit = {}) {
		const token = await authService.ensureValidToken();
		const headers = new Headers(init.headers);
		headers.set('Authorization', `Bearer ${token}`);

		if (init.body && !headers.has('Content-Type')) {
			headers.set('Content-Type', 'application/json');
		}

		return fetch(url, { ...init, headers });
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

	function startDelete(id: string) {
		deletingId = id;
		errorMessage = '';
		successMessage = '';
	}

	function cancelDelete() {
		deletingId = null;
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
			await invalidateAll();
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
			await invalidateAll();
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
			deletingId = null;
			successMessage = 'Tegevuse tüüp kustutati.';
			await invalidateAll();
		} catch {
			errorMessage = 'Tegevuse tüübi kustutamine ebaõnnestus.';
		} finally {
			isSubmitting = false;
		}
	}
</script>

<h1 class="mb-4 text-2xl font-semibold text-slate-900">Tegevuse tüübid</h1>

<section class="mb-6 rounded-xl border border-slate-300 bg-[#174834] p-5 shadow-sm">
	<h2 class="mb-4 text-lg font-semibold !text-white">Loo tegevuse tüüp</h2>
	<label class="create-row text-sm text-slate-700">
		<span class="create-label font-medium !text-slate-100">Nimi</span>
		<input
			type="text"
			bind:value={createName}
			placeholder="nt Istutamine"
			class="create-input rounded-lg border border-slate-300 px-3 py-2 text-slate-900 transition outline-none focus:border-emerald-500 focus:ring-1 focus:ring-emerald-500"
			class:create-input={true}
		/>
	</label>

	<div class="mt-5">
		<button
			type="button"
			onclick={createActivityType}
			disabled={isSubmitting}
			class="inline-flex cursor-pointer items-center rounded-lg bg-emerald-600 px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-emerald-700 disabled:cursor-not-allowed disabled:opacity-60"
		>
			{isSubmitting ? 'Loomisel...' : 'Loo'}
		</button>
	</div>
</section>

{#if errorMessage}
	<div
		class="mb-4 flex items-center justify-between rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700"
	>
		<span>{errorMessage}</span>
		<button
			type="button"
			onclick={dismissError}
			class="dismiss-btn cursor-pointer text-red-700 hover:text-red-900"
		>
			✕
		</button>
	</div>
{/if}

{#if successMessage}
	<div
		class="mb-4 flex items-center justify-between rounded-lg border border-emerald-200 bg-emerald-50 px-3 py-2 text-sm text-emerald-700"
	>
		<span>{successMessage}</span>
		<button
			type="button"
			onclick={dismissSuccess}
			class="dismiss-btn cursor-pointer text-emerald-700 hover:text-emerald-900"
		>
			✕
		</button>
	</div>
{/if}

<section class="my-[1rem] bg-white p-[1rem]">
	<h2 class="text-xl font-semibold text-slate-900">NB!</h2>
	<p class="text-bold mt-2 text-lg text-red-700">
		Kustutades tegevuse tüübi, kustuvad ka kõik tegevused, mis tegevuse tüübiga seotud.
	</p>
</section>

{#if isLoading}
	<p>Laetakse tegevuse tüüpe...</p>
{:else if activityTypes.length === 0}
	<p>Tegevuse tüüpe ei leitud.</p>
{:else}
	<div class="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
		<table class="min-w-full divide-y divide-slate-200 text-base">
			<thead>
				<tr>
					<th class="px-4 py-3 text-left font-semibold text-slate-700">Nimi</th>
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
									class="w-full rounded-lg border border-slate-300 px-2 py-1.5 transition outline-none focus:border-emerald-500"
								/>
							</td>
							<td class="px-4 py-3">
								<div class="flex gap-2">
									<button
										type="button"
										onclick={() => updateActivityType(item.id)}
										disabled={isSubmitting}
										class="btn-green inline-flex cursor-pointer items-center rounded-lg border border-[#174834] bg-[#174834] px-3 py-1.5 text-sm font-medium text-white hover:bg-[#235c44] disabled:opacity-60"
									>
										Salvesta
									</button>
									<button
										type="button"
										onclick={cancelEdit}
										class="btn-red inline-flex cursor-pointer items-center rounded-lg border border-red-600 bg-red-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-red-700"
									>
										Tühista
									</button>
								</div>
							</td>
						{:else}
							<td class="px-4 py-3 text-slate-900">{item.activityTypeName}</td>
							<td class="px-4 py-3">
								<div class="flex gap-2">
									<button
										type="button"
										onclick={() => startEdit(item)}
										class="btn-green inline-flex cursor-pointer items-center rounded-lg border border-[#174834] bg-[#174834] px-3 py-1.5 text-sm font-medium text-white hover:bg-[#235c44]"
									>
										Muuda
									</button>
									<button
										type="button"
										onclick={() => startDelete(item.id)}
										disabled={isSubmitting}
										class="btn-red inline-flex cursor-pointer items-center rounded-lg border border-red-600 bg-red-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-red-700 disabled:opacity-60"
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

{#if deletingId}
	{@const deleteItem = activityTypes.find((t) => t.id === deletingId)}
	<div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
		<div class="w-full max-w-md rounded-xl border-2 border-red-500 bg-red-50 p-6 shadow-xl">
			<h2 class="mb-4 text-xl font-semibold text-red-800">Kustuta tegevuse tüüp</h2>
			<p class="mb-6 text-base text-red-700">
				Kas olete kindel, et soovite kustuta tegevuse tüübi "{deleteItem?.activityTypeName}"? Seda
				toimingut ei saa tagasi võtta.
			</p>
			<div class="flex gap-3">
				<button
					type="button"
					onclick={() => deleteActivityType(deletingId!)}
					disabled={isSubmitting}
					class="btn-red inline-flex cursor-pointer items-center rounded-lg border border-red-600 bg-red-600 px-4 py-2 text-sm font-semibold text-white hover:bg-red-700 disabled:opacity-60"
				>
					{isSubmitting ? 'Kustutamisel...' : 'Kustuta'}
				</button>
				<button
					type="button"
					onclick={cancelDelete}
					class="btn-green inline-flex cursor-pointer items-center rounded-lg border border-[#174834] bg-[#174834] px-4 py-2 text-sm font-semibold text-white hover:bg-[#235c44]"
				>
					Tühista
				</button>
			</div>
		</div>
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

	.btn-green {
		background: #174834 !important;
		border-color: #174834 !important;
		color: #ffffff !important;
	}
	.btn-green:hover {
		background: #235c44 !important;
		border-color: #235c44 !important;
	}

	.btn-red {
		background: #dc2626 !important;
		border-color: #dc2626 !important;
		color: #ffffff !important;
	}
	.btn-red:hover {
		background: #b91c1c !important;
		border-color: #b91c1c !important;
	}

	.dismiss-btn {
		background: transparent !important;
		border: none !important;
		padding: 0 !important;
	}
</style>
