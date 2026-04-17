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
			{ label: 'Ava töölaud', path: `/admin/${data.company.id}/dashboard` },
			{ label: 'Ava kinnistud', path: `/admin/${data.company.id}/landproperty` },
			{ label: 'Ava tegevused', path: `/admin/${data.company.id}/activity` }
		];
	});
</script>

<h1>Ettevõtte tööruum</h1>

{#if data.company}
	<section class="card">
		<p class="meta">Valitud ettevõte</p>
		<h2>{data.company.name}</h2>
		<p><strong>Ettevõtte ID:</strong> {data.company.id}</p>

		<div class="actions">
			{#each companyActions as action (action.path)}
				<button type="button" onclick={() => openSection(action.path)}>{action.label}</button>
			{/each}
		</div>
	</section>
{/if}

<style>
	.card {
		padding: 1.1rem;
		border: 1px solid #e5e7eb;
		border-radius: 0.75rem;
		background: #fff;
	}

	.meta {
		margin: 0;
		font-size: 0.78rem;
		text-transform: uppercase;
		letter-spacing: 0.08em;
		font-weight: 700;
	}

	h2 {
		margin: 0.35rem 0 0.85rem;
		font-size: 1.2rem;
	}

	.actions {
		margin-top: 1rem;
		display: flex;
		flex-wrap: wrap;
		gap: 0.6rem;
	}

	button {
		border: 1px solid #d1d5db;
		border-radius: 0.5rem;
		background: #fff;
		padding: 0.5rem 0.9rem;
		font: inherit;
		cursor: pointer;
	}

	.error {
		color: #b91c1c;
	}
</style>
