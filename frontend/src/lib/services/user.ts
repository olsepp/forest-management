import { apiFetch } from '$lib/utils/api-fetch';
import type { UserListDto, UserDetailsDto, UserProfileDto } from '$lib/dtos/user/user.dto';

type FetchFn = typeof window.fetch;

class UserService {
	async getAll(fetchFn?: FetchFn): Promise<UserListDto[]> {
		return apiFetch('/api/users', fetchFn);
	}

	async getById(id: string, fetchFn?: FetchFn): Promise<UserDetailsDto> {
		return apiFetch(`/api/users/${id}`, fetchFn);
	}

	async create(user: unknown): Promise<UserDetailsDto> {
		return apiFetch('/api/users', undefined, {
			method: 'POST',
			body: JSON.stringify(user)
		});
	}

	async update(id: string, user: unknown): Promise<UserDetailsDto> {
		return apiFetch(`/api/users/${id}`, undefined, {
			method: 'PUT',
			body: JSON.stringify(user)
		});
	}

	async getProfile(fetchFn?: FetchFn): Promise<UserProfileDto> {
		return apiFetch('/api/users/profile', fetchFn);
	}
}

export const userService = new UserService();
