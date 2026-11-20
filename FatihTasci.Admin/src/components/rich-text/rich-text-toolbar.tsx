import { useState, type ReactNode } from "react";
import { useEditorState, type Editor } from "@tiptap/react";
import {
  Baseline,
  Bold,
  Heading1,
  Heading2,
  Italic,
  List,
  ListOrdered,
  Redo2,
  RemoveFormatting,
  TextAlignCenter,
  TextAlignEnd,
  TextAlignStart,
  Underline,
  Undo2,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { cn } from "@/lib/utils";
import { TEXT_COLORS, textColorClass, type TextAlignKey, type TextColorKey } from "./rich-text-format";

interface ToolButtonProps {
  label: string;
  active?: boolean;
  disabled?: boolean;
  onClick: () => void;
  children: ReactNode;
}

function ToolButton({ label, active, disabled, onClick, children }: ToolButtonProps) {
  return (
    <Button
      type="button"
      variant={active ? "secondary" : "ghost"}
      size="icon-sm"
      title={label}
      aria-label={label}
      aria-pressed={active}
      disabled={disabled}
      onClick={onClick}
    >
      {children}
    </Button>
  );
}

function ToolSeparator() {
  return <span aria-hidden className="mx-1 h-5 w-px bg-border" />;
}

function activeAlign(editor: Editor): TextAlignKey | null {
  if (editor.isActive({ textAlign: "center" })) return "center";
  if (editor.isActive({ textAlign: "right" })) return "right";
  return null;
}

function ColorPicker({ editor, current }: { editor: Editor; current: TextColorKey | null }) {
  const [open, setOpen] = useState(false);

  const apply = (key: TextColorKey | null) => {
    const chain = editor.chain().focus();
    if (key) chain.setTextColor(key).run();
    else chain.unsetTextColor().run();
    setOpen(false);
  };

  return (
    <Popover open={open} onOpenChange={setOpen}>
      <PopoverTrigger asChild>
        <Button type="button" variant={current ? "secondary" : "ghost"} size="icon-sm" title="Yazı rengi" aria-label="Yazı rengi">
          <Baseline className={cn(current && textColorClass(current))} />
        </Button>
      </PopoverTrigger>
      {/* Kapanınca odak düğmeye değil editöre döner (apply odağı editöre verir). */}
      <PopoverContent align="start" className="grid w-44 gap-0.5 p-1" onCloseAutoFocus={(e) => e.preventDefault()}>
        <Button type="button" variant={current ? "ghost" : "secondary"} size="sm" className="w-full justify-start" onClick={() => apply(null)}>
          <span aria-hidden className="size-4 rounded-full border bg-background" />
          Varsayılan
        </Button>
        {TEXT_COLORS.map((color) => (
          <Button
            key={color.key}
            type="button"
            variant={current === color.key ? "secondary" : "ghost"}
            size="sm"
            className="w-full justify-start"
            onClick={() => apply(color.key)}
          >
            <span aria-hidden className={cn("size-4 rounded-full bg-current", textColorClass(color.key))} />
            {color.label}
          </Button>
        ))}
      </PopoverContent>
    </Popover>
  );
}

export function RichTextToolbar({ editor }: { editor: Editor }) {
  const state = useEditorState({
    editor,
    selector: ({ editor: e }) => ({
      bold: e.isActive("bold"),
      italic: e.isActive("italic"),
      underline: e.isActive("underline"),
      large: e.isActive("heading", { level: 3 }),
      heading: e.isActive("heading", { level: 4 }),
      bulletList: e.isActive("bulletList"),
      orderedList: e.isActive("orderedList"),
      align: activeAlign(e),
      color: (e.getAttributes("textColor").color as TextColorKey | undefined) ?? null,
      canUndo: e.can().undo(),
      canRedo: e.can().redo(),
    }),
  });

  const run = () => editor.chain().focus();

  return (
    <div role="toolbar" aria-label="Biçimlendirme" className="flex flex-wrap items-center gap-0.5 border-b bg-muted/50 p-1">
      <ToolButton label="Kalın (Ctrl+B)" active={state.bold} onClick={() => run().toggleBold().run()}>
        <Bold />
      </ToolButton>
      <ToolButton label="İtalik (Ctrl+I)" active={state.italic} onClick={() => run().toggleItalic().run()}>
        <Italic />
      </ToolButton>
      <ToolButton label="Altı çizili (Ctrl+U)" active={state.underline} onClick={() => run().toggleUnderline().run()}>
        <Underline />
      </ToolButton>
      <ColorPicker editor={editor} current={state.color} />
      <ToolSeparator />
      <ToolButton label="Büyük başlık" active={state.large} onClick={() => run().toggleHeading({ level: 3 }).run()}>
        <Heading1 />
      </ToolButton>
      <ToolButton label="Başlık" active={state.heading} onClick={() => run().toggleHeading({ level: 4 }).run()}>
        <Heading2 />
      </ToolButton>
      <ToolSeparator />
      <ToolButton label="Sola yasla" active={state.align === null} onClick={() => run().setTextAlign(null).run()}>
        <TextAlignStart />
      </ToolButton>
      <ToolButton label="Ortala" active={state.align === "center"} onClick={() => run().setTextAlign("center").run()}>
        <TextAlignCenter />
      </ToolButton>
      <ToolButton label="Sağa yasla" active={state.align === "right"} onClick={() => run().setTextAlign("right").run()}>
        <TextAlignEnd />
      </ToolButton>
      <ToolSeparator />
      <ToolButton label="Madde listesi" active={state.bulletList} onClick={() => run().toggleBulletList().run()}>
        <List />
      </ToolButton>
      <ToolButton label="Numaralı liste" active={state.orderedList} onClick={() => run().toggleOrderedList().run()}>
        <ListOrdered />
      </ToolButton>
      <ToolSeparator />
      <ToolButton label="Biçimi temizle" onClick={() => run().unsetAllMarks().clearNodes().setTextAlign(null).run()}>
        <RemoveFormatting />
      </ToolButton>
      <ToolButton label="Geri al (Ctrl+Z)" disabled={!state.canUndo} onClick={() => run().undo().run()}>
        <Undo2 />
      </ToolButton>
      <ToolButton label="Yinele (Ctrl+Y)" disabled={!state.canRedo} onClick={() => run().redo().run()}>
        <Redo2 />
      </ToolButton>
    </div>
  );
}
