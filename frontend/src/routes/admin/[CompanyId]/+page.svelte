<script lang="ts">
	import { resolve } from '$app/paths';
	import { goto } from '$app/navigation';
	import type { CompanyDto } from '$lib/dtos/company/company.dto';

	let { data }: { data: { company: CompanyDto } } = $props();

	function openSection(path: string) {
		goto(resolve(path as unknown as '/'));
	}

	const companyActions = $derived.by(() => {
		if (!data.company) return [] as { label: string; path: string }[];

		return [
			{ label: 'Tegevused', path: `/admin/${data.company.id}/activity` },
			{ label: 'Töölaud', path: `/admin/${data.company.id}/dashboard` },
			{ label: 'Kinnistud', path: `/admin/${data.company.id}/landproperty` }
		];
	});
</script>

{#if data.company}
	<header class="company-header">
		<p class="label">Ettevõtte tööruum</p>
		<h1>{data.company.name}</h1>
	</header>

	<nav class="nav-buttons">
		{#each companyActions as action (action.path)}
			<button type="button" class="nav-button" onclick={() => openSection(action.path)}>
				<span class="nav-label">{action.label}</span>
				<svg
					class="nav-arrow"
					xmlns="http://www.w3.org/2000/svg"
					fill="none"
					viewBox="0 0 24 24"
					stroke="currentColor"
					stroke-width="2"
				>
					<path stroke-linecap="round" stroke-linejoin="round" d="M9 5l7 7-7 7" />
				</svg>
			</button>
		{/each}
	</nav>
{/if}

<style>
	.company-header {
		margin-bottom: 2rem;
	}

	.label {
		font-size: 0.75rem;
		text-transform: uppercase;
		letter-spacing: 0.1em;
		font-weight: 600;
		color: #56645d;
		margin: 0 0 0.5rem;
	}

	h1 {
		font-size: 1.75rem;
		font-weight: 700;
		color: #1f2a24;
		margin: 0;
		letter-spacing: -0.02em;
	}

	.nav-buttons {
		display: flex;
		flex-direction: column;
		gap: 0.85rem;
	}

	.nav-button {
		display: flex;
		align-items: center;
		justify-content: space-between;
		padding: 1.25rem 1.5rem;
		border: 1px solid #d8e1dc;
		border-radius: 0.75rem;
		background: #f7faf8;
		color: #1f2a24;
		font-size: 1.1rem;
		font-weight: 600;
		cursor: pointer;
		transition: all 0.18s ease;
		width: 100%;
		text-align: left;
	}

	.nav-button:hover {
		background: #eef3ef;
		border-color: #bfcfc6;
	}

	.nav-button:active {
		background: #e4eee8;
	}

	.nav-arrow {
		width: 1.25rem;
		height: 1.25rem;
		color: #56645d;
		flex-shrink: 0;
	}
</style>
