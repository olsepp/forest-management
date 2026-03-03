<script lang="ts">
	import type { CompanyDto } from '$lib/types/company';
	let { data } = $props<{ data: { company: CompanyDto } }>();
</script>

<h1>Admin company page</h1>
<p>Company ID: {data.company.id}</p>
<p>Selected company: {data.company.name}</p>
