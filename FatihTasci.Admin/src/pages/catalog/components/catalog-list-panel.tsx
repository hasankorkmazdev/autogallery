import { Pencil, Plus } from "lucide-react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { ScrollArea } from "@/components/ui/scroll-area";

export interface CatalogListItem {
  id: number;
  name: string;
  slug: string;
  isActive: boolean;
  count?: number;
}

interface CatalogListPanelProps {
  title: string;
  description: string;
  emptyText: string;
  items: CatalogListItem[];
  selectedId?: number | null;
  disabled?: boolean;
  onSelect?: (id: number) => void;
  onAdd: () => void;
  onEdit: (id: number) => void;
}

export function CatalogListPanel({
  title,
  description,
  emptyText,
  items,
  selectedId,
  disabled,
  onSelect,
  onAdd,
  onEdit,
}: CatalogListPanelProps) {
  return (
    <Card className="flex min-h-0 flex-col">
      <CardHeader className="flex flex-row items-start justify-between gap-2">
        <div className="flex flex-col gap-1">
          <CardTitle>{title}</CardTitle>
          <CardDescription>{description}</CardDescription>
        </div>
        <Button size="sm" disabled={disabled} onClick={onAdd}>
          <Plus className="w-4 h-4 mr-1" />
          Ekle
        </Button>
      </CardHeader>
      <CardContent className="min-h-0 flex-1">
        <ScrollArea className="h-[60vh]">
          <div className="flex flex-col gap-1 pr-3">
            {items.length === 0 && <p className="text-sm text-muted-foreground">{emptyText}</p>}
            {items.map((item) => (
              <div key={item.id} className="flex items-center gap-1">
                <Button
                  variant={item.id === selectedId ? "secondary" : "ghost"}
                  size="row"
                  className="flex-1"
                  onClick={() => onSelect?.(item.id)}>
                  <span className="flex flex-1 flex-col items-start">
                    <span>{item.name}</span>
                    <span className="text-xs text-muted-foreground">/{item.slug}</span>
                  </span>
                  {item.count !== undefined && <Badge variant="outline">{item.count}</Badge>}
                  {!item.isActive && <Badge variant="secondary">Pasif</Badge>}
                </Button>
                <Button variant="ghost" size="icon-sm" title="Düzenle" onClick={() => onEdit(item.id)}>
                  <Pencil className="w-4 h-4" />
                </Button>
              </div>
            ))}
          </div>
        </ScrollArea>
      </CardContent>
    </Card>
  );
}
