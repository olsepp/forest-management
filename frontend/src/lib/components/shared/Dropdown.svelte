<script lang="ts">
	import type { Snippet } from 'svelte';

	type Option = {
		value: string;
		label: string;
	};

	type Props = {
		value?: string;
		options?: Option[];
		disabled?: boolean;
		placeholder?: string;
		required?: boolean;
		footer?: Snippet;
	};

	let {
		value = $bindable(''),
		options = [],
		disabled = false,
		placeholder = 'Vali',
		required = false,
		footer
	}: Props = $props();

	let isOpen = $state(false);
	let container: HTMLDivElement;

	let selectedLabel = $derived(options.find((o) => o.value === value)?.label ?? '');

	function toggle() {
		if (disabled) return;
		isOpen = !isOpen;
	}

	function select(val: string) {
		value = val;
		isOpen = false;
	}

	function handleWindowClick(event: MouseEvent) {
		if (isOpen && container && !container.contains(event.target as Node)) {
			isOpen = false;
		}
	}

	function handleKeydown(event: KeyboardEvent) {
		if (isOpen && event.key === 'Escape') {
			isOpen = false;
		}
	}
</script>

<svelte:window onclick={handleWindowClick} onkeydown={handleKeydown} />

<div class="dropdown" bind:this={container}>
	<button
		type="button"
		class="dropdown-trigger"
		onclick={toggle}
		aria-expanded={isOpen}
		disabled={disabled}
	>
		<span class="dropdown-label">{selectedLabel || placeholder}</span>
		<svg
			class="dropdown-arrow"
			class:open={isOpen}
			viewBox="0 0 24 24"
			fill="none"
			stroke="currentColor"
			stroke-width="2.5"
			stroke-linecap="round"
			stroke-linejoin="round"
			aria-hidden="true"
		>
			<path d="M6 9l6 6 6-6" />
		</svg>
	</button>

	{#if isOpen}
		<div class="dropdown-menu">
			{#each options as opt (opt.value)}
				<button
					type="button"
					class="dropdown-option"
					class:selected={value === opt.value}
					onclick={() => select(opt.value)}
				>
					{opt.label}
				</button>
			{/each}

			{#if options.length === 0 && !footer}
				<div class="dropdown-empty">Pole valitav</div>
			{/if}

			{#if footer}
				<div class="dropdown-footer">
					{@render footer()}
				</div>
			{/if}
		</div>
	{/if}
</div>

<style>
	.dropdown {
		position: relative;
	}

	.dropdown-trigger {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.5rem;
		width: 100%;
		padding: 0.5rem 0.75rem;
		min-width: 160px;
		border: 1px solid #cad6cf;
		border-radius: 0.6rem;
		background: #fcfdfc;
		color: #1f2a24;
		font-size: 0.9rem;
		font-family: inherit;
		cursor: pointer;
		transition:
			border-color 0.15s ease,
			box-shadow 0.15s ease;
	}

	.dropdown-trigger:hover:not(:disabled) {
		border-color: #96b1a4;
	}

	.dropdown-trigger:focus {
		outline: none;
		border-color: #1f5a42;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.12);
	}

	.dropdown-trigger:disabled {
		opacity: 0.6;
		cursor: not-allowed;
	}

	.dropdown-label {
		overflow: hidden;
		text-overflow: ellipsis;
		white-space: nowrap;
	}

	.dropdown-arrow {
		width: 1rem;
		height: 1rem;
		flex-shrink: 0;
		color: #56645d;
		transition: transform 0.2s ease;
	}

	.dropdown-arrow.open {
		transform: rotate(180deg);
	}

	.dropdown-menu {
		position: absolute;
		top: calc(100% + 4px);
		left: 0;
		right: 0;
		z-index: 50;
		background: #fcfdfc !important;
		border: 1px solid #cad6cf;
		border-radius: 0.6rem;
		box-shadow: 0 4px 14px rgba(21, 41, 32, 0.12);
		max-height: 240px;
		overflow-y: auto;
	}

	.dropdown-option {
		display: block;
		width: 100%;
		padding: 0.6rem 0.75rem;
		border: none !important;
		background: transparent !important;
		color: #1f2a24;
		font-size: 0.9rem;
		font-family: inherit;
		text-align: left;
		cursor: pointer;
		transition: background 0.15s ease;
	}

	.dropdown-option:hover {
		background: #174834 !important;
		color: #ffffff !important;
	}

	.dropdown-option.selected {
		background: #1f5a42 !important;
		color: #ffffff !important;
		font-weight: 600;
	}

	.dropdown-option:first-child {
		border-radius: 0.6rem 0.6rem 0 0;
	}

	.dropdown-option:last-child {
		border-radius: 0 0 0.6rem 0.6rem;
	}

	.dropdown-menu:has(.dropdown-option:first-child:last-child) .dropdown-option {
		border-radius: 0.6rem;
	}

	.dropdown-empty {
		padding: 0.6rem 0.75rem;
		color: #56645d;
		font-size: 0.85rem;
	}

	.dropdown-footer {
		border-top: 1px solid #cad6cf;
		padding: 0.6rem 0.75rem;
	}
</style>
