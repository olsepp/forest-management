import type { PageLoad } from './$types';
import { companyService } from '$lib/services/company';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	return {
		company: await companyService.getById(params.CompanyId, fetchFn)
	};
};
