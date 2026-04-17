import type { PageLoad } from './$types';
import { companyService } from '$lib/services/company';

export const load: PageLoad = async ({ fetch: fetchFn }) => {
	return {
		companies: await companyService.getAll(fetchFn)
	};
};
