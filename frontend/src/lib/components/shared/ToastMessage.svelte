<script lang="ts">
	import { fly } from 'svelte/transition';

	type ToastVariant = 'success' | 'error';

	type Props = {
		message: string;
		variant?: ToastVariant;
		visible: boolean;
	};

	let { message, variant = 'success', visible }: Props = $props();
</script>

{#if visible && message}
	<div class="toast-host" aria-live="polite" aria-atomic="true">
		<div
			class="toast"
			class:is-error={variant === 'error'}
			in:fly={{ x: 220, duration: 220 }}
			out:fly={{ x: 220, duration: 180 }}
			role="status"
		>
			<p>{message}</p>
		</div>
	</div>
{/if}

<style>
	.toast-host {
		position: fixed;
		top: 0.8rem;
		right: 0.8rem;
		z-index: 1100;
		pointer-events: none;
	}

	.toast {
		max-width: min(26rem, calc(100vw - 1.6rem));
		border-radius: 0.8rem;
		border: 1px solid #9ad8b4;
		background: #eefaf2;
		color: #114229;
		padding: 0.72rem 0.9rem;
		box-shadow: 0 8px 20px rgba(15, 40, 27, 0.2);
	}

	.toast p {
		margin: 0;
		font-size: 0.94rem;
		font-weight: 700;
		line-height: 1.35;
	}

	.toast.is-error {
		border-color: #f6b7bb;
		background: #fff3f3;
		color: #8f1f2b;
	}

	@media (max-width: 640px) {
		.toast-host {
			top: 0.65rem;
			left: 0.65rem;
			right: 0.65rem;
		}

		.toast {
			max-width: 100%;
		}
	}
</style>
