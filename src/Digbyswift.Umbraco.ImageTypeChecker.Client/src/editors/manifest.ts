import consts from '../../vite-consts';

export const manifests: Array<UmbExtensionManifest> = [
    {
        type: "propertyEditorUi",
        name: `${consts.dataTypeName} Ui`,
        alias: `${consts.projectNamespace}.Ui`,
        elementName: consts.dataTypeAlias,
        js: () => import("./editor.element.js"),
        meta: {
            label: consts.dataTypeName,
            propertyEditorSchemaAlias: consts.projectNamespace,
            icon: "icon-picture",
            group: "media",
            supportsReadOnly: true,
        }
    },
];
