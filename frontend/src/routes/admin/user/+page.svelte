<script lang="ts">
	import { onMount } from 'svelte';
	import { resolve } from '$app/paths';
	import { PUBLIC_API_URL } from '$env/static/public';
	import { authService } from '$lib/services/auth';
	import type { UserListDto, UserDetailsDto } from '$lib/dtos/user/user.dto';

	const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

	let users = $state<UserListDto[]>([]);
	let userDetailsById = $state<Record<string, UserDetailsDto>>({});
	let expandedUserIds = $state<string[]>([]);
	let isLoading = $state(true);
	let errorMessage = $state('');

	function getFirstName(user: UserListDto | UserDetailsDto): string {
		if (typeof user.firstName === 'string' && user.firstName.trim()) return user.firstName;
		return '-';
	}

	function getLastName(user: UserListDto | UserDetailsDto): string {
		if (typeof user.lastName === 'string' && user.lastName.trim()) return user.lastName;
		return '-';
	}

	function isExpanded(userId: string): boolean {
		return expandedUserIds.includes(userId);
	}

	function toggleExpand(userId: string) {
		expandedUserIds = isExpanded(userId)
			? expandedUserIds.filter((id) => id !== userId)
			: [...expandedUserIds, userId];
	}

	function detailEntries(user: UserDetailsDto): [string, unknown][] {
		return Object.entries(user).sort(([a], [b]) => a.localeCompare(b));
	}

	onMount(async () => {
		try {
			errorMessage = '';
			isLoading = true;

			const token = await authService.ensureValidToken();
			const response = await fetch(`${apiBaseUrl}/api/users`, {
				headers: {
					Authorization: `Bearer ${token}`
				}
			});

			if (!response.ok) {
				errorMessage =
					response.status === 401
						? 'Ligipääs puudub. Logige uuesti sisse.'
						: response.status === 403
							? 'Keelatud. Vajalik on admini roll.'
							: 'Kasutajate laadimine ebaõnnestus.';
				return;
			}

			users = await response.json();

			const detailsEntries = await Promise.all(
				users.map(async (user) => {
					try {
						const detailsResponse = await fetch(`${apiBaseUrl}/api/users/${user.id}`, {
							headers: {
								Authorization: `Bearer ${token}`
							}
						});

						if (!detailsResponse.ok) {
							return [user.id, user] as const;
						}

						const details = (await detailsResponse.json()) as UserDetailsDto;
						return [user.id, { ...user, ...details }] as const;
					} catch {
						return [user.id, user] as const;
					}
				})
			);

			userDetailsById = Object.fromEntries(detailsEntries);
		} catch {
			errorMessage = 'Kasutajate laadimine ebaõnnestus.';
		} finally {
			isLoading = false;
		}
	});
</script>

<div class="mb-4 flex items-center justify-between gap-3">
	<h1 class="text-2xl font-semibold text-slate-900">Kasutajad</h1>
	<a
		href={resolve('/admin/user/new')}
		class="inline-flex items-center rounded-lg bg-emerald-800 px-3 py-2 text-sm font-semibold text-white no-underline hover:bg-emerald-900 hover:no-underline"
		style="color: white !important; text-decoration: none !important;"
	>
		Loo kasutaja
	</a>
</div>

{#if isLoading}
	<p>Laetakse kasutajaid...</p>
{:else if errorMessage}
	<p class="rounded-lg border border-red-200 bg-red-50 px-3 py-2 text-sm text-red-700">
		{errorMessage}
	</p>
{:else if users.length === 0}
	<p>Kasutajaid ei leitud.</p>
{:else}
	<div class="overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm">
		<table class="min-w-full divide-y divide-slate-200 text-sm">
			<thead class="bg-slate-50">
				<tr>
					<th class="px-4 py-3 text-left font-semibold text-slate-700">Eesnimi</th>
					<th class="px-4 py-3 text-left font-semibold text-slate-700">Perekonnanimi</th>
					<th class="px-4 py-3 text-left font-semibold text-slate-700">Email</th>
					<th class="px-4 py-3 text-left font-semibold text-slate-700">Toimingud</th>
				</tr>
			</thead>
			<tbody class="divide-y divide-slate-100">
				{#each users as user (user.id)}
					<tr class="hover:bg-slate-50">
						<td class="px-4 py-3 text-slate-900"
							>{getFirstName(userDetailsById[user.id] ?? user)}</td
						>
						<td class="px-4 py-3 text-slate-900">{getLastName(userDetailsById[user.id] ?? user)}</td
						>
						<td class="px-4 py-3 text-slate-700">{user.email}</td>
						<td class="px-4 py-3">
							<button
								type="button"
								onclick={() => toggleExpand(user.id)}
								class="expand-toggle"
								aria-label={isExpanded(user.id)
									? 'Peida kasutaja detailid'
									: 'Näita kasutaja detaile'}
								aria-expanded={isExpanded(user.id)}
							>
								<svg
									class={`expand-icon ${isExpanded(user.id) ? 'open' : ''}`}
									viewBox="0 0 24 24"
									fill="none"
									stroke="currentColor"
									stroke-width="2.75"
									stroke-linecap="round"
									stroke-linejoin="round"
									aria-hidden="true"
								>
									<path d="M6 9l6 6 6-6" />
								</svg>
							</button>
						</td>
					</tr>
					{#if isExpanded(user.id)}
						<tr class="bg-slate-50/60">
							<td colspan="4" class="px-4 py-3">
								<div class="rounded-lg border border-slate-200 bg-white p-3">
									<p class="mb-2 text-xs font-semibold tracking-wide text-slate-500 uppercase">
										Kõik saadaolevad kasutaja väljad
									</p>
									<dl class="grid grid-cols-1 gap-2 sm:grid-cols-2">
										{#each detailEntries(userDetailsById[user.id] ?? user) as [key, value] (key)}
											<div class="rounded border border-slate-200 bg-slate-50 p-2">
												<dt class="text-xs font-semibold text-slate-600">{key}</dt>
												<dd class="mt-0.5 font-mono text-xs break-all text-slate-800">
													{typeof value === 'string' ? value : JSON.stringify(value)}
												</dd>
											</div>
										{/each}
									</dl>
								</div>
							</td>
						</tr>
					{/if}
				{/each}
			</tbody>
		</table>
	</div>
{/if}
