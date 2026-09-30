document.querySelectorAll("[data-related-toggle]").forEach((toggle) => {
    const panel = document.getElementById(toggle.getAttribute("aria-controls"));
    if (!panel) return;
    const update = () => {
        panel.hidden = !toggle.checked;
        toggle.setAttribute("aria-expanded", String(toggle.checked));
        panel.querySelectorAll("select").forEach((select) => {
            select.disabled = !toggle.checked;
        });
    };
    toggle.addEventListener("change", update);
    update();
});
