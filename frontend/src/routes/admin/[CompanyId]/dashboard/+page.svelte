<script lang="ts">
	import { page } from '$app/stores';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import type { CompanyDto } from '$lib/types/company';
	import { onMount } from 'svelte';

	type LandPropertyListDto = {
		id: string;
		status: 'Active' | 'Inactive' | 'Sold' | string | number;
	};

	type PropertyCadasterLinkDto = {
		id: string;
		cadastralNumber: string;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let company = $state<CompanyDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');

	let totalProperties = $state(0);
	let totalActiveProperties = $state(0);
	let totalCadasters = $state(0);

	function normalizeStatus(status: LandPropertyListDto['status'] | null | undefined): string {
		if (typeof status === 'string') {
			return status.toLowerCase();
		}

		if (typeof status === 'number') {
			if (status === 0) return 'active';
			if (status === 1) return 'inactive';
			if (status === 2) return 'sold';
		}

		return 'inactive';
	}

	async function loadCadastersForProperty(
		propertyId: string,
		token: string
	): Promise<PropertyCadasterLinkDto[]> {
		const response = await fetch(`${apiBaseUrl}/api/cadasters/by-land-property/${propertyId}`, {
			headers: {
				Authorization: `Bearer ${token}`
			}
		});

		if (!response.ok) return [];

		const data = (await response.json()) as PropertyCadasterLinkDto[];
		return Array.isArray(data)
			? data.filter((item) => Boolean(item?.id) && Boolean(item?.cadastralNumber))
			: [];
	}

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;

			const companyId = $page.params.CompanyId;
			if (!companyId) {
				errorMessage = 'Missing company id';
				return;
			}

			const token = await authService.ensureValidToken();

			const [companyResponse, propertiesResponse] = await Promise.all([
				fetch(`${apiBaseUrl}/api/companies/${companyId}`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				}),
				fetch(`${apiBaseUrl}/api/landproperties/search?companyId=${companyId}`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				})
			]);

			if (!companyResponse.ok) {
				errorMessage =
					companyResponse.status === 401
						? 'Unauthorized. Please sign in again.'
						: 'Failed to load company';
				return;
			}

			if (!propertiesResponse.ok) {
				errorMessage =
					propertiesResponse.status === 401
						? 'Unauthorized. Please sign in again.'
						: 'Failed to load dashboard data';
				return;
			}

			company = await companyResponse.json();
			const properties = (await propertiesResponse.json()) as LandPropertyListDto[];

			totalProperties = properties.length;
			totalActiveProperties = properties.filter((item) => normalizeStatus(item.status) === 'active').length;

			const cadasterResults = await Promise.all(
				properties.map((property) => loadCadastersForProperty(property.id, token))
			);

			totalCadasters = cadasterResults.reduce((sum, cadasters) => sum + cadasters.length, 0);
		} catch {
			errorMessage = 'Failed to load dashboard data';
		} finally {
			isLoading = false;
		}
	});
</script>

<h1 class="mb-2 text-2xl font-semibold text-slate-900">Company dashboard</h1>
<p class="mb-6 text-sm text-slate-600">
	{#if company}
		Overview for <span class="font-medium text-slate-800">{company.name}</span>
	{:else}
		Overview
	{/if}
</p>

{#if isLoading}
	<p class="text-slate-600">Loading dashboard...</p>
{:else if errorMessage}
	<div class="rounded-lg border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">{errorMessage}</div>
{:else}
	<div class="grid gap-4 md:grid-cols-3">
		<div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
			<p class="text-xs font-semibold uppercase tracking-wide text-slate-500">Total properties</p>
			<p class="mt-2 text-3xl font-bold text-slate-900">{totalProperties}</p>
		</div>

		<div class="rounded-xl border border-emerald-200 bg-emerald-50 p-4 shadow-sm">
			<p class="text-xs font-semibold uppercase tracking-wide text-emerald-700">Active properties</p>
			<p class="mt-2 text-3xl font-bold text-emerald-800">{totalActiveProperties}</p>
		</div>

		<div class="rounded-xl border border-blue-200 bg-blue-50 p-4 shadow-sm">
			<p class="text-xs font-semibold uppercase tracking-wide text-blue-700">Total cadasters</p>
			<p class="mt-2 text-3xl font-bold text-blue-800">{totalCadasters}</p>
		</div>
	</div>
{/if}
