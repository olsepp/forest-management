import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { LandPropertyListDto } from '$lib/dtos/land-property/land-property-list.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const companyId = params.CompanyId;
	const qs = new URLSearchParams({ companyId, Status: 'Active' }).toString();
	const properties = await ssrSafeApiFetch<LandPropertyListDto[]>(
		`/api/landproperties/search?${qs}`,
		fetchFn
	);

	return {
		properties: properties ?? []
	};
};
