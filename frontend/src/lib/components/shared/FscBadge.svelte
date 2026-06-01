<script lang="ts">
	let { isFsc = false }: { isFsc?: boolean } = $props();
</script>

{#if isFsc}
	<span class="fsc-badge">FSC 100%</span>
{/if}

<style>
	.fsc-badge {
		display: inline-flex;
		align-items: center;
		padding: 0.15rem 0.45rem;
		border-radius: 9999px;
		font-size: 0.7rem;
		font-weight: 700;
		letter-spacing: 0.04em;
		background: #166534;
		color: #ffffff;
		text-transform: uppercase;
	}
</style>
