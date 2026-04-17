import type { PageLoad } from './$types';
import { activityTypeService } from '$lib/services/activity-type';

export const load: PageLoad = async ({ fetch: fetchFn }) => {
	return {
		activityTypes: await activityTypeService.getAll(fetchFn)
	};
};
