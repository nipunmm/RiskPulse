/* ============================================================
   Risk Pulse — Design Demo (vanilla JS, hardcoded data)
   Handles: screen switching, nav dropdown, modals, swatches and
   the Theme Lab that rewrites the CSS custom properties that the
   real site.css consumes.
   ============================================================ */
(function () {
    "use strict";

    var root = document.documentElement;

    /* ---------------- Colour math ---------------- */
    function hexToRgb(hex) {
        hex = hex.replace("#", "");
        if (hex.length === 3) hex = hex.split("").map(function (c) { return c + c; }).join("");
        var n = parseInt(hex, 16);
        return { r: (n >> 16) & 255, g: (n >> 8) & 255, b: n & 255 };
    }
    function clamp(v, a, b) { return Math.min(b, Math.max(a, v)); }
    function rgbToHex(r, g, b) {
        return "#" + [r, g, b].map(function (v) {
            return clamp(Math.round(v), 0, 255).toString(16).padStart(2, "0");
        }).join("");
    }
    function rgbToHsl(r, g, b) {
        r /= 255; g /= 255; b /= 255;
        var max = Math.max(r, g, b), min = Math.min(r, g, b);
        var h = 0, s = 0, l = (max + min) / 2;
        if (max !== min) {
            var d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            switch (max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                default: h = (r - g) / d + 4;
            }
            h /= 6;
        }
        return { h: h * 360, s: s, l: l };
    }
    function hslToHex(h, s, l) {
        h = ((h % 360) + 360) % 360 / 360; s = clamp(s, 0, 1); l = clamp(l, 0, 1);
        var r, g, b;
        if (s === 0) { r = g = b = l; }
        else {
            var hue2rgb = function (p, q, t) {
                if (t < 0) t += 1; if (t > 1) t -= 1;
                if (t < 1 / 6) return p + (q - p) * 6 * t;
                if (t < 1 / 2) return q;
                if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
                return p;
            };
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = hue2rgb(p, q, h + 1 / 3);
            g = hue2rgb(p, q, h);
            b = hue2rgb(p, q, h - 1 / 3);
        }
        return rgbToHex(r * 255, g * 255, b * 255);
    }
    function mix(hexA, hexB, t) {
        var a = hexToRgb(hexA), b = hexToRgb(hexB);
        return rgbToHex(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t);
    }
    function luminance(hex) {
        var c = hexToRgb(hex);
        var f = function (v) { v /= 255; return v <= 0.03928 ? v / 12.92 : Math.pow((v + 0.055) / 1.055, 2.4); };
        return 0.2126 * f(c.r) + 0.7152 * f(c.g) + 0.0722 * f(c.b);
    }
    function contrast(hexA, hexB) {
        var l1 = luminance(hexA), l2 = luminance(hexB);
        var hi = Math.max(l1, l2), lo = Math.min(l1, l2);
        return (hi + 0.05) / (lo + 0.05);
    }
    function getVar(name) { return getComputedStyle(root).getPropertyValue(name).trim(); }

    /* ---------------- Accent ramp ---------------- */
    function applyAccent(base) {
        var rgb = hexToRgb(base);
        var hsl = rgbToHsl(rgb.r, rgb.g, rgb.b);
        var set = function (k, v) { root.style.setProperty(k, v); };
        set("--orange", base);
        set("--orange-hover-start", hslToHex(hsl.h, hsl.s * 0.95, clamp(hsl.l * 0.80, 0.12, 0.9)));
        set("--orange-hover-end", hslToHex(hsl.h, hsl.s * 1.08, clamp(hsl.l * 1.16, 0.12, 0.95)));
        set("--orange-soft", hslToHex(hsl.h, hsl.s, clamp(hsl.l * 1.34, 0.12, 0.92)));
        set("--orange-deep", hslToHex(hsl.h, clamp(hsl.s * 1.02, 0, 1), clamp(hsl.l * 0.72, 0.12, 0.9)));
        set("--orange-wash", mix(base, "#ffffff", 0.92));
        set("--orange-rgb", rgb.r + ", " + rgb.g + ", " + rgb.b);
        document.getElementById("accentPicker").value = base;
        document.getElementById("accentHex").textContent = base.toLowerCase();
        markPresets(base);
        renderContrast();
        renderSwatches();
    }
    function applyChrome(name, triplet, hex) {
        root.style.setProperty(name, hex);
        if (triplet) {
            var c = hexToRgb(hex);
            root.style.setProperty(triplet, c.r + ", " + c.g + ", " + c.b);
        }
    }

    /* ---------------- Contrast readout ---------------- */
    function padChip(bg, fg, text) {
        return '<span class="demo-pad-chip" style="background:' + bg + ';color:' + fg + ';border-color:' + bg + '">' + text + '</span>';
    }
    function badge(ratio, min) {
        var pass = ratio >= min;
        return '<span class="demo-badge-aa ' + (pass ? "demo-aa-pass" : "demo-aa-fail") + '">' +
            ratio.toFixed(2) + ":1 " + (pass ? "AA" : "fail") + "</span>";
    }
    function renderContrast() {
        var accent = getVar("--orange");
        var navyDeep = getVar("--navy-deep");
        var white = "#ffffff";
        var box = document.getElementById("contrastBox");
        if (!box) return;
        box.innerHTML =
            '<div class="demo-contrast-row"><span class="demo-pad">' + padChip(accent, navyDeep, "Aa") + 'navy text on accent</span>' + badge(contrast(accent, navyDeep), 4.5) + '</div>' +
            '<div class="demo-contrast-row"><span class="demo-pad">' + padChip(accent, white, "Aa") + 'white text on accent</span>' + badge(contrast(accent, white), 4.5) + '</div>' +
            '<div class="demo-contrast-row"><span class="demo-pad">' + padChip(white, navyDeep, "Aa") + 'accent vs white (edge)</span>' + badge(contrast(accent, white), 3) + '</div>';
    }

    /* ---------------- Swatch grid ---------------- */
    var SWATCHES = [
        { n: "--orange", u: "Primary action" },
        { n: "--orange-deep", u: "Pressed / rim" },
        { n: "--orange-wash", u: "Accent wash" },
        { n: "--navy-deep", u: "Chrome base" },
        { n: "--navy-mid", u: "Chrome mid" },
        { n: "--ink-raised", u: "Raised chrome" },
        { n: "--steel", u: "Chrome cyan" },
        { n: "--steel-deep", u: "Links / info" },
        { n: "--steel-wash", u: "Info wash" },
        { n: "--success", u: "Success" },
        { n: "--warning", u: "Warning" },
        { n: "--error", u: "Error" },
        { n: "--violet", u: "Violet" },
        { n: "--info", u: "Info" },
        { n: "--neutral", u: "Neutral" },
        { n: "--page-bg", u: "Page canvas" },
        { n: "--surface", u: "Surface" },
        { n: "--border", u: "Border" },
        { n: "--chart-1", u: "Chart 1" },
        { n: "--chart-2", u: "Chart 2" },
        { n: "--chart-3", u: "Chart 3" },
        { n: "--chart-4", u: "Chart 4" },
        { n: "--chart-5", u: "Chart 5" },
        { n: "--chart-6", u: "Chart 6" }
    ];
    function renderSwatches() {
        var host = document.getElementById("swatchGrid");
        if (!host) return;
        host.innerHTML = SWATCHES.map(function (s) {
            var val = getVar(s.n);
            return '<div class="demo-swatch">' +
                '<div class="demo-swatch-chip" style="background:' + val + '"></div>' +
                '<div class="demo-swatch-meta"><div class="demo-swatch-name">' + s.n + '</div>' +
                '<div class="demo-swatch-hex">' + val + '</div>' +
                '<div class="demo-swatch-hex">' + s.u + '</div></div></div>';
        }).join("");
    }

    /* ---------------- Presets ---------------- */
    var PRESETS = [
        { name: "Brand orange", hex: "#f58220" },
        { name: "Amber", hex: "#f59e0b" },
        { name: "Coral", hex: "#ea580c" },
        { name: "Rose", hex: "#e11d48" },
        { name: "Magenta", hex: "#bb1088" },
        { name: "Violet", hex: "#7c3aed" },
        { name: "Indigo", hex: "#4f46e5" },
        { name: "Sapphire", hex: "#0284c7" },
        { name: "Teal", hex: "#0f766e" },
        { name: "Emerald", hex: "#047857" }
    ];
    var currentAccent = "#f58220";
    function renderPresets() {
        var host = document.getElementById("presets");
        host.innerHTML = PRESETS.map(function (p) {
            var pressed = p.hex.toLowerCase() === currentAccent.toLowerCase();
            return '<button type="button" class="demo-preset" data-hex="' + p.hex + '" title="' + p.name + '" aria-label="' + p.name + '" aria-pressed="' + pressed + '" style="background:' + p.hex + '"></button>';
        }).join("");
    }
    function markPresets() {
        var buttons = document.querySelectorAll(".demo-preset");
        for (var i = 0; i < buttons.length; i++) {
            buttons[i].setAttribute("aria-pressed", buttons[i].getAttribute("data-hex").toLowerCase() === currentAccent.toLowerCase());
        }
    }

    /* ---------------- Screen switching ---------------- */
    function showScreen(name) {
        document.querySelectorAll(".demo-screen").forEach(function (s) {
            s.hidden = s.getAttribute("data-screen") !== name;
        });
        document.querySelectorAll(".demo-tab").forEach(function (t) {
            if (t.classList.contains("dropdown-item")) return;
            t.classList.toggle("active", t.getAttribute("data-screen") === name);
        });
        window.scrollTo({ top: 0, behavior: "smooth" });
    }

    /* ---------------- Wiring ---------------- */
    function init() {
        renderPresets();
        renderSwatches();
        renderContrast();

        document.addEventListener("click", function (e) {
            var tab = e.target.closest(".demo-tab");
            if (tab && tab.getAttribute("data-screen")) {
                showScreen(tab.getAttribute("data-screen"));
                document.getElementById("templatesDd").classList.remove("open");
                return;
            }
            var preset = e.target.closest(".demo-preset");
            if (preset) {
                currentAccent = preset.getAttribute("data-hex");
                document.getElementById("accentPicker").value = currentAccent;
                applyAccent(currentAccent);
                renderPresets();
                return;
            }
            var opener = e.target.closest("[data-modal]");
            if (opener) { openModal(opener.getAttribute("data-modal")); return; }
            if (e.target.closest("[data-close]")) { closeModals(); return; }

            var ddToggle = e.target.closest("#templatesDd .dropdown-toggle");
            if (ddToggle) {
                document.getElementById("templatesDd").classList.toggle("open");
                return;
            }
            if (!e.target.closest("#templatesDd")) {
                document.getElementById("templatesDd").classList.remove("open");
            }
        });

        // Accent + chrome pickers
        document.getElementById("accentPicker").addEventListener("input", function (e) {
            currentAccent = e.target.value;
            document.getElementById("accentHex").textContent = currentAccent;
            applyAccent(currentAccent);
            markPresets();
        });
        document.getElementById("navyDeepPicker").addEventListener("input", function (e) {
            applyChrome("--navy-deep", "--navy-deep-rgb", e.target.value);
            renderSwatches();
            renderContrast();
        });
        document.getElementById("navyMidPicker").addEventListener("input", function (e) {
            applyChrome("--navy-mid", "--navy-mid-rgb", e.target.value);
            renderSwatches();
        });
        document.getElementById("steelPicker").addEventListener("input", function (e) {
            applyChrome("--steel", "--steel-rgb", e.target.value);
            renderSwatches();
        });

        document.getElementById("resetTheme").addEventListener("click", resetTheme);

        document.getElementById("copyVars").addEventListener("click", function () {
            var lines = ["--orange", "--orange-hover-start", "--orange-hover-end", "--orange-soft", "--orange-deep", "--orange-wash", "--orange-rgb",
                "--navy-deep", "--navy-deep-rgb", "--navy-mid", "--navy-mid-rgb", "--steel", "--steel-rgb"];
            var text = lines.map(function (k) { return "  " + k + ": " + getVar(k) + ";"; }).join("\n");
            if (navigator.clipboard) {
                navigator.clipboard.writeText(":root {\n" + text + "\n}").then(function () {
                    flash("Copied CSS variables to clipboard.");
                });
            } else {
                flash("Clipboard not available.");
            }
        });

        var fab = document.getElementById("themeFab");
        var panel = document.getElementById("themePanel");
        fab.addEventListener("click", function () { panel.classList.toggle("open"); });
        document.getElementById("modalBackdrop").addEventListener("click", closeModals);

        document.addEventListener("keydown", function (e) {
            if (e.key === "Escape") { panel.classList.remove("open"); closeModals(); return; }
            var t = e.target;
            if ((t.tagName === "INPUT" || t.tagName === "SELECT" || t.tagName === "TEXTAREA")) return;
            if (e.key === "t" || e.key === "T") panel.classList.toggle("open");
        });
    }

    function openModal(id) {
        document.getElementById("modalBackdrop").classList.add("show");
        document.getElementById(id).classList.add("show");
    }
    function closeModals() {
        document.getElementById("modalBackdrop").classList.remove("show");
        document.querySelectorAll(".demo-modal").forEach(function (m) { m.classList.remove("show"); });
    }
    function resetTheme() {
        ["--orange", "--orange-hover-start", "--orange-hover-end", "--orange-soft", "--orange-deep", "--orange-wash", "--orange-rgb",
            "--navy-deep", "--navy-deep-rgb", "--navy-mid", "--navy-mid-rgb", "--steel", "--steel-rgb"
        ].forEach(function (k) { root.style.removeProperty(k); });
        currentAccent = "#f58220";
        document.getElementById("navyDeepPicker").value = "#0a192f";
        document.getElementById("navyMidPicker").value = "#112240";
        document.getElementById("steelPicker").value = "#38bdf8";
        renderPresets();
        renderSwatches();
        renderContrast();
        document.getElementById("accentPicker").value = currentAccent;
        document.getElementById("accentHex").textContent = currentAccent;
    }
    function flash(msg) {
        var el = document.createElement("div");
        el.textContent = msg;
        el.style.cssText = "position:fixed;left:50%;bottom:28px;transform:translateX(-50%);z-index:1070;" +
            "background:var(--navy-deep);color:#fff;padding:10px 16px;border-radius:999px;font-size:.8rem;" +
            "box-shadow:0 18px 40px -18px rgba(0,0,0,.7)";
        document.body.appendChild(el);
        setTimeout(function () { el.remove(); }, 1800);
    }

    document.addEventListener("DOMContentLoaded", init);
})();
