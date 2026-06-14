import * as fs from 'node:fs';
import consts from './vite-consts';

export function postBuildCopyToPackage() {
    return {
        name: 'copy-to-package',
        closeBundle() {
            const assetsPath = consts.clientPluginPath + '/assets';

            if (!fs.existsSync(assetsPath)) {
                return;
            }

            fs.rmSync(
                consts.nugetStaticAssetsPath + '/assets', {
                recursive: true,
                force: true
            });
            fs.cpSync(
                assetsPath,
                consts.nugetStaticAssetsPath + '/assets',
                {
                    recursive: true,
                    filter: source => !source.endsWith('.ts')
                }
            );
        }
    };
}

export function postBuildCopyToUmbraco() {
    return {
        name: 'copy-to-umbraco',
        closeBundle() {
            fs.rmSync(
                consts.umbracoPluginPath, {
                recursive: true,
                force: true
            });
            fs.cpSync(
                consts.nugetStaticAssetsPath,
                consts.umbracoPluginPath,
                {
                    recursive: true,
                    filter: source => !source.endsWith('.ts')
                }
            );
        }
    };
}
