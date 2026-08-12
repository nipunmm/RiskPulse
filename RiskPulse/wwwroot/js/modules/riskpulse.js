/* ============================================================================
   Risk Pulse shared JS module — single home for cross-page UI + AJAX helpers.
   Referenced from Views/Shared/_Layout.cshtml; page views call the RiskPulse.*
   namespace and keep only their own validation, column configs, and wiring.
   Requires jQuery + SweetAlert2 (Toast) + Bootstrap 5 bundle at call time.
   ============================================================================ */
window.RiskPulse = (function ($, Swal) {
    'use strict';

    var genericError = 'An error occurred. Please try again.';

    function toastSuccess(title) {
        Toast.fire({ icon: 'success', title: title });
    }

    function toastError(title) {
        Toast.fire({ icon: 'error', title: title });
    }

    function toastGenericError() {
        toastError(genericError);
    }

    // POSTs a JSON ApiResponse<T> request; routes res.success -> success(res),
    // res.success === false -> fail(res), and HTTP errors -> toastGenericError.
    function postJson(url, payload, handlers) {
        $.ajax({
            url: url,
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(payload),
            success: function (res) {
                if (res.success) {
                    handlers.success(res);
                } else {
                    handlers.fail(res);
                }
            },
            error: function () { toastGenericError(); }
        });
    }

    // GET variant with the same success/fail routing.
    function getJson(url, data, handlers) {
        $.ajax({
            url: url,
            type: 'GET',
            data: data,
            success: function (res) {
                if (res.success) {
                    handlers.success(res);
                } else {
                    handlers.fail(res);
                }
            },
            error: function () { toastGenericError(); }
        });
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
    function populateSelect($select, data) {
        $select.empty().append($.map(data, function (o) {
            return $('<option>').val(o.value).text(o.label);
        }));
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
            confirmButtonColor: '#ef4444',
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
            lengthMenu: [10, 25, 50]
        }, cfg));
    }

    return {
        genericError: genericError,
        toastSuccess: toastSuccess,
        toastError: toastError,
        toastGenericError: toastGenericError,
        postJson: postJson,
        getJson: getJson,
        serializeForm: serializeForm,
        populateSelect: populateSelect,
        showModal: showModal,
        hideModal: hideModal,
        confirmDelete: confirmDelete,
        initGrid: initGrid
    };
})(jQuery, Swal);
