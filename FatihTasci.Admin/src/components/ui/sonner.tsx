import {
  CircleCheck,
  Info,
  LoaderCircle,
  OctagonX,
  TriangleAlert,
} from "lucide-react";
import { useTheme } from "@/components/theme-provider";
import { Toaster as Sonner, toast as sonnerToast } from "sonner";
import React from "react";

type ToasterProps = React.ComponentProps<typeof Sonner>;

const Toaster = ({ ...props }: ToasterProps) => {
  const { theme = "system" } = useTheme();

  return (
    <Sonner
      theme={theme as ToasterProps["theme"]}
      className="toaster group"
      duration={15000}
      style={{
        ...props.style,
        "--toast-duration": `${props.duration ?? 15000}ms`,
      } as React.CSSProperties}
      icons={{
        success: <CircleCheck className="h-4 w-4" />,
        info: <Info className="h-4 w-4" />,
        warning: <TriangleAlert className="h-4 w-4" />,
        error: <OctagonX className="h-4 w-4" />,
        loading: <LoaderCircle className="h-4 w-4 animate-spin" />,
      }}
      toastOptions={{
        classNames: {
          toast:
            "group toast group-[.toaster]:bg-background group-[.toaster]:text-foreground group-[.toaster]:border-border group-[.toaster]:shadow-lg relative overflow-hidden",
          description: "group-[.toast]:text-foreground/75",
          actionButton:
            "group-[.toast]:bg-primary group-[.toast]:text-primary-foreground",
          cancelButton:
            "group-[.toast]:bg-muted group-[.toast]:text-muted-foreground",
        },
      }}
      {...props}
    />
  );
};

const toast = {
  ...sonnerToast,
  success: (message: string | React.ReactNode, data?: any) => {
    return sonnerToast.success(
      <div className="flex flex-col gap-0.5">
        <div className="font-bold text-[13px]">Başarılı</div>
        <div className="text-foreground/75 text-xs leading-tight">
          {message}
        </div>
      </div>,
      data,
    );
  },
  error: (message: string | React.ReactNode, data?: any) => {
    return sonnerToast.error(
      <div className="flex flex-col gap-0.5">
        <div className="font-bold text-[13px]">Hata</div>
        <div className="text-foreground/75 text-xs leading-tight">
          {message}
        </div>
      </div>,
      data,
    );
  },
  warning: (message: string | React.ReactNode, data?: any) => {
    return sonnerToast.warning(
      <div className="flex flex-col gap-0.5">
        <div className="font-bold text-[13px]">Uyarı</div>
        <div className="text-foreground/75 text-xs leading-tight">
          {message}
        </div>
      </div>,
      data,
    );
  },
  info: (message: string | React.ReactNode, data?: any) => {
    return sonnerToast.info(
      <div className="flex flex-col gap-0.5">
        <div className="font-bold text-[13px]">Bilgi</div>
        <div className="text-foreground/75 text-xs leading-tight">
          {message}
        </div>
      </div>,
      data,
    );
  },
};

export { Toaster, toast };
