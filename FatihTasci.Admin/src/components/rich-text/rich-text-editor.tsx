import { useEffect, useRef } from "react";
import { EditorContent, useEditor } from "@tiptap/react";
import { cn } from "@/lib/utils";
import { descriptionExtensions } from "./rich-text-extensions";
import { toEditorHtml } from "./rich-text-format";
import { RichTextToolbar } from "./rich-text-toolbar";
import "./rich-text.css";

interface RichTextEditorProps {
  id: string;
  /** HTML; boş editör "" döner */
  value: string;
  onChange: (html: string) => void;
  placeholder?: string;
  invalid?: boolean;
}

function contentAttributes(id: string, invalid?: boolean): Record<string, string> {
  return {
    id,
    class: "rich-text-content",
    role: "textbox",
    "aria-multiline": "true",
    ...(invalid ? { "aria-invalid": "true" } : {}),
  };
}

/**
 * Araç açıklaması editörü. Yazı alanı vitrindeki gibi beyaz zeminde gösterilir, renkler sitede nasıl
 * görünecekse öyle görünür. Değer yalnız ilk açılışta okunur (form sıfırlanınca aynı HTML gelir).
 */
export function RichTextEditor({ id, value, onChange, placeholder = "", invalid }: RichTextEditorProps) {
  const onChangeRef = useRef(onChange);
  useEffect(() => {
    onChangeRef.current = onChange;
  });

  const editor = useEditor({
    extensions: descriptionExtensions(placeholder),
    content: toEditorHtml(value),
    editorProps: { attributes: contentAttributes(id, invalid) },
    onUpdate: ({ editor: e }) => onChangeRef.current(e.isEmpty ? "" : e.getHTML()),
  });

  useEffect(() => {
    editor?.setOptions({ editorProps: { attributes: contentAttributes(id, invalid) } });
  }, [editor, id, invalid]);

  return (
    <div
      className={cn(
        "overflow-hidden rounded-md border border-input shadow-sm focus-within:ring-1 focus-within:ring-ring",
        invalid && "border-destructive",
      )}
    >
      {editor && <RichTextToolbar editor={editor} />}
      <EditorContent editor={editor} />
    </div>
  );
}
