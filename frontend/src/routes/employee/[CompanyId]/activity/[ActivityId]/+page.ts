import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { ActivityDto, ActivityTypeListDto } from '$lib/dtos/activity/activity.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const activity = await ssrSafeApiFetch<ActivityDto>(
		`/api/activities/${params.ActivityId}`,
		fetchFn
	);
	const activityTypes = await ssrSafeApiFetch<ActivityTypeListDto[]>(
		'/api/activitytypes',
		fetchFn
	);

	return {
		activity,
		activityTypes: activityTypes ?? []
	};
};
