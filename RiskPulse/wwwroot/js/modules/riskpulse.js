/* ============================================================================
   Risk Pulse shared JS module — single home for cross-page UI + AJAX helpers.
   Referenced from Views/Shared/_Layout.cshtml; page views call the RiskPulse.*
   namespace and keep only their own validation, column configs, and wiring.
   Requires jQuery + SweetAlert2 (Toast) + Bootstrap 5 bundle + Select2 at
   call time (selects are auto-enhanced with the rp theme on page load).
   ============================================================================ */
window.RiskPulse = (function ($, Swal) {
    'use strict';

    var genericError = 'An error occurred. Please try again.';

    // Shared toast (top-end, auto-dismiss) used by toastSuccess/toastError.
    var Toast = Swal.mixin({
        toast: true,
        position: 'top-end',
        showConfirmButton: false,
        timer: 3000,
        timerProgressBar: true
    });

    function toastSuccess(title) {
        Toast.fire({ icon: 'success', title: title });
    }

    function toastError(title) {
        Toast.fire({ icon: 'error', title: title });
    }

    function toastGenericError() {
        toastError(genericError);
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

    // Shared transport for postJson/getJson; owns ApiResponse routing + lock.
    // handlers.complete (optional) runs after every outcome (success/fail/http
    // error) so callers can reset their own UI state alongside the unlock.
    function requestJson(ajaxOpts, handlers, $trigger) {
        var locked = $trigger && $trigger.length ? $trigger : null;
        if (locked && locked.prop('disabled')) return;
        if (locked) locked.prop('disabled', true);
        $.ajax($.extend(ajaxOpts, {
            success: function (res) {
                if (res.success) { handlers.success(res); } else { handlers.fail(res); }
            },
            error: function () { toastGenericError(); },
            complete: function () {
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
    // >=10 options; dropdowns inside modals attach to the modal so they can
    // never be clipped or hidden behind its stacking context.
    function initSelect2(selector) {
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

    // Shared delete-confirmation dialog; calls onConfirmed only on confirm.
    function confirmDelete(title, text, onConfirmed) {
        Swal.fire({
            title: title,
            text: text,
            icon: 'warning',
            showCancelButton: true,
            customClass: { confirmButton: 'rp-swal-confirm' },
            confirmButtonText: 'Delete',
            cancelButtonText: 'Cancel'
        }).then(function (result) {
            if (result.isConfirmed) onConfirmed();
        });
    }

    // DataTables init with the shared client-side defaults. cfg must supply
    // ajax, columns, and order (dataSrc: 'data' is DataTables' own default).
    function initGrid($table, cfg) {
        return $table.DataTable($.extend({
            pageLength: 10,
            lengthMenu: [10, 25, 50],
            language: {
                emptyTable: 'No records found.',
                zeroRecords: 'No records found.'
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
            case 'Active': return pill('Active', 'success');
            case 'Locked': return pill('Locked', 'warning');
            case 'Draft': return pill('Draft', 'neutral');
            default: return pill('Inactive', 'neutral');
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

    // Marks a field as invalid: toast + red highlight + focus. The highlight
    // clears on the field's first input/change. A missing $el (rules that map
    // to no single field) still shows the toast and returns false. Enhanced
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
        clearFieldErrors: clearFieldErrors
    };
})(jQuery, Swal);
