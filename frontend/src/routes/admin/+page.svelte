<script lang="ts">
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';
	import type { CompanyListDto } from '$lib/dtos/company/company.dto';

	let { data }: { data: { companies: CompanyListDto[] } } = $props();

	function openCompany(companyId: string) {
		goto(resolve('/admin/[CompanyId]', { CompanyId: companyId }));
	}
</script>

<h1>Admini ettevõtte valik</h1>

{#if data.companies.length === 0}
	<p>Ettevõtteid ei leitud.</p>
{:else}
	<p class="intro-text">Valige ettevõte:</p>
	<div class="company-grid" role="list">
		{#each data.companies as company (company.id)}
			<button class="company-card" type="button" onclick={() => openCompany(company.id)}>
				<span class="company-name">{company.name}</span>
				<!-- <span class="company-action">Ava ettevõte</span> -->
			</button>
		{/each}
	</div>
{/if}

<style>
	.intro-text {
		margin: 0.5rem 0 1rem;
		color: #4b5563;
	}

	.company-grid {
		display: grid;
		grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
		gap: 1.25rem;
	}

	.company-card {
		display: flex;
		flex-direction: column;
		align-items: center;
		justify-content: center;
		min-height: 140px;
		width: 100%;
		padding: 1.5rem;
		background: linear-gradient(135deg, #1f5a42, #2d6f52);
		border: none;
		border-radius: 12px;
		box-shadow: 0 4px 12px rgba(31, 90, 66, 0.25);
		cursor: pointer;
		text-align: center;
		transition:
			transform 0.2s ease,
			box-shadow 0.2s ease,
			background 0.2s ease;
	}

	.company-card:hover,
	.company-card:focus-visible {
		transform: translateY(-3px);
		box-shadow: 0 10px 24px rgba(31, 90, 66, 0.35);
		background: linear-gradient(135deg, #174834, #1f5a42);
		outline: none;
	}

	.company-card:active {
		transform: translateY(-1px);
		box-shadow: 0 6px 12px rgba(31, 90, 66, 0.3);
		background: linear-gradient(135deg, #143d2c, #174834);
	}

	.company-name {
		font-size: 1.2rem;
		font-weight: 700;
		color: #ffffff;
	}
</style>
