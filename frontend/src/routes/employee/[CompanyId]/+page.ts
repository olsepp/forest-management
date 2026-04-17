import type { PageLoad } from './$types';
import { companyService } from '$lib/services/company';
import { activityService } from '$lib/services/activity';
import type { ActivityDto } from '$lib/dtos/activity/activity.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const company = await companyService.getById(params.CompanyId, fetchFn);
	const activities = (await activityService.getByCompany(params.CompanyId, fetchFn)) ?? [];
	const sorted = activities
		.filter((a): a is ActivityDto => Boolean(a?.id))
		.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
		.slice(0, 5);

	return {
		company,
		activities: sorted
	};
};
