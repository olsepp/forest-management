import type { PageLoad } from './$types';
import { companyService } from '$lib/services/company';
import { dashboardService } from '$lib/services/dashboard';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const companyId = params.CompanyId;

	const [company, summary] = await Promise.all([
		companyService.getById(companyId, fetchFn),
		dashboardService.getSummary(companyId, fetchFn)
	]);

	return {
		company,
		summary
	};
};
