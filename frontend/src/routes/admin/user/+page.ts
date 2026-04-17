import type { PageLoad } from './$types';
import { userService } from '$lib/services/user';

export const load: PageLoad = async ({ fetch: fetchFn }) => {
	const users = await userService.getAll(fetchFn);

	const detailsEntries = await Promise.all(
		users.map(async (user) => {
			try {
				const details = await userService.getById(user.id, fetchFn);
				return [user.id, { ...user, ...details }] as const;
			} catch {
				return [user.id, user] as const;
			}
		})
	);

	const userDetailsById = Object.fromEntries(detailsEntries);

	return {
		users,
		userDetailsById: userDetailsById as Record<
			string,
			(typeof userDetailsById)[keyof typeof userDetailsById]
		>
	};
};
