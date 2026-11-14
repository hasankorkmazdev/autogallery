import { useState } from "react";
import { Images, Plus } from "lucide-react";
import { toast } from "sonner";
import { Button } from "@/components/ui/button";
import { FieldDescription } from "@/components/ui/field";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { useQuery } from "@/hooks/use-query";
import { showroomService } from "@/api/services/showroom.service";
import type { ShowroomScene } from "@/api/types";
import { DeleteSceneDialog } from "./components/delete-scene-dialog";
import { SceneDialog, type SceneDialogState } from "./components/scene-dialog";
import { SceneGrid } from "./components/scene-grid";

// Sunucudaki ShowroomService.MaxScenes ile aynı.
const MAX_SCENES = 8;

type ScenesAction = () => Promise<{ data: ShowroomScene[]; message: string }>;

export default function ShowroomPage() {
  const query = useQuery("showroom-scenes", showroomService.getAll);
  // İşlemlerden dönen güncel liste; henüz işlem yapılmadıysa sorgu sonucu kullanılır.
  const [changed, setChanged] = useState<ShowroomScene[] | null>(null);
  const [dialog, setDialog] = useState<SceneDialogState | null>(null);
  const [deleting, setDeleting] = useState<ShowroomScene | null>(null);
  const [uploads, setUploads] = useState<Record<number, number>>({});
  const scenes = changed ?? query.data;

  const run = async (action: ScenesAction, quiet = false) => {
    const previous = scenes;
    try {
      const result = await action();
      setChanged(result.data);
      if (!quiet) toast.success(result.message);
    } catch (error) {
      toast.error((error as Error).message);
      setChanged(previous);
    }
  };

  const setProgress = (id: number, percent: number | null) =>
    setUploads((prev) => {
      const next = { ...prev };
      if (percent === null) delete next[id];
      else next[id] = percent;
      return next;
    });

  const upload = async (scene: ShowroomScene, file: File) => {
    setProgress(scene.id, 0);
    await run(() => showroomService.uploadImage(scene.id, file, (p) => setProgress(scene.id, p)));
    setProgress(scene.id, null);
  };

  const reorder = (ordered: ShowroomScene[]) => {
    setChanged(ordered);
    run(() => showroomService.reorder(ordered.map((x) => x.id)), true);
  };

  const visibleCount = scenes?.filter((x) => x.isActive && x.mediumUrl).length ?? 0;
  const full = (scenes?.length ?? 0) >= MAX_SCENES;

  const renderContent = () => {
    if (!scenes) return <PageLoader />;
    if (scenes.length === 0) {
      return <FieldDescription>Henüz sahne yok. “Sahne ekle” ile başlayın; ana sayfadaki bölüm sahne eklenince görünür.</FieldDescription>;
    }
    return (
      <div className="flex flex-col gap-3">
        <FieldDescription>
          Sitede {visibleCount} / {scenes.length} sahne görünüyor. Yalnızca fotoğrafı olan ve “Sitede göster” açık sahneler
          yayınlanır. Kartları sürükleyerek sıralayın. Fotoğraf: yatay, en az 2400 px genişlik önerilir (JPG, PNG, WebP, en fazla 25 MB).
        </FieldDescription>
        <SceneGrid
          scenes={scenes}
          uploads={uploads}
          onReorder={reorder}
          onEdit={(scene) => setDialog({ scene })}
          onUpload={upload}
          onRemoveImage={(scene) => run(() => showroomService.deleteImage(scene.id))}
          onDelete={setDeleting}
        />
      </div>
    );
  };

  return (
    <PageLayout>
      <PageHeader
        title="Showroom"
        description="Ana sayfadaki “Ayrıcalıklı Galeri Atmosferi” bölümünün sahneleri."
        icon={<Images className="w-8 h-8" />}
        actions={
          <Button disabled={!scenes || full} title={full ? `En fazla ${MAX_SCENES} sahne eklenebilir.` : undefined}
            onClick={() => setDialog({ scene: null })}>
            <Plus className="w-4 h-4 mr-2" />
            Sahne ekle
          </Button>
        }
      />
      {renderContent()}
      <SceneDialog state={dialog} onOpenChange={(open) => !open && setDialog(null)} onSaved={setChanged} />
      <DeleteSceneDialog
        scene={deleting}
        onOpenChange={(open) => !open && setDeleting(null)}
        onConfirm={(scene) => run(() => showroomService.remove(scene.id))}
      />
    </PageLayout>
  );
}
