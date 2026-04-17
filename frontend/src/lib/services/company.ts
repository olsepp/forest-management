import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from './auth';
import type { CompanyListDto, CompanyDto } from '$lib/dtos/company/company.dto';

const API_BASE_URL = PUBLIC_API_URL || 'http://localhost:5000';

type FetchFn = typeof window.fetch;

class CompanyService {
	async getAll(fetchFn?: FetchFn): Promise<CompanyListDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/companies`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch companies: ${response.statusText}`);
		}
		return response.json();
	}

	async getById(id: string, fetchFn?: FetchFn): Promise<CompanyDto> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/companies/${id}`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch company: ${response.statusText}`);
		}
		return response.json();
	}
}

export const companyService = new CompanyService();
