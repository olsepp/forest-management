<script lang="ts">
	import { user } from '$lib/stores/auth.store';
	import type { CompanyListDto } from '$lib/dtos/company/company.dto';
	import { goto } from '$app/navigation';
	import { resolve } from '$app/paths';

	let { data }: { data: { companies: CompanyListDto[] } } = $props();

	let currentUserId = $derived($user?.userId?.trim() ?? '');

	function openCompany(companyId: string) {
		goto(resolve('/employee/[CompanyId]', { CompanyId: companyId }));
	}
</script>

<section class="employee-card hero">
	<div class="hero-content">
		<div class="hero-text">
			<p class="kicker">Ettevõtte valik</p>
			<h1 class="employee-page-title">Vali ettevõte</h1>
		</div>
		{#if currentUserId}
			<a
				class="profile-shortcut"
				href={resolve('/employee/user/[userId]', { userId: currentUserId })}
			>
				Minu andmed
			</a>
		{/if}
	</div>
</section>

{#if data.companies.length === 0}
	<div class="employee-state-block is-empty">Ettevõtteid ei leitud.</div>
{:else}
	<div class="company-grid" role="list" aria-label="Saadaval olevad ettevõtted">
		{#each data.companies as company (company.id)}
			<button class="company-card" type="button" onclick={() => openCompany(company.id)}>
				<span class="company-name">{company.name}</span>
			</button>
		{/each}
	</div>
{/if}

<style>
	.hero {
		margin-bottom: 0.9rem;
		padding: 1rem;
		background: linear-gradient(180deg, #ffffff 0%, #f5f8fc 100%);
		border-color: #d3dde8;
	}

	.hero-content {
		display: flex;
		flex-wrap: wrap;
		align-items: center;
		justify-content: space-between;
		gap: 0.75rem;
	}

	.hero-text {
		display: flex;
		flex-direction: column;
		gap: 0.25rem;
	}

	.kicker {
		margin: 0;
		font-size: 0.72rem;
		font-weight: 700;
		text-transform: uppercase;
		letter-spacing: 0.03em;
		color: #3f5a4b;
	}

	.profile-shortcut {
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-height: 3rem;
		padding: 0.65rem 0.95rem;
		border-radius: 0.85rem;
		background: #316347;
		color: white;
		font-size: 0.95rem;
		font-weight: 700;
		text-decoration: none;
		box-shadow: 0 2px 8px rgba(15, 37, 28, 0.08);
	}

	.profile-shortcut:active {
		transform: translateY(1px);
	}

	.profile-shortcut:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.25);
	}

	p {
		margin: 0;
		color: #334155;
	}

	.company-grid {
		display: grid;
		grid-template-columns: 1fr;
		gap: 0.75rem;
	}

	.company-card {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 100%;
		min-height: 4.5rem;
		padding: 0.92rem;
		border: 1px solid #cfd8e3;
		border-radius: 1rem;
		background: #fff;
		text-align: center;
		cursor: pointer;
		transition:
			border-color 0.18s ease,
			box-shadow 0.18s ease,
			transform 0.18s ease;
	}

	.company-card:hover {
		border-color: #aebed0;
		box-shadow: 0 6px 16px rgba(15, 23, 42, 0.12);
		transform: translateY(-1px);
	}

	.company-card:active {
		transform: scale(0.97);
	}

	.company-card:focus-visible {
		outline: none;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.25);
	}

	.company-name {
		font-size: 1.25rem;
		font-weight: 700;
		color: #0f172a;
	}

	@media (min-width: 640px) {
		h1 {
			font-size: 1.25rem;
		}

		.company-grid {
			grid-template-columns: repeat(2, minmax(0, 1fr));
		}
	}
</style>
