import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from './auth';
import type { UserListDto, UserDetailsDto, UserProfileDto } from '$lib/dtos/user/user.dto';

const API_BASE_URL = PUBLIC_API_URL;

type FetchFn = typeof window.fetch;

class UserService {
	async getAll(fetchFn?: FetchFn): Promise<UserListDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/users`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch users: ${response.statusText}`);
		}
		return response.json();
	}

	async getById(id: string, fetchFn?: FetchFn): Promise<UserDetailsDto> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/users/${id}`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch user: ${response.statusText}`);
		}
		return response.json();
	}

	async create(user: unknown): Promise<UserDetailsDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/users`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(user)
		});
		if (!response.ok) {
			throw new Error(`Failed to create user: ${response.statusText}`);
		}
		return response.json();
	}

	async update(id: string, user: unknown): Promise<UserDetailsDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/users/${id}`, {
			method: 'PUT',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(user)
		});
		if (!response.ok) {
			throw new Error(`Failed to update user: ${response.statusText}`);
		}
		return response.json();
	}

	async getProfile(fetchFn?: FetchFn): Promise<UserProfileDto> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/users/profile`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch profile: ${response.statusText}`);
		}
		return response.json();
	}
}

export const userService = new UserService();
