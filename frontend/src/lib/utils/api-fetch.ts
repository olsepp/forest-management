import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from '$lib/services/auth';
import { withRetry } from '$lib/utils/retry';

const API_BASE_URL = PUBLIC_API_URL;
type FetchFn = typeof window.fetch;

/**
 * Fetch wrapper with automatic token injection and retry logic.
 * Retries up to 2 times (3 total attempts) with exponential backoff
 * for transient network failures (5xx, timeouts, DNS failures).
 * Does NOT retry 4xx errors (auth, validation, not-found).
 */
export async function apiFetch<T = unknown>(
	path: string,
	fetchFn?: FetchFn,
	init?: RequestInit
): Promise<T> {
	const fetcher = fetchFn ?? fetch;

	return withRetry(async () => {
		const token = await authService.ensureValidToken();
		const response = await fetcher(`${API_BASE_URL}${path}`, {
			...init,
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`,
				...(init?.headers as Record<string, string> | undefined)
			}
		});

		if (!response.ok) {
			throw new Error(`${response.status} ${response.statusText}`);
		}

		return response.json() as Promise<T>;
	});
}

/**
 * SSR-safe variant of `apiFetch`.  During server-side rendering no auth
 * token is available, so this returns `null` instead of throwing.
 * The SvelteKit client-side hydration will re-run the load function and
 * populate real data.
 */
export async function ssrSafeApiFetch<T>(
	path: string,
	fetchFn?: FetchFn,
	init?: RequestInit
): Promise<T | null> {
	try {
		return await apiFetch<T>(path, fetchFn, init);
	} catch {
		return null;
	}
}
