import type { PageLoad } from './$types';
import type { ActivityDto } from '$lib/dtos/activity/activity.dto';
import type { ActivityTypeListDto } from '$lib/dtos/activity-type/activity-type.dto';
import type { UserListDto } from '$lib/dtos/user/user.dto';
import type { PagedResult } from '$lib/services/activity';
import { activityService } from '$lib/services/activity';
import { activityTypeService } from '$lib/services/activity-type';
import { userService } from '$lib/services/user';

export const load: PageLoad = async ({ params, fetch: fetchFn, url }) => {
	const companyId = params.CompanyId;
	const startDate = url.searchParams.get('startDate') ?? undefined;
	const endDate = url.searchParams.get('endDate') ?? undefined;
	const activityTypeId = url.searchParams.get('activityTypeId') ?? undefined;
	const userId = url.searchParams.get('userId') ?? undefined;
	const skip = parseInt(url.searchParams.get('skip') ?? '0', 10);
	const take = parseInt(url.searchParams.get('take') ?? '20', 10);

	const result = await activityService.getByCompanyFiltered(
		companyId,
		skip,
		take,
		startDate,
		endDate,
		activityTypeId,
		userId,
		fetchFn
	);

	const sortedActivities = (result?.items ?? [])
		.filter((item) => Boolean(item?.id))
		.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

	const [activityTypes, users] = await Promise.all([
		activityTypeService.getAll(fetchFn),
		userService.getAll(fetchFn)
	]);

	return {
		activities: sortedActivities,
		total: result?.total ?? 0,
		skip,
		take,
		activityTypes,
		users
	};
};
