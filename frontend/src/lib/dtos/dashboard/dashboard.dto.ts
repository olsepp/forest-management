export type LandPropertyListDto = {
	id: string;
	status: 'Active' | 'Inactive' | 'Sold' | string | number;
};

export type PropertyCadasterLinkDto = {
	id: string;
	cadastralNumber: string;
};

export type ForestStandListDto = {
	id: string;
};

export type ActivityListDto = {
	id: string;
	date: string;
	description?: string;
	activityTypeName?: string;
	userName?: string;
};

export type ActivityChartPoint = {
	label: string;
	count: number;
	x: number;
	y: number;
};
