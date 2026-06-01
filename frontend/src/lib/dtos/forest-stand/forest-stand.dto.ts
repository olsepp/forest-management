export type RecentActivityDto = {
	id: string;
	description: string;
	quantity: number;
	unit: string | null;
	date: string;
	activityTypeName: string;
	userName: string;
	cadasterCadastralNumber: string | null;
	forestStandNumber: number;
};

export type ActivityListDto = {
	id: string;
	description: string;
	quantity: number;
	unit: string | null;
	date: string;
	activityTypeName: string;
	userName: string;
	cadasterCadastralNumber: string | null;
	forestStandNumber: number;
	locationDescription: string | null;
	applicationStatus: string | null;
};

export type ForestStandDto = {
	id: string;
	number: number;
	area: number;
	totalVolume: number;
	isActive: boolean;
	validFrom: string;
	validTo: string | null;
	cadasterId: string;
	cadasterCadastralNumber: string;
	landPropertyId: string;
	landPropertyName: string;
	landPropertyIsFsc: boolean;
	recentActivities: RecentActivityDto[];
};

export type ForestStandSummaryDto = {
	id: string;
	number: number;
	cadasterId: string;
	cadasterCadastralNumber: string;
	landPropertyId?: string;
	landPropertyName?: string;
};

export type ForestStandUpdateDto = {
	id: string;
	number: number;
	area: number;
	totalVolume: number;
	isActive: boolean;
	validFrom: string;
	validTo: string | null;
	cadasterId: string;
};

export type CadasterSummaryDto = {
	id: string;
	cadastralNumber: string;
	landPropertyId: string;
	landPropertyName: string;
	landPropertyIsFsc: boolean;
};
