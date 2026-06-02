import type { PageLoad } from './$types';
import { cadasterService } from '$lib/services/cadaster';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const cadaster = await cadasterService.getById(params.CadasterId, fetchFn);
	return {
		cadaster: {
			id: cadaster.id,
			cadastralNumber: cadaster.cadastralNumber,
			landPropertyId: cadaster.landPropertyId,
			landPropertyName: cadaster.landPropertyName,
			landPropertyIsFsc: cadaster.landPropertyIsFsc
		}
	};
};
