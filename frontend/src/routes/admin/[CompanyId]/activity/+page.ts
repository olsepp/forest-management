import type { PageLoad } from './$types';
import type { ActivityDto } from '$lib/dtos/activity/activity.dto';
import type { ActivityTypeListDto } from '$lib/dtos/activity-type/activity-type.dto';
import type { UserListDto } from '$lib/dtos/user/user.dto';
import { activityService } from '$lib/services/activity';
import { activityTypeService } from '$lib/services/activity-type';
import { userService } from '$lib/services/user';

export const load: PageLoad = async ({ params, fetch: fetchFn, url }) => {
	const companyId = params.CompanyId;
	const startDate = url.searchParams.get('startDate') ?? undefined;
	const endDate = url.searchParams.get('endDate') ?? undefined;
	const activityTypeId = url.searchParams.get('activityTypeId') ?? undefined;
	const userId = url.searchParams.get('userId') ?? undefined;

	const activities = await activityService.getByCompanyFiltered(
		companyId,
		startDate,
		endDate,
		activityTypeId,
		userId,
		fetchFn
	);

	const sortedActivities = (activities ?? [])
		.filter((item) => Boolean(item?.id))
		.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

	const [activityTypes, users] = await Promise.all([
		activityTypeService.getAll(fetchFn),
		userService.getAll(fetchFn)
	]);

	return {
		activities: sortedActivities,
		activityTypes,
		users
	};
};
