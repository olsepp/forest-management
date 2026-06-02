/**
 * Retry a function with exponential backoff for transient network failures.
 * Designed for poor-connectivity mobile scenarios (3G, spotty coverage).
 *
 * @param fn       The async function to retry
 * @param maxRetries Maximum number of retries (default 2 = 3 total attempts)
 * @param baseDelayMs  Base delay in ms before first retry (default 800)
 * @returns The resolved value of fn
 */
export async function withRetry<T>(
	fn: () => Promise<T>,
	maxRetries = 2,
	baseDelayMs = 800
): Promise<T> {
	let lastError: unknown;

	for (let attempt = 0; attempt <= maxRetries; attempt++) {
		try {
			return await fn();
		} catch (error) {
			lastError = error;

			// Don't retry 4xx errors (client errors like 401, 404, 400).
			if (error instanceof Error && /40[0-9]/.test(error.message)) {
				throw error;
			}

			if (attempt < maxRetries) {
				const delay = baseDelayMs * Math.pow(2, attempt);
				await new Promise((resolve) => setTimeout(resolve, delay));
			}
		}
	}

	throw lastError;
}
