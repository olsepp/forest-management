import { json } from '@sveltejs/kit';
import type { RequestHandler } from './$types';

export const GET: RequestHandler = async ({ url, fetch }) => {
	const katastriNr = url.searchParams.get('katastri_nr')?.trim();
	console.info('[api/forest-stands] GET called', { katastriNr });

	if (!katastriNr) {
		return json({ error: 'Missing katastri_nr query parameter.' }, { status: 400 });
	}

	const wfsUrl =
		`https://gsavalik.envir.ee/geoserver/metsaregister/ows?service=WFS&version=2.0.0` +
		`&request=GetFeature&typeNames=metsaregister:eraldis` +
		`&outputFormat=application/json` +
		`&srsName=EPSG:4326` +
		`&cql_filter=katastri_nr='${katastriNr}'`;

	try {
		const response = await fetch(wfsUrl);
		if (!response.ok) {
			return json({ error: 'Upstream service error' }, { status: 502 });
		}
		const data = await response.json();
		return json(data);
	} catch (e) {
		console.error('[api/forest-stands] upstream fetch failed', e);
		return json({ error: 'Failed to load forest stands from upstream service.' }, { status: 502 });
	}
};
