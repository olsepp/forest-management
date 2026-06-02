import { apiFetch } from '$lib/utils/api-fetch';
import type { CadasterDto, CadasterUpdateDto } from '$lib/dtos/cadaster/cadaster.dto';

type FetchFn = typeof window.fetch;

class CadasterService {
	async getById(id: string, fetchFn?: FetchFn): Promise<CadasterDto> {
		return apiFetch(`/api/cadasters/${id}`, fetchFn);
	}

	async getByLandProperty(landPropertyId: string, fetchFn?: FetchFn): Promise<CadasterDto[]> {
		return apiFetch(`/api/cadasters/by-land-property/${landPropertyId}`, fetchFn);
	}

	async update(id: string, cadaster: CadasterUpdateDto): Promise<CadasterDto> {
		return apiFetch(`/api/cadasters/${id}`, undefined, {
			method: 'PUT',
			body: JSON.stringify(cadaster)
		});
	}
}

export const cadasterService = new CadasterService();
