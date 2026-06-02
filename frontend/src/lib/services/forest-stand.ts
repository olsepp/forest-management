import { apiFetch } from '$lib/utils/api-fetch';
import type { ForestStandDto, ForestStandUpdateDto } from '$lib/dtos/forest-stand/forest-stand.dto';

type FetchFn = typeof window.fetch;

class ForestStandService {
	async getById(id: string, fetchFn?: FetchFn): Promise<ForestStandDto> {
		return apiFetch(`/api/foreststands/${id}`, fetchFn);
	}

	async getByCadaster(cadasterId: string, fetchFn?: FetchFn): Promise<ForestStandDto[]> {
		return apiFetch(`/api/foreststands/by-cadaster/${cadasterId}`, fetchFn);
	}

	async update(id: string, forestStand: ForestStandUpdateDto): Promise<ForestStandDto> {
		return apiFetch(`/api/foreststands/${id}`, undefined, {
			method: 'PUT',
			body: JSON.stringify(forestStand)
		});
	}
}

export const forestStandService = new ForestStandService();
