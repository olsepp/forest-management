import type { LayoutServerLoad } from './$types';

/**
 * Layout server load function.
 * Auth is handled client-side; this only passes through the redirectTo param
 * so the sign-in page can redirect back after login.
 *
 * redirectTo is sanitized here to prevent open-redirect attacks:
 * it must be a relative path starting with "/" and must not start with "//"
 * (which browsers treat as protocol-relative, i.e. an external URL).
 */
export const load: LayoutServerLoad = async ({ url }) => {
	let redirectTo = url.searchParams.get('redirectTo') ?? '/';

	if (!redirectTo.startsWith('/') || redirectTo.startsWith('//')) {
		redirectTo = '/';
	}

	return {
		redirectTo
	};
};
