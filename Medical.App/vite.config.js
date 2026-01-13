import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    host: '0.0.0.0', // 允许通过 IP 地址访问
    port: 5173,
    open: true,
    strictPort: false // 如果端口被占用，自动尝试下一个可用端口
  }
});
