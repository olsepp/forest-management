export type LandPropertyListDto = {
	id: string;
	name: string;
	registrationNumber: number;
	county: string;
	parish?: string;
	village?: string;
	boughtDate?: string | null;
	soldDate?: string | null;
	status: 'Active' | 'Inactive' | 'Sold' | number | string;
	companyId?: string;
	companyName?: string;
	cadastralNumbers?: string[];
	cadasters?: PropertyCadasterLinkDto[];
	isFsc?: boolean;
};

export type PropertyCadasterLinkDto = {
	id: string;
	cadastralNumber: string;
};
