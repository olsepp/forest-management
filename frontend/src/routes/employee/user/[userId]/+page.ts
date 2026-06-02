import type { PageLoad } from './$types';
import { ssrSafeApiFetch } from '$lib/utils/api-fetch';
import type { UserProfileDto } from '$lib/dtos/user/user.dto';

export const load: PageLoad = async ({ fetch: fetchFn }) => {
	const profile = await ssrSafeApiFetch<UserProfileDto>('/api/users/profile', fetchFn);
	return { profile };
};
