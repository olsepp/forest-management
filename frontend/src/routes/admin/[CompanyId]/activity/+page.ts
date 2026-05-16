import type { PageLoad, ActivityDto } from './$types';
import { activityService } from '$lib/services/activity';

export const load: PageLoad = async ({ params, fetch: fetchFn, url }) => {
	const companyId = params.CompanyId;
	const startDate = url.searchParams.get('startDate');
	const endDate = url.searchParams.get('endDate');

	let activities: ActivityDto[] = [];

	if (startDate && endDate) {
		activities = await activityService.getByCompanyDateRange(companyId, startDate, endDate, fetchFn);
	} else {
		activities = await activityService.getByCompany(companyId, fetchFn);
	}

	const sortedActivities = (activities ?? [])
		.filter((item) => Boolean(item?.id))
		.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

	return {
		activities: sortedActivities
	};
};
