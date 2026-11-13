import path from "path";
import react from "@vitejs/plugin-react";
import { defineConfig } from "vite";

// Panel Server üzerinden /panel altında sunulur. Geliştirmede /api ve /media istekleri
// Vite proxy'si ile Server'a (http://localhost:5080) gider, CORS gerekmez.
const serverUrl = process.env.VITE_SERVER_URL ?? "http://localhost:5080";

export default defineConfig({
  base: "/panel/",
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": { target: serverUrl, changeOrigin: true },
      "/media": { target: serverUrl, changeOrigin: true },
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
