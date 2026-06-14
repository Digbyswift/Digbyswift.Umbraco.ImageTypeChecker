import { defineConfig } from 'vite';
import { postBuildCopyToPackage, postBuildCopyToUmbraco } from './vite-plugins';
import consts from "./vite-consts";

export default defineConfig({
    plugins: [
        postBuildCopyToPackage(),
        postBuildCopyToUmbraco()
    ],
    build: {
        lib: {
            entry: consts.clientPluginPath + '/bundle.manifests.ts',
            formats: ['es'],
            fileName: 'manifests'
        },
        outDir: consts.nugetStaticAssetsPath,
        emptyOutDir: true,
        sourcemap: true,
        rollupOptions: {
            external: [/^@umbraco-cms/]
        }
    }
});
