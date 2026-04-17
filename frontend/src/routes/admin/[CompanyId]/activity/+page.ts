import type { PageLoad } from './$types';
import { activityService } from '$lib/services/activity';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const activities = await activityService.getByCompany(params.CompanyId, fetchFn);

	const sortedActivities = (activities ?? [])
		.filter((item) => Boolean(item?.id))
		.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

	return {
		activities: sortedActivities
	};
};
