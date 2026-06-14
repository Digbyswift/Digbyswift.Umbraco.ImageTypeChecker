import { css, html } from '@umbraco-cms/backoffice/external/lit';
import { UmbLitElement } from '@umbraco-cms/backoffice/lit-element';
import { UmbPropertyValueChangeEvent } from '@umbraco-cms/backoffice/property-editor';
import consts from '../../vite-consts';

const IMAGE_TYPES = [
  { value: 0, label: 'Unknown' },
  { value: 1, label: 'Photo' },
  { value: 2, label: 'Graphic' }
];

class DigbyswiftImageTypeCheckerElement extends UmbLitElement {
  static properties = {
    value: { attribute: false },
    readonly: { type: Boolean, reflect: true }
  };

  static styles = css`
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

  #value = null;

  constructor() {
    super();
    this.readonly = false;
  }

  set value(value) {
    const oldValue = this.#value;
    this.#value = this.#normalizeValue(value);
    this.requestUpdate('value', oldValue);
  }

  get value() {
    return this.#value;
  }

  #normalizeValue(value) {
    if (value === null || value === undefined || value === '') {
      return null;
    }

    const parsedValue = typeof value === 'string' ? this.#parseValue(value) : value;
    const imageType = Number.parseInt(parsedValue?.imageType, 10);

    if (!IMAGE_TYPES.some((option) => option.value === imageType)) {
      return null;
    }

    return {
      imageType,
      manuallyOverridden: parsedValue?.manuallyOverridden === true
    };
  }

  #parseValue(value) {
    try {
      return JSON.parse(value);
    } catch {
      return null;
    }
  }

  #getImageTypeLabel(value) {
    return IMAGE_TYPES.find((option) => option.value === value)?.label ?? 'Not set';
  }

  #setImageType(imageType) {
    if (this.readonly) {
      return;
    }

    this.value = {
      imageType,
      manuallyOverridden: true
    };

    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  #clearValue() {
    if (this.readonly) {
      return;
    }

    this.value = null;
    this.dispatchEvent(new UmbPropertyValueChangeEvent());
  }

  render() {
    const currentImageType = this.value?.imageType;
    const hasValue = currentImageType !== undefined;
    const status = hasValue ? this.#getImageTypeLabel(currentImageType) : 'Not set';

    return html`
      <div class="editor">
        <div class="status">
          <span class="status-label">Current analysis</span>
          <uui-tag look=${hasValue ? 'primary' : 'secondary'}>${status}</uui-tag>
          ${this.value?.manuallyOverridden ? html`<uui-tag look="warning">Manual override</uui-tag>` : ''}
        </div>

        <div class="actions">
          ${IMAGE_TYPES.map((option) => html`
            <uui-button
              compact
              look=${option.value === currentImageType && this.value?.manuallyOverridden ? 'primary' : 'secondary'}
              label=${option.label}
              ?disabled=${this.readonly}
              ?selected=${option.value === currentImageType && this.value?.manuallyOverridden}
              @click=${() => this.#setImageType(option.value)}>
              ${option.label}
            </uui-button>
          `)}

          <uui-button
            compact
            look="secondary"
            label="Clear override and reanalyse"
            ?disabled=${this.readonly || !hasValue}
            @click=${this.#clearValue}>
            Clear override and reanalyse
          </uui-button>
        </div>
      </div>
    `;
  }
}

customElements.define(consts.dataTypeAlias, DigbyswiftImageTypeCheckerElement);

export { DigbyswiftImageTypeCheckerElement as element };
