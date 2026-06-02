import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { LandPropertyDto, CadasterLinkDto, ActivityDto } from '$lib/dtos/land-property/land-property.dto';

export const load: PageLoad = async ({ params, fetch: fetchFn }) => {
	const property = await ssrSafeApiFetch<LandPropertyDto>(
		`/api/landproperties/${params.LandPropertyId}`,
		fetchFn
	);
	const cadasters = await ssrSafeApiFetch<CadasterLinkDto[]>(
		`/api/cadasters/by-land-property/${params.LandPropertyId}`,
		fetchFn
	);
	const rawActivities = await ssrSafeApiFetch<ActivityDto[]>(
		`/api/activities/by-property/${params.LandPropertyId}`,
		fetchFn
	);

	return {
		property,
		cadasters: cadasters ?? [],
		activities: (rawActivities ?? [])
			.filter((a) => Boolean(a?.id))
			.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime())
	};
};
