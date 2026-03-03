<script lang="ts">
	import { goto } from '$app/navigation';
	import type { CompanyListDto } from '$lib/types/company';

	let { data } = $props<{ data: { companies: CompanyListDto[] } }>();
	let selectedCompanyId = $state('');

	function handleContinue() {
		const selectedCompany = data.companies.find((company: CompanyListDto) => company.id === selectedCompanyId);
		if (!selectedCompany) return;

		goto(`/admin/${selectedCompany.id}`);
	}
</script>

<h1>Admin company selection</h1>

{#if data.companies.length === 0}
	<p>No companies found.</p>
{:else}
	<label for="company">Choose company</label>
	<select id="company" bind:value={selectedCompanyId}>
		<option value="" disabled>Select a company</option>
		{#each data.companies as company}
			<option value={company.id}>{company.name}</option>
		{/each}
	</select>

	<button onclick={handleContinue} disabled={!selectedCompanyId}>Continue</button>
{/if}
