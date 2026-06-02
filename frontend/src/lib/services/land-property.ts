import { apiFetch } from '$lib/utils/api-fetch';
import type {
	LandPropertyDto,
	LandPropertyUpdateDto
} from '$lib/dtos/land-property/land-property.dto';

type FetchFn = typeof window.fetch;

class LandPropertyService {
	async getAll(fetchFn?: FetchFn): Promise<LandPropertyDto[]> {
		return apiFetch('/api/landproperties', fetchFn);
	}

	async getById(id: string, fetchFn?: FetchFn): Promise<LandPropertyDto> {
		return apiFetch(`/api/landproperties/${id}`, fetchFn);
	}

	async getByCompany(companyId: string, fetchFn?: FetchFn): Promise<LandPropertyDto[]> {
		return this.search({ companyId, status: 'Active' }, fetchFn);
	}

	async search(
		params: {
			companyId?: string;
			status?: string;
			searchName?: string;
			county?: string;
			city?: string;
			isFsc?: boolean;
		},
		fetchFn?: FetchFn
	): Promise<LandPropertyDto[]> {
		const queryParams = new URLSearchParams();
		if (params.companyId) queryParams.append('companyId', params.companyId);
		if (params.status) queryParams.append('Status', params.status);
		if (params.searchName) queryParams.append('SearchName', params.searchName);
		if (params.county) queryParams.append('County', params.county);
		if (params.city) queryParams.append('City', params.city);
		if (params.isFsc !== undefined) queryParams.append('isFsc', String(params.isFsc));
		return apiFetch(`/api/landproperties/search?${queryParams.toString()}`, fetchFn);
	}

	async create(property: unknown): Promise<LandPropertyDto> {
		return apiFetch('/api/landproperties', undefined, {
			method: 'POST',
			body: JSON.stringify(property)
		});
	}

	async update(id: string, property: LandPropertyUpdateDto): Promise<LandPropertyDto> {
		return apiFetch(`/api/landproperties/${id}`, undefined, {
			method: 'PUT',
			body: JSON.stringify(property)
		});
	}
}

export const landPropertyService = new LandPropertyService();
