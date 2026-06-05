import type { PageLoad } from './$types';
import { landPropertyService } from '$lib/services/land-property';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const companyId = params.CompanyId;
	const properties = await landPropertyService.getSoldByCompany(companyId, fetchFn);
	return {
		properties
	};
};
