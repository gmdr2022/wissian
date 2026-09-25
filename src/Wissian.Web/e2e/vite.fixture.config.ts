import { resolve } from 'node:path'
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  root: resolve(import.meta.dirname, 'fixture'),
  plugins: [react()],
  build: { outDir: resolve(import.meta.dirname, '../../../.local/web-fixture'), emptyOutDir: true, sourcemap: false },
})
