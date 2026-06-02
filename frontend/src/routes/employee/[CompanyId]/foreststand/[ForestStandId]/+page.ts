import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { ForestStandDto, ActivityListDto } from '$lib/dtos/forest-stand/forest-stand.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const forestStandId = params.ForestStandId;

	const forestStand = await ssrSafeApiFetch<ForestStandDto>(
		`/api/foreststands/${forestStandId}`,
		fetchFn
	);
	const activities = await ssrSafeApiFetch<ActivityListDto[]>(
		`/api/activities/by-foreststand/${forestStandId}`,
		fetchFn
	);

	return {
		forestStand,
		activities: activities ?? []
	};
};
