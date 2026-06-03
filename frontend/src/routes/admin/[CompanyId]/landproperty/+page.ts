import type { PageLoad } from './$types';
import { landPropertyService } from '$lib/services/land-property';

export const load: PageLoad = async ({ params, fetch: fetchFn, url }) => {
	const companyId = params.CompanyId;
	const skip = parseInt(url.searchParams.get('skip') ?? '0', 10);
	const take = parseInt(url.searchParams.get('take') ?? '20', 10);
	const searchText = url.searchParams.get('searchText') ?? undefined;
	const county = url.searchParams.get('county') ?? undefined;
	const isFsc = url.searchParams.get('isFsc') === 'true' ? true : undefined;

	const [result, counties] = await Promise.all([
		landPropertyService.searchPaged({ companyId, searchText, county, isFsc }, skip, take, fetchFn),
		landPropertyService.getCounties(companyId, fetchFn)
	]);

	return {
		properties: result?.items ?? [],
		total: result?.total ?? 0,
		skip,
		take,
		searchText: searchText ?? '',
		county: county ?? '',
		isFsc: isFsc ?? false,
		counties: counties ?? []
	};
};
