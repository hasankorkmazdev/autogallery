import { Button } from "@/components/ui/button";

interface ListPaginationProps {
  pageIndex: number;
  totalPages: number;
  totalCount: number;
  onPageChange: (pageIndex: number) => void;
}

export function ListPagination({ pageIndex, totalPages, totalCount, onPageChange }: ListPaginationProps) {
  return (
    <div className="flex flex-col gap-2 pt-4 md:flex-row md:items-center md:justify-between">
      <span className="text-sm text-muted-foreground">Toplam {totalCount} kayıt</span>
      <div className="flex items-center gap-2">
        <Button variant="outline" size="sm" disabled={pageIndex === 0} onClick={() => onPageChange(pageIndex - 1)}>
          Önceki
        </Button>
        <span className="text-sm">
          Sayfa {Math.min(pageIndex + 1, Math.max(totalPages, 1))} / {Math.max(totalPages, 1)}
        </span>
        <Button
          variant="outline"
          size="sm"
          disabled={pageIndex + 1 >= totalPages}
          onClick={() => onPageChange(pageIndex + 1)}>
          Sonraki
        </Button>
      </div>
    </div>
  );
}
