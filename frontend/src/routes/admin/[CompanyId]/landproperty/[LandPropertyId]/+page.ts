import type { PageLoad } from './$types';
import { landPropertyService } from '$lib/services/land-property';
import { cadasterService } from '$lib/services/cadaster';
import { activityService } from '$lib/services/activity';
import type { LandPropertyDto } from '$lib/dtos/land-property/land-property.dto';
import type { CadasterLinkDto } from '$lib/dtos/land-property/land-property.dto';
import type { ActivityDto } from '$lib/dtos/activity/activity.dto';

export const load: PageLoad = async ({ params, fetch }) => {
	const landPropertyId = params.LandPropertyId;

	const [property, cadasters, activities] = await Promise.all([
		landPropertyService.getById(landPropertyId, fetch).catch(() => null as LandPropertyDto | null),
		cadasterService.getByLandProperty(landPropertyId, fetch).catch(() => [] as CadasterLinkDto[]),
		activityService.getByProperty(landPropertyId, fetch).catch(() => [] as ActivityDto[])
	]);

	const filteredCadasters = (cadasters ?? []).filter((item) => Boolean(item?.id));
	const sortedActivities = (activities ?? [])
		.filter((item) => Boolean(item?.id))
		.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());

	return {
		property,
		cadasters: filteredCadasters,
		activities: sortedActivities
	};
};
