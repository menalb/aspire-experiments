import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig(({ mode }) => {
    const env = loadEnv(mode, process.cwd(), '');
    return {
        plugins: [react()],
        server: {
            port: parseInt(env.PORT),
            proxy: {
                '/api': {
                    target: process.env.services__api__https__0 ||
                        process.env.services__api__http__0,
                    changeOrigin: true,
                    rewrite: (path) => path.replace(/^\/api/, ''),
                    secure: false,
                },
                '/v1/traces': {
                    headers: parseHeaders(process.env['OTEL_EXPORTER_OTLP_HEADERS']),
                    changeOrigin: true,
                    secure: false,
                    target: process.env['ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL'],
                },
            }
        },
        build: {
            outDir: 'dist',
            rollupOptions: {
                input: './index.html'
            }
        }
    }
})

function parseHeaders(s: any) {
    const headers: any[] = s.split(','); // Split by comma
    const result: any = {};

    headers.forEach((header) => {
        const [key, value] = header.split('='); // Split by equal sign
        result[key.trim()] = value.trim(); // Add to the object, trimming spaces
    });

    return result;
}