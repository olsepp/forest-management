import type { PageLoad } from './$types';
import { landPropertyService } from '$lib/services/land-property';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const companyId = params.CompanyId;
	const [activeProperties, inactiveProperties] = await Promise.all([
		landPropertyService.search({ companyId, status: 'Active' }, fetchFn),
		landPropertyService.search({ companyId, status: 'Inactive' }, fetchFn)
	]);
	const properties = [...activeProperties, ...inactiveProperties];
	return {
		properties
	};
};
