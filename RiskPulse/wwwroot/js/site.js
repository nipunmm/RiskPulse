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
        if (isCollapsed) {
            document.documentElement.setAttribute('data-sidebar', 'collapsed');
        } else {
            document.documentElement.removeAttribute('data-sidebar');
            closeFlyout();
        }
        if (toggleBtn) {
            toggleBtn.setAttribute('aria-expanded', isCollapsed ? 'false' : 'true');
        }
    }
}

// Submenu Accordion Toggle
function toggleSubmenu(parentBtn) {
    if (!parentBtn) return;

    var sidebar = document.querySelector('.sidebar');
    if (sidebar && sidebar.classList.contains('collapsed')) {
        toggleFlyout(parentBtn);
        return;
    }

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

// Collapsed-sidebar flyout submenu (appended to body to escape sidebar clipping)
function getFlyout() {
    var flyout = document.querySelector('.sidebar-flyout');
    if (!flyout) {
        flyout = document.createElement('div');
        flyout.className = 'sidebar-flyout';
        flyout.setAttribute('role', 'menu');
        document.body.appendChild(flyout);
    }
    return flyout;
}

function closeFlyout() {
    var flyout = getFlyout();
    var wasShown = flyout.classList.contains('show');
    flyout.classList.remove('show');
    flyout.innerHTML = '';
    if (wasShown) {
        var parent = document.querySelector('.sidebar-parent[aria-expanded="true"]');
        if (parent && parent.closest('.sidebar.collapsed')) {
            parent.setAttribute('aria-expanded', 'false');
        }
    }
}

function toggleFlyout(parentBtn) {
    var flyout = getFlyout();

    if (flyout.classList.contains('show')) {
        closeFlyout();
        return;
    }

    var sidebarGroup = parentBtn.closest('.sidebar-group');
    if (!sidebarGroup) return;

    var label = parentBtn.querySelector('span') ? parentBtn.querySelector('span').textContent.trim() : '';
    var submenu = sidebarGroup.querySelector('.sidebar-submenu');
    var links = submenu ? submenu.querySelectorAll('a') : [];

    var html = label ? '<div class="sidebar-flyout-title">' + label + '</div>' : '';
    for (var i = 0; i < links.length; i++) {
        var link = links[i];
        var active = link.classList.contains('active') ? ' active' : '';
        html += '<a class="nav-link' + active + '" href="' + link.getAttribute('href') + '">' + link.innerHTML + '</a>';
    }
    flyout.innerHTML = html;

    var rect = parentBtn.getBoundingClientRect();
    flyout.style.top = rect.top + 'px';
    flyout.style.left = (rect.right + 6) + 'px';
    flyout.classList.add('show');

    var flyoutRect = flyout.getBoundingClientRect();
    if (rect.right + flyoutRect.width + 6 > window.innerWidth) {
        flyout.style.left = Math.max(6, rect.left - flyoutRect.width - 6) + 'px';
    }

    parentBtn.setAttribute('aria-expanded', 'true');
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

    var flyout = document.querySelector('.sidebar-flyout');
    if (flyout && flyout.classList.contains('show')) {
        if (!flyout.contains(e.target) && !e.target.closest('.sidebar-group')) {
            closeFlyout();
        }
    }

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

// Close flyout when the sidebar scrolls (it stays fixed relative to the viewport)
document.querySelector('.sidebar-nav').addEventListener('scroll', function () {
    closeFlyout();
});

// Keyboard shortcuts: "/" focuses the top search bar, "Escape" closes the flyout
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        closeFlyout();
        return;
    }
    if (e.key === '/' && !e.ctrlKey && !e.metaKey && !e.altKey) {
        var searchInput = document.querySelector('.search-input');
        if (searchInput && document.activeElement !== searchInput) {
            e.preventDefault();
            searchInput.focus();
        }
    }
});
