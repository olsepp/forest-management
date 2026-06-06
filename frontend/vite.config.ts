import tailwindcss from '@tailwindcss/vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { defineConfig } from 'vite';

export default defineConfig({
	plugins: [tailwindcss(), sveltekit()],
	server: {
		proxy: {
			// In development, proxy /api requests to the local backend.
			// In production, nginx handles this routing instead.
			'/api': {
				target: 'http://localhost:5255',
				changeOrigin: true
			}
		}
	}
});
