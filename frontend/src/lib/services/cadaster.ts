import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from './auth';
import type { CadasterDto, CadasterUpdateDto } from '$lib/dtos/cadaster/cadaster.dto';

const API_BASE_URL = PUBLIC_API_URL;

type FetchFn = typeof window.fetch;

class CadasterService {
	async getById(id: string, fetchFn?: FetchFn): Promise<CadasterDto> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/cadasters/${id}`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch cadaster: ${response.statusText}`);
		}
		return response.json();
	}

	async getByLandProperty(landPropertyId: string, fetchFn?: FetchFn): Promise<CadasterDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/cadasters/by-land-property/${landPropertyId}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch cadasters: ${response.statusText}`);
		}
		return response.json();
	}

	async update(id: string, cadaster: CadasterUpdateDto): Promise<CadasterDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/cadasters/${id}`, {
			method: 'PUT',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(cadaster)
		});
		if (!response.ok) {
			throw new Error(`Failed to update cadaster: ${response.statusText}`);
		}
		return response.json();
	}
}

export const cadasterService = new CadasterService();
