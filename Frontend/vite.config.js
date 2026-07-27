import { defineConfig } from "vite";
import { resolve } from 'path';

export default defineConfig({   
    base: './', 
    build: {
        outDir: "../AgroEco.UI/Frontend",
        emptyOutDir: true,
        
        rollupOptions: {
            input: {
                main: resolve(__dirname, 'index.html'),
                'crear-cuenta': resolve(__dirname, 'src/html/crear-cuenta.html'),
                dashboard: resolve(__dirname, 'src/html/dasboard.html')
            }
        } 
    }
});