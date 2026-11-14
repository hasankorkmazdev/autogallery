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
    var counter = section.querySelector("[data-atmosphere-current]");
    if (!track || panels.length === 0) return;

    var desktop = window.matchMedia("(min-width: 960px)");
    var reducedMotion = window.matchMedia("(prefers-reduced-motion: reduce)");
    var current = 0;

    function pad(n) { return (n < 9 ? "0" : "") + (n + 1); }

    function activate(index) {
      current = (index + panels.length) % panels.length;
      panels.forEach(function (panel, i) {
        var active = i === current;
        panel.classList.toggle("is-active", active);
        panel.setAttribute("aria-current", active ? "true" : "false");
      });
      if (counter) counter.textContent = pad(current);
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

    // Tam ekran: yalnız fotoğrafı olan sahneler arasında gezinilir.
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
    var full = section.querySelector("[data-atmosphere-full]");
    if (full) full.addEventListener("click", function () { open(panels[current]); });
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

  // Stok filtresi: marka değişince eski seri seçimi temizlenir; boş alanlar adrese yazılmaz.
  document.querySelectorAll("[data-stock-filter]").forEach(function (form) {
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
})();
