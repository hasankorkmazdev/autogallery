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

    document.addEventListener("keydown", function (e) {
      var tag = (document.activeElement && document.activeElement.tagName) || "";
      if (tag === "INPUT" || tag === "TEXTAREA" || tag === "SELECT") return;
      if (e.key === "ArrowRight") show(current + 1);
      if (e.key === "ArrowLeft") show(current - 1);
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
