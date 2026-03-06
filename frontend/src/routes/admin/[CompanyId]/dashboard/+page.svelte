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

	type ActivityListDto = {
		id: string;
		date: string;
	};

	type ActivityChartPoint = {
		label: string;
		count: number;
		x: number;
		y: number;
	};

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let company = $state<CompanyDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');

	let totalProperties = $state(0);
	let totalActiveProperties = $state(0);
	let totalCadasters = $state(0);
	let activityChartPoints = $state<ActivityChartPoint[]>([]);
	let maxDailyActivityCount = $state(0);

	const chartWidth = 880;
	const chartHeight = 240;
	const chartPadding = { top: 16, right: 16, bottom: 28, left: 16 };

	function dayKey(date: Date): string {
		const year = date.getFullYear();
		const month = String(date.getMonth() + 1).padStart(2, '0');
		const day = String(date.getDate()).padStart(2, '0');
		return `${year}-${month}-${day}`;
	}

	function last30DayKeys(): string[] {
		const keys: string[] = [];
		const today = new Date();
		today.setHours(0, 0, 0, 0);

		for (let offset = 29; offset >= 0; offset -= 1) {
			const d = new Date(today);
			d.setDate(today.getDate() - offset);
			keys.push(dayKey(d));
		}

		return keys;
	}

	function shortLabel(isoDate: string): string {
		const d = new Date(`${isoDate}T00:00:00`);
		return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
	}

	function buildActivityChartPoints(activities: ActivityListDto[]): ActivityChartPoint[] {
		const keys = last30DayKeys();
		const countsByDay: Record<string, number> = {};

		for (const key of keys) countsByDay[key] = 0;

		for (const activity of activities) {
			const date = new Date(activity.date);
			if (Number.isNaN(date.getTime())) continue;
			const key = dayKey(date);
			if (key in countsByDay) countsByDay[key] += 1;
		}

		const maxCount = Math.max(...keys.map((key) => countsByDay[key]), 0);
		maxDailyActivityCount = maxCount;

		const effectiveMax = Math.max(maxCount, 1);
		const innerWidth = chartWidth - chartPadding.left - chartPadding.right;
		const innerHeight = chartHeight - chartPadding.top - chartPadding.bottom;

		return keys.map((key, index) => {
			const count = countsByDay[key];
			const x = chartPadding.left + (index / (keys.length - 1)) * innerWidth;
			const y = chartPadding.top + (1 - count / effectiveMax) * innerHeight;
			return {
				label: shortLabel(key),
				count,
				x,
				y
			};
		});
	}

	const activityPolylinePoints = $derived.by(() =>
		activityChartPoints.map((point) => `${point.x},${point.y}`).join(' ')
	);

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

			const [companyResponse, propertiesResponse, activitiesResponse] = await Promise.all([
				fetch(`${apiBaseUrl}/api/companies/${companyId}`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				}),
				fetch(`${apiBaseUrl}/api/landproperties/search?companyId=${companyId}`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				}),
				fetch(`${apiBaseUrl}/api/activities`, {
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

			if (!activitiesResponse.ok) {
				errorMessage =
					activitiesResponse.status === 401
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

			const activities = (await activitiesResponse.json()) as ActivityListDto[];
			activityChartPoints = buildActivityChartPoints(Array.isArray(activities) ? activities : []);
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

	<section class="mt-6 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
		<div class="mb-4 flex items-center justify-between">
			<h2 class="text-lg font-semibold text-slate-900">Activity trend (last 30 days)</h2>
			<p class="text-xs text-slate-500">Max/day: {maxDailyActivityCount}</p>
		</div>

		{#if activityChartPoints.length === 0}
			<p class="text-sm text-slate-600">No activity data available.</p>
		{:else}
			<div class="chart-wrap">
				<svg viewBox={`0 0 ${chartWidth} ${chartHeight}`} class="activity-chart" role="img" aria-label="Activity trend line chart">
					<line
						x1={chartPadding.left}
						y1={chartHeight - chartPadding.bottom}
						x2={chartWidth - chartPadding.right}
						y2={chartHeight - chartPadding.bottom}
						stroke="#e2e8f0"
					/>
					<polyline
						points={activityPolylinePoints}
						fill="none"
						stroke="#0f766e"
						stroke-width="2.5"
						stroke-linecap="round"
						stroke-linejoin="round"
					/>
					{#each activityChartPoints as point, index}
						<circle cx={point.x} cy={point.y} r="2.75" fill="#0f766e">
							<title>{point.label}: {point.count} activities</title>
						</circle>
						{#if index % 5 === 0 || index === activityChartPoints.length - 1}
							<text
								x={point.x}
								y={chartHeight - 8}
								text-anchor="middle"
								font-size="10"
								fill="#64748b"
							>
								{point.label}
							</text>
						{/if}
					{/each}
				</svg>
			</div>
		{/if}
	</section>
{/if}

<style>
	.chart-wrap {
		overflow-x: auto;
	}

	.activity-chart {
		width: 100%;
		min-width: 760px;
		height: auto;
		display: block;
	}
</style>
