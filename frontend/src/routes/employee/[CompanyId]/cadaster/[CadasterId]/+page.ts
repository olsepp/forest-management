import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { CadasterDto, ForestStandListDto, ActivityListDto } from '$lib/dtos/cadaster/cadaster.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const cadasterId = params.CadasterId;

	const cadaster = await ssrSafeApiFetch<CadasterDto>(
		`/api/cadasters/${cadasterId}`,
		fetchFn
	);
	const forestStands = await ssrSafeApiFetch<ForestStandListDto[]>(
		`/api/foreststands/by-cadaster/${cadasterId}`,
		fetchFn
	);
	const activities = await ssrSafeApiFetch<ActivityListDto[]>(
		`/api/activities/by-cadaster/${cadasterId}`,
		fetchFn
	);

	return {
		cadaster,
		forestStands: forestStands ?? [],
		activities: activities ?? []
	};
};
