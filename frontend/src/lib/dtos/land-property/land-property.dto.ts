export type PropertyStatus = 'Active' | 'Inactive' | 'Sold';

export type PropertyCadasterLinkDto = {
	id: string;
	cadastralNumber: string;
};

export type LandPropertyDto = {
	id: string;
	name: string;
	registrationNumber: number;
	county: string;
	parish: string;
	village: string;
	boughtDate: string | null;
	soldDate: string | null;
	status: PropertyStatus | number | string;
	cadastralNumbers?: string[];
	cadasters?: PropertyCadasterLinkDto[];
	companyId: string;
	companyName: string;
	isFsc?: boolean;
};

export type LandPropertyUpdateDto = {
	id: string;
	name: string;
	registrationNumber: number;
	county: string;
	parish: string;
	village: string;
	boughtDate: string | null;
	soldDate: string | null;
	status: PropertyStatus | number;
	companyId: string;
	isFsc?: boolean;
};

export type CadasterLinkDto = {
	id: string;
	cadastralNumber: string;
	forestArea?: number;
	forestStandCount?: number;
};

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
	forestStandNumber: number | null;
	landPropertyId: string | null;
	landPropertyName: string | null;
	applicationStatus: number | null;
};
