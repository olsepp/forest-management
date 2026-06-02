import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { CadasterDto } from '$lib/dtos/cadaster/cadaster.dto';
import type { ActivityTypeListDto } from '$lib/dtos/activity-type/activity-type.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const cadaster = await ssrSafeApiFetch<CadasterDto>(
		`/api/cadasters/${params.CadasterId}`,
		fetchFn
	);
	const activityTypes = await ssrSafeApiFetch<ActivityTypeListDto[]>(
		'/api/activitytypes',
		fetchFn
	);

	return {
		cadaster: cadaster
			? {
					id: cadaster.id,
					cadastralNumber: cadaster.cadastralNumber,
					landPropertyId: cadaster.landPropertyId,
					landPropertyName: cadaster.landPropertyName,
					landPropertyIsFsc: cadaster.landPropertyIsFsc
				}
			: null,
		activityTypes: activityTypes ?? []
	};
};
