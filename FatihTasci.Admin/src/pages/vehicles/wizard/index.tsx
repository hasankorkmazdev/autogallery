import { useCallback, useEffect, useRef, useState } from "react";
import { useNavigate, useParams, useSearchParams } from "react-router-dom";
import { Car } from "lucide-react";
import { toast } from "sonner";
import { PageContent, PageHeader, PageLayout } from "@/components/layout/page-layout";
import { PageLoader } from "@/components/page-loader";
import { vehiclesService } from "@/api/services/vehicles.service";
import type { VehicleDetail } from "@/api/types";
import { WizardStepsNav } from "./components/wizard-steps-nav";
import { WizardHeaderActions } from "./components/wizard-header-actions";
import { StepFooter } from "./components/step-footer";
import { StepIdentity } from "./components/step-identity";
import { StepTechnical } from "./components/step-technical";
import { StepCondition } from "./components/step-condition";
import { StepBodyParts } from "./components/step-body-parts";
import { StepFeatures } from "./components/step-features";
import { StepMedia } from "./components/step-media";
import { StepPricing } from "./components/step-pricing";
import { WIZARD_STEPS, type StepProps, type StepSaveHandler } from "./types";

const LAST_STEP = WIZARD_STEPS.length;

function parseStep(value: string | null) {
  const step = Number(value);
  return Number.isInteger(step) && step >= 1 && step <= LAST_STEP ? step : 1;
}

// Araç giriş sihirbazı (7 adım). Adımlar arasında serbestçe geçilir;
// geçişte açık adımın formu doğrulanıp değiştiyse taslağa kaydedilir.
export default function VehicleWizardPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const step = id ? parseStep(searchParams.get("step")) : 1;
  const [vehicle, setVehicle] = useState<VehicleDetail | null>(null);
  const [loading, setLoading] = useState(!!id);
  const [saving, setSaving] = useState(false);
  const saveRef = useRef<StepSaveHandler | null>(null);

  useEffect(() => {
    if (!id) {
      setVehicle(null);
      setLoading(false);
      return;
    }
    let cancelled = false;
    setLoading(true);
    vehiclesService
      .get(id)
      .then((result) => {
        if (!cancelled) setVehicle(result);
      })
      .catch((error: Error) => {
        toast.error(error.message);
        navigate("/vehicles");
      })
      .finally(() => {
        if (!cancelled) setLoading(false);
      });
    return () => {
      cancelled = true;
    };
  }, [id, navigate]);

  const registerSave = useCallback((handler: StepSaveHandler | null) => {
    saveRef.current = handler;
  }, []);

  const runSave = useCallback(async () => {
    if (!saveRef.current) return true;
    setSaving(true);
    try {
      return await saveRef.current();
    } finally {
      setSaving(false);
    }
  }, []);

  const goToStep = useCallback(
    async (target: number) => {
      if (saving || target === step) return;
      if (!(await runSave())) return;
      setSearchParams({ step: String(target) });
    },
    [saving, step, runSave, setSearchParams],
  );

  const handleCreated = useCallback(
    (created: VehicleDetail) => navigate(`/vehicles/${created.id}?step=2`, { replace: true }),
    [navigate],
  );

  const renderStep = () => {
    if (!vehicle) {
      return <StepIdentity vehicle={null} onSaved={handleCreated} registerSave={registerSave} />;
    }
    const props: StepProps = { vehicle, onSaved: setVehicle, registerSave };
    switch (step) {
      case 2:
        return <StepTechnical {...props} />;
      case 3:
        return <StepCondition {...props} />;
      case 4:
        return <StepBodyParts {...props} />;
      case 5:
        return <StepFeatures {...props} />;
      case 6:
        return <StepMedia {...props} />;
      case 7:
        return <StepPricing {...props} beforeStatusChange={runSave} />;
      default:
        return <StepIdentity {...props} />;
    }
  };

  const renderContent = () => {
    if (loading) return <PageLoader />;
    return (
      <div className="flex flex-col gap-6">
        <WizardStepsNav current={step} enabled={!!vehicle} onSelect={goToStep} />
        {/* key: araç değişince (kopya / yeni kayıt) adım formu sıfırlanır */}
        <div key={`${vehicle?.id ?? "new"}-${step}`}>{renderStep()}</div>
        <StepFooter
          step={step}
          lastStep={LAST_STEP}
          isNew={!vehicle}
          saving={saving}
          onBack={() => goToStep(step - 1)}
          onNext={() => (vehicle ? goToStep(step + 1) : runSave())}
          onSave={runSave}
        />
      </div>
    );
  };

  const title = vehicle ? vehicle.title : "Yeni araç";
  const description = vehicle
    ? `İlan No ${vehicle.listingNo} · ${vehicle.brandName} ${vehicle.seriesName} ${vehicle.modelName}`
    : "Önce marka, seri ve modeli seçin; taslak otomatik oluşturulur.";

  return (
    <PageLayout>
      <PageHeader
        title={title}
        description={description}
        icon={<Car className="w-8 h-8" />}
        actions={vehicle && <WizardHeaderActions vehicle={vehicle} onCopied={(copy) => navigate(`/vehicles/${copy.id}?step=2`)} />}
      />
      <PageContent className="overflow-y-auto">{renderContent()}</PageContent>
    </PageLayout>
  );
}
