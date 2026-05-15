import { PUBLIC_API_URL } from '$env/static/public';
import { authService } from './auth';

const API_BASE_URL = PUBLIC_API_URL;

type FetchFn = typeof window.fetch;

class DashboardService {
	async getSummary(companyId: string, fetchFn?: FetchFn): Promise<unknown> {
		const token = await authService.ensureValidToken();
		const response = await (fetchFn ?? fetch)(
			`${API_BASE_URL}/api/dashboard/${companyId}/summary`,
			{
				headers: {
					'Content-Type': 'application/json',
					Authorization: `Bearer ${token}`
				}
			}
		);
		if (!response.ok) {
			throw new Error(`Failed to fetch dashboard summary: ${response.statusText}`);
		}
		return response.json();
	}
}

export const dashboardService = new DashboardService();
