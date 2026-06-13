import tailwindcss from '@tailwindcss/vite';
import { sveltekit } from '@sveltejs/kit/vite';
import { SvelteKitPWA } from '@vite-pwa/sveltekit';
import { defineConfig } from 'vite';

export default defineConfig({
	plugins: [
		tailwindcss(),
		sveltekit(),
		SvelteKitPWA({
			registerType: 'autoUpdate',
			injectRegister: 'auto',
			workbox: {
				globPatterns: ['client/**/*.{js,css,html,ico,png,svg,webp,woff2}']
			},
			manifest: {
				name: 'Metsalo',
				short_name: 'Metsalo',
				description: 'Metsanduslike tööde logimise ja haldamise tööriist',
				theme_color: '#ffffff',
				background_color: '#ffffff',
				display: 'standalone',
				start_url: '/sign-in',
				scope: '/',
				id: '/',
				lang: 'et',
				icons: [
					{
        				src: '/app_icon_192x192.png',
        				sizes: '192x192',
        				type: 'image/png'
    				},
					{
						src: '/app_icon.png',
						sizes: '512x512',
						type: 'image/png',
						purpose: 'any'
					},
					{
						src: '/app_icon.png',
						sizes: '512x512',
						type: 'image/png',
						purpose: 'maskable'
					}
				]
			}
		})
	],
	server: {
		proxy: {
			'/api': {
				target: 'http://localhost:5255',
				changeOrigin: true
			}
		}
	}
});
