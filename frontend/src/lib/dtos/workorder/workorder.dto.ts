export type WorkOrderStatus = 0 | 1; // 0 = Sent, 1 = Completed

export type WorkOrderCreateDto = {
	assignedToId: string;
	activityTypeId: string;
	cadasterId: string;
	forestStandId: string | null;
	quantity: number;
	unit: string | null;
	notes: string | null;
};

export type WorkOrderUpdateDto = {
	id: string;
	assignedToId: string;
	activityTypeId: string;
	cadasterId: string;
	forestStandId: string | null;
	quantity: number;
	unit: string | null;
	notes: string | null;
};

export type WorkOrderDto = {
	id: string;
	assignedToId: string;
	assignedToUserName: string;
	assignedById: string;
	assignedByUserName: string;
	activityTypeId: string;
	activityTypeName: string;
	forestStandId: string | null;
	forestStandNumber: number | null;
	cadasterId: string;
	cadasterCadastralNumber: string;
	status: WorkOrderStatus;
	quantity: number;
	unit: string | null;
	notes: string | null;
	createdAt: string;
};

export type WorkOrderListDto = {
	id: string;
	assignedToUserName: string;
	activityTypeName: string;
	cadasterCadastralNumber: string;
	cadasterId: string;
	forestStandNumber: number | null;
	forestStandId: string | null;
	status: WorkOrderStatus;
	quantity: number;
	unit: string | null;
	createdAt: string;
};
