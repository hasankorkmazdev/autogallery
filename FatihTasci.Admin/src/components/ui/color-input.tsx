interface ColorInputProps {
  id?: string;
  value: string;
  onChange: (value: string) => void;
}

// Renk örneği + hex değeri; tarayıcının renk seçicisini standart bir kontrol olarak sunar.
function ColorInput({ id, value, onChange }: ColorInputProps) {
  return (
    <div className="flex h-8 items-center gap-3 rounded-md border border-input bg-transparent px-3 shadow-sm">
      <span className="relative size-5 shrink-0 overflow-hidden rounded-full border">
        <input
          id={id}
          type="color"
          value={value}
          autoComplete="off"
          onChange={(e) => onChange(e.target.value)}
          className="absolute inset-[-8px] size-[200%] cursor-pointer border-0 p-0"
        />
      </span>
      <span className="font-mono text-sm uppercase text-muted-foreground">
        {value}
      </span>
    </div>
  );
}

export { ColorInput };
