import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { ForestStandDto } from '$lib/dtos/forest-stand/forest-stand.dto';
import type { ActivityTypeListDto } from '$lib/dtos/activity-type/activity-type.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const forestStand = await ssrSafeApiFetch<ForestStandDto>(
		`/api/foreststands/${params.ForestStandId}`,
		fetchFn
	);
	const activityTypes = await ssrSafeApiFetch<ActivityTypeListDto[]>(
		'/api/activitytypes',
		fetchFn
	);

	return {
		forestStand: forestStand
			? {
					id: forestStand.id,
					number: forestStand.number,
					cadasterId: forestStand.cadasterId,
					cadasterCadastralNumber: forestStand.cadasterCadastralNumber,
					landPropertyId: forestStand.landPropertyId,
					landPropertyName: forestStand.landPropertyName,
					landPropertyIsFsc: forestStand.landPropertyIsFsc
				}
			: null,
		activityTypes: activityTypes ?? []
	};
};
