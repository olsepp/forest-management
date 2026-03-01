import type { Handle } from '@sveltejs/kit';

/**
 * Server-side hook.
 * JWT auth is handled entirely client-side via the auth store and localStorage.
 * No server-side session or cookie is used.
 */
export const handle: Handle = async ({ event, resolve }) => {
	return resolve(event);
};
