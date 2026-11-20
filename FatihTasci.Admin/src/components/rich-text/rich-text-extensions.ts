import { Extension, Mark, mergeAttributes } from "@tiptap/core";
import StarterKit from "@tiptap/starter-kit";
import { Placeholder } from "@tiptap/extensions";
import {
  alignFromElement,
  colorFromElement,
  textAlignClass,
  textColorClass,
  type TextAlignKey,
  type TextColorKey,
} from "./rich-text-format";

declare module "@tiptap/core" {
  interface Commands<ReturnType> {
    textColor: {
      setTextColor: (color: TextColorKey) => ReturnType;
      unsetTextColor: () => ReturnType;
    };
    textAlign: {
      /** null sola yaslar (varsayılan) */
      setTextAlign: (align: TextAlignKey | null) => ReturnType;
    };
  }
}

/** Paletteki renk: <span class="tc-red">. Paletin dışındaki renkler (yapıştırılan style dahil) atılır. */
const TextColor = Mark.create({
  name: "textColor",

  addAttributes() {
    return {
      color: {
        default: null,
        parseHTML: (element) => colorFromElement(element),
        renderHTML: (attributes) => (attributes.color ? { class: textColorClass(attributes.color) } : {}),
      },
    };
  },

  parseHTML() {
    return [{ tag: "span", getAttrs: (element) => (colorFromElement(element) ? null : false) }];
  },

  renderHTML({ HTMLAttributes }) {
    return ["span", mergeAttributes(HTMLAttributes), 0];
  },

  addCommands() {
    return {
      setTextColor:
        (color) =>
        ({ commands }) =>
          commands.setMark(this.name, { color }),
      unsetTextColor:
        () =>
        ({ commands }) =>
          commands.unsetMark(this.name),
    };
  },
});

const ALIGN_TYPES = ["paragraph", "heading"];

/** Paragraf / başlık hizası: class="ta-center" ya da "ta-right"; sol varsayılandır. */
const TextAlign = Extension.create({
  name: "textAlign",

  addGlobalAttributes() {
    return [
      {
        types: ALIGN_TYPES,
        attributes: {
          textAlign: {
            default: null,
            parseHTML: (element) => alignFromElement(element),
            renderHTML: (attributes) => (attributes.textAlign ? { class: textAlignClass(attributes.textAlign) } : {}),
          },
        },
      },
    ];
  },

  addCommands() {
    return {
      setTextAlign:
        (align) =>
        ({ commands }) =>
          ALIGN_TYPES.map((type) => commands.updateAttributes(type, { textAlign: align })).some(Boolean),
    };
  },
});

/** Sunucunun izin verdiği etiketlerle sınırlı: p, br, strong, em, u, ul, ol, li, h3, h4, span. */
export function descriptionExtensions(placeholder: string) {
  return [
    StarterKit.configure({
      heading: { levels: [3, 4] },
      blockquote: false,
      code: false,
      codeBlock: false,
      horizontalRule: false,
      link: false,
      strike: false,
      // Listeden sonra kendiliğinden boş paragraf eklemesin: sitede fazladan boş satır olur.
      trailingNode: false,
    }),
    TextColor,
    TextAlign,
    Placeholder.configure({ placeholder }),
  ];
}
