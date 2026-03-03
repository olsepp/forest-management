import { PUBLIC_API_URL } from '$env/static/public';
import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import type { CompanyListDto } from '$lib/types/company';

const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

export const load: PageServerLoad = async ({ fetch }) => {
	const response = await fetch(`${apiBaseUrl}/api/companies`);

	if (!response.ok) {
		throw error(response.status, 'Failed to load companies');
	}

	const companies: CompanyListDto[] = await response.json();

	return {
		companies
	};
};

