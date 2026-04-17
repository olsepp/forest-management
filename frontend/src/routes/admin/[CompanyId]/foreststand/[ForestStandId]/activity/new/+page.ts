import type { PageLoad } from './$types';
import { forestStandService } from '$lib/services/forest-stand';
import { cadasterService } from '$lib/services/cadaster';
import type { ForestStandSummaryDto } from '$lib/dtos/forest-stand/forest-stand.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const forestStandId = params.ForestStandId;

	const forestStand = await forestStandService.getById(forestStandId, fetchFn);
	const cadaster = await cadasterService.getById(forestStand.cadasterId, fetchFn).catch(() => null);

	const landPropertyId = cadaster?.landPropertyId ?? forestStand.landPropertyId;
	const landPropertyName = cadaster?.landPropertyName ?? forestStand.landPropertyName;

	return {
		forestStand: {
			id: forestStand.id,
			number: forestStand.number,
			cadasterId: forestStand.cadasterId,
			cadasterCadastralNumber: forestStand.cadasterCadastralNumber,
			landPropertyId: landPropertyId,
			landPropertyName: landPropertyName
		} as ForestStandSummaryDto
	};
};
