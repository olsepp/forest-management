import { apiFetch } from '$lib/utils/api-fetch';
import type { CompanyListDto, CompanyDto } from '$lib/dtos/company/company.dto';

type FetchFn = typeof window.fetch;

class CompanyService {
	async getAll(fetchFn?: FetchFn): Promise<CompanyListDto[]> {
		return apiFetch('/api/companies', fetchFn);
	}

	async getById(id: string, fetchFn?: FetchFn): Promise<CompanyDto> {
		return apiFetch(`/api/companies/${id}`, fetchFn);
	}
}

export const companyService = new CompanyService();
