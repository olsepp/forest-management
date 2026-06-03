import { apiFetch } from '$lib/utils/api-fetch';
import type {
	LandPropertyDto,
	LandPropertyUpdateDto
} from '$lib/dtos/land-property/land-property.dto';

type FetchFn = typeof window.fetch;

export type PagedResult<T> = {
	items: T[];
	total: number;
};

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

	async searchPaged(
		params: {
			companyId?: string;
			status?: string;
			searchText?: string;
			county?: string;
			isFsc?: boolean;
		},
		skip = 0,
		take = 20,
		fetchFn?: FetchFn
	): Promise<PagedResult<LandPropertyDto>> {
		const queryParams = new URLSearchParams();
		queryParams.append('skip', String(skip));
		queryParams.append('take', String(take));
		if (params.companyId) queryParams.append('companyId', params.companyId);
		if (params.status) queryParams.append('Status', params.status);
		if (params.searchText) queryParams.append('SearchText', params.searchText);
		if (params.county) queryParams.append('County', params.county);
		if (params.isFsc !== undefined) queryParams.append('isFsc', String(params.isFsc));
		return apiFetch(`/api/landproperties/search-paged?${queryParams.toString()}`, fetchFn);
	}

	async getCounties(companyId: string, fetchFn?: FetchFn): Promise<string[]> {
		return apiFetch(`/api/landproperties/counties?companyId=${companyId}`, fetchFn);
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
