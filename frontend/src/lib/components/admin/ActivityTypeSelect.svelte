<script lang="ts">
	import { onMount } from 'svelte';
	import { user } from '$lib/stores/auth.store';
	import { activityTypeService } from '$lib/services/activity-type';
	import type {
		ActivityTypeListDto,
		ActivityTypeDto
	} from '$lib/dtos/activity-type/activity-type.dto';
	import Dropdown from '$lib/components/shared/Dropdown.svelte';

	type Props = {
		value?: string;
		disabled?: boolean;
		allowCreate?: boolean;
		placeholder?: string;
		oncreated?: (type: ActivityTypeDto) => void;
	};

	let {
		value = $bindable(''),
		disabled = false,
		allowCreate = true,
		placeholder = 'Vali tegevuse tüüp',
		oncreated
	}: Props = $props();

	let activityTypes = $state<ActivityTypeListDto[]>([]);
	let isLoading = $state(true);
	let isCreating = $state(false);
	let createName = $state('');
	let errorMessage = $state('');

	let isAdmin = $derived(($user?.role ?? '').toLowerCase() === 'admin');
	let showCreate = $derived(allowCreate && isAdmin && !disabled);
	let options = $derived(
		activityTypes.map((t) => ({ value: t.id, label: t.activityTypeName }))
	);
	let effectivePlaceholder = $derived(isLoading ? 'Laadimine...' : placeholder);
	let effectiveDisabled = $derived(disabled || isLoading);

	async function loadTypes() {
		try {
			errorMessage = '';
			isLoading = true;
			const types = await activityTypeService.getAll();
			activityTypes = Array.isArray(types) ? types : [];
			if (!value && activityTypes.length > 0) {
				value = activityTypes[0].id;
			}
		} catch {
			errorMessage = 'Tegevuse tüüpe ei õnnestunud laadida.';
			activityTypes = [];
		} finally {
			isLoading = false;
		}
	}

	async function handleCreate(event: SubmitEvent) {
		event.preventDefault();
		const name = createName.trim();
		if (!name) {
			errorMessage = 'Nimi on kohustuslik.';
			return;
		}
		if (name.length > 50) {
			errorMessage = 'Nimi võib olla kuni 50 tähemärki.';
			return;
		}

		try {
			isCreating = true;
			errorMessage = '';
			const created = await activityTypeService.create({ activityTypeName: name });
			activityTypes = [
				...activityTypes,
				{ id: created.id, activityTypeName: created.activityTypeName }
			];
			value = created.id;
			createName = '';
			oncreated?.(created);
		} catch (err) {
			const message = err instanceof Error ? err.message : '';
			errorMessage = message.includes('401')
				? 'Ligipääs puudub. Logige uuesti sisse.'
				: 'Tegevuse tüübi loomine ebaõnnestus.';
		} finally {
			isCreating = false;
		}
	}

	onMount(loadTypes);
</script>

<Dropdown
	{options}
	bind:value
	disabled={effectiveDisabled}
	placeholder={effectivePlaceholder}
	required
>
	{#snippet footer()}
		{#if showCreate}
			<form class="create-form" onsubmit={handleCreate}>
				<input
					type="text"
					bind:value={createName}
					placeholder="Lisa uus tüüp..."
					maxlength="50"
					disabled={isCreating}
					class="create-input"
				/>
				<button
					type="submit"
					class="create-btn"
					disabled={isCreating || !createName.trim()}
					aria-label="Lisa tegevuse tüüp"
				>
					{isCreating ? '...' : '+'}
				</button>
			</form>
		{/if}
		{#if errorMessage}
			<p class="create-error">{errorMessage}</p>
		{/if}
	{/snippet}
</Dropdown>

<style>
	.create-form {
		display: flex;
		gap: 0.4rem;
		align-items: center;
	}

	.create-input {
		flex: 1;
		min-width: 0;
		padding: 0.4rem 0.55rem;
		border: 1px solid #cad6cf;
		border-radius: 0.45rem;
		font-size: 0.85rem;
		font-family: inherit;
		background: #ffffff;
		color: #1f2a24;
	}

	.create-input:focus {
		outline: none;
		border-color: #1f5a42;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.12);
	}

	.create-input:disabled {
		opacity: 0.6;
	}

	.create-btn {
		flex-shrink: 0;
		display: inline-flex;
		align-items: center;
		justify-content: center;
		min-width: 1.9rem;
		height: 1.9rem;
		padding: 0 0.5rem;
		border: 1px solid #1f5a42;
		border-radius: 0.45rem;
		background: #1f5a42;
		color: #f8fdfb;
		font-size: 1.05rem;
		font-weight: 700;
		cursor: pointer;
		transition: background 0.15s ease;
	}

	.create-btn:hover:not(:disabled) {
		background: #174834;
	}

	.create-btn:disabled {
		opacity: 0.5;
		cursor: not-allowed;
	}

	.create-error {
		margin: 0.4rem 0 0;
		color: #b91c1c;
		font-size: 0.8rem;
	}
</style>
