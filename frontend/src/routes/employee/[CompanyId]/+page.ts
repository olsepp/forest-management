import type { PageLoad } from './$types';
import { companyService } from '$lib/services/company';
import { activityService } from '$lib/services/activity';
import type { CompanyDto } from '$lib/dtos/company/company.dto';
import type { ActivityDto } from '$lib/dtos/activity/activity.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	try {
		const [company, recentActivities] = await Promise.all([
			companyService.getById(params.CompanyId, fetchFn),
			loadRecentActivities(params.CompanyId, fetchFn)
		]);

		return { company, recentActivities };
	} catch {
		// SSR: no auth token — return empty state, client will hydrate
		return { company: null as CompanyDto | null, recentActivities: [] as ActivityDto[] };
	}
};

async function loadRecentActivities(
	companyId: string,
	fetchFn: typeof window.fetch
): Promise<ActivityDto[]> {
	try {
		const stored = localStorage.getItem('auth_user');
		if (!stored) return [];

		const user = JSON.parse(stored) as { userId?: string };
		if (!user?.userId) return [];

		return await activityService.getRecentByUser(user.userId, 5, companyId, fetchFn);
	} catch {
		return [];
	}
}
