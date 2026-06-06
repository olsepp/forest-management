import { authService } from './auth';

// API calls use relative paths so they are routed through nginx in production
// and through the Vite dev proxy (configured in vite.config.ts) in development.
const API_BASE_URL = '';

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
