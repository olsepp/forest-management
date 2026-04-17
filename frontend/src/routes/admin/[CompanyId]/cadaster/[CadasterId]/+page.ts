import type { PageLoad } from './$types';
import { cadasterService } from '$lib/services/cadaster';
import { forestStandService } from '$lib/services/forest-stand';
import { activityService } from '$lib/services/activity';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const cadasterId = params.CadasterId;

	const [cadaster, forestStands, activities] = await Promise.all([
		cadasterService.getById(cadasterId, fetchFn),
		forestStandService.getByCadaster(cadasterId, fetchFn),
		activityService.getByCadaster(cadasterId, fetchFn)
	]);

	return {
		cadaster,
		forestStands,
		activities
	};
};
