// Sidebar Toggle Logic (Desktop & Mobile)
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

// Restore saved sidebar state on DOMContentLoaded for Desktop
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

// Close sidebar when clicking outside on mobile
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

// Search "/" keyboard shortcut
document.addEventListener('keydown', function (e) {
    if (e.key === '/' && !e.ctrlKey && !e.metaKey && !e.altKey) {
        var searchInput = document.querySelector('.search-input');
        if (searchInput && document.activeElement !== searchInput) {
            e.preventDefault();
            searchInput.focus();
        }
    }
});
