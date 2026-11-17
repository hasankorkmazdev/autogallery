const MAX_POSTER_WIDTH = 1920;

export interface CapturedPoster {
  poster: Blob;
  width: number;
  height: number;
  duration: number;
}

const waitFor = (video: HTMLVideoElement, event: "loadeddata" | "seeked") =>
  new Promise<void>((resolve, reject) => {
    video.addEventListener(event, () => resolve(), { once: true });
    video.addEventListener("error", () => reject(new Error("Video bu tarayıcıda açılamadı. H.264 kodlu bir MP4 yükleyin.")), {
      once: true,
    });
  });

/**
 * Videonun ilk karesini afiş olarak alır (sunucuda video işleme yok). Tarayıcı videoyu açamıyorsa ziyaretçinin
 * tarayıcısı da oynatamaz; bu yüzden hata, yüklemeden önce uygun olmayan dosyayı da yakalar.
 */
export async function capturePoster(file: File): Promise<CapturedPoster> {
  const url = URL.createObjectURL(file);
  const video = document.createElement("video");
  try {
    video.muted = true;
    video.playsInline = true;
    video.preload = "auto";
    const loaded = waitFor(video, "loadeddata");
    video.src = url;
    await loaded;

    // 0. saniye bazı kodlayıcılarda siyah kare olabilir; ilk karelerden biri alınır.
    const seeked = waitFor(video, "seeked");
    video.currentTime = Math.min(0.05, video.duration / 2 || 0);
    await seeked;

    const scale = Math.min(1, MAX_POSTER_WIDTH / video.videoWidth);
    const canvas = document.createElement("canvas");
    canvas.width = Math.round(video.videoWidth * scale);
    canvas.height = Math.round(video.videoHeight * scale);
    const context = canvas.getContext("2d");
    if (!context || canvas.width === 0) throw new Error("Videonun ilk karesi alınamadı.");
    context.drawImage(video, 0, 0, canvas.width, canvas.height);

    // WebP desteklemeyen tarayıcı PNG döndürür; sunucu her iki durumda WebP'ye çevirir.
    const poster = await new Promise<Blob | null>((resolve) => canvas.toBlob(resolve, "image/webp", 0.92));
    if (!poster) throw new Error("Videonun ilk karesi alınamadı.");

    return { poster, width: video.videoWidth, height: video.videoHeight, duration: video.duration };
  } finally {
    video.removeAttribute("src");
    video.load();
    URL.revokeObjectURL(url);
  }
}
