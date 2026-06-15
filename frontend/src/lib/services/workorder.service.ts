import { apiFetch } from '$lib/utils/api-fetch';
import type {
	WorkOrderDto,
	WorkOrderListDto,
	WorkOrderCreateDto,
	WorkOrderUpdateDto
} from '$lib/dtos/workorder/workorder.dto';

type FetchFn = typeof window.fetch;

class WorkOrderService {
	async getById(id: string, fetchFn?: FetchFn): Promise<WorkOrderDto> {
		return apiFetch(`/api/workorders/${id}`, fetchFn);
	}

	async getByCompany(companyId: string, fetchFn?: FetchFn): Promise<WorkOrderDto[]> {
		return apiFetch(`/api/workorders/by-company/${companyId}`, fetchFn);
	}

	async getMyByCompany(
		companyId: string,
		fetchFn?: FetchFn
	): Promise<WorkOrderListDto[]> {
		return apiFetch(`/api/workorders/by-company/${companyId}/my`, fetchFn);
	}

	async create(dto: WorkOrderCreateDto): Promise<WorkOrderDto> {
		return apiFetch('/api/workorders', undefined, {
			method: 'POST',
			body: JSON.stringify(dto)
		});
	}

	async update(id: string, dto: WorkOrderUpdateDto): Promise<WorkOrderDto> {
		return apiFetch(`/api/workorders/${id}`, undefined, {
			method: 'PUT',
			body: JSON.stringify(dto)
		});
	}

	async complete(id: string): Promise<WorkOrderDto> {
		return apiFetch(`/api/workorders/${id}/complete`, undefined, {
			method: 'POST'
		});
	}

	async revert(id: string): Promise<WorkOrderDto> {
		return apiFetch(`/api/workorders/${id}/revert`, undefined, {
			method: 'POST'
		});
	}

	async delete(id: string): Promise<void> {
		await apiFetch(`/api/workorders/${id}`, undefined, { method: 'DELETE' });
	}
}

export const workOrderService = new WorkOrderService();
