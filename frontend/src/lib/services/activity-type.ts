import { apiFetch } from '$lib/utils/api-fetch';
import type {
	ActivityTypeListDto,
	ActivityTypeDto
} from '$lib/dtos/activity-type/activity-type.dto';

type FetchFn = typeof window.fetch;

class ActivityTypeService {
	async getAll(fetchFn?: FetchFn): Promise<ActivityTypeListDto[]> {
		return apiFetch('/api/activitytypes', fetchFn);
	}

	async create(type: unknown): Promise<ActivityTypeDto> {
		return apiFetch('/api/activitytypes', undefined, {
			method: 'POST',
			body: JSON.stringify(type)
		});
	}
}

export const activityTypeService = new ActivityTypeService();
