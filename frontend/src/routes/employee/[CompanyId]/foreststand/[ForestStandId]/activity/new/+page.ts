import type { PageLoad } from './$types';
import { forestStandService } from '$lib/services/forest-stand';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const forestStand = await forestStandService.getById(params.ForestStandId, fetchFn);
	return {
		forestStand: {
			id: forestStand.id,
			number: forestStand.number,
			cadasterId: forestStand.cadasterId,
			cadasterCadastralNumber: forestStand.cadasterCadastralNumber,
			landPropertyId: forestStand.landPropertyId,
			landPropertyName: forestStand.landPropertyName,
			landPropertyIsFsc: forestStand.landPropertyIsFsc
		}
	};
};
