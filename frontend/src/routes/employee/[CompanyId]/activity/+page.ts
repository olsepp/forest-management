import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { ActivityDto } from '$lib/dtos/activity/activity.dto';
import type { PagedResult } from '$lib/services/activity';

export const load: PageLoad = async ({ params, fetch: fetchFn, url }) => {
	const skip = parseInt(url.searchParams.get('skip') ?? '0', 10);
	const take = parseInt(url.searchParams.get('take') ?? '20', 10);

	const raw = await ssrSafeApiFetch<PagedResult<ActivityDto>>(
		`/api/activities/by-company/${params.CompanyId}/my?skip=${skip}&take=${take}`,
		fetchFn
	);
	const activities = raw
		? raw.items
			.filter((item) => Boolean(item?.id))
			.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
		: null;

	return { activities, total: raw?.total ?? 0, skip, take };
};
