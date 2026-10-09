import { defineConfig } from "vite";
import { resolve } from 'path';
import { readdirSync } from 'fs';

const htmlDir = resolve(__dirname, 'src/html');
const htmlFiles = readdirSync(htmlDir).filter(f => f.endsWith('.html'));
const input = { main: resolve(__dirname, 'index.html') };

htmlFiles.forEach(file => {
    const name = file.replace('.html', '');
    input[name] = resolve(htmlDir, file);
});

export default defineConfig({   
    base: './', 
    build: {
        outDir: "../AgroEco.UI/Frontend",
        emptyOutDir: true,
        rollupOptions: { input } 
    }
});