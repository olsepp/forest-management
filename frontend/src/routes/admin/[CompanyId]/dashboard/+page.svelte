<script lang="ts">
	import { page } from '$app/stores';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import type { CompanyDto } from '$lib/dtos/company/company.dto';
	import type { ActivityDto } from '$lib/dtos/activity/activity.dto';
	import { onMount } from 'svelte';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	interface ActivityCountByDay {
		date: string;
		count: number;
	}

	function dayKey(date: Date): string {
		const year = date.getFullYear();
		const month = String(date.getMonth() + 1).padStart(2, '0');
		const day = String(date.getDate()).padStart(2, '0');
		return `${year}-${month}-${day}`;
	}

	function last30DayKeys(): string[] {
		const keys: string[] = [];
		const millisecondsPerDay = 24 * 60 * 60 * 1000;
		const now = new Date();
		const todayStartTimestamp = new Date(
			now.getFullYear(),
			now.getMonth(),
			now.getDate()
		).getTime();

		for (let offset = 29; offset >= 0; offset -= 1) {
			const d = new Date(todayStartTimestamp - offset * millisecondsPerDay);
			keys.push(dayKey(d));
		}

		return keys;
	}

	function normalizeAndBuildChartPoints(
		apiCounts: ActivityCountByDay[]
	): { label: string; count: number; x: number; y: number }[] {
		const keys = last30DayKeys();
		const countsMap: Record<string, number> = {};
		for (const key of keys) countsMap[key] = 0;
		for (const item of apiCounts) {
			countsMap[item.date] = item.count;
		}

		const countValues = keys.map((key) => countsMap[key] || 0);
		const maxCount = Math.max(...countValues, 0);
		const effectiveMax = Math.max(maxCount, 1);
		const innerWidth = chartWidth - chartPadding.left - chartPadding.right;
		const innerHeight = chartHeight - chartPadding.top - chartPadding.bottom;

		return keys.map((key, index) => {
			const count = countsMap[key] || 0;
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

	interface DashboardSummary {
		totalProperties: number;
		totalActiveProperties: number;
		totalCadasters: number;
		activityCountsByDay: ActivityCountByDay[];
		recentActivities: ActivityDto[];
	}

	let company = $state<CompanyDto | null>(null);
	let isLoading = $state(true);
	let errorMessage = $state('');

	let totalProperties = $state(0);
	let totalActiveProperties = $state(0);
	let totalCadasters = $state(0);
	let maxDailyActivityCount = $state(0);
	let recentActivities = $state<ActivityDto[]>([]);
	let activityCountsByDay = $state<ActivityCountByDay[]>([]);
	let activityChartPointsArray = $state<{ label: string; count: number; x: number; y: number }[]>(
		[]
	);

	const companyId = $derived($page.params.CompanyId ?? '');

	const chartWidth = 880;
	const chartHeight = 240;
	const chartPadding = { top: 16, right: 16, bottom: 28, left: 16 };

	function shortLabel(isoDate: string): string {
		const d = new Date(`${isoDate}T00:00:00`);
		return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
	}

	function formatDateTime(value: string): string {
		const date = new Date(value);
		if (Number.isNaN(date.getTime())) return '—';
		return date.toLocaleString();
	}

	const activityPolylinePoints = $derived.by(() =>
		activityChartPointsArray.map((point) => `${point.x},${point.y}`).join(' ')
	);

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;

			const companyId = $page.params.CompanyId;
			if (!companyId) {
				errorMessage = 'Puudub ettevõtte ID.';
				return;
			}

			const token = await authService.ensureValidToken();

			const [companyResponse, dashboardResponse] = await Promise.all([
				fetch(`${apiBaseUrl}/api/companies/${companyId}`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				}),
				fetch(`${apiBaseUrl}/api/dashboard/${companyId}/summary`, {
					headers: {
						Authorization: `Bearer ${token}`
					}
				})
			]);

			if (!companyResponse.ok) {
				errorMessage =
					companyResponse.status === 401
						? 'Ligipääs puudub. Logige uuesti sisse.'
						: 'Ettevõtte laadimine ebaõnnestus.';
				return;
			}

			if (!dashboardResponse.ok) {
				errorMessage =
					dashboardResponse.status === 401
						? 'Ligipääs puudub. Logige uuesti sisse.'
						: 'Töölaua andmete laadimine ebaõnnestus.';
				return;
			}

			company = await companyResponse.json();
			const summary = (await dashboardResponse.json()) as DashboardSummary;
			console.log('Dashboard summary:', summary);

			totalProperties = summary.totalProperties;
			totalActiveProperties = summary.totalActiveProperties;
			totalCadasters = summary.totalCadasters;
			activityCountsByDay = summary.activityCountsByDay ?? [];
			activityChartPointsArray = normalizeAndBuildChartPoints(activityCountsByDay);
			maxDailyActivityCount = Math.max(...activityCountsByDay.map((d) => d.count), 0);
			recentActivities = summary.recentActivities ?? [];
		} catch {
			errorMessage = 'Töölaua andmete laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	});
</script>

<h1 class="mb-2 text-2xl font-semibold text-slate-900">Ettevõtte töölaud</h1>
<p class="mb-6 text-sm text-slate-600">
	{#if company}
		Ülevaade ettevõttele <span class="font-medium text-slate-800">{company.name}</span>
	{:else}
		Ülevaade
	{/if}
</p>

{#if isLoading}
	<p class="text-slate-600">Laadakse töölauda...</p>
{:else if errorMessage}
	<div class="rounded-lg border border-rose-200 bg-rose-50 p-3 text-sm text-rose-700">
		{errorMessage}
	</div>
{:else}
	<div class="grid gap-4 md:grid-cols-3">
		<div class="rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
			<p class="text-xs font-semibold tracking-wide text-slate-500 uppercase">Kinnistuid kokku</p>
			<p class="mt-2 text-3xl font-bold text-slate-900">{totalProperties}</p>
		</div>

		<div class="rounded-xl border border-emerald-200 bg-emerald-50 p-4 shadow-sm">
			<p class="text-xs font-semibold tracking-wide text-emerald-700 uppercase">
				Aktiivseid kinnistuid
			</p>
			<p class="mt-2 text-3xl font-bold text-emerald-800">{totalActiveProperties}</p>
		</div>

		<div class="rounded-xl border border-blue-200 bg-blue-50 p-4 shadow-sm">
			<p class="text-xs font-semibold tracking-wide text-blue-700 uppercase">
				Katastrite arv kokku
			</p>
			<p class="mt-2 text-3xl font-bold text-blue-800">{totalCadasters}</p>
		</div>
	</div>

	<section class="mt-6 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
		<div class="mb-4 flex items-center justify-between">
			<h2 class="text-lg font-semibold text-slate-900">Tegevuste trend (viimased 30 päeva)</h2>
			<p class="text-xs text-slate-500">Maks/päev: {maxDailyActivityCount}</p>
		</div>

		{#if !activityChartPointsArray.length && !recentActivities.length}
			<p class="text-sm text-slate-600">Tegevusandmed puuduvad.</p>
		{:else}
			<div class="chart-wrap">
				<svg
					viewBox={`0 0 ${chartWidth} ${chartHeight}`}
					class="activity-chart"
					role="img"
					aria-label="Tegevuste trendijoonis"
				>
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
					{#each activityChartPointsArray as point, index (point.label)}
						<circle cx={point.x} cy={point.y} r="2.75" fill="#0f766e">
							<title>{point.label}: {point.count} tegevust</title>
						</circle>
						{#if index % 5 === 0 || index === activityChartPointsArray.length - 1}
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

	<section class="mt-6 rounded-xl border border-slate-200 bg-white p-4 shadow-sm">
		<div class="mb-4 flex items-center justify-between gap-3">
			<h2 class="text-lg font-semibold text-slate-900">Viimased tegevused</h2>
			<a
				class="text-sm font-medium text-teal-700 hover:text-teal-800"
				href={resolve('/admin/[CompanyId]/activity', { CompanyId: companyId })}
				>Ava kõik tegevused →</a
			>
		</div>

		{#if recentActivities.length === 0}
			<p class="text-sm text-slate-600">Tegevused puuduvad.</p>
		{:else}
			<div class="overflow-hidden rounded-lg border border-slate-200">
				<table class="min-w-full text-sm">
					<thead>
						<tr class="border-b border-slate-200">
							<th class="px-3 py-2.5 text-left font-semibold text-slate-600">Kuupäev</th>
							<th class="px-3 py-2.5 text-left font-semibold text-slate-600">Tüüp</th>
							<th class="px-3 py-2.5 text-left font-semibold text-slate-600">Kirjeldus</th>
							<th class="px-3 py-2.5 text-left font-semibold text-slate-600">Kasutaja</th>
							<th class="px-3 py-2.5 text-right font-semibold text-slate-600"></th>
						</tr>
					</thead>
					<tbody>
						{#each recentActivities as activity (activity.id)}
							<tr class="border-b border-slate-100 text-slate-700">
								<td class="px-3 py-2.5">{formatDateTime(activity.date)}</td>
								<td class="px-3 py-2.5">{activity.activityTypeName ?? '—'}</td>
								<td class="px-3 py-2.5">{activity.description ?? '—'}</td>
								<td class="px-3 py-2.5">{activity.userName ?? '—'}</td>
								<td class="px-3 py-2.5 text-right">
									<a
										class="group inline-flex h-8 w-8 items-center justify-center rounded-full border border-[#cad6cf] bg-white text-[#1f5a42] shadow-sm transition-all hover:border-[#1f5a42] hover:bg-[#174834]"
										href={resolve('/admin/[CompanyId]/activity/[ActivityId]', {
											CompanyId: companyId,
											ActivityId: activity.id
										})}
										aria-label="Vaata tegevust"
									>
										<svg
											class="h-4 w-4 transition-transform group-hover:translate-x-0.5 group-hover:stroke-white"
											fill="none"
											viewBox="0 0 24 24"
											stroke="currentColor"
											stroke-width="2.5"
											stroke-linecap="round"
											stroke-linejoin="round"
										>
											<path d="M5 12h14M12 5l7 7-7 7" />
										</svg>
									</a>
								</td>
							</tr>
						{/each}
					</tbody>
				</table>
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
