import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
} from "@/components/ui/alert-dialog";
import type { Delivery } from "@/api/types";

interface DeleteDeliveryDialogProps {
  delivery: Delivery | null;
  onOpenChange: (open: boolean) => void;
  onConfirm: (delivery: Delivery) => void;
}

export function DeleteDeliveryDialog({ delivery, onOpenChange, onConfirm }: DeleteDeliveryDialogProps) {
  return (
    <AlertDialog open={delivery !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Teslimat silinsin mi?</AlertDialogTitle>
          <AlertDialogDescription>
            “{delivery?.vehicleTitle}” teslimatı ve fotoğrafı silinir, sitede artık görünmez. Yalnızca gizlemek için
            düzenleyip “Sitede göster”i kapatabilirsiniz.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction onClick={() => delivery && onConfirm(delivery)}>Sil</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
