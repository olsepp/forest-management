<script lang="ts">
	import { onMount } from 'svelte';

	interface Props {
		value?: string;
		label?: string;
		placeholder?: string;
	}

	let { value = $bindable(''), label = '', placeholder = 'Vali kuupäev' }: Props = $props();

	let isOpen = $state(false);
	let currentDate = $state(new Date());
	let selectedDate = $state<Date | null>(null);

	const year = $derived(currentDate.getFullYear());
	const month = $derived(currentDate.getMonth());

	const monthNames = [
		'Jaanuar',
		'Veebruar',
		'Märts',
		'Aprill',
		'Mai',
		'Juuni',
		'Juuli',
		'August',
		'September',
		'Oktoober',
		'November',
		'Detsember'
	];

	const dayNames = ['E', 'T', 'K', 'N', 'R', 'L', 'P'];

	const calendarDays = $derived(getCalendarDays(year, month));

	function getCalendarDays(year: number, month: number) {
		const firstDay = new Date(year, month, 1);
		const lastDay = new Date(year, month + 1, 0);
		const daysInMonth = lastDay.getDate();

		let startDayOfWeek = firstDay.getDay();
		startDayOfWeek = startDayOfWeek === 0 ? 6 : startDayOfWeek - 1;

		const days: (number | null)[] = [];

		for (let i = 0; i < startDayOfWeek; i++) {
			days.push(null);
		}

		for (let day = 1; day <= daysInMonth; day++) {
			days.push(day);
		}

		while (days.length % 7 !== 0) {
			days.push(null);
		}

		return days;
	}

	function prevMonth() {
		currentDate = new Date(year, month - 1, 1);
	}

	function nextMonth() {
		currentDate = new Date(year, month + 1, 1);
	}

	function selectDay(day: number) {
		const newDate = new Date(year, month, day);
		selectedDate = newDate;
		value = formatDate(newDate);
		isOpen = false;
	}

	function formatDate(date: Date): string {
		const y = date.getFullYear();
		const m = String(date.getMonth() + 1).padStart(2, '0');
		const d = String(date.getDate()).padStart(2, '0');
		return `${y}-${m}-${d}`;
	}

	function isToday(day: number): boolean {
		const today = new Date();
		return (
			day === today.getDate() &&
			month === today.getMonth() &&
			year === today.getFullYear()
		);
	}

	function isSelected(day: number): boolean {
		if (!selectedDate) return false;
		return (
			day === selectedDate.getDate() &&
			month === selectedDate.getMonth() &&
			year === selectedDate.getFullYear()
		);
	}

	function handleInputClick(event: MouseEvent) {
		event.stopPropagation();
		isOpen = !isOpen;
		if (isOpen && value) {
			selectedDate = new Date(value);
			currentDate = new Date(value);
		} else if (isOpen) {
			currentDate = new Date();
		}
	}

	function handleClickOutside(event: MouseEvent) {
		if (isOpen) {
			isOpen = false;
		}
	}

	onMount(() => {
		document.addEventListener('click', handleClickOutside);
	});
</script>

<div class="date-picker-container" onclick={(e) => e.stopPropagation()}>
	{#if label}
		<label class="picker-label">
			{label}
		</label>
	{/if}

	<button
		type="button"
		class="date-input-button"
		onclick={handleInputClick}
		aria-haspopup="dialog"
		aria-expanded={isOpen}
	>
		<span class="input-value">{value || placeholder}</span>
		<svg class="calendar-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
			<rect x="3" y="4" width="18" height="18" rx="2" ry="2" />
			<line x1="16" y1="2" x2="16" y2="6" />
			<line x1="8" y1="2" x2="8" y2="6" />
			<line x1="3" y1="10" x2="21" y2="10" />
		</svg>
	</button>

	{#if isOpen}
		<div class="calendar-popup" role="dialog" aria-modal="true" aria-label="Kuupäeva valik">
			<div class="calendar-header">
				<button type="button" class="nav-btn" onclick={prevMonth} aria-label="Eelmine kuu">
					<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
						<path d="M15 18l-6-6 6-6" />
					</svg>
				</button>
				<span class="month-year">{monthNames[month]} {year}</span>
				<button type="button" class="nav-btn" onclick={nextMonth} aria-label="Järgmine kuu">
					<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
						<path d="M9 18l6-6-6-6" />
					</svg>
				</button>
			</div>

			<div class="calendar-grid">
				{#each dayNames as dayName}
					<div class="day-name">{dayName}</div>
				{/each}

				{#each calendarDays as day}
					{#if day === null}
						<div class="day-cell empty"></div>
					{:else}
						<button
							type="button"
							class="day-cell"
							class:today={isToday(day)}
							class:selected={isSelected(day)}
							onclick={() => selectDay(day)}
						>
							{day}
						</button>
					{/if}
				{/each}
			</div>
		</div>
	{/if}
</div>

<style>
	.date-picker-container {
		position: relative;
		display: inline-flex;
		flex-direction: column;
		gap: 0.25rem;
	}

	.picker-label {
		font-size: 0.875rem;
		color: #475569;
		font-weight: 500;
	}

	.date-input-button {
		display: flex;
		align-items: center;
		justify-content: space-between;
		gap: 0.5rem;
		padding: 0.5rem 0.75rem;
		background: #fcfdfc;
		border: 1px solid #cad6cf;
		border-radius: 0.5rem;
		font-size: 0.875rem;
		color: #1f2a24;
		cursor: pointer;
		transition: all 0.2s ease;
		min-height: 48px;
		text-align: left;
		width: 100%;
	}

	.date-input-button:hover {
		border-color: #1f5a42;
		background: #f4f7f5;
	}

	.date-input-button:focus-visible {
		outline: none;
		border-color: #1f5a42;
		box-shadow: 0 0 0 3px rgba(31, 90, 66, 0.12);
	}

	.input-value {
		flex: 1;
	}

	.input-value:empty::before {
		content: attr(data-placeholder);
		color: #94a3b8;
	}

	.calendar-icon {
		width: 1.25rem;
		height: 1.25rem;
		color: #637a55;
		flex-shrink: 0;
	}

	.calendar-popup {
		position: absolute;
		top: 100%;
		left: 0;
		margin-top: 0.5rem;
		background: #ffffff;
		border: 1px solid #d8e1dc;
		border-radius: 0.75rem;
		box-shadow: 0 8px 24px rgba(20, 41, 31, 0.15);
		padding: 1rem;
		z-index: 1000;
		min-width: 320px;
		animation: slideDown 0.2s ease-out;
	}

	@keyframes slideDown {
		from {
			opacity: 0;
			transform: translateY(-8px);
		}
		to {
			opacity: 1;
			transform: translateY(0);
		}
	}

	.calendar-header {
		display: flex;
		align-items: center;
		justify-content: space-between;
		margin-bottom: 0.75rem;
	}

	.nav-btn {
		display: flex;
		align-items: center;
		justify-content: center;
		width: 2.5rem;
		height: 2.5rem;
		background: transparent;
		border: 1px solid #d8e1dc;
		border-radius: 0.5rem;
		color: #1f5a42;
		cursor: pointer;
		transition: all 0.2s ease;
	}

	.nav-btn:hover {
		background: #edf3ef;
		border-color: #1f5a42;
	}

	.nav-btn svg {
		width: 1.25rem;
		height: 1.25rem;
	}

	.month-year {
		font-size: 1rem;
		font-weight: 600;
		color: #1f2a24;
	}

	.calendar-grid {
		display: grid;
		grid-template-columns: repeat(7, 1fr);
		gap: 0.25rem;
	}

	.day-name {
		display: flex;
		align-items: center;
		justify-content: center;
		height: 2rem;
		font-size: 0.75rem;
		font-weight: 600;
		color: #637a55;
		text-transform: uppercase;
	}

	.day-cell {
		display: flex;
		align-items: center;
		justify-content: center;
		height: 2.75rem;
		background: transparent;
		border: 1px solid transparent;
		border-radius: 0.5rem;
		font-size: 0.875rem;
		color: #1f2a24;
		cursor: pointer;
		transition: all 0.15s ease;
	}

	.day-cell:hover:not(.selected) {
		background: #edf3ef;
		border-color: #d8e1dc;
	}

	.day-cell.today {
		background: #ddefe4;
		border-color: #1f5a42;
		color: #1f5a42;
		font-weight: 600;
	}

	.day-cell.selected {
		background: #1f5a42;
		border-color: #1f5a42;
		color: #ffffff;
		font-weight: 600;
	}

	.day-cell.selected:hover {
		background: #174834;
		border-color: #174834;
	}

	.day-cell.empty {
		cursor: default;
	}
</style>
