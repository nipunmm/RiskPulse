/* ============================================================================
   Risk Pulse shared JS module — single home for cross-page UI + AJAX helpers.
   Referenced from Views/Shared/_Layout.cshtml; page views call the RiskPulse.*
   namespace and keep only their own validation, column configs, and wiring.
   Requires jQuery + SweetAlert2 + Bootstrap 5 bundle + Select2 at call time
   (selects are auto-enhanced with the rp theme on page load). Feedback is
   delivered via centered SweetAlert2 dialogs (no toast pills).
   ============================================================================ */
window.RiskPulse = (function ($, Swal) {
    'use strict';

    var genericError = 'An error occurred. Please try again.';

    // Shared SweetAlert2 dialog (centered, OK action). Success/error helpers
    // below add the icon; title + optional text make up the message body.
    function alertDialog(icon, title, text) {
        return Swal.fire({
            icon: icon,
            title: title,
            text: text || '',
            confirmButtonText: 'OK',
            customClass: {
                popup: 'rp-swal-alert',
                title: 'rp-swal-alert',
                htmlContainer: 'rp-swal-alert'
            }
        });
    }

    function toastSuccess(title, text) {
        return alertDialog('success', title, text);
    }

    function toastError(title, text) {
        return alertDialog('error', title, text);
    }

    function toastGenericError() {
        return alertDialog('error', genericError);
    }

    // Escapes &, <, >, ", ' so DB text can be safely interpolated into HTML
    // strings (DataTables render callbacks build HTML from grid row data).
    function escapeHtml(value) {
        return String(value === null || value === undefined ? '' : value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    }

    // POSTs a JSON ApiResponse<T> request; routes res.success -> success(res),
    // res.success === false -> fail(res), and HTTP errors -> toastGenericError.
    // When $trigger is supplied it is disabled while the request is in flight
    // (and re-enabled in complete) so a second click cannot double-submit.
    function postJson(url, payload, handlers, $trigger) {
        return requestJson({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload)
        }, handlers, $trigger);
    }

    // GET variant with the same success/fail routing and optional trigger lock.
    function getJson(url, data, handlers, $trigger) {
        return requestJson({ url: url, type: 'GET', data: data }, handlers, $trigger);
    }

    // Shared loading overlay (created on first use, covers modals too).
    // requestJson shows it around every transport call; handlers.overlay
    // === false opts a call out (e.g. silent background refreshes).
    function showLoading() {
        var $overlay = $('#rp-loading-overlay');
        if (!$overlay.length) {
            $overlay = $(
                '<div id="rp-loading-overlay" role="status" aria-live="polite">' +
                '  <div class="rp-loading-card">' +
                '    <span class="rp-spinner" aria-hidden="true"></span>' +
                '    <span class="rp-loading-text">Loading…</span>' +
                '  </div>' +
                '</div>'
            ).appendTo(document.body);
        }
        $overlay.addClass('show');
    }

    function hideLoading() {
        $('#rp-loading-overlay').removeClass('show');
    }

    // Shared transport for postJson/getJson; owns ApiResponse routing + lock.
    // handlers.complete (optional) runs after every outcome (success/fail/http
    // error) so callers can reset their own UI state alongside the unlock.
    function requestJson(ajaxOpts, handlers, $trigger) {
        var locked = $trigger && $trigger.length ? $trigger : null;
        if (locked && locked.prop('disabled')) return;
        if (locked) locked.prop('disabled', true);
        var withOverlay = handlers.overlay !== false;
        if (withOverlay) showLoading();
        $.ajax($.extend(ajaxOpts, {
            success: function (res) {
                if (res.success) { handlers.success(res); } else { handlers.fail(res); }
            },
            error: function () { toastGenericError(); },
            complete: function () {
                if (withOverlay) hideLoading();
                if (locked) locked.prop('disabled', false);
                if (handlers.complete) handlers.complete();
            }
        }));
    }

    // Builds a JSON payload from a form. checkboxes are emitted as booleans,
    // numbers are coerced with || 0 so the JSON body sends numbers, not strings.
    function serializeForm($form, opts) {
        opts = opts || {};
        var payload = {};
        $.each($form.serializeArray(), function (i, f) { payload[f.name] = f.value; });

        (opts.checkboxes || []).forEach(function (name) {
            var $el = $form.find('[name="' + name + '"]');
            if ($el.length) payload[name] = $el.is(':checked');
        });

        (opts.numbers || []).forEach(function (name) {
            if (payload.hasOwnProperty(name)) payload[name] = +payload[name] || 0;
        });

        return payload;
    }

    // Populates a select with options from a value/label array.
    // Optional placeholderValue/placeholderLabel prepend a prompt option.
    // Items may carry an optional `color` (hex -> data-color swatch) or
    // `kind` (success/warning/danger/neutral -> data-kind pill); both are read
    // by the shared select2 template renderer. After populating a select that
    // is already enhanced, change.select2 refreshes its display without
    // firing any app-level change handlers.
    function populateSelect($select, data, placeholderValue, placeholderLabel) {
        var $options = $.map(data, function (o) {
            var $option = $('<option>').val(o.value).text(o.label);
            if (o.color) $option.attr('data-color', o.color);
            if (o.kind) $option.attr('data-kind', o.kind);
            return $option;
        });
        if (placeholderValue !== undefined) {
            $options.unshift($('<option>').val(placeholderValue).text(placeholderLabel));
        }
        $select.empty().append($options);
        if ($select.hasClass('select2-hidden-accessible')) {
            $select.trigger('change.select2');
        }
    }

    // --- Select2 (rp theme) ---

    // Shared result/selection renderer: hex data-color -> swatch square,
    // data-kind -> pill chip (statusPill palette), plain text otherwise.
    // Markup must be returned as a jQuery object — select2 runs strings
    // through escapeMarkup, so any HTML string would render as literal text.
    function renderSelect2Option(state) {
        if (!state.id) return state.text;
        var $el = $(state.element);
        var kind = $el.data('kind');
        var color = $el.data('color');
        if (color) {
            return $('<span class="rp-select-option-label">').append(
                $('<span class="rp-select-swatch">').css('background-color', color),
                document.createTextNode(state.text)
            );
        }
        if (kind) return $(pill(state.text, kind));
        return state.text;
    }

    // Enhances selects with the rp-themed select2. Search appears only at
    // >=10 options by default; pass { minimumResultsForSearch: 0 } in
    // overrides to always show it. Dropdowns inside modals attach to the
    // modal so they can never be clipped or hidden behind its stacking context.
    function initSelect2(selector, overrides) {
        $(selector).each(function () {
            var $select = $(this);
            if ($select.data('select2')) return;

            var opts = {
                theme: 'rp',
                width: '100%',
                minimumResultsForSearch: 10,
                templateResult: renderSelect2Option,
                templateSelection: renderSelect2Option,
                language: { noResults: function () { return 'No matching options.'; } }
            };

            var $modal = $select.closest('.modal');
            opts.dropdownParent = $modal.length ? $modal : $(document.body);

            var placeholder = $select.attr('data-placeholder');
            if (placeholder) opts.placeholder = placeholder;

            if (overrides) $.extend(opts, overrides);

            $select.select2(opts);
        });
    }

    // Programmatic modal open/close via the Bootstrap 5 API (no $.fn.modal).
    function showModal(id) {
        bootstrap.Modal.getOrCreateInstance(document.getElementById(id)).show();
    }

    function hideModal(formEl) {
        bootstrap.Modal.getOrCreateInstance($(formEl).closest('.modal')[0]).hide();
    }

    // Shared confirmation dialog (guide: "Please Confirm" title, Yes/No
    // actions); calls onConfirmed only on confirm. The caller's title/text
    // become the question line so context is never lost.
    function confirmDelete(title, text, onConfirmed) {
        Swal.fire({
            title: 'Please Confirm',
            text: $.trim((title || '') + ' ' + (text || '')),
            icon: 'warning',
            showCancelButton: true,
            customClass: { popup: 'rp-swal-confirm', title: 'rp-swal-confirm' },
            confirmButtonText: 'Yes',
            cancelButtonText: 'No'
        }).then(function (result) {
            if (result.isConfirmed) onConfirmed();
        });
    }

    // DataTables init with the shared client-side defaults: paginated at
    // 10 rows/page with a page-size selector (10/25/50/All); the info line
    // shows the current window across the filtered total. cfg must supply
    // ajax, columns, and order (dataSrc: 'data' is DataTables' own default).
    function initGrid($table, cfg) {
        return $table.DataTable($.extend({
            pageLength: 10,
            lengthMenu: [[10, 25, 50, -1], [10, 25, 50, 'All']],
            lengthChange: true,
            paging: true,
            info: true,
            dom: "<'rp-dt-top'lf><'rp-dt-body't><'rp-dt-bottom'ip>",
            language: {
                emptyTable: 'No records found.',
                zeroRecords: 'No records found.',
                info: 'Showing _START_ to _END_ of _TOTAL_ records',
                infoEmpty: 'No records found',
                infoFiltered: '(filtered from _TOTAL_ total)',
                lengthMenu: 'Show _MENU_ entries per page',
                search: '',
                paginate: { first: '«', previous: '‹', next: '›', last: '»' }
            }
        }, cfg));
    }

    // Generic pill builder; label is always escaped. Optional kind -> rp-pill-{kind},
    // optional icon -> FontAwesome icon name rendered before the label.
    function pill(label, kind, icon) {
        var cls = kind ? ' rp-pill-' + kind : '';
        var html = icon ? '<i class="fas fa-' + icon + ' me-1"></i>' : '';
        return '<span class="rp-pill' + cls + '">' + html + escapeHtml(label) + '</span>';
    }

    // Maps a template/user/assessment status to its RAG pill markup.
    function statusPill(status) {
        switch (status) {
            case 'Active': return pill('Active', 'success', 'circle-check');
            case 'Locked': return pill('Locked', 'warning', 'triangle-exclamation');
            case 'Draft': return pill('Draft', 'neutral', 'circle-minus');
            default: return pill('Inactive', 'neutral', 'circle-minus');
        }
    }

    // Maps a status value to its RAG kind (success/warning/neutral); used by
    // views to tag <option>s with data-kind so the rp theme renders them as
    // colored pill chips inside select2 dropdowns.
    function statusKind(status) {
        switch (status) {
            case 'Active': return 'success';
            case 'Locked': return 'warning';
            default: return 'neutral';
        }
    }

    // Marks a field as invalid: error dialog + red highlight + focus. The
    // highlight clears on the field's first input/change. A missing $el (rules
    // that map to no single field) still shows the dialog and returns false. Enhanced
    // selects hide their <select>, so the error class lands on the visible
    // rp-themed select2 container instead; picking an option fires the hidden
    // select's change event, which clears it.
    function validationError($el, message) {
        toastError(message);
        if ($el && $el.length) {
            var data = $el.is('select') ? $el.data('select2') : null;
            var enhanced = !!(data && data.$container);
            var $target = enhanced ? data.$container : $el;
            $target.addClass('rp-input-error');
            $el.one('input change', function () { $target.removeClass('rp-input-error'); });
            if (enhanced) { $el.select2('focus'); } else { $el.focus(); }
        }
        return false;
    }

    // Strips the error highlight from every field in a form (used on modal
    // reset), including enhanced select2 containers.
    function clearFieldErrors($form) {
        $form.find('.rp-input-error').removeClass('rp-input-error');
    }

    // Shared compact multi-select: a dense searchable checkbox list with a live
    // count, Select all/Clear, and an optional per-row preview action. Suited to
    // modal/admin screens where the card picker is too heavy.
    // cfg: { containerSel, options:[{value,label,code}], selected?, searchPlaceholder,
    //        emptyText, previewOpen(id, $trigger), onSelectionChange(ids) }
    // Returns { selectedIds(), labels(), setSelected(ids), isChecked(id) }.
    function createChecklistPicker(cfg) {
        var options = cfg.options || [];
        var selected = new Set();
        var valid = new Set();
        $.each(options, function (i, o) { valid.add(o.value); });
        (cfg.selected || []).forEach(function (id) { if (valid.has(id)) selected.add(id); });

        var query = '';

        var $root = $(cfg.containerSel);
        var $head = $(
            '<div class="rp-check-picker-head">' +
                '<div class="rp-check-search">' +
                    '<i class="fas fa-magnifying-glass" aria-hidden="true"></i>' +
                    '<input type="text" class="rp-check-search-input" autocomplete="off" />' +
                '</div>' +
                '<div class="rp-check-actions">' +
                    '<button type="button" class="rp-check-mini-btn" data-act="all">Select all</button>' +
                    '<button type="button" class="rp-check-mini-btn" data-act="clear">Clear</button>' +
                '</div>' +
            '</div>'
        );
        var $list = $('<div class="rp-check-list" role="listbox" aria-multiselectable="true"></div>');
        var $foot = $('<div class="rp-check-foot"><span class="rp-check-count"></span></div>');

        $root.empty().addClass('rp-check-picker').append($head, $list, $foot);

        var $search = $head.find('.rp-check-search-input');
        var $count = $foot.find('.rp-check-count');
        $search.attr('placeholder', cfg.searchPlaceholder || 'Search templates...');

        function matches(o) {
            if (!query) return true;
            var q = query.toLowerCase();
            return String(o.label || '').toLowerCase().indexOf(q) >= 0
                || String(o.code || '').toLowerCase().indexOf(q) >= 0;
        }

        function refreshCount() {
            var n = selected.size;
            $count.text(n + ' of ' + options.length + ' selected');
            $count.toggleClass('rp-check-count-ok', n > 0);
        }

        function notify() {
            if (cfg.onSelectionChange) cfg.onSelectionChange(selectedIds());
        }

        function rowFor(o) {
            var $row = $('<label class="rp-check-row">').attr('data-id', o.value);
            $('<input type="checkbox" class="rp-checkbox">')
                .attr('value', o.value)
                .prop('checked', selected.has(o.value))
                .appendTo($row);

            var $main = $('<span class="rp-check-row-main">');
            if (o.code) $('<span class="rp-pill rp-pill-neutral">').text(o.code).appendTo($main);
            $('<span class="rp-check-row-title">').text(o.label || String(o.code || o.value)).appendTo($main);
            $main.appendTo($row);

            if (cfg.previewOpen) {
                $('<button type="button" class="rp-pick-preview" title="Preview template" aria-label="Preview template">')
                    .append('<i class="fas fa-eye"></i>')
                    .attr('data-id', o.value)
                    .appendTo($row);
            }
            return $row;
        }

        function render() {
            $list.empty();
            var shown = $.grep(options, matches);
            if (!shown.length) {
                $('<p class="rp-body-sm rp-text-muted mb-0 rp-check-empty">')
                    .text(options.length ? 'No matches for your search.' : (cfg.emptyText || 'No templates available yet.'))
                    .appendTo($list);
            } else {
                $.each(shown, function (i, o) { $list.append(rowFor(o)); });
            }
            renderCount();
        }

        function renderCount() {
            var n = selected.size;
            $count.text(n + ' of ' + options.length + ' selected');
            $count.toggleClass('rp-check-count-ok', n > 0);
        }

        function selectedIds() {
            var ids = Array.from(selected);
            ids.sort(function (a, b) { return a - b; });
            return ids;
        }

        function labels() {
            var out = [];
            $.each(options, function (i, o) { if (selected.has(o.value)) out.push(o.label || String(o.code || o.value)); });
            return out;
        }

        function setSelected(ids) {
            selected.clear();
            (ids || []).forEach(function (id) { if (valid.has(id)) selected.add(id); });
            query = '';
            $search.val('');
            render();
            if (cfg.onSelectionChange) cfg.onSelectionChange(selectedIds());
        }

        $search.on('input', function () { query = $.trim(this.value); render(); });

        $head.on('click', '.rp-check-mini-btn', function () {
            var act = $(this).data('act');
            $.each($.grep(options, matches), function (i, o) {
                if (act === 'all') selected.add(o.value);
                else selected.delete(o.value);
            });
            render();
            if (cfg.onSelectionChange) cfg.onSelectionChange(selectedIds());
        });

        $list.on('change', 'input.rp-checkbox', function () {
            var id = +this.value;
            if (this.checked) selected.add(id); else selected.delete(id);
            renderCount();
            if (cfg.onSelectionChange) cfg.onSelectionChange(selectedIds());
        });

        $list.on('click', '.rp-pick-preview', function (e) {
            e.preventDefault();
            e.stopPropagation();
            if (cfg.previewOpen) cfg.previewOpen(+$(this).data('id'), $(this));
        });

        render();
        if (cfg.onSelectionChange) cfg.onSelectionChange(selectedIds());

        return {
            selectedIds: selectedIds,
            labels: labels,
            setSelected: setSelected,
            isChecked: function (id) { return selected.has(id); }
        };
    }

    // Shared read-only SAQ/KRI preview modal. cfg: { modalId, bodyId, labelId,
    // saqUrl, kriUrl }. Returns { openSaq(id, $trigger), openKri(id, $trigger) }.
    function templatePreview(cfg) {
        var previewKind = 'saq';
        var previewId = 0;
        var previewTrigger = null;
        var $body = $('#' + cfg.bodyId);

        function openSaq(id, $trigger) { open('saq', id, $trigger, 'SAQ template preview'); }
        function openKri(id, $trigger) { open('kri', id, $trigger, 'KRI template preview'); }

        function open(kind, id, $trigger, title) {
            previewKind = kind;
            previewId = id;
            previewTrigger = $trigger;
            $('#' + cfg.labelId).text(title);
            renderPreviewLoading();
            showModal(cfg.modalId);
            fetchTemplatePreview();
        }

        function fetchUrl() {
            return (previewKind === 'saq' ? cfg.saqUrl : cfg.kriUrl) + '?id=' + previewId;
        }

        function fetchTemplatePreview() {
            getJson(fetchUrl(), null, {
                success: function (res) { renderTemplatePreview(res.data); },
                fail: function (res) { renderPreviewError(res.message || genericError); },
                complete: function () {
                    // HTTP errors only reach the module's generic toast; surface an inline retry too.
                    if ($('#' + cfg.bodyId).find('.rp-preview-loading').length) {
                        renderPreviewError('Unable to load the template preview. Please try again.');
                    }
                }
            });
        }

        function renderPreviewLoading() {
            $('#' + cfg.bodyId).html(
                '<div class="rp-preview-loading text-center p-4">' +
                    '<i class="fas fa-spinner fa-spin fa-2x mb-3"></i>' +
                    '<p class="rp-body-sm rp-text-muted mb-0">Loading template details...</p>' +
                '</div>'
            );
        }

        function renderPreviewError(message) {
            $('#' + cfg.bodyId).html(
                '<div class="rp-preview-error">' +
                    '<i class="fas fa-triangle-exclamation"></i>' +
                    '<p class="rp-body-sm mb-3">' + escapeHtml(message) + '</p>' +
                    '<button type="button" class="rp-btn rp-btn-secondary rp-preview-retry"><i class="fas fa-rotate-right"></i> Retry</button>' +
                '</div>'
            );
        }

        function renderTemplatePreview(data) {
            if (previewKind === 'saq') renderSaqPreview(data);
            else renderKriPreview(data);
        }

        function renderSaqPreview(data) {
            var $body = $('#' + cfg.bodyId).empty();
            var questions = data.questions || [];

            var $meta = $('<div class="rp-preview-meta">');
            if (data.saqCode) $meta.append($(pill(data.saqCode, 'neutral')));
            $meta.append($(pill(data.saqStatus || '—', statusKind(data.saqStatus))));
            $meta.append($(pill(questions.length + (questions.length === 1 ? ' question' : ' questions'), 'info')));
            if (data.assignmentLabel) {
                $meta.append(
                    $('<span class="rp-preview-assignment"><i class="fas fa-sitemap me-1"></i></span>')
                        .append(document.createTextNode(data.assignmentLabel))
                );
            }
            $meta.appendTo($body);

            if (!questions.length) {
                $('<p class="rp-body-sm rp-text-muted mb-0">').text('This template has no questions yet.').appendTo($body);
                return;
            }

            $.each(questions, function (i, q) {
                var $q = $('<div class="rp-preview-question">');
                var $head = $('<div class="rp-preview-question-head">');
                $('<span class="rp-body-sm rp-text-muted fw-semibold rp-preview-qnum">').text('Q' + (i + 1) + '.').appendTo($head);
                $('<span class="rp-body-md">').text(q.questionText).appendTo($head);
                if (q.allowComment) {
                    $('<span class="rp-preview-comment"><i class="fas fa-comment-dots me-1"></i>Comment allowed</span>').appendTo($head);
                }
                $head.appendTo($q);

                var $opts = $('<div class="rp-preview-options">');
                if (q.options && q.options.length) {
                    $.each(q.options, function (j, o) {
                        $('<span class="rp-preview-option">').text(o.optionText).appendTo($opts);
                    });
                } else {
                    $('<span class="rp-body-sm rp-text-muted">').text('No answer options.').appendTo($opts);
                }
                $opts.appendTo($q);
                $q.appendTo($body);
            });
        }

        function renderKriPreview(data) {
            var $body = $('#' + cfg.bodyId).empty();
            var kris = data.kris || [];

            var $meta = $('<div class="rp-preview-meta">');
            if (data.kriCode) $meta.append($(pill(data.kriCode, 'neutral')));
            $meta.append($(pill(data.kriStatus || '—', statusKind(data.kriStatus))));
            $meta.append($(pill(kris.length + (kris.length === 1 ? ' indicator' : ' indicators'), 'info')));
            if (data.assignmentLabel) {
                $meta.append(
                    $('<span class="rp-preview-assignment"><i class="fas fa-sitemap me-1"></i></span>')
                        .append(document.createTextNode(data.assignmentLabel))
                );
            }
            $meta.appendTo($body);

            if (!kris.length) {
                $('<p class="rp-body-sm rp-text-muted mb-0">').text('This template has no indicators yet.').appendTo($body);
                return;
            }

            $.each(kris, function (i, k) {
                var $row = $('<div class="rp-preview-kri-row">');
                $('<span class="rp-body-sm rp-text-muted fw-semibold rp-preview-qnum">').text((i + 1) + '.').appendTo($row);
                $('<span class="rp-body-md rp-preview-kri-desc">').text(k.kriDesc).appendTo($row);

                var $limits = $('<span class="rp-preview-kri-limits">');
                $('<span class="rp-level-dot rp-level-dot--green" role="img" aria-label="Green threshold" title="Green"></span>').appendTo($limits);
                $('<span class="rp-preview-kri-limit">').text(k.greenLimit).appendTo($limits);
                $('<span class="rp-level-dot rp-level-dot--amber" role="img" aria-label="Amber threshold" title="Amber"></span>').appendTo($limits);
                $('<span class="rp-preview-kri-limit">').text(k.amberLimit).appendTo($limits);
                $('<span class="rp-level-dot rp-level-dot--red" role="img" aria-label="Red threshold" title="Red"></span>').appendTo($limits);
                $('<span class="rp-preview-kri-limit">').text(k.redLimit).appendTo($limits);
                $limits.appendTo($row);

                if (k.allowComment) {
                    $('<span class="rp-preview-comment"><i class="fas fa-comment-dots me-1"></i>Comment allowed</span>').appendTo($row);
                }
                $row.appendTo($body);
            });
        }

        $('#' + cfg.bodyId).on('click', '.rp-preview-retry', function () {
            fetchTemplatePreview();
        });

        $('#' + cfg.modalId).on('hidden.bs.modal', function () {
            $('#' + cfg.bodyId).empty();
            if (previewTrigger && previewTrigger.length) previewTrigger.focus();
            previewTrigger = null;
        });

        return {
            openSaq: openSaq,
            openKri: openKri
        };
    }

    // Auto-enhance every styled select on page load (page scripts that
    // populate selects afterwards are covered by populateSelect's refresh;
    // dynamically created selects call initSelect2 explicitly).
    $(function () { initSelect2('select.rp-input'); });

    return {
        genericError: genericError,
        toastSuccess: toastSuccess,
        toastError: toastError,
        toastGenericError: toastGenericError,
        escapeHtml: escapeHtml,
        postJson: postJson,
        getJson: getJson,
        serializeForm: serializeForm,
        populateSelect: populateSelect,
        initSelect2: initSelect2,
        showModal: showModal,
        hideModal: hideModal,
        confirmDelete: confirmDelete,
        initGrid: initGrid,
        pill: pill,
        statusPill: statusPill,
        statusKind: statusKind,
        validationError: validationError,
        clearFieldErrors: clearFieldErrors,
        createChecklistPicker: createChecklistPicker,
        templatePreview: templatePreview,
        showLoading: showLoading,
        hideLoading: hideLoading
    };
})(jQuery, Swal);
