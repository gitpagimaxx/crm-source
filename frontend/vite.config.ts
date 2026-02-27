import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// https://vite.dev/config/
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');
  const apiBase = env.VITE_API_BASE_URL || 'https://localhost:7148';
  console.log('VITE_API_BASE_URL:', apiBase);
  return {
    plugins: [react()],
    server: {
      proxy: {
        '/auth': {
          target: apiBase,
          changeOrigin: true,
          secure: false,
        },
        '/customers': {
          target: apiBase,
          changeOrigin: true,
          secure: false,
        },
      },
    },
  };
});

