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
  			return json({ error: 'Upstream service error' }, { status: 502 });
		}
		const data = await response.json();
		return json(data);
	} catch (e) {
		console.error('[api/cadastral-unit] upstream fetch failed', e);
		return json({ error: 'Failed to load cadastral unit from upstream service.' }, { status: 502 });
	}
};