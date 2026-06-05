<script lang="ts">
	import { fly } from 'svelte/transition';
	import { onDestroy } from 'svelte';

	type ToastVariant = 'success' | 'error';

	type Props = {
		message: string;
		variant?: ToastVariant;
		visible: boolean;
		closeable?: boolean;
		autoDismissMs?: number | null;
		onclose?: () => void;
	};

	let {
		message,
		variant = 'success',
		visible,
		closeable = true,
		autoDismissMs = null,
		onclose
	}: Props = $props();

	let dismissTimer: ReturnType<typeof setTimeout> | null = null;

	function startAutoDismiss() {
		clearAutoDismiss();
		if (autoDismissMs != null && autoDismissMs > 0) {
			dismissTimer = setTimeout(() => {
				onclose?.();
			}, autoDismissMs);
		}
	}

	function clearAutoDismiss() {
		if (dismissTimer != null) {
			clearTimeout(dismissTimer);
			dismissTimer = null;
		}
	}

	function handleClose() {
		clearAutoDismiss();
		onclose?.();
	}

	$effect(() => {
		if (visible && message) {
			startAutoDismiss();
		} else {
			clearAutoDismiss();
		}
	});

	onDestroy(() => {
		clearAutoDismiss();
	});
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
			{#if closeable}
				<button
					type="button"
					class="close-btn"
					aria-label="Sulge"
					onclick={handleClose}
				>
					<svg
						viewBox="0 0 24 24"
						fill="none"
						stroke="currentColor"
						stroke-width="2.5"
						stroke-linecap="round"
						stroke-linejoin="round"
						width="16"
						height="16"
						aria-hidden="true"
					>
						<path d="M18 6L6 18M6 6l12 12" />
					</svg>
				</button>
			{/if}
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
		display: flex;
		align-items: flex-start;
		gap: 0.5rem;
	}

	.toast p {
		margin: 0;
		font-size: 0.94rem;
		font-weight: 700;
		line-height: 1.35;
		flex: 1;
	}

	.close-btn {
		flex-shrink: 0;
		border: none;
		background: transparent;
		padding: 0.2rem;
		cursor: pointer;
		color: inherit;
		opacity: 0.65;
		border-radius: 0.35rem;
		line-height: 1;
	}

	.close-btn:hover {
		opacity: 1;
		background: rgba(0, 0, 0, 0.06);
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
