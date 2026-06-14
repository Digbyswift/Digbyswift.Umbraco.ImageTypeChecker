const projectNamespace = 'Digbyswift.Umbraco.ImageTypeChecker';

export default {
    projectNamespace,
    dataTypeName: 'Image Type Checker',
    dataTypeAlias: 'digbyswift-image-type-checker',
    clientPluginPath: './src',
    nugetStaticAssetsPath: `../${projectNamespace}/wwwroot/App_Plugins/${projectNamespace}`,
    umbracoPluginPath: `../Umbraco.Cms.v17.x/App_Plugins/${projectNamespace}`
}