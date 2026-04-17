import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from './auth';
import type { ActivityDto, ActivityUpdateDto } from '$lib/dtos/activity/activity.dto';

const API_BASE_URL = PUBLIC_API_URL || 'http://localhost:5000';

type FetchFn = typeof window.fetch;

class ActivityService {
	async getAll(fetchFn?: FetchFn): Promise<ActivityDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/activities`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch activities: ${response.statusText}`);
		}
		return response.json();
	}

	async getById(id: string, fetchFn?: FetchFn): Promise<ActivityDto> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/activities/${id}`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch activity: ${response.statusText}`);
		}
		return response.json();
	}

	async getByCompany(companyId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/activities/by-company/${companyId}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch activities: ${response.statusText}`);
		}
		return response.json();
	}

	async getMyByCompany(companyId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/activities/by-company/${companyId}/my`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch my activities: ${response.statusText}`);
		}
		return response.json();
	}

	async getByCadaster(cadasterId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/activities/by-cadaster/${cadasterId}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch activities: ${response.statusText}`);
		}
		return response.json();
	}

	async getByForestStand(forestStandId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/activities/by-foreststand/${forestStandId}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch activities: ${response.statusText}`);
		}
		return response.json();
	}

	async getByProperty(propertyId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/activities/by-property/${propertyId}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch activities: ${response.statusText}`);
		}
		return response.json();
	}

	async getRecentByUser(
		userId: string,
		count: number = 5,
		companyId?: string,
		fetchFn?: FetchFn
	): Promise<ActivityDto[]> {
		const token = await authService.ensureValidToken();
		const queryParams = new URLSearchParams();
		queryParams.append('count', count.toString());
		if (companyId) queryParams.append('companyId', companyId);
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/activities/by-user/${userId}/recent?${queryParams.toString()}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch recent activities: ${response.statusText}`);
		}
		return response.json();
	}

	async create(activity: unknown): Promise<ActivityDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/activities`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(activity)
		});
		if (!response.ok) {
			throw new Error(`Failed to create activity: ${response.statusText}`);
		}
		return response.json();
	}

	async update(id: string, activity: ActivityUpdateDto): Promise<ActivityDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/activities/${id}`, {
			method: 'PUT',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(activity)
		});
		if (!response.ok) {
			throw new Error(`Failed to update activity: ${response.statusText}`);
		}
		return response.json();
	}

	async delete(id: string): Promise<void> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/activities/${id}`, {
			method: 'DELETE',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to delete activity: ${response.statusText}`);
		}
	}
}

export const activityService = new ActivityService();
