import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';

export const GET: RequestHandler = async ({ url, fetch }) => {
	const tunnus = url.searchParams.get('tunnus')?.trim();
	console.info('[api/cadastral-unit] GET called', { tunnus });

	if (!tunnus) {
		return json({ error: 'Missing tunnus query parameter.' }, { status: 400 });
	}

	const wfsUrl =
		`https://gsavalik.envir.ee/geoserver/kataster/wfs?service=WFS&version=2.0.0` +
		`&request=GetFeature&typeNames=kataster:ky_kehtiv` +
		`&outputFormat=application/json` +
		`&srsName=EPSG:4326` +
		`&CQL_FILTER=tunnus='${tunnus}'`;

	try {
		const response = await fetch(wfsUrl);

		if (!response.ok) {
			const errorText = await response.text();
			console.error('[api/cadastral-unit] upstream error', errorText);
			return json({ error: `WFS request failed with status ${response.status}.` }, { status: response.status });
		}

		const data = await response.json();
		console.info('[api/cadastral-unit] features count', data?.features?.length ?? 0);
		return json(data);
	} catch (e) {
		console.error('[api/cadastral-unit] upstream fetch failed', e);
		return json({ error: 'Failed to load cadastral unit from upstream service.' }, { status: 502 });
	}
};