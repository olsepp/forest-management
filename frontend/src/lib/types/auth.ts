/**
 * Authentication types for the frontend application
 * These types correspond to the DTOs from the backend API
 */

// Request types
export interface LoginRequest {
	username: string;
	password: string;
}

export interface RegisterRequest {
	username: string;
	email: string;
	password: string;
	confirmPassword: string;
}

// Response types

/**
 * Shared shape returned by both login and register endpoints.
 */
export interface AuthResponse {
	userId: string;
	username: string;
	email: string;
	role: string;
	token: string;
	tokenExpiresAt: string;
	refreshToken: string;
	refreshTokenExpiresAt: string;
}

export type LoginResponse = AuthResponse;
export type RegisterResponse = AuthResponse;

export interface RefreshTokenResponse {
	token: string;
	tokenExpiresAt: string;
	refreshToken: string;
	refreshTokenExpiresAt: string;
}

// User type for application use
export interface User {
	userId: string;
	username: string;
	email: string;
	role: string;
}

// Auth state type
export interface AuthState {
	user: User | null;
	token: string | null;
	tokenExpiresAt: Date | null;
	refreshToken: string | null;
	refreshTokenExpiresAt: Date | null;
	isAuthenticated: boolean;
}

// Error response type
export interface AuthError {
	message: string;
	code?: string;
}
