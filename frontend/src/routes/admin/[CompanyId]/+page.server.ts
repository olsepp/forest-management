import { PUBLIC_API_URL } from '$env/static/public';
import { error } from '@sveltejs/kit';
import type { PageServerLoad } from './$types';
import type { CompanyDto } from '$lib/types/company';

const apiBaseUrl = PUBLIC_API_URL || 'http://localhost:5255';

export const load: PageServerLoad = async ({ fetch, params }) => {
	const response = await fetch(`${apiBaseUrl}/api/companies/${params.CompanyId}`);

	if (!response.ok) {
		throw error(response.status, 'Failed to load company');
	}

	const company: CompanyDto = await response.json();

	return {
		company
	};
};

