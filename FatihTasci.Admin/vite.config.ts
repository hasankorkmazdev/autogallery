import path from "path";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// Panel Server üzerinden /panel altında sunulur. Geliştirmede /api, /media ve /images (marka logoları)
// istekleri Vite proxy'si ile Server'a (http://localhost:5080) gider, CORS gerekmez.
const serverUrl = process.env.VITE_SERVER_URL ?? "http://localhost:5080";

export default defineConfig({
  base: "/panel/",
  plugins: [react()],
  server: {
    // Sabit port: 5173 doluysa başka porta kaymaz, hata verir (CLAUDE.md "Komutlar").
    port: 5173,
    strictPort: true,
    proxy: {
      "/api": { target: serverUrl, changeOrigin: true },
      "/media": { target: serverUrl, changeOrigin: true },
      "/images": { target: serverUrl, changeOrigin: true },
    },
  },
  build: {
    outDir: path.resolve(__dirname, "../FatihTasci.Server/wwwroot/panel"),
    emptyOutDir: true,
  },
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
});
