// Vitrin — Faz 1 küçük etkileşimler (çerçeve yok). Faz 2'de Alpine.js / GSAP ile genişletilecek.
(function () {
  "use strict";

  // Galeri: küçük resme tıklayınca ana fotoğraf değişir; ok tuşlarıyla gezinilir.
  document.querySelectorAll("[data-gallery]").forEach(function (gallery) {
    var thumbs = Array.prototype.slice.call(gallery.querySelectorAll(".thumb"));
    var main = gallery.querySelector("[data-gallery-main]");
    var image = gallery.querySelector("[data-gallery-image]");
    if (!main || !image || thumbs.length === 0) return;

    var current = 0;
    function show(index) {
      current = (index + thumbs.length) % thumbs.length;
      var thumb = thumbs[current];
      image.src = thumb.getAttribute("data-medium");
      main.href = thumb.getAttribute("data-large");
      thumbs.forEach(function (t, i) { t.classList.toggle("active", i === current); });
    }

    thumbs.forEach(function (thumb, index) {
      thumb.addEventListener("click", function () { show(index); });
    });

    var lightbox = gallery.querySelector("[data-lightbox]");

    document.addEventListener("keydown", function (e) {
      if (lightbox && lightbox.open) return;
      var tag = (document.activeElement && document.activeElement.tagName) || "";
      if (tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT") return;
      if (e.key === "ArrowRight") show(current + 1);
      if (e.key === "ArrowLeft") show(current - 1);
    });

    // Tam ekran galeri (lightbox): ana fotoğrafa tıklayınca o fotoğraftan açılır.
    // Fotoğrafa tıklayınca sonrakine geçer; oklar / klavye / kaydırma ile de gezinilir. Kapanınca sayfadaki fotoğraf eşlenir.
    if (!lightbox || typeof lightbox.showModal !== "function") return;

    var photo = lightbox.querySelector("[data-lightbox-img]");
    var stage = lightbox.querySelector("[data-lightbox-stage]");
    var counter = lightbox.querySelector("[data-lightbox-index]");
    var strip = Array.prototype.slice.call(lightbox.querySelectorAll("[data-lightbox-thumb]"));
    var items = strip.length > 0
      ? strip.map(function (t) { return { src: t.getAttribute("data-large"), alt: t.getAttribute("data-alt") }; })
      : [{ src: main.getAttribute("href"), alt: image.getAttribute("alt") }];
    var shown = 0;

    function preload(index) {
      var item = items[(index + items.length) % items.length];
      if (item) new Image().src = item.src;
    }

    function go(index) {
      shown = (index + items.length) % items.length;
      photo.src = items[shown].src;
      photo.alt = items[shown].alt || "";
      if (counter) counter.textContent = shown + 1;
      strip.forEach(function (t, i) {
        var active = i === shown;
        t.classList.toggle("active", active);
        if (active) t.setAttribute("aria-current", "true");
        else t.removeAttribute("aria-current");
      });
      if (strip[shown]) strip[shown].scrollIntoView({ block: "nearest", inline: "center" });
      preload(shown + 1);
      preload(shown - 1);
    }

    main.addEventListener("click", function (e) {
      e.preventDefault();
      go(current);
      document.documentElement.classList.add("lightbox-open");
      lightbox.showModal();
    });

    lightbox.addEventListener("close", function () {
      document.documentElement.classList.remove("lightbox-open");
      show(shown); // sayfadaki galeri, son bakılan fotoğrafta kalsın
    });

    var close = lightbox.querySelector("[data-lightbox-close]");
    var prev = lightbox.querySelector("[data-lightbox-prev]");
    var next = lightbox.querySelector("[data-lightbox-next]");
    if (close) close.addEventListener("click", function () { lightbox.close(); });
    if (prev) prev.addEventListener("click", function () { go(shown - 1); });
    if (next) next.addEventListener("click", function () { go(shown + 1); });
    strip.forEach(function (t, i) { t.addEventListener("click", function () { go(i); }); });

    lightbox.addEventListener("keydown", function (e) {
      if (e.key === "ArrowRight") { e.preventDefault(); go(shown + 1); }
      if (e.key === "ArrowLeft") { e.preventDefault(); go(shown - 1); }
    });

    // Fotoğrafın dışındaki boş alana tıklayınca kapanır.
    stage.addEventListener("click", function (e) {
      if (e.target === stage) lightbox.close();
    });

    // Fotoğraf: tıklayınca sonrakine geçer; dokunmatikte yana kaydırınca önceki / sonraki.
    var startX = null, startY = 0, moved = false;
    photo.addEventListener("pointerdown", function (e) {
      startX = e.clientX; startY = e.clientY; moved = false;
    });
    photo.addEventListener("pointermove", function (e) {
      if (startX !== null && Math.abs(e.clientX - startX) + Math.abs(e.clientY - startY) > 10) moved = true;
    });
    photo.addEventListener("pointerup", function (e) {
      var dx = startX === null ? 0 : e.clientX - startX;
      startX = null;
      if (moved && Math.abs(dx) > 50 && items.length > 1) go(dx < 0 ? shown + 1 : shown - 1);
    });
    photo.addEventListener("click", function () {
      if (!moved && items.length > 1) go(shown + 1);
    });
  });

  // Hasar şeması: parçanın üzerine gelince / dokununca durumu yazılır.
  document.querySelectorAll(".damage").forEach(function (diagram) {
    var hint = diagram.querySelector("[data-damage-hint]");
    if (!hint) return;
    var initial = hint.textContent;
    diagram.querySelectorAll(".part").forEach(function (part) {
      function show() { hint.textContent = part.getAttribute("data-label"); }
      part.addEventListener("mouseenter", show);
      part.addEventListener("focus", show);
      part.addEventListener("click", show);
      part.addEventListener("mouseleave", function () { hint.textContent = initial; });
    });
  });

  // Talep formu: test sürüşü seçilince tarih alanı açılır.
  document.querySelectorAll("[data-lead-type]").forEach(function (select) {
    var dateField = select.form.querySelector("[data-preferred-date]");
    if (!dateField) return;
    function sync() { dateField.hidden = select.value !== "2"; }
    select.addEventListener("change", sync);
    sync();
  });

  // Galeri atmosferi (ana sayfa): masaüstünde tıklanan / odaklanan panel genişler ve sahneler
  // kendiliğinden ilerler; mobilde kartlar yatay kaydırılır, ortadaki kart aktif sayılır.
  document.querySelectorAll("[data-atmosphere]").forEach(function (section) {
    var track = section.querySelector("[data-atmosphere-track]");
    var panels = Array.prototype.slice.call(section.querySelectorAll("[data-atmosphere-panel]"));
    if (!track || panels.length === 0) return;

    var desktop = window.matchMedia("(min-width: 960px)");
    var reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    var current = 0;

    function activate(index) {
      current = (index + panels.length) % panels.length;
      panels.forEach(function (panel, i) {
        var active = i === current;
        panel.classList.toggle("is-active", active);
        panel.setAttribute("aria-current", active ? "true" : "false");
      });
    }

    function scrollToPanel(index) {
      activate(index);
      var panel = panels[current];
      track.scrollTo({
        left: panel.offsetLeft - (track.clientWidth - panel.clientWidth) / 2,
        behavior: reducedMotion.matches ? "auto" : "smooth"
      });
    }

    function go(index) {
      if (desktop.matches) activate(index);
      else scrollToPanel(index);
    }

    panels.forEach(function (panel, index) {
      panel.addEventListener("click", function () { go(index); });
      panel.addEventListener("focus", function () { if (desktop.matches) activate(index); });
    });

    track.addEventListener("keydown", function (e) {
      if (e.key === "ArrowRight") { e.preventDefault(); go(current + 1); focusCurrent(); }
      if (e.key === "ArrowLeft") { e.preventDefault(); go(current - 1); focusCurrent(); }
    });
    function focusCurrent() { if (desktop.matches) panels[current].focus({ preventScroll: true }); }

    var prev = section.querySelector("[data-atmosphere-prev]");
    var next = section.querySelector("[data-atmosphere-next]");
    if (prev) prev.addEventListener("click", function () { go(current - 1); });
    if (next) next.addEventListener("click", function () { go(current + 1); });

    // Mobil: kaydırma bitince ortaya en yakın kart aktif olur.
    var scrollTimer;
    track.addEventListener("scroll", function () {
      if (desktop.matches) return;
      clearTimeout(scrollTimer);
      scrollTimer = setTimeout(function () {
        var center = track.scrollLeft + track.clientWidth / 2;
        var nearest = 0, best = Infinity;
        panels.forEach(function (panel, i) {
          var d = Math.abs(panel.offsetLeft + panel.clientWidth / 2 - center);
          if (d < best) { best = d; nearest = i; }
        });
        activate(nearest);
      }, 80);
    }, { passive: true });

    // Masaüstü: bölüm ekrandayken ve kullanıcı etkileşimde değilken sahneler 6 saniyede bir ilerler.
    var visible = false, paused = false;
    if ("IntersectionObserver" in window) {
      new IntersectionObserver(function (entries) { visible = entries[0].isIntersecting; }, { threshold: 0.4 }).observe(track);
    }
    section.addEventListener("mouseenter", function () { paused = true; });
    section.addEventListener("mouseleave", function () { paused = false; });
    section.addEventListener("focusin", function () { paused = true; });
    section.addEventListener("focusout", function () { paused = false; });
    setInterval(function () {
      if (!desktop.matches || reducedMotion.matches || !visible || paused || document.hidden) return;
      if (dialog && dialog.open) return;
      activate(current + 1);
    }, 6000);

    // Büyüt (tam ekran): yalnız fotoğrafı olan sahneler arasında gezinilir.
    var dialog = section.querySelector("[data-atmosphere-dialog]");
    if (!dialog || typeof dialog.showModal !== "function") return;
    var dialogImg = dialog.querySelector("[data-atmosphere-dialog-img]");
    var dialogCaption = dialog.querySelector("[data-atmosphere-dialog-caption]");
    var withImage = panels.filter(function (p) { return p.getAttribute("data-image"); });
    var shown = 0;

    function show(i) {
      shown = (i + withImage.length) % withImage.length;
      var panel = withImage[shown];
      dialogImg.src = panel.getAttribute("data-image");
      dialogImg.alt = panel.getAttribute("data-title");
      dialogCaption.textContent = panel.getAttribute("data-title");
    }
    function open(panel) {
      var i = withImage.indexOf(panel);
      show(i < 0 ? 0 : i);
      dialog.showModal();
    }

    section.querySelectorAll("[data-atmosphere-zoom]").forEach(function (button) {
      button.addEventListener("click", function (e) {
        e.stopPropagation();
        open(button.closest("[data-atmosphere-panel]"));
      });
    });
    var close = dialog.querySelector("[data-atmosphere-close]");
    var dialogPrev = dialog.querySelector("[data-atmosphere-dialog-prev]");
    var dialogNext = dialog.querySelector("[data-atmosphere-dialog-next]");
    if (close) close.addEventListener("click", function () { dialog.close(); });
    if (dialogPrev) dialogPrev.addEventListener("click", function () { show(shown - 1); });
    if (dialogNext) dialogNext.addEventListener("click", function () { show(shown + 1); });
    dialog.addEventListener("click", function (e) { if (e.target === dialog) dialog.close(); });
    dialog.addEventListener("keydown", function (e) {
      if (e.key === "ArrowRight") show(shown + 1);
      if (e.key === "ArrowLeft") show(shown - 1);
    });
  });

  // Üst menü telefonda / tablette yatay kayar: açık sayfanın bağlantısı görünür alana getirilir; kenarda kalan
  // bağlantılar soluklaşır (.is-start / .is-end kaydırmanın başında / sonunda olunduğunu söyler).
  document.querySelectorAll(".main-nav").forEach(function (nav) {
    function sync() {
      nav.classList.toggle("is-scrollable", nav.scrollWidth > nav.clientWidth + 1);
      nav.classList.toggle("is-start", nav.scrollLeft <= 1);
      nav.classList.toggle("is-end", nav.scrollLeft + nav.clientWidth >= nav.scrollWidth - 1);
    }
    var active = nav.querySelector("a.active");
    if (active && nav.scrollWidth > nav.clientWidth) {
      nav.scrollLeft = active.offsetLeft - (nav.clientWidth - active.offsetWidth) / 2;
    }
    nav.addEventListener("scroll", sync, { passive: true });
    window.addEventListener("resize", sync);
    sync();
  });

  // Teslimatlar (ana sayfa): ortadaki kart büyük, yanlar küçük ve soluk. Etkin noktanın dolma animasyonu
  // (--slide-duration) bitince sıradaki ortaya gelir; durmak .is-paused ile animasyonu dondurur, kaldığı yerden sürer.
  // Fare ortadaki kartın üzerindeyken, klavye odağı içindeyken, ekranda değilken, sekme gizliyken, tam ekran açıkken ya da ziyaretçi
  // durdurduysa durur. Yan karta tıklamak onu ortaya getirir, ortadakine tıklamak tam ekranı açar; telefonda yana
  // kaydırılır. "Hareketi azalt" açıksa animasyon olmadığından kendiliğinden dönmez.
  document.querySelectorAll("[data-coverflow]").forEach(function (root) {
    var stage = root.querySelector(".dcf-stage");
    var cards = Array.prototype.slice.call(root.querySelectorAll("[data-cf-card]"));
    var captions = Array.prototype.slice.call(root.querySelectorAll("[data-cf-caption]"));
    var dots = Array.prototype.slice.call(root.querySelectorAll("[data-cf-to]"));
    var toggle = root.querySelector("[data-cf-toggle]");
    var count = cards.length;
    var current = 0;
    var state = { stopped: false, hover: false, focus: false, offscreen: true, dialog: false };
    var mouse = false;

    // Ortadaki kart değişince fare artık onun üzerinde olmayabilir (ya da yeni ortadakinin üzerindedir)
    function syncHover() {
      state.hover = mouse && cards[current].matches(":hover");
      sync();
    }

    function sync() {
      var paused = state.stopped || state.hover || state.focus || state.offscreen || state.dialog || document.hidden;
      root.classList.toggle("is-paused", paused);
      if (toggle) {
        toggle.classList.toggle("is-stopped", state.stopped);
        toggle.setAttribute("aria-label", state.stopped ? "Dönmeyi başlat" : "Dönmeyi durdur");
      }
    }

    function layout() {
      var wide = stage.clientWidth >= 900;
      cards.forEach(function (card, i) {
        var offset = i - current;
        if (offset > count / 2) offset -= count;
        if (offset < -count / 2) offset += count;
        var distance = Math.abs(offset);
        var shift = offset * (wide ? 58 : 66);
        var scale = distance === 0 ? 1 : distance === 1 ? 0.8 : 0.64;
        card.style.setProperty("transform", "translateX(calc(-50% + " + shift + "%)) scale(" + scale + ")");
        card.style.setProperty("opacity", distance === 0 ? "1" : distance === 1 ? ".55" : distance === 2 ? ".22" : "0");
        card.style.setProperty("filter", distance === 0 ? "none" : "grayscale(.4) brightness(.65)");
        card.style.setProperty("z-index", String(10 - distance));
        card.style.setProperty("pointer-events", distance > 1 ? "none" : "auto");
      });
    }

    function go(index) {
      current = (index + count) % count;
      cards.forEach(function (card, i) {
        var active = i === current;
        card.classList.toggle("is-active", active);
        card.setAttribute("aria-hidden", active ? "false" : "true");
        var button = card.querySelector("[data-delivery-open]");
        if (button) button.tabIndex = active ? 0 : -1;
      });
      captions.forEach(function (caption, i) {
        caption.classList.toggle("is-active", i === current);
        caption.setAttribute("aria-hidden", i === current ? "false" : "true");
      });
      dots.forEach(function (dot) { dot.classList.remove("is-active"); dot.setAttribute("aria-current", "false"); });
      if (dots.length) {
        void root.offsetWidth; // etkin noktanın dolma animasyonu baştan başlasın
        dots[current].classList.add("is-active");
        dots[current].setAttribute("aria-current", "true");
      }
      layout();
      syncHover();
    }

    layout();
    window.addEventListener("resize", layout);
    if (count < 2) return;

    root.addEventListener("animationend", function (e) {
      if (e.animationName === "dcf-fill" && dots[current] && dots[current].contains(e.target)) go(current + 1);
    });
    dots.forEach(function (dot) {
      dot.addEventListener("click", function () { go(parseInt(dot.getAttribute("data-cf-to"), 10)); });
    });
    var prev = root.querySelector("[data-cf-prev]");
    var next = root.querySelector("[data-cf-next]");
    if (prev) prev.addEventListener("click", function () { go(current - 1); });
    if (next) next.addEventListener("click", function () { go(current + 1); });
    if (toggle) toggle.addEventListener("click", function () { state.stopped = !state.stopped; sync(); });
    root.addEventListener("keydown", function (e) {
      if (e.key === "ArrowRight") { go(current + 1); e.preventDefault(); }
      if (e.key === "ArrowLeft") { go(current - 1); e.preventDefault(); }
    });

    // Yan karta tıklamak onu ortaya getirir (tam ekran açılmaz); kaydırmadan sonra gelen tıklama yok sayılır.
    var startX = null, startY = null, swiped = false;
    stage.addEventListener("pointerdown", function (e) { startX = e.clientX; startY = e.clientY; swiped = false; });
    stage.addEventListener("pointerup", function (e) {
      if (startX === null) return;
      var dx = e.clientX - startX, dy = e.clientY - startY;
      startX = null;
      if (Math.abs(dx) > 40 && Math.abs(dx) > Math.abs(dy)) {
        swiped = true;
        go(current + (dx < 0 ? 1 : -1));
      }
    });
    stage.addEventListener("click", function (e) {
      var card = e.target.closest("[data-cf-card]");
      if (swiped || (card && !card.classList.contains("is-active"))) {
        e.preventDefault();
        e.stopPropagation();
        if (!swiped && card) go(cards.indexOf(card));
        swiped = false;
        return;
      }
      if (card) { state.dialog = true; sync(); } // ortadaki kart: tam ekran açılıyor
    }, true);

    // Yalnız fare ortadaki kartın üzerindeyken dur (fotoğrafa bakılıyor). Bölüm sayfa genişliğinde olduğundan tamamında
    // durursa kaydırıp bakan ziyaretçide hiç dönmüyor gibi görünür; dokunmatikte "hover" kalıcı olmasın diye yalnız fare.
    stage.addEventListener("pointerover", function (e) { mouse = e.pointerType === "mouse"; syncHover(); });
    stage.addEventListener("pointerleave", function () { mouse = false; syncHover(); });
    // Klavyeyle içinde gezinirken dur (fareyle tıklanan düğme odak alsa da döner)
    function syncFocus() {
      var focused = document.activeElement;
      state.focus = !!focused && root.contains(focused) && focused.matches(":focus-visible");
      sync();
    }
    root.addEventListener("focusin", syncFocus);
    root.addEventListener("focusout", function () { setTimeout(syncFocus, 0); });
    document.addEventListener("visibilitychange", sync);
    if ("IntersectionObserver" in window) {
      new IntersectionObserver(function (entries) { state.offscreen = !entries[0].isIntersecting; sync(); }, { threshold: 0.35 }).observe(stage);
    } else {
      state.offscreen = false;
    }
    var section = root.closest("[data-deliveries]");
    var dialog = section ? section.querySelector("[data-deliveries-dialog]") : null;
    if (dialog) dialog.addEventListener("close", function () { state.dialog = false; sync(); });

    sync();
  });

  // Teslimatlar (ana sayfa ve /teslimatlar): fotoğrafa tıklayınca tam ekran açılır, oklar / klavye ile bölümdeki
  // teslimatlar arasında gezinilir.
  document.querySelectorAll("[data-deliveries]").forEach(function (section) {
    var items = Array.prototype.slice.call(section.querySelectorAll("[data-delivery]"));
    if (items.length === 0) return;

    var dialog = section.querySelector("[data-deliveries-dialog]");
    if (!dialog || typeof dialog.showModal !== "function") return;
    var dialogImg = dialog.querySelector("[data-deliveries-dialog-img]");
    var dialogCaption = dialog.querySelector("[data-deliveries-dialog-caption]");
    var shown = 0;

    function show(i) {
      shown = (i + items.length) % items.length;
      var item = items[shown];
      dialogImg.src = item.getAttribute("data-large");
      dialogImg.alt = item.getAttribute("data-alt");
      dialogCaption.textContent = item.getAttribute("data-caption");
    }

    items.forEach(function (item, index) {
      var open = item.querySelector("[data-delivery-open]");
      if (!open) return;
      open.addEventListener("click", function () {
        show(index);
        document.documentElement.classList.add("lightbox-open");
        dialog.showModal();
      });
    });
    dialog.addEventListener("close", function () {
      document.documentElement.classList.remove("lightbox-open");
      // Kapanınca odak son gösterilen teslimatın kartına döner.
      var open = items[shown].querySelector("[data-delivery-open]");
      if (open) open.focus({ preventScroll: true });
    });
    var close = dialog.querySelector("[data-deliveries-close]");
    var dialogPrev = dialog.querySelector("[data-deliveries-dialog-prev]");
    var dialogNext = dialog.querySelector("[data-deliveries-dialog-next]");
    if (close) close.addEventListener("click", function () { dialog.close(); });
    if (dialogPrev) dialogPrev.addEventListener("click", function () { show(shown - 1); });
    if (dialogNext) dialogNext.addEventListener("click", function () { show(shown + 1); });
    dialog.addEventListener("click", function (e) { if (e.target === dialog) dialog.close(); });
    dialog.addEventListener("keydown", function (e) {
      if (items.length < 2) return;
      if (e.key === "ArrowRight") show(shown + 1);
      if (e.key === "ArrowLeft") show(shown - 1);
    });
  });

  // Ana sayfa girişi (_Intro): drone videosu ekrana gelince yüklenir ve sessiz döngüde oynar; ekrandan çıkınca
  // ya da sekme gizlenince durur. "Hareketi azalt", veri tasarrufu ya da çok yavaş bağlantıda yalnız poster kalır.
  // Ziyaretçi durdurursa tercih bu tarayıcıda hatırlanır ve video o zaman hiç indirilmez.
  document.querySelectorAll("[data-intro]").forEach(function (intro) {
    var video = intro.querySelector("[data-intro-video]");
    var toggle = intro.querySelector("[data-intro-toggle]");
    if (!video || !toggle || typeof video.play !== "function") return;
    var connection = navigator.connection || {};
    var slow = connection.saveData || /(^|-)2g$/.test(connection.effectiveType || "");
    if (window.matchMedia("(prefers-reduced-motion: reduce)").matches || slow) return;

    var stopped = false;
    try { stopped = window.localStorage.getItem("intro-paused") === "1"; } catch (e) { stopped = false; }
    var visible = true;

    function play() {
      if (stopped || !visible || document.hidden) return;
      if (!video.getAttribute("src")) {
        video.src = video.getAttribute(intro.clientWidth >= 1024 ? "data-src-hd" : "data-src-sd");
      }
      var started = video.play();
      // Otomatik oynatma engellenirse (ör. iOS düşük güç modu) poster kalır, düğme "oynat" gösterir.
      if (started && typeof started.catch === "function") started.catch(render);
    }
    function render() {
      var playing = !video.paused;
      toggle.classList.toggle("is-paused", !playing);
      toggle.setAttribute("aria-label", playing ? "Videoyu durdur" : "Videoyu oynat");
    }

    video.addEventListener("playing", function () { intro.classList.add("is-playing"); });
    video.addEventListener("play", render);
    video.addEventListener("pause", render);
    toggle.addEventListener("click", function () {
      stopped = !video.paused;
      try { window.localStorage.setItem("intro-paused", stopped ? "1" : "0"); } catch (e) { /* depolama kapalı */ }
      if (stopped) video.pause(); else play();
      render();
    });
    document.addEventListener("visibilitychange", function () {
      if (document.hidden) video.pause(); else play();
    });
    if ("IntersectionObserver" in window) {
      new IntersectionObserver(function (entries) {
        visible = entries[0].isIntersecting;
        if (visible) play(); else video.pause();
      }).observe(intro);
    } else {
      play();
    }
    // Düğme "durdur" olarak başlar; ziyaretçi daha önce durdurduysa "oynat".
    toggle.hidden = false;
    if (stopped) render();
  });

  // İlan filtresi: marka değişince eski model seçimi temizlenir; boş alanlar adrese yazılmaz.
  document.querySelectorAll("[data-listings-filter]").forEach(function (form) {
    var brand = form.querySelector("select[name=brand]");
    var series = form.querySelector("select[name=series]");
    if (brand && series) {
      brand.addEventListener("change", function () { series.value = ""; });
    }
    form.addEventListener("submit", function () {
      Array.prototype.forEach.call(form.elements, function (el) {
        if (el.name && el.value === "") el.disabled = true;
      });
    });
  });

  // Ana sayfa hızlı araması (_Intro): Model listesi seçili markanınkilere iner (marka seçilmeden pasif); boş alanlar
  // adrese yazılmaz. Geri tuşuyla dönülünce (bfcache) gönderirken pasifleşen alanlar yeniden açılır.
  document.querySelectorAll("[data-quick-search]").forEach(function (form) {
    var brand = form.querySelector("[data-qs-brand]");
    var series = form.querySelector("[data-qs-series]");
    if (!brand || !series) return;
    var placeholder = series.options[0];
    var all = Array.prototype.slice.call(series.querySelectorAll("option[data-brand]"));

    function sync(keep) {
      var selected = keep ? series.value : "";
      series.textContent = ""; // markaya göre gruplar (JS yokken) kalkar
      series.appendChild(placeholder);
      all.forEach(function (option) {
        if (option.getAttribute("data-brand") === brand.value) series.appendChild(option);
      });
      series.value = all.some(function (o) { return o.value === selected && o.parentNode === series; }) ? selected : "";
      series.disabled = !brand.value || series.options.length < 2;
    }
    brand.addEventListener("change", function () { sync(false); });
    sync(true);
    form.addEventListener("submit", function () {
      Array.prototype.forEach.call(form.elements, function (el) {
        if (el.name && el.value === "") el.disabled = true;
      });
    });
    window.addEventListener("pageshow", function () {
      brand.disabled = false;
      sync(true);
    });
  });

  // Ana sayfa araç kaydırıcısı (_Showcase): sekme bir listeyi gösterir (← → ile sekmeler arası geçilir); oklar etkin
  // listeyi görünen kart sayısı kadar kaydırır, baştaki / sondaki ok pasifleşir, liste sığıyorsa oklar gizlenir.
  // JS yoksa sekmeler ve oklar gizli kalır, listeler başlıklarıyla alt alta görünür.
  document.querySelectorAll("[data-rails]").forEach(function (section) {
    var tabs = Array.prototype.slice.call(section.querySelectorAll("[data-rail-tab]"));
    var rails = Array.prototype.slice.call(section.querySelectorAll("[data-rail]"));
    var tablist = section.querySelector("[data-rail-tabs]");
    var arrows = section.querySelector("[data-rail-arrows]");
    var prev = section.querySelector("[data-rail-prev]");
    var next = section.querySelector("[data-rail-next]");
    if (rails.length === 0 || !arrows) return;
    var reduce = window.matchMedia("(prefers-reduced-motion: reduce)");
    var current = 0;

    function track() { return rails[current].querySelector("[data-rail-track]"); }
    function syncArrows() {
      var t = track();
      arrows.hidden = t.scrollWidth <= t.clientWidth + 1;
      prev.disabled = t.scrollLeft <= 1;
      next.disabled = t.scrollLeft + t.clientWidth >= t.scrollWidth - 1;
    }
    function select(index, focus) {
      current = index;
      tabs.forEach(function (tab, i) {
        var active = i === index;
        tab.classList.toggle("is-active", active);
        tab.setAttribute("aria-selected", active ? "true" : "false");
        tab.tabIndex = active ? 0 : -1;
        if (active && focus) tab.focus();
      });
      rails.forEach(function (rail, i) { rail.hidden = i !== index; });
      syncArrows();
    }
    function step(direction) {
      var t = track();
      var item = t.querySelector(".rail-item");
      var gap = parseFloat(window.getComputedStyle(t).columnGap) || 0;
      var width = item ? item.getBoundingClientRect().width + gap : t.clientWidth;
      var perView = Math.max(1, Math.floor(t.clientWidth / width));
      t.scrollBy({ left: direction * width * perView, behavior: reduce.matches ? "auto" : "smooth" });
    }

    tabs.forEach(function (tab, index) {
      tab.addEventListener("click", function () { select(index, false); });
      tab.addEventListener("keydown", function (e) {
        if (e.key === "ArrowRight") { e.preventDefault(); select((index + 1) % tabs.length, true); }
        else if (e.key === "ArrowLeft") { e.preventDefault(); select((index - 1 + tabs.length) % tabs.length, true); }
      });
    });
    rails.forEach(function (rail) {
      rail.querySelector("[data-rail-track]").addEventListener("scroll", syncArrows, { passive: true });
      var title = rail.querySelector("[data-rail-title]");
      if (title) title.hidden = true; // sekmeler (ya da bölüm başlığı) adı zaten gösteriyor
    });
    prev.addEventListener("click", function () { step(-1); });
    next.addEventListener("click", function () { step(1); });
    window.addEventListener("resize", syncArrows);
    if (tablist) tablist.hidden = false;
    select(0, false);
  });

  // Konum haritası: Google Haritalar yalnız ziyaretçi "Haritayı göster"e basınca yüklenir (KVKK).
  // Tercih bu tarayıcıda hatırlanır; sonraki ziyaretlerde harita doğrudan açılır.
  document.querySelectorAll("[data-map]").forEach(function (map) {
    var button = map.querySelector("[data-map-load]");

    function load() {
      if (map.querySelector("iframe")) return;
      var frame = document.createElement("iframe");
      frame.src = map.getAttribute("data-map-src");
      frame.title = "Galeri konumu haritası";
      frame.setAttribute("allowfullscreen", "");
      map.appendChild(frame);
      map.classList.add("is-loaded");
    }

    var remembered = false;
    try { remembered = window.localStorage.getItem("map-consent") === "1"; } catch (e) { remembered = false; }
    if (remembered) load();

    if (button) {
      button.addEventListener("click", function () {
        try { window.localStorage.setItem("map-consent", "1"); } catch (e) { /* depolama kapalı */ }
        load();
      });
    }
  });

  // Teklif formu (Hizmetler → Aracımı Satmak İstiyorum, Services/_SellForm.cshtml). Fotoğraflar birikerek seçilir,
  // önizlenir, tek tek kaldırılır ve tarayıcıda küçültülür (uzun kenar 2000 px JPEG): telefondan yükleme hızlanır.
  // Zorunlu alanlar göndermeden önce denetlenir; sunucu hatasında tarayıcı seçili dosyaları geri dolduramaz, fotoğraflar
  // boşa gitmesin. Asıl doğrulama, sınırlar ve EXIF temizliği sunucudadır. JS yoksa form düz multipart gönderilir.
  document.querySelectorAll("[data-sell-form]").forEach(function (form) {
    var maxPhotos = parseInt(form.getAttribute("data-max-photos"), 10) || 8;
    var maxBytes = parseInt(form.getAttribute("data-max-bytes"), 10) || 15 * 1024 * 1024;
    var maxEdge = 2000;
    var input = form.querySelector("[data-photo-input]");
    var previews = form.querySelector("[data-photo-previews]");
    var status = form.querySelector("[data-photo-status]");
    var submit = form.querySelector("button[type=submit]");
    var submitLabel = submit ? submit.textContent : "";
    var fields = Array.prototype.slice.call(form.querySelectorAll("[data-error]"));
    var photos = [];
    var queued = 0;
    var busy = 0;

    function errorTarget(name) { return form.querySelector('[data-valmsg-for="' + name + '"]'); }

    function fieldMessage(field) {
      if (field.checkValidity()) return "";
      if (field.validity.valueMissing && field.getAttribute("data-error-required")) return field.getAttribute("data-error-required");
      return field.getAttribute("data-error");
    }

    function showError(field, message) {
      var target = errorTarget(field.name);
      if (target) target.textContent = message;
      if (message) field.setAttribute("aria-invalid", "true");
      else field.removeAttribute("aria-invalid");
    }

    // Hata gösterilen alan düzeltilince mesaj kalkar (sunucudan gelen hatalar dahil).
    fields.forEach(function (field) {
      var eventName = field.type === "checkbox" || field.tagName === "SELECT" ? "change" : "input";
      field.addEventListener(eventName, function () {
        var target = errorTarget(field.name);
        if (target && target.textContent) showError(field, fieldMessage(field));
      });
    });

    function setBusy(delta) {
      busy += delta;
      if (!submit) return;
      submit.disabled = busy > 0;
      submit.textContent = busy > 0 ? "Fotoğraflar hazırlanıyor..." : submitLabel;
    }

    form.addEventListener("submit", function (event) {
      if (busy > 0) { event.preventDefault(); return; }
      var firstInvalid = null;
      fields.forEach(function (field) {
        var message = fieldMessage(field);
        showError(field, message);
        if (message && !firstInvalid) firstInvalid = field;
      });
      if (firstInvalid) {
        event.preventDefault();
        firstInvalid.focus();
        return;
      }
      if (submit) { submit.disabled = true; submit.textContent = "Gönderiliyor..."; }
    });

    // Geri tuşuyla sayfaya dönülürse (bfcache) düğme yeniden kullanılabilir.
    window.addEventListener("pageshow", function () {
      if (submit && busy === 0) { submit.disabled = false; submit.textContent = submitLabel; }
    });

    // Seçimi biriktirmek için dosya listesi DataTransfer ile yeniden kurulur; desteklenmiyorsa alan olduğu gibi kalır.
    var canManage = false;
    try { canManage = !!(input && previews && status && window.URL && new DataTransfer().items); } catch (e) { canManage = false; }
    if (!canManage) return;

    function showPhotoError(message) {
      var target = errorTarget("Photos");
      if (target) target.textContent = message;
    }

    function isAllowed(file) {
      return /^image\/(jpeg|png|webp)$/.test(file.type) || (!file.type && /\.(jpe?g|png|webp)$/i.test(file.name));
    }

    // Uzun kenarı 2000 px'e indirip JPEG'e çevirir; küçük dosya ya da daha büyük sonuç olursa dosya olduğu gibi kalır.
    function shrink(file) {
      return new Promise(function (resolve) {
        var url = URL.createObjectURL(file);
        var image = new Image();
        image.onload = function () {
          var width = image.naturalWidth;
          var height = image.naturalHeight;
          var scale = Math.min(1, maxEdge / Math.max(width, height));
          if (scale === 1 && file.size <= 1.5 * 1024 * 1024) { URL.revokeObjectURL(url); resolve(file); return; }
          try {
            var canvas = document.createElement("canvas");
            canvas.width = Math.round(width * scale);
            canvas.height = Math.round(height * scale);
            var context = canvas.getContext("2d");
            context.fillStyle = "#fff";
            context.fillRect(0, 0, canvas.width, canvas.height);
            context.drawImage(image, 0, 0, canvas.width, canvas.height);
            URL.revokeObjectURL(url);
            canvas.toBlob(function (blob) {
              if (!blob || blob.size >= file.size) { resolve(file); return; }
              resolve(new File([blob], file.name.replace(/\.[^.]+$/, "") + ".jpg", { type: "image/jpeg", lastModified: Date.now() }));
            }, "image/jpeg", 0.85);
          } catch (e) {
            URL.revokeObjectURL(url);
            resolve(file);
          }
        };
        image.onerror = function () { URL.revokeObjectURL(url); resolve(file); };
        image.src = url;
      });
    }

    function sync() {
      var transfer = new DataTransfer();
      photos.forEach(function (photo) { transfer.items.add(photo.file); });
      input.files = transfer.files;
      render();
    }

    function render() {
      previews.textContent = "";
      photos.forEach(function (photo, index) {
        var item = document.createElement("li");
        item.className = "photo-preview";
        var image = document.createElement("img");
        image.src = photo.url;
        image.alt = (index + 1) + ". fotoğraf";
        var remove = document.createElement("button");
        remove.type = "button";
        remove.className = "photo-remove";
        remove.setAttribute("aria-label", (index + 1) + ". fotoğrafı kaldır");
        remove.textContent = "×";
        remove.addEventListener("click", function () {
          URL.revokeObjectURL(photo.url);
          photos.splice(photos.indexOf(photo), 1);
          showPhotoError("");
          sync();
          input.focus();
        });
        item.appendChild(image);
        item.appendChild(remove);
        previews.appendChild(item);
      });
      status.textContent = photos.length ? photos.length + " / " + maxPhotos + " fotoğraf seçildi" : "";
    }

    input.addEventListener("change", function () {
      var selected = Array.prototype.slice.call(input.files);
      var messages = [];
      var accepted = [];
      var overflow = false;
      selected.forEach(function (file) {
        if (!isAllowed(file)) messages.push(file.name + ": yalnızca JPG, PNG ve WebP fotoğraf yüklenebilir.");
        else if (photos.length + queued + accepted.length >= maxPhotos) overflow = true;
        else accepted.push(file);
      });
      if (overflow) messages.push("En fazla " + maxPhotos + " fotoğraf yükleyebilirsiniz.");

      // Yeni seçim hazırlanana kadar alanda yalnız önceki fotoğraflar durur.
      sync();
      showPhotoError(messages.join(" "));
      if (accepted.length === 0) return;

      var remaining = accepted.length;
      queued += remaining;
      setBusy(1);
      accepted.reduce(function (chain, file) {
        return chain.then(function () { return shrink(file); }).then(function (result) {
          queued--;
          remaining--;
          if (result.size > maxBytes) {
            messages.push(file.name + ": fotoğraf en fazla " + Math.round(maxBytes / 1048576) + " MB olabilir.");
            showPhotoError(messages.join(" "));
            return;
          }
          photos.push({ file: result, url: URL.createObjectURL(result) });
          sync();
        });
      }, Promise.resolve()).then(done, done);

      function done() {
        queued -= remaining;
        setBusy(-1);
      }
    });
  });
})();
