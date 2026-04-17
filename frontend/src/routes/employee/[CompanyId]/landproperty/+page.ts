import type { PageLoad } from './$types';
import { landPropertyService } from '$lib/services/land-property';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const properties = await landPropertyService.getByCompany(params.CompanyId, fetchFn);
	return {
		properties
	};
};
