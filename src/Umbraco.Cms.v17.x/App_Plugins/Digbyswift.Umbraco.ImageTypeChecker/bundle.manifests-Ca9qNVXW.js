const a = "Digbyswift.Umbraco.ImageTypeChecker", e = {
  projectNamespace: a,
  dataTypeName: "Image Type Checker",
  dataTypeAlias: "digbyswift-image-type-checker"
}, t = [
  {
    type: "propertyEditorUi",
    name: `${e.dataTypeName} Ui`,
    alias: `${e.projectNamespace}.Ui`,
    elementName: e.dataTypeAlias,
    js: () => import("./editor.element-BCIEnn4E.js"),
    meta: {
      label: e.dataTypeName,
      propertyEditorSchemaAlias: e.projectNamespace,
      icon: "icon-picture",
      group: "media",
      supportsReadOnly: !0
    }
  }
], p = [
  ...t
];
export {
  e as c,
  p as m
};
//# sourceMappingURL=bundle.manifests-Ca9qNVXW.js.map
