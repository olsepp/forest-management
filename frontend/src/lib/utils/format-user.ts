export function formatUserName(user: {
	userFirstName?: string | null;
	userLastName?: string | null;
	userName: string;
}): string {
	const fullName = `${user.userFirstName ?? ''} ${user.userLastName ?? ''}`.trim();
	return fullName || user.userName;
}
