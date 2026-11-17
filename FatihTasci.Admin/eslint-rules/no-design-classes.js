// CLAUDE.md kural 18: shadcn bileşenlerine görünüm (design) class'ı verilmez.
// Görünüm tek yerden (components/ui + variant/size) yönetilir; sayfada yalnızca yerleşim class'ı serbesttir.
import fs from "node:fs";
import path from "node:path";

const UI_DIR = path.resolve("src/components/ui");

// components/ui altındaki büyük harfle başlayan tüm export'lar shadcn bileşenidir.
function loadUiComponents() {
  const names = new Set();
  for (const f of fs.readdirSync(UI_DIR)) {
    const src = fs.readFileSync(path.join(UI_DIR, f), "utf8");
    for (const m of src.matchAll(/export\s+(?:const|function)\s+([A-Z]\w*)/g)) names.add(m[1]);
    for (const m of src.matchAll(/export\s*\{([^}]*)\}/g)) {
      for (const part of m[1].split(",")) {
        const n = part.trim().split(/\s+as\s+/).pop();
        if (/^[A-Z]/.test(n)) names.add(n);
      }
    }
  }
  return names;
}

// Kontrol bileşenleri: yalnızca konumlandırma (margin, genişlik-doldurma, flex/grid çocuğu davranışı).
// Boyut/padding/renk vb. için variant veya size kullanılır.
const STRICT = new Set([
  "Button", "Input", "NumberInput", "IPInput", "PhoneInput", "Textarea", "Checkbox", "Switch",
  "Slider", "Progress", "Badge", "SelectTrigger", "DatePicker", "TimePicker", "Label", "FieldLabel",
  "FieldDescription", "FieldError", "Separator", "SidebarMenuButton", "SidebarTrigger",
  "DropdownMenuItem", "SelectItem", "Combobox", "TabsTrigger", "AlertDialogAction", "AlertDialogCancel",
]);

// Alan kontrollerinde sabit genişlik (w-16, w-[150px]) yerleşimdir; Button/Badge boyutunu size belirler.
const WIDTH_OK = new Set([
  "Input", "NumberInput", "IPInput", "PhoneInput", "Textarea", "SelectTrigger", "Combobox", "DatePicker", "TimePicker", "Button",
]);
const WIDTH = [/^w-/, /^min-w-/];

const BREAKPOINT = /^(?:sm|md|lg|xl|2xl|max-sm|max-md|max-lg|max-xl|max-2xl):/;

const POSITIONING = [
  /^pointer-events-(?:none|auto)$/,
  /^overscroll-/,
  /^(?:-?rotate-\d+|opacity-(?:0|100))$/,
  /^(?:isolate|isolation-\w+)$/,
  /^justify-(?:start|end|center|between)$/,
  /^(?:group|peer)(?:\/[\w-]+)?$/,
  /^-?m[trblxy]?-/, /^w-full$/, /^h-full$/, /^min-w-0$/, /^max-w-/, /^flex-(?:1|auto|none|initial|shrink-0|grow|grow-0)$/,
  /^(?:grow|grow-0|shrink|shrink-0)$/, /^basis-/, /^order-/, /^col-/, /^row-/, /^self-/, /^justify-self-/, /^place-self-/,
  /^(?:hidden|block|inline|inline-block|flex|inline-flex|grid|contents)$/,
  /^(?:relative|absolute|fixed|sticky|static)$/, /^-?(?:top|right|bottom|left|inset|inset-x|inset-y|z)-/, /^-?translate-/,
  /^(?:truncate|line-clamp-\d+)$/, /^text-(?:left|center|right|start|end)$/,
];

const LAYOUT_EXTRA = [
  /^text-(?:left|center|right|start|end)$/,
  /^w-/, /^h-/, /^size-/, /^min-w-/, /^min-h-/, /^max-h-/, /^-?p[trblxy]?-/, /^gap(?:-[xy])?-/, /^space-[xy]-/,
  /^flex-(?:row|col|wrap|nowrap|row-reverse|col-reverse)$/, /^items-/, /^justify-/, /^content-/, /^place-/,
  /^grid-cols-/, /^grid-rows-/, /^auto-(?:cols|rows)-/, /^grid-flow-/, /^overflow-/, /^whitespace-/, /^break-/,
  /^aspect-/, /^divide-[xy]$/, /^shrink-0$/, /^(?:invisible|visible|sr-only|pointer-events-none|select-none)$/,
];

function stripPrefix(token) {
  // yalnızca breakpoint öneki yerleşimdir; hover:/dark:/data-[…]:/group-… görünüm sayılır.
  let t = token;
  while (BREAKPOINT.test(t)) t = t.replace(BREAKPOINT, "");
  return t;
}

function isAllowed(token, strict, widthOk) {
  if (/^[a-z0-9-]+:/.test(token) && !BREAKPOINT.test(token)) return false;
  const t = stripPrefix(token);
  if (t.includes(":")) return false;
  const lists = strict ? [...POSITIONING, ...(widthOk ? WIDTH : [])] : [...POSITIONING, ...LAYOUT_EXTRA];
  return lists.some((re) => re.test(t));
}

function collectStrings(node, out) {
  if (!node) return;
  switch (node.type) {
    case "Literal":
      if (typeof node.value === "string") out.push(node.value);
      break;
    case "TemplateLiteral":
      for (const q of node.quasis) out.push(q.value.cooked ?? "");
      node.expressions.forEach((e) => collectStrings(e, out));
      break;
    case "JSXExpressionContainer":
      collectStrings(node.expression, out);
      break;
    case "ConditionalExpression":
      collectStrings(node.consequent, out);
      collectStrings(node.alternate, out);
      break;
    case "LogicalExpression":
      collectStrings(node.left, out);
      collectStrings(node.right, out);
      break;
    case "CallExpression":
      node.arguments.forEach((a) => collectStrings(a, out));
      break;
    case "ObjectExpression":
      node.properties.forEach((p) => p.key && collectStrings(p.key, out));
      break;
    case "ArrayExpression":
      node.elements.forEach((e) => collectStrings(e, out));
      break;
    default:
  }
}

const components = loadUiComponents();

/** Dönüştürme araçları için: bu class bu bileşende serbest mi? */
export function isAllowedClass(token, name) {
  return isAllowed(token, STRICT.has(name), WIDTH_OK.has(name));
}

export default {
  meta: {
    type: "problem",
    messages: {
      design:
        "<{{name}}> bileşenine görünüm class'ı verilmez: {{tokens}}. Görünüm variant/size ile components/ui'dan yönetilir (CLAUDE.md kural 18); yalnızca yerleşim class'ı serbest.",
    },
    schema: [],
  },
  create(context) {
    return {
      JSXOpeningElement(node) {
        if (node.name.type !== "JSXIdentifier") return;
        const name = node.name.name;
        if (!components.has(name)) return;
        const attr = node.attributes.find((a) => a.type === "JSXAttribute" && a.name.name === "className");
        if (!attr || !attr.value) return;
        const strings = [];
        collectStrings(attr.value, strings);
        const strict = STRICT.has(name);
        const bad = [
          ...new Set(
            strings
              .flatMap((s) => s.split(/\s+/))
              .filter(Boolean)
              .filter((t) => !isAllowed(t, strict, WIDTH_OK.has(name))),
          ),
        ];
        if (bad.length === 0) return;
        context.report({
          node: attr,
          messageId: "design",
          data: { name, tokens: bad.slice(0, 6).join(" ") + (bad.length > 6 ? " …" : "") },
        });
      },
    };
  },
};
