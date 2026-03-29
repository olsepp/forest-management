import { PUBLIC_API_URL } from '$env/static/public';
import { getAccessToken, getRefreshToken, setAuth, updateTokens, clearAuth, isAuthenticated, needsTokenRefresh } from '$lib/stores/auth.store';
import type { LoginRequest, LoginResponse, RegisterRequest, RegisterResponse, RefreshTokenResponse } from '$lib/types/auth';

const API_BASE_URL = PUBLIC_API_URL || 'http://localhost:5000';

/**
 * Resolve the default post-auth route for a user role.
 */
export type AppRoleRoute = '/' | '/admin' | '/employee';

export function getDefaultRouteForRole(role: string | null | undefined): AppRoleRoute {
	const normalizedRole = role?.trim().toLowerCase();

	if (normalizedRole === 'admin') return '/admin';
	if (normalizedRole === 'employee') return '/employee';

	return '/';
}

/**
 * Custom error class for auth errors
 */
export class AuthServiceError extends Error {
	constructor(
		message: string,
		public code?: string,
		public status?: number
	) {
		super(message);
		this.name = 'AuthServiceError';
	}
}

/**
 * Authentication service for handling API calls
 */
class AuthService {
	/**
	 * In-flight refresh promise used to deduplicate concurrent refresh calls.
	 * If multiple callers invoke refreshToken() simultaneously, they all await
	 * the same promise instead of firing separate requests (which would cause
	 * the backend to invalidate the refresh token mid-flight on rotation).
	 */
	private refreshPromise: Promise<RefreshTokenResponse> | null = null;

	/**
	 * Login with username and password
	 */
	async login(credentials: LoginRequest): Promise<LoginResponse> {
		try {
			const response = await fetch(`${API_BASE_URL}/api/account/login`, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify(credentials)
			});

			if (!response.ok) {
				const error = await response.json().catch(() => ({ message: 'Login failed' }));
				throw new AuthServiceError(
					error.message || 'Login failed',
					error.code,
					response.status
				);
			}

			const data: LoginResponse = await response.json();

			// Store auth data in the store
			setAuth(data);

			return data;
		} catch (error) {
			if (error instanceof AuthServiceError) {
				throw error;
			}
			throw new AuthServiceError(
				'Network error. Please check your connection.',
				'NETWORK_ERROR'
			);
		}
	}

	/**
	 * Register a new user
	 */
	async register(data: RegisterRequest): Promise<RegisterResponse> {
		try {
			const response = await fetch(`${API_BASE_URL}/api/account/register`, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify(data)
			});

			if (!response.ok) {
				const error = await response.json().catch(() => ({ message: 'Registration failed' }));
				throw new AuthServiceError(
					error.message || 'Registration failed',
					error.code,
					response.status
				);
			}

			const result: RegisterResponse = await response.json();

			// Store auth data in the store
			setAuth(result);

			return result;
		} catch (error) {
			if (error instanceof AuthServiceError) {
				throw error;
			}
			throw new AuthServiceError(
				'Network error. Please check your connection.',
				'NETWORK_ERROR'
			);
		}
	}

	/**
	 * Refresh the access token.
	 * Concurrent callers share a single in-flight request so the backend
	 * refresh token is never consumed more than once per expiry window.
	 */
	async refreshToken(): Promise<RefreshTokenResponse> {
		// Return the existing in-flight promise if one is already running
		if (this.refreshPromise) {
			return this.refreshPromise;
		}

		this.refreshPromise = this._doRefresh();

		try {
			return await this.refreshPromise;
		} finally {
			this.refreshPromise = null;
		}
	}

	/**
	 * Internal implementation of the token refresh request.
	 */
	private async _doRefresh(): Promise<RefreshTokenResponse> {
		const token = getRefreshToken();

		if (!token) {
			throw new AuthServiceError(
				'No refresh token available',
				'NO_REFRESH_TOKEN',
				401
			);
		}

		try {
			const response = await fetch(`${API_BASE_URL}/api/account/refresh`, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/json'
				},
				body: JSON.stringify({ refreshToken: token })
			});

			if (!response.ok) {
				// Clear auth if refresh fails
				clearAuth();
				throw new AuthServiceError(
					'Token refresh failed. Please login again.',
					'REFRESH_FAILED',
					response.status
				);
			}

			const data: RefreshTokenResponse = await response.json();

			// Update stored tokens
			updateTokens(
				data.token,
				new Date(data.tokenExpiresAt),
				data.refreshToken,
				new Date(data.refreshTokenExpiresAt)
			);

			return data;
		} catch (error) {
			if (error instanceof AuthServiceError) {
				throw error;
			}
			clearAuth();
			throw new AuthServiceError(
				'Network error during token refresh.',
				'NETWORK_ERROR'
			);
		}
	}

	/**
	 * Logout the current user
	 */
	async logout(): Promise<void> {
		const token = getAccessToken();

		try {
			// Try to call logout endpoint if user is authenticated
			if (token) {
				await fetch(`${API_BASE_URL}/api/account/logout`, {
					method: 'POST',
					headers: {
						Authorization: `Bearer ${token}`,
						'Content-Type': 'application/json'
					}
				});
			}
		} catch {
			// Ignore network errors during logout
			// We still want to clear local auth state
		} finally {
			// Always clear local auth state
			clearAuth();
		}
	}

	/**
	 * Check if token needs refresh and refresh if needed.
	 * Returns the current valid access token.
	 */
	async ensureValidToken(): Promise<string> {
		// Subscribe to get current value
		let currentIsAuthenticated = false;
		let currentNeedsRefresh = false;

		const unsubAuth = isAuthenticated.subscribe((v) => (currentIsAuthenticated = v));
		const unsubRefresh = needsTokenRefresh.subscribe((v) => (currentNeedsRefresh = v));
		unsubAuth();
		unsubRefresh();

		if (!currentIsAuthenticated) {
			throw new AuthServiceError('Not authenticated', 'NOT_AUTHENTICATED', 401);
		}

		// Check if token needs refresh
		if (currentNeedsRefresh) {
			await this.refreshToken();
		}

		return getAccessToken()!;
	}
}

// Create singleton instance
export const authService = new AuthService();
