import type { PageLoad } from './$types';
import { userService } from '$lib/services/user';

export const load: PageLoad = async ({ fetch: fetchFn }) => {
	return {
		profile: await userService.getProfile(fetchFn)
	};
};
