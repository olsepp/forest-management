/**
 * Enable SSR for employee pages so the browser receives pre-rendered HTML
 * immediately on hard refresh.  The page load functions return placeholder
 * data on the server (where no auth token exists), then the client-side
 * hydration populates real data once the JS bundle loads.
 */
export const ssr = true;
