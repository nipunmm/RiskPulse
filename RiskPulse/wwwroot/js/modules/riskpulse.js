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
        if (kind) return pill(state.text, kind);
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
        showLoading: showLoading,
        hideLoading: hideLoading
    };
})(jQuery, Swal);
