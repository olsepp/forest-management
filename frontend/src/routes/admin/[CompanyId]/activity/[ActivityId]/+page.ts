import type { PageLoad } from './$types';
import { activityService } from '$lib/services/activity';
import { activityTypeService } from '$lib/services/activity-type';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const [activity, activityTypes] = await Promise.all([
		activityService.getById(params.ActivityId, fetchFn),
		activityTypeService.getAll(fetchFn)
	]);

	return {
		activity,
		activityTypes
	};
};
