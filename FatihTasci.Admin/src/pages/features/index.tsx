import { useCallback, useEffect, useState } from "react";
import { ListChecks, Pencil, Plus } from "lucide-react";
import { toast } from "sonner";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { catalogService } from "@/api/services/catalog.service";
import type { FeatureGroup } from "@/api/types";
import { FeatureDialog, type FeatureDialogState } from "./components/feature-dialog";

export default function FeaturesPage() {
  const [groups, setGroups] = useState<FeatureGroup[] | null>(null);
  const [dialog, setDialog] = useState<FeatureDialogState | null>(null);

  const load = useCallback(() => {
    catalogService
      .getFeatures(true)
      .then(setGroups)
      .catch((error: Error) => toast.error(error.message));
  }, []);

  useEffect(() => {
    load();
  }, [load]);

  const renderGroups = () => {
    if (!groups) return <PageLoader />;
    return (
      <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
        {groups.map((group) => (
          <Card key={group.category}>
            <CardHeader className="flex flex-row items-center justify-between gap-2">
              <CardTitle>{group.categoryName}</CardTitle>
              <Button size="icon-sm" variant="outline" title="Bu kategoriye ekle" onClick={() => setDialog({ feature: null, category: group.category })}>
                <Plus className="w-4 h-4" />
              </Button>
            </CardHeader>
            <CardContent className="flex flex-col gap-1">
              {group.features.map((feature) => (
                <div key={feature.id} className="flex items-center justify-between gap-2 text-sm">
                  <span className="flex items-center gap-2">
                    {feature.name}
                    {!feature.isActive && <Badge variant="secondary">Pasif</Badge>}
                  </span>
                  <Button variant="ghost" size="icon-sm" title="Düzenle" onClick={() => setDialog({ feature, category: feature.category })}>
                    <Pencil className="w-4 h-4" />
                  </Button>
                </div>
              ))}
            </CardContent>
          </Card>
        ))}
      </div>
    );
  };

  return (
    <PageLayout>
      <PageHeader
        title="Donanımlar"
        description="Araç sihirbazındaki donanım listesi. Dört kategoride yönetilir."
        icon={<ListChecks className="w-8 h-8" />}
        actions={
          <Button onClick={() => setDialog({ feature: null, category: null })}>
            <Plus className="w-4 h-4 mr-2" />
            Donanım ekle
          </Button>
        }
      />
      {renderGroups()}
      <FeatureDialog state={dialog} onOpenChange={(open) => !open && setDialog(null)} onSaved={load} />
    </PageLayout>
  );
}
