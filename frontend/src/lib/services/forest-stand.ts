import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from './auth';
import type { ForestStandDto, ForestStandUpdateDto } from '$lib/dtos/forest-stand/forest-stand.dto';

const API_BASE_URL = PUBLIC_API_URL || 'http://localhost:5000';

type FetchFn = typeof window.fetch;

class ForestStandService {
	async getById(id: string, fetchFn?: FetchFn): Promise<ForestStandDto> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(`${API_BASE_URL}/api/foreststands/${id}`, {
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			}
		});
		if (!response.ok) {
			throw new Error(`Failed to fetch forest stand: ${response.statusText}`);
		}
		return response.json();
	}

	async getByCadaster(cadasterId: string, fetchFn?: FetchFn): Promise<ForestStandDto[]> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/foreststands/by-cadaster/${cadasterId}`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch forest stands: ${response.statusText}`);
		}
		return response.json();
	}

	async update(id: string, forestStand: ForestStandUpdateDto): Promise<ForestStandDto> {
		const token = await authService.ensureValidToken();
		const response = await fetch(`${API_BASE_URL}/api/foreststands/${id}`, {
			method: 'PUT',
			headers: {
				'Content-Type': 'application/json',
				Authorization: `Bearer ${token}`
			},
			body: JSON.stringify(forestStand)
		});
		if (!response.ok) {
			throw new Error(`Failed to update forest stand: ${response.statusText}`);
		}
		return response.json();
	}
}

export const forestStandService = new ForestStandService();
