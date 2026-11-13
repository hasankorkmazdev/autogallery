import * as React from "react";
import { ChevronDown, ChevronUp } from "lucide-react";
import { NumericFormat, type NumericFormatProps } from "react-number-format";

import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { cn } from "@/lib/utils";

export interface NumberInputProps extends Omit<
  NumericFormatProps,
  "value" | "onValueChange"
> {
  value?: number;
  onValueChange?: (value?: number) => void;
}

// Yerleşim class'ları (genişlik, flex, margin) sarmalayıcıya, geri kalanı Input'a gider;
// böylece artı/eksi okları her zaman kutunun kenarında kalır.
const LAYOUT_CLASS =
  /^(?:(?:sm|md|lg|xl|2xl):)*(?:-?m[trblxy]?-|w-|min-w-|max-w-|flex-|grow|shrink|basis-|col-|row-|self-|order-)/;

function splitClassName(className?: string) {
  const tokens = (className ?? "").split(/\s+/).filter(Boolean);
  return {
    wrapper: tokens.filter((t) => LAYOUT_CLASS.test(t)).join(" "),
    input: tokens.filter((t) => !LAYOUT_CLASS.test(t)).join(" "),
  };
}

const NumberInput = React.forwardRef<HTMLInputElement, NumberInputProps>(
  ({ className, value, onValueChange, min, max, step = 1, ...props }, ref) => {
    const [localValue, setLocalValue] = React.useState<number | undefined>(
      value,
    );

    React.useEffect(() => {
      setLocalValue(value);
    }, [value]);

    const handleIncrement = () => {
      const newValue = (localValue ?? 0) + (Number(step) || 1);
      if (max !== undefined && newValue > Number(max)) return;
      setLocalValue(newValue);
      onValueChange?.(newValue);
    };

    const handleDecrement = () => {
      const newValue = (localValue ?? 0) - (Number(step) || 1);
      if (min !== undefined && newValue < Number(min)) return;
      setLocalValue(newValue);
      onValueChange?.(newValue);
    };

    const handleKeyDown = (e: React.KeyboardEvent<HTMLInputElement>) => {
      if (props.disabled || props.readOnly) return;

      if (e.key === "ArrowUp") {
        e.preventDefault();
        handleIncrement();
      } else if (e.key === "ArrowDown") {
        e.preventDefault();
        handleDecrement();
      }
    };

    const { wrapper, input } = splitClassName(className);

    return (
      <div className={cn("relative", wrapper)}>
        <NumericFormat
          value={localValue}
          onValueChange={(values, sourceInfo) => {
            // Ignore updates that come from prop changes to prevent infinite loops
            if (sourceInfo.source === "prop") return;

            const { floatValue } = values;
            if (floatValue !== localValue) {
              setLocalValue(floatValue);
              onValueChange?.(floatValue);
            }
          }}
          min={min}
          max={max}
          getInputRef={ref}
          customInput={Input}
          onKeyDown={handleKeyDown}
          className={cn("pr-10", input)}
          {...props}
        />
        <div className="absolute right-0 top-0 flex h-full flex-col border-l">
          <Button
            type="button"
            variant="ghost"
            size="icon"
            tabIndex={-1}
            className="h-1/2 w-8 rounded-none border-b px-0 hover:bg-muted"
            onClick={handleIncrement}
            disabled={
              props.disabled ||
              (max !== undefined && (localValue ?? 0) >= Number(max))
            }>
            <ChevronUp className="h-3 w-3" />
            <span className="sr-only">Increment</span>
          </Button>
          <Button
            type="button"
            variant="ghost"
            size="icon"
            tabIndex={-1}
            className="h-1/2 w-8 rounded-none px-0 hover:bg-muted"
            onClick={handleDecrement}
            disabled={
              props.disabled ||
              (min !== undefined && (localValue ?? 0) <= Number(min))
            }>
            <ChevronDown className="h-3 w-3" />
            <span className="sr-only">Decrement</span>
          </Button>
        </div>
      </div>
    );
  },
);
NumberInput.displayName = "NumberInput";

export { NumberInput };
