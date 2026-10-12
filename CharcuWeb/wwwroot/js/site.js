(function () {
    const navbar = document.getElementById("mainNavbar");
    if (!navbar) return;

    // Páginas con cabecera grande (home y catálogo)
    const hero = document.querySelector(".hero, .catalog-hero");

    // Sin cabecera grande: barra sólida siempre
    if (!hero) {
        navbar.classList.add("scrolled");
        return;
    }

    function update() {
        const limit = hero.offsetHeight - navbar.offsetHeight;
        navbar.classList.toggle("scrolled", window.scrollY > Math.max(limit, 10));
    }

    window.addEventListener("scroll", update, { passive: true });
    window.addEventListener("resize", update);
    update();
})();