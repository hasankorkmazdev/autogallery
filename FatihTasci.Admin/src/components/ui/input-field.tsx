import * as React from "react";

import {
  Field,
  FieldDescription,
  FieldError,
  FieldLabel,
} from "@/components/ui/field";
import { Input } from "@/components/ui/input";
import { PasswordInput } from "@/components/ui/password-input";

interface InputFieldProps extends React.ComponentProps<typeof Input> {
  id: string;
  label: React.ReactNode;
  /** Yardımcı metin; hata varsa yerine hata gösterilir. */
  description?: React.ReactNode;
  error?: React.ReactNode;
  /** Etiketin sağında gösterilir (örn. "Şifremi unuttum" bağlantısı). */
  labelAction?: React.ReactNode;
  /** Şifre alanı: göster/gizle butonlu PasswordInput kullanır. */
  password?: boolean;
}

// Etiket + input + açıklama/hata dörtlüsü (CLAUDE.md kural 11 ve 12); formlar bunu kullanır.
const InputField = React.forwardRef<HTMLInputElement, InputFieldProps>(
  ({ id, label, description, error, labelAction, password, required, ...props }, ref) => {
    const Control = password ? PasswordInput : Input;

    return (
      <Field>
        <div className="flex items-center">
          <FieldLabel htmlFor={id}>
            {label}
          </FieldLabel>
          {labelAction && <div className="ml-auto">{labelAction}</div>}
        </div>
        <Control
          ref={ref}
          id={id}
          required={required}
          aria-invalid={error ? true : undefined}
          {...props}
        />
        {error ? (
          <FieldError>{error}</FieldError>
        ) : (
          description && <FieldDescription>{description}</FieldDescription>
        )}
      </Field>
    );
  },
);
InputField.displayName = "InputField";

export { InputField };
