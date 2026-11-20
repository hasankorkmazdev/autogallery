import * as React from "react";
import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { Combobox } from "@/components/ui/combobox";
import { NumberInput } from "@/components/ui/number-input";
import { Switch } from "@/components/ui/switch";
import { Textarea } from "@/components/ui/textarea";
import { RichTextEditor } from "@/components/rich-text/rich-text-editor";
import type { EnumOption } from "@/api/types";

// Etiket + kontrol + açıklama/hata dörtlüsü (CLAUDE.md kural 11 ve 12). InputField'ın diğer
// kontroller için karşılıkları; formlar react-hook-form Controller ile bunları kullanır.

interface BaseFieldProps {
  id: string;
  label: React.ReactNode;
  description?: React.ReactNode;
  error?: React.ReactNode;
  required?: boolean;
}

function FieldFooter({ description, error }: { description?: React.ReactNode; error?: React.ReactNode }) {
  if (error) return <FieldError>{error}</FieldError>;
  if (description) return <FieldDescription>{description}</FieldDescription>;
  return null;
}

export interface SelectOption extends EnumOption {
  /** Seçeneğin solunda gösterilir (örn. marka logosu) */
  icon?: React.ReactNode;
}

interface SelectFieldProps extends BaseFieldProps {
  options: SelectOption[];
  value: number | null | undefined;
  onChange: (value: number | null) => void;
  placeholder?: string;
  disabled?: boolean;
}

export function SelectField({
  id,
  label,
  description,
  error,
  required,
  options,
  value,
  onChange,
  placeholder = "Seçin",
  disabled,
}: SelectFieldProps) {
  return (
    <Field>
      <FieldLabel htmlFor={id} required={required}>
        {label}
      </FieldLabel>
      <Combobox
        id={id}
        value={value === null || value === undefined ? "" : String(value)}
        onValueChange={(v) => onChange(v === "" ? null : Number(v))}
        options={options.map((option) => ({ value: String(option.value), label: option.label, icon: option.icon }))}
        placeholder={placeholder}
        disabled={disabled}
        aria-invalid={error ? true : undefined}
      />
      <FieldFooter description={description} error={error} />
    </Field>
  );
}

export interface TextSelectOption {
  value: string;
  label: string;
  icon?: React.ReactNode;
}

interface TextSelectFieldProps extends BaseFieldProps {
  options: TextSelectOption[];
  value: string | null | undefined;
  onChange: (value: string | null) => void;
  placeholder?: string;
  disabled?: boolean;
}

/** Değeri metin olan seçim kutusu (örn. Guid kimlikli kullanıcılar); sayısal değerler için SelectField. */
export function TextSelectField({
  id,
  label,
  description,
  error,
  required,
  options,
  value,
  onChange,
  placeholder = "Seçin",
  disabled,
}: TextSelectFieldProps) {
  return (
    <Field>
      <FieldLabel htmlFor={id} required={required}>
        {label}
      </FieldLabel>
      <Combobox
        id={id}
        value={value ?? ""}
        onValueChange={(v) => onChange(v === "" ? null : v)}
        options={options}
        placeholder={placeholder}
        disabled={disabled}
        aria-invalid={error ? true : undefined}
      />
      <FieldFooter description={description} error={error} />
    </Field>
  );
}

interface NumberFieldProps extends BaseFieldProps {
  value: number | null | undefined;
  onChange: (value: number | null) => void;
  min?: number;
  max?: number;
  step?: number;
  suffix?: string;
  thousandSeparator?: boolean;
  disabled?: boolean;
  placeholder?: string;
}

export function NumberField({
  id,
  label,
  description,
  error,
  required,
  value,
  onChange,
  min,
  max,
  step,
  suffix,
  thousandSeparator = true,
  disabled,
  placeholder,
}: NumberFieldProps) {
  return (
    <Field>
      <FieldLabel htmlFor={id} required={required}>
        {label}
      </FieldLabel>
      <NumberInput
        id={id}
        value={value ?? undefined}
        onValueChange={(v) => onChange(v ?? null)}
        min={min}
        max={max}
        step={step}
        suffix={suffix}
        thousandSeparator={thousandSeparator ? "." : undefined}
        decimalSeparator=","
        decimalScale={0}
        allowNegative={false}
        disabled={disabled}
        placeholder={placeholder}
        autoComplete="off"
        aria-invalid={error ? true : undefined}
      />
      <FieldFooter description={description} error={error} />
    </Field>
  );
}

interface TextareaFieldProps extends BaseFieldProps, Omit<React.ComponentProps<typeof Textarea>, "id"> {}

export const TextareaField = React.forwardRef<HTMLTextAreaElement, TextareaFieldProps>(
  ({ id, label, description, error, required, ...props }, ref) => (
    <Field>
      <FieldLabel htmlFor={id} required={required}>
        {label}
      </FieldLabel>
      <Textarea ref={ref} id={id} autoComplete="off" aria-invalid={error ? true : undefined} {...props} />
      <FieldFooter description={description} error={error} />
    </Field>
  ),
);
TextareaField.displayName = "TextareaField";

interface RichTextFieldProps extends BaseFieldProps {
  value: string;
  onChange: (html: string) => void;
  placeholder?: string;
}

/** Renk, başlık, hizalama ve liste destekli metin (HTML). Sunucu izinli etiket / sınıf dışını temizler. */
export function RichTextField({ id, label, description, error, required, value, onChange, placeholder }: RichTextFieldProps) {
  return (
    <Field>
      <FieldLabel htmlFor={id} required={required}>
        {label}
      </FieldLabel>
      <RichTextEditor id={id} value={value} onChange={onChange} placeholder={placeholder} invalid={!!error} />
      <FieldFooter description={description} error={error} />
    </Field>
  );
}

interface SwitchFieldProps extends BaseFieldProps {
  checked: boolean;
  onChange: (checked: boolean) => void;
  disabled?: boolean;
}

export function SwitchField({ id, label, description, error, checked, onChange, disabled }: SwitchFieldProps) {
  return (
    <Field>
      <div className="flex items-center justify-between gap-4">
        <FieldLabel htmlFor={id}>{label}</FieldLabel>
        <Switch id={id} checked={checked} onCheckedChange={onChange} disabled={disabled} />
      </div>
      <FieldFooter description={description} error={error} />
    </Field>
  );
}
