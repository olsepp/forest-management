import type { PageLoad } from './$types';
import { cadasterService } from '$lib/services/cadaster';
import { forestStandService } from '$lib/services/forest-stand';
import { activityService } from '$lib/services/activity';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const forestStandId = params.ForestStandId;

	const forestStand = await forestStandService.getById(forestStandId, fetchFn);
	const cadaster = await cadasterService.getById(forestStand.cadasterId, fetchFn);
	const activities = await activityService.getByForestStand(forestStandId, fetchFn);

	return {
		cadaster,
		forestStand,
		activities
	};
};
