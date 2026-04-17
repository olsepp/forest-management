import type { PageLoad } from './$types';
import { landPropertyService } from '$lib/services/land-property';
import { cadasterService } from '$lib/services/cadaster';
import { activityService } from '$lib/services/activity';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const property = await landPropertyService.getById(params.LandPropertyId, fetchFn);
	const cadasters = await cadasterService.getByLandProperty(params.LandPropertyId, fetchFn);
	const activities = await activityService.getByProperty(params.LandPropertyId, fetchFn);
	const myActivities = (activities ?? [])
		.filter((a) => Boolean(a?.id))
		.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

	return {
		property,
		cadasters,
		activities: myActivities
	};
};
