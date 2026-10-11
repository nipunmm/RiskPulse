// ============================================================
// RISK PULSE — Top-nav & shell logic
// ============================================================

// --- Mobile nav drawer ---
function toggleNav() {
    var nav = document.getElementById('rpNav');
    var toggleBtn = document.getElementById('rpNavToggle');
    if (!nav) return;

    var isOpen = nav.classList.toggle('open');
    if (toggleBtn) {
        toggleBtn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
    }
}

function closeNav() {
    var nav = document.getElementById('rpNav');
    var toggleBtn = document.getElementById('rpNavToggle');
    if (!nav || !nav.classList.contains('open')) return;
    nav.classList.remove('open');
    if (toggleBtn) toggleBtn.setAttribute('aria-expanded', 'false');
}

document.addEventListener('DOMContentLoaded', function () {
    var toggleBtn = document.getElementById('rpNavToggle');
    if (toggleBtn) {
        toggleBtn.addEventListener('click', function (e) {
            e.stopPropagation();
            toggleNav();
        });
    }
});

// Close the mobile nav when clicking a plain link inside it (dropdown
// toggles are excluded so Bootstrap menus keep working)
document.addEventListener('click', function (e) {
    var nav = document.getElementById('rpNav');
    if (!nav || !nav.classList.contains('open')) return;
    if (nav.contains(e.target) && !e.target.closest('.dropdown-toggle')) return;
    if (e.target.closest('#rpNavToggle')) return;
    closeNav();
});

// Close the mobile nav on Escape
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') closeNav();
});