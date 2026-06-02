import type { PageLoad } from './$types';
import { apiFetch } from '$lib/utils/api-fetch';
import type { CompanyListDto } from '$lib/dtos/company/company.dto';

export const load: PageLoad = async ({ fetch: fetchFn }) => {
	try {
		const companies = await apiFetch<CompanyListDto[]>('/api/companies', fetchFn);
		return { companies };
	} catch {
		// SSR: no auth token available — return empty state, client will hydrate
		return { companies: [] as CompanyListDto[] };
	}
};
