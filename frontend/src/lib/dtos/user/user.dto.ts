export type UserListDto = {
	id: string;
	username?: string;
	email: string;
	role?: string;
	firstName?: string;
	lastName?: string;
	[key: string]: unknown;
};

export type UserDetailsDto = UserListDto & {
	[key: string]: unknown;
};

export type UserProfileDto = {
	id?: string;
	userId?: string;
	username?: string;
	email?: string;
	firstName?: string;
	lastName?: string;
	role?: string;
	phoneNumber?: string;
};
