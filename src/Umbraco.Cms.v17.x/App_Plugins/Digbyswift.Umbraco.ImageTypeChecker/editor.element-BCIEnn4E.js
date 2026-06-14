import { css as u, html as r } from "@umbraco-cms/backoffice/external/lit";
import { UmbLitElement as n } from "@umbraco-cms/backoffice/lit-element";
import { UmbPropertyValueChangeEvent as i } from "@umbraco-cms/backoffice/property-editor";
import { c as o } from "./bundle.manifests-Ca9qNVXW.js";
const s = [
  { value: 0, label: "Unknown" },
  { value: 1, label: "Photo" },
  { value: 2, label: "Graphic" }
];
class d extends n {
  static properties = {
    value: { attribute: !1 },
    readonly: { type: Boolean, reflect: !0 }
  };
  static styles = u`
    :host {
      display: block;
    }

    .editor {
      display: grid;
      gap: var(--uui-size-space-4);
      max-width: 540px;
    }

    .status {
      align-items: center;
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-3);
    }

    .status-label {
      color: var(--uui-color-text-alt);
      font-size: 13px;
      font-weight: 700;
    }

    .actions {
      display: flex;
      flex-wrap: wrap;
      gap: var(--uui-size-space-2);
    }

    uui-button[selected] {
      --uui-button-border-color: var(--uui-color-selected);
    }
  `;
  #e = null;
  constructor() {
    super(), this.readonly = !1;
  }
  set value(e) {
    const a = this.#e;
    this.#e = this.#a(e), this.requestUpdate("value", a);
  }
  get value() {
    return this.#e;
  }
  #a(e) {
    if (e == null || e === "")
      return null;
    const a = typeof e == "string" ? this.#t(e) : e, l = Number.parseInt(a?.imageType, 10);
    return s.some((t) => t.value === l) ? {
      imageType: l,
      manuallyOverridden: a?.manuallyOverridden === !0
    } : null;
  }
  #t(e) {
    try {
      return JSON.parse(e);
    } catch {
      return null;
    }
  }
  #l(e) {
    return s.find((a) => a.value === e)?.label ?? "Not set";
  }
  #r(e) {
    this.readonly || (this.value = {
      imageType: e,
      manuallyOverridden: !0
    }, this.dispatchEvent(new i()));
  }
  #s() {
    this.readonly || (this.value = null, this.dispatchEvent(new i()));
  }
  render() {
    const e = this.value?.imageType, a = e !== void 0, l = a ? this.#l(e) : "Not set";
    return r`
      <div class="editor">
        <div class="status">
          <span class="status-label">Current analysis</span>
          <uui-tag look=${a ? "primary" : "secondary"}>${l}</uui-tag>
          ${this.value?.manuallyOverridden ? r`<uui-tag look="warning">Manual override</uui-tag>` : ""}
        </div>

        <div class="actions">
          ${s.map((t) => r`
            <uui-button
              compact
              look=${t.value === e && this.value?.manuallyOverridden ? "primary" : "secondary"}
              label=${t.label}
              ?disabled=${this.readonly}
              ?selected=${t.value === e && this.value?.manuallyOverridden}
              @click=${() => this.#r(t.value)}>
              ${t.label}
            </uui-button>
          `)}

          <uui-button
            compact
            look="secondary"
            label="Clear override and reanalyse"
            ?disabled=${this.readonly || !a}
            @click=${this.#s}>
            Clear override and reanalyse
          </uui-button>
        </div>
      </div>
    `;
  }
}
customElements.define(o.dataTypeAlias, d);
export {
  d as element
};
//# sourceMappingURL=editor.element-BCIEnn4E.js.map
