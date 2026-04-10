export type ActivityStatus = 'Pending' | 'Approved' | 'Rejected';

export type ActivityDto = {
	id: string;
	description: string;
	quantity: number;
	unit: string | null;
	notes: string | null;
	date: string;
	userId: string;
	userName: string;
	activityTypeId: string;
	activityTypeName: string;
	cadasterId: string | null;
	cadasterCadastralNumber: string | null;
	forestStandId: string | null;
	forestStandNumber: number;
	landPropertyId: string | null;
	landPropertyName: string | null;
	applicationStatus: ActivityStatus | null;
};

export type ActivityTypeListDto = {
	id: string;
	activityTypeName: string;
};

export type ActivityUpdateDto = {
	id: string;
	description: string;
	quantity: number;
	unit: string | null;
	notes: string | null;
	date: string;
	activityTypeId: string;
	forestStandId: string | null;
	cadasterId: string | null;
	applicationStatus: ActivityStatus | null;
};
