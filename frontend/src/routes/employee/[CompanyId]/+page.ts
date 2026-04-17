import type { PageLoad } from './$types';
import { companyService } from '$lib/services/company';
import { activityService } from '$lib/services/activity';
import type { ActivityDto } from '$lib/dtos/activity/activity.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const company = await companyService.getById(params.CompanyId, fetchFn);

	return {
		company
	};
};
