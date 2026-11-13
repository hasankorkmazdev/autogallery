import * as React from "react"
import * as AvatarPrimitive from "@radix-ui/react-avatar"
import { createContext, useContext } from "react"

import { cn } from "@/lib/utils"

type AvatarShape = "circle" | "rounded"

const AvatarShapeContext = createContext<AvatarShape>("circle")

const shapeClass: Record<AvatarShape, string> = {
  circle: "rounded-full",
  rounded: "rounded-lg",
}

const Avatar = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Root>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Root> & {
    shape?: AvatarShape
  }
>(({ className, shape = "circle", ...props }, ref) => (
  <AvatarShapeContext.Provider value={shape}>
    <AvatarPrimitive.Root
      ref={ref}
      className={cn(
        "relative flex h-10 w-10 shrink-0 overflow-hidden",
        shapeClass[shape],
        className
      )}
      {...props}
    />
  </AvatarShapeContext.Provider>
))
Avatar.displayName = AvatarPrimitive.Root.displayName

const AvatarImage = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Image>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Image>
>(({ className, ...props }, ref) => (
  <AvatarPrimitive.Image
    ref={ref}
    className={cn("aspect-square h-full w-full", className)}
    {...props}
  />
))
AvatarImage.displayName = AvatarPrimitive.Image.displayName

const AvatarFallback = React.forwardRef<
  React.ElementRef<typeof AvatarPrimitive.Fallback>,
  React.ComponentPropsWithoutRef<typeof AvatarPrimitive.Fallback>
>(({ className, ...props }, ref) => {
  const shape = useContext(AvatarShapeContext)
  return (
    <AvatarPrimitive.Fallback
      ref={ref}
      className={cn(
        "flex h-full w-full items-center justify-center bg-muted",
        shapeClass[shape],
        className
      )}
      {...props}
    />
  )
})
AvatarFallback.displayName = AvatarPrimitive.Fallback.displayName

export { Avatar, AvatarImage, AvatarFallback }
