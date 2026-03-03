<script lang="ts">
	let { children } = $props();
</script>

<section class="min-h-[calc(100vh-2rem)] flex-1 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
	{@render children()}
</section>
