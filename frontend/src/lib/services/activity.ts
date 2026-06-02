import { apiFetch } from '$lib/utils/api-fetch';
import type { ActivityDto, ActivityUpdateDto } from '$lib/dtos/activity/activity.dto';

type FetchFn = typeof window.fetch;

class ActivityService {
	async getAll(fetchFn?: FetchFn): Promise<ActivityDto[]> {
		return apiFetch('/api/activities', fetchFn);
	}

	async getById(id: string, fetchFn?: FetchFn): Promise<ActivityDto> {
		return apiFetch(`/api/activities/${id}`, fetchFn);
	}

	async getByCompany(companyId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		return apiFetch(`/api/activities/by-company/${companyId}`, fetchFn);
	}

	async getMyByCompany(companyId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		return apiFetch(`/api/activities/by-company/${companyId}/my`, fetchFn);
	}

	async getByCadaster(cadasterId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		return apiFetch(`/api/activities/by-cadaster/${cadasterId}`, fetchFn);
	}

	async getByForestStand(forestStandId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		return apiFetch(`/api/activities/by-foreststand/${forestStandId}`, fetchFn);
	}

	async getByProperty(propertyId: string, fetchFn?: FetchFn): Promise<ActivityDto[]> {
		return apiFetch(`/api/activities/by-property/${propertyId}`, fetchFn);
	}

	async getByCompanyFiltered(
		companyId: string,
		startDate?: string,
		endDate?: string,
		activityTypeId?: string,
		userId?: string,
		fetchFn?: FetchFn
	): Promise<ActivityDto[]> {
		const params = new URLSearchParams();
		if (startDate) params.append('startDate', startDate);
		if (endDate) params.append('endDate', endDate);
		if (activityTypeId) params.append('activityTypeId', activityTypeId);
		if (userId) params.append('userId', userId);
		const qs = params.toString();
		return apiFetch(`/api/activities/by-company/${companyId}/filtered${qs ? `?${qs}` : ''}`, fetchFn);
	}

	async getRecentByUser(
		userId: string,
		count: number = 5,
		companyId?: string,
		fetchFn?: FetchFn
	): Promise<ActivityDto[]> {
		const params = new URLSearchParams();
		params.append('count', String(count));
		if (companyId) params.append('companyId', companyId);
		return apiFetch(`/api/activities/by-user/${userId}/recent?${params.toString()}`, fetchFn);
	}

	async create(activity: unknown): Promise<ActivityDto> {
		return apiFetch('/api/activities', undefined, {
			method: 'POST',
			body: JSON.stringify(activity)
		});
	}

	async update(id: string, activity: ActivityUpdateDto): Promise<ActivityDto> {
		return apiFetch(`/api/activities/${id}`, undefined, {
			method: 'PUT',
			body: JSON.stringify(activity)
		});
	}

	async exportToExcel(
		companyId: string,
		startDate?: string,
		endDate?: string,
		activityTypeId?: string,
		userId?: string
	): Promise<Blob> {
		const params = new URLSearchParams();
		if (startDate) params.append('startDate', startDate);
		if (endDate) params.append('endDate', endDate);
		if (activityTypeId) params.append('activityTypeId', activityTypeId);
		if (userId) params.append('userId', userId);
		const qs = params.toString();
		const url = qs
			? `${import.meta.env.PUBLIC_API_URL}/api/activities/by-company/${companyId}/export?${qs}`
			: `${import.meta.env.PUBLIC_API_URL}/api/activities/by-company/${companyId}/export`;
		const { authService } = await import('./auth');
		const token = await authService.ensureValidToken();
		const response = await fetch(url, {
			headers: { Authorization: `Bearer ${token}` }
		});
		if (!response.ok) {
			throw new Error(`Failed to export activities: ${response.statusText}`);
		}
		return response.blob();
	}

	async delete(id: string): Promise<void> {
		await apiFetch(`/api/activities/${id}`, undefined, { method: 'DELETE' });
	}
}

export const activityService = new ActivityService();
