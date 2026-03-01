import { writable, derived, readable, get } from 'svelte/store';
import { browser } from '$app/environment';
import type { User, LoginResponse } from '$lib/types/auth';

/**
 * A clock store that ticks every 30 seconds in the browser.
 * Used to drive reactive expiry checks in derived stores.
 */
const clock = readable(new Date(), (set) => {
	if (!browser) return;
	const interval = setInterval(() => set(new Date()), 30_000);
	return () => clearInterval(interval);
});

/**
 * Authentication store using Svelte's writable/derived stores
 * Manages auth state with tokens stored in localStorage
 */

// Create writable stores
const userStore = writable<User | null>(null);
const tokenStore = writable<string | null>(null);
const tokenExpiresAtStore = writable<Date | null>(null);
const refreshTokenStore = writable<string | null>(null);
const refreshTokenExpiresAtStore = writable<Date | null>(null);

/**
 * Initialize auth store from localStorage
 */
function initializeFromStorage(): void {
	if (!browser) return;

	const storedUser = localStorage.getItem('auth_user');
	const storedToken = localStorage.getItem('auth_token');
	const storedTokenExpiresAt = localStorage.getItem('auth_token_expires_at');
	const storedRefreshToken = localStorage.getItem('auth_refresh_token');
	const storedRefreshTokenExpiresAt = localStorage.getItem('auth_refresh_token_expires_at');

	if (storedUser) {
		try {
			userStore.set(JSON.parse(storedUser));
		} catch {
			userStore.set(null);
		}
	}

	tokenStore.set(storedToken);
	tokenExpiresAtStore.set(storedTokenExpiresAt ? new Date(storedTokenExpiresAt) : null);
	refreshTokenStore.set(storedRefreshToken);
	refreshTokenExpiresAtStore.set(storedRefreshTokenExpiresAt ? new Date(storedRefreshTokenExpiresAt) : null);
}

// Initialize on module load (browser only)
if (browser) {
	initializeFromStorage();
}

/**
 * Derived store for authentication status.
 * Depends on `clock` so it re-evaluates every 30 seconds, catching mid-session expiry.
 */
export const isAuthenticated = derived(
	[tokenStore, userStore, tokenExpiresAtStore, clock],
	([$token, $user, $tokenExpiresAt, $now]) => {
		return !!$token && !!$user && !!$tokenExpiresAt && $now < $tokenExpiresAt;
	}
);

/**
 * Derived store to check if token needs refresh (within 5 minutes of expiry).
 * Depends on `clock` so it re-evaluates every 30 seconds.
 */
export const needsTokenRefresh = derived(
	[tokenExpiresAtStore, clock],
	([$tokenExpiresAt, $now]) => {
		if (!$tokenExpiresAt) return false;
		const fiveMinutes = 5 * 60 * 1000;
		return new Date($tokenExpiresAt.getTime() - fiveMinutes) < $now;
	}
);

/**
 * Set authentication data from login response
 */
export function setAuth(response: LoginResponse): void {
	userStore.set({
		userId: response.userId,
		username: response.username,
		email: response.email,
		role: response.role
	});
	tokenStore.set(response.token);
	tokenExpiresAtStore.set(new Date(response.tokenExpiresAt));
	refreshTokenStore.set(response.refreshToken);
	refreshTokenExpiresAtStore.set(new Date(response.refreshTokenExpiresAt));

	saveToStorage();
}

/**
 * Update tokens after refresh
 */
export function updateTokens(
	token: string,
	tokenExpiresAt: Date,
	refreshToken: string,
	refreshTokenExpiresAt: Date
): void {
	tokenStore.set(token);
	tokenExpiresAtStore.set(tokenExpiresAt);
	refreshTokenStore.set(refreshToken);
	refreshTokenExpiresAtStore.set(refreshTokenExpiresAt);

	saveToStorage();
}

/**
 * Save auth data to localStorage.
 * Null values are explicitly removed so stale data never lingers.
 */
function saveToStorage(): void {
	if (!browser) return;

	const user = get(userStore);
	const token = get(tokenStore);
	const tokenExpiresAt = get(tokenExpiresAtStore);
	const refreshToken = get(refreshTokenStore);
	const refreshTokenExpiresAt = get(refreshTokenExpiresAtStore);

	if (user) {
		localStorage.setItem('auth_user', JSON.stringify(user));
	} else {
		localStorage.removeItem('auth_user');
	}

	if (token) {
		localStorage.setItem('auth_token', token);
	} else {
		localStorage.removeItem('auth_token');
	}

	if (tokenExpiresAt) {
		localStorage.setItem('auth_token_expires_at', tokenExpiresAt.toISOString());
	} else {
		localStorage.removeItem('auth_token_expires_at');
	}

	if (refreshToken) {
		localStorage.setItem('auth_refresh_token', refreshToken);
	} else {
		localStorage.removeItem('auth_refresh_token');
	}

	if (refreshTokenExpiresAt) {
		localStorage.setItem('auth_refresh_token_expires_at', refreshTokenExpiresAt.toISOString());
	} else {
		localStorage.removeItem('auth_refresh_token_expires_at');
	}
}

/**
 * Clear authentication data (logout)
 */
export function clearAuth(): void {
	userStore.set(null);
	tokenStore.set(null);
	tokenExpiresAtStore.set(null);
	refreshTokenStore.set(null);
	refreshTokenExpiresAtStore.set(null);

	if (browser) {
		localStorage.removeItem('auth_user');
		localStorage.removeItem('auth_token');
		localStorage.removeItem('auth_token_expires_at');
		localStorage.removeItem('auth_refresh_token');
		localStorage.removeItem('auth_refresh_token_expires_at');
	}
}

/**
 * Get the current access token
 */
export function getAccessToken(): string | null {
	return get(tokenStore);
}

/**
 * Get the current refresh token
 */
export function getRefreshToken(): string | null {
	return get(refreshTokenStore);
}

// Export stores for direct subscription (read-only — use setAuth/clearAuth to mutate)
export const user = {
	subscribe: userStore.subscribe
};

export const token = {
	subscribe: tokenStore.subscribe
};

export const refreshToken = {
	subscribe: refreshTokenStore.subscribe
};
