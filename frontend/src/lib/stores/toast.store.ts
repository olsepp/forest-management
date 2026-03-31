import { writable } from 'svelte/store';

type ToastVariant = 'success' | 'error';

type ToastState = {
	visible: boolean;
	message: string;
	variant: ToastVariant;
};

const DEFAULT_DURATION_MS = 5000;

const initialState: ToastState = {
	visible: false,
	message: '',
	variant: 'success'
};

const toast = writable<ToastState>(initialState);
let hideTimer: ReturnType<typeof setTimeout> | null = null;

function clearHideTimer() {
	if (!hideTimer) return;
	clearTimeout(hideTimer);
	hideTimer = null;
}

function hideToast() {
	clearHideTimer();
	toast.update((current) => ({ ...current, visible: false }));
}

function showToast(message: string, variant: ToastVariant = 'success', durationMs = DEFAULT_DURATION_MS) {
	clearHideTimer();
	toast.set({
		visible: true,
		message,
		variant
	});

	hideTimer = setTimeout(() => {
		toast.update((current) => ({ ...current, visible: false }));
		hideTimer = null;
	}, Math.max(0, durationMs));
}

export const toastStore = {
	subscribe: toast.subscribe,
	showToast,
	hideToast
};

