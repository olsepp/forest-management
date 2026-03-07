import type { RequestHandler } from './$types';

const sleep = (ms: number) => new Promise((resolve) => setTimeout(resolve, ms));

async function fetchTileWithRetry(wmsUrl: string, retries = 2): Promise<Response> {
	for (let attempt = 0; attempt <= retries; attempt++) {
		try {
			return await globalThis.fetch(wmsUrl, {
				signal: AbortSignal.timeout(10_000)
			});
		} catch (error) {
			if (attempt === retries) throw error;
			await sleep(200 * (attempt + 1));
		}
	}

	throw new Error('Tile fetch retry loop exited unexpectedly');
}

export const GET: RequestHandler = async ({ url }) => {
	const params = url.searchParams.toString();
	const wmsUrl = `https://kaart.maaamet.ee/wms/alus-geo?${params}`;

	try {
		const response = await fetchTileWithRetry(wmsUrl);

		const contentType = response.headers.get('Content-Type');
		console.log('[map-tiles] status:', response.status, 'content-type:', contentType);

		if (!response.ok || contentType?.includes('text') || contentType?.includes('xml')) {
			const text = await response.text();
			console.error('[map-tiles] upstream non-image response:', text.slice(0, 1000));
			return new Response(null, { status: 502 });
		}

		const imageBuffer = await response.arrayBuffer();

		return new Response(imageBuffer, {
			headers: {
				'Content-Type': contentType ?? 'image/png',
				'Cache-Control': 'public, max-age=86400'
			}
		});
	} catch (error) {
		console.error('[map-tiles] fetch failed after retries', error);
		return new Response(null, { status: 502 });
	}
};
