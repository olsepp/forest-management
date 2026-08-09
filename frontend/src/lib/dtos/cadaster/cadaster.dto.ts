export type ForestStandListDto = {
	id: string;
	number: number;
	area: number;
	totalVolume: number;
	isActive: boolean;
};

export type ActivityListDto = {
	id: string;
	description: string;
	quantity: number;
	unit: string | null;
	date: string;
	activityTypeName: string;
	userName: string;
	userFirstName: string | null;
	userLastName: string | null;
	cadasterCadastralNumber: string | null;
	forestStandNumber: number;
	locationDescription: string | null;
	applicationStatus: string | null;
};

export type RecentActivityDto = {
	id: string;
	description: string;
	quantity: number;
	unit: string | null;
	date: string;
	activityTypeName: string;
	userName: string;
	userFirstName: string | null;
	userLastName: string | null;
	cadasterCadastralNumber: string | null;
	forestStandNumber: number;
};

export type CadasterDto = {
	id: string;
	cadastralNumber: string;
	forestArea: number;
	arableArea: number;
	grasslandArea: number;
	yardArea: number;
	buildingFootprintArea: number;
	underwaterArea: number;
	otherArea: number;
	soilQualityIndex: number;
	calculatedVolume: number;
	volumeGrowth: number;
	landPropertyId: string;
	landPropertyName: string;
	landPropertyIsFsc: boolean;
	forestStands: ForestStandListDto[];
	recentActivities: RecentActivityDto[];
};

export type CadasterUpdateDto = {
	id: string;
	cadastralNumber: string;
	forestArea: number;
	arableArea: number;
	grasslandArea: number;
	yardArea: number;
	buildingFootprintArea: number;
	underwaterArea: number;
	otherArea: number;
	soilQualityIndex: number;
	calculatedVolume: number;
	volumeGrowth: number;
	landPropertyId: string;
};
