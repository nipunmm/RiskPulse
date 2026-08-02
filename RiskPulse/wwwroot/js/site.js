// ============================================================
// STASIS ENTERPRISE — Sidebar & Layout Logic
// ============================================================

// Sidebar Collapse / Expand Toggle (Desktop & Mobile)
function toggleSidebar() {
    var sidebar = document.querySelector('.sidebar');
    var mainContent = document.querySelector('.main-content');
    var toggleBtn = document.getElementById('sidebarToggle');
    var isMobile = window.innerWidth < 768;

    if (!sidebar) return;

    if (isMobile) {
        sidebar.classList.toggle('open');
        var isOpen = sidebar.classList.contains('open');
        if (toggleBtn) {
            toggleBtn.setAttribute('aria-expanded', isOpen ? 'true' : 'false');
        }
    } else {
        sidebar.classList.toggle('collapsed');
        if (mainContent) {
            mainContent.classList.toggle('sidebar-collapsed');
        }
        var isCollapsed = sidebar.classList.contains('collapsed');
        localStorage.setItem('sidebarCollapsed', isCollapsed ? 'true' : 'false');
        if (toggleBtn) {
            toggleBtn.setAttribute('aria-expanded', isCollapsed ? 'false' : 'true');
        }
    }
}

// Submenu Accordion Toggle
function toggleSubmenu(parentBtn) {
    if (!parentBtn) return;

    var sidebarGroup = parentBtn.closest('.sidebar-group');
    if (!sidebarGroup) return;

    var submenu = sidebarGroup.querySelector('.sidebar-submenu');
    if (!submenu) return;

    var isOpen = submenu.classList.contains('open');

    if (isOpen) {
        submenu.classList.remove('open');
        parentBtn.classList.remove('expanded');
        parentBtn.setAttribute('aria-expanded', 'false');
    } else {
        submenu.classList.add('open');
        parentBtn.classList.add('expanded');
        parentBtn.setAttribute('aria-expanded', 'true');
    }
}

// Restore saved sidebar state on DOMContentLoaded
document.addEventListener('DOMContentLoaded', function () {
    if (window.innerWidth >= 768) {
        var isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        var sidebar = document.querySelector('.sidebar');
        var mainContent = document.querySelector('.main-content');
        var toggleBtn = document.getElementById('sidebarToggle');

        if (isCollapsed) {
            if (sidebar) sidebar.classList.add('collapsed');
            if (mainContent) mainContent.classList.add('sidebar-collapsed');
            if (toggleBtn) toggleBtn.setAttribute('aria-expanded', 'false');
        }
    }
});

// Close sidebar on mobile when clicking outside
document.addEventListener('click', function (e) {
    var sidebar = document.querySelector('.sidebar');
    var toggleBtn = document.getElementById('sidebarToggle');
    if (window.innerWidth < 768 && sidebar && sidebar.classList.contains('open')) {
        if (!sidebar.contains(e.target) && (!toggleBtn || !toggleBtn.contains(e.target))) {
            sidebar.classList.remove('open');
            if (toggleBtn) {
                toggleBtn.setAttribute('aria-expanded', 'false');
            }
        }
    }
});

// Keyboard shortcut "/" for top search bar focus
document.addEventListener('keydown', function (e) {
    if (e.key === '/' && !e.ctrlKey && !e.metaKey && !e.altKey) {
        var searchInput = document.querySelector('.search-input');
        if (searchInput && document.activeElement !== searchInput) {
            e.preventDefault();
            searchInput.focus();
        }
    }
});
