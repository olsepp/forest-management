import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from './auth';
import type {
	LandPropertyDto,
	LandPropertyUpdateDto
} from '$lib/dtos/land-property/land-property.dto';

const API_BASE_URL = PUBLIC_API_URL || 'http://localhost:5000';

type FetchFn = typeof window.fetch;

class LandPropertyService {
	async getAll(fetchFn?: FetchFn): Promise<LandPropertyDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/landproperties`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch land properties: ${response.statusText}`);
		}
		return response.json();
	}

	async getById(id: string, fetchFn?: FetchFn): Promise<LandPropertyDto> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/landproperties/${id}`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch land property: ${response.statusText}`);
		}
		return response.json();
	}

	async getByCompany(companyId: string, fetchFn?: FetchFn): Promise<LandPropertyDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/landproperties/by-company/${companyId}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch land properties: ${response.statusText}`);
		}
		return response.json();
	}

	async search(companyId: string, fetchFn?: FetchFn): Promise<LandPropertyDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/landproperties/search?companyId=${companyId}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch land properties: ${response.statusText}`);
		}
		return response.json();
	}

	async create(property: unknown): Promise<LandPropertyDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/landproperties`, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(property)
		});
		if (!response.ok) {
			throw new Error(`Failed to create land property: ${response.statusText}`);
		}
		return response.json();
	}

	async update(id: string, property: LandPropertyUpdateDto): Promise<LandPropertyDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/landproperties/${id}`, {
			method: 'PUT',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(property)
		});
		if (!response.ok) {
			throw new Error(`Failed to update land property: ${response.statusText}`);
		}
		return response.json();
	}
}

export const landPropertyService = new LandPropertyService();
