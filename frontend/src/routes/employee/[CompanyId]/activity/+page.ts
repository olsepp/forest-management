import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { ActivityDto } from '$lib/dtos/activity/activity.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const raw = await ssrSafeApiFetch<ActivityDto[]>(
		`/api/activities/by-company/${params.CompanyId}/my`,
		fetchFn
	);
	const activities = (raw ?? [])
		.filter((item) => Boolean(item?.id))
		.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

	return { activities };
};
