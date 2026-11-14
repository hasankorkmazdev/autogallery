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
import type { ShowroomScene } from "@/api/types";

interface DeleteSceneDialogProps {
  scene: ShowroomScene | null;
  onOpenChange: (open: boolean) => void;
  onConfirm: (scene: ShowroomScene) => void;
}

export function DeleteSceneDialog({ scene, onOpenChange, onConfirm }: DeleteSceneDialogProps) {
  return (
    <AlertDialog open={scene !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogHeader>
          <AlertDialogTitle>Sahne silinsin mi?</AlertDialogTitle>
          <AlertDialogDescription>
            “{scene?.title}” sahnesi ve fotoğrafı silinir, sitede artık görünmez. Yalnızca gizlemek için düzenleyip
            “Sitede göster”i kapatabilirsiniz.
          </AlertDialogDescription>
        </AlertDialogHeader>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction onClick={() => scene && onConfirm(scene)}>Sil</AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  );
}
