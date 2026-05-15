import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from './auth';
import type {
	ActivityTypeListDto,
	ActivityTypeDto
} from '$lib/dtos/activity-type/activity-type.dto';

const API_BASE_URL = PUBLIC_API_URL;

type FetchFn = typeof window.fetch;

class ActivityTypeService {
	async getAll(fetchFn?: FetchFn): Promise<ActivityTypeListDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/activitytypes`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch activity types: ${response.statusText}`);
		}
		return response.json();
	}

	async create(type: unknown): Promise<ActivityTypeDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/activitytypes`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(type)
		});
		if (!response.ok) {
			throw new Error(`Failed to create activity type: ${response.statusText}`);
		}
		return response.json();
	}
}

export const activityTypeService = new ActivityTypeService();
