// ============================================================
// STASIS ENTERPRISE — Unified Validation Layer
// One consistent validation mechanism (jQuery Validate) with
// theme-matched inline field errors, form summaries, and
// server-error mapping for the whole system.
// ============================================================
(function ($) {
    'use strict';

    // ---- Global defaults matching the Stasis theme ----
    $.validator.setDefaults({
        errorClass: 'input-validation-error',
        validClass: 'input-validation-valid',
        errorElement: 'div',
        errorPlacement: function (error, element) {
            var $element = $(element);

            // Select2: place after the rendered container
            if ($element.hasClass('select2-hidden-accessible')) {
                error.insertAfter($element.next('.select2-container'));
                return;
            }

            // Checkboxes / radios: place after the form-check wrapper
            if ($element.is(':checkbox, :radio')) {
                var $check = $element.closest('.form-check');
                if ($check.length) {
                    error.insertAfter($check);
                    return;
                }
            }

            // Default: directly after the input
            error.insertAfter($element);
        },
        highlight: function (element) {
            var $element = $(element);
            $element.addClass('input-validation-error').removeClass('input-validation-valid');

            // Reflect invalid state on the rendered Select2 container
            if ($element.hasClass('select2-hidden-accessible')) {
                $element.next('.select2-container').find('.select2-selection')
                    .addClass('input-validation-error').removeClass('input-validation-valid');
            }
        },
        unhighlight: function (element) {
            var $element = $(element);
            $element.removeClass('input-validation-error').addClass('input-validation-valid');

            if ($element.hasClass('select2-hidden-accessible')) {
                $element.next('.select2-container').find('.select2-selection')
                    .removeClass('input-validation-error').addClass('input-validation-valid');
            }
        },
        messages: {
            required: 'This field is required.',
            email: 'Enter a valid email address.',
            minlength: 'Must be at least {0} characters.',
            maxlength: 'Must not exceed {0} characters.',
            number: 'Enter a valid number.',
            digits: 'Enter digits only.',
            equalTo: 'Values must match.'
        }
    });

    // Generic group rule: at least one checkbox in a same-named group must be checked.
    $.validator.addMethod('requireOnePermission', function (value, element) {
        return $(element.form).find('[name="' + element.name + '"]:checked').length > 0;
    }, 'At least one option must be selected.');

    // ---- Initialize a validated form ----
    // options:
    //   summarySelector : selector of the form-level .validation-summary element
    //   customRules     : per-field validator rules
    //   customMessages  : per-field validator messages
    //   onSubmit        : function($form, event) — called only when the form is valid
    function initValidatedForm(formId, options) {
        options = options || {};
        var $form = $('#' + formId);
        var $summary = options.summarySelector ? $(options.summarySelector) : $();

        if (!$form.length) {
            if (window.console) console.warn('[validation] Form #' + formId + ' not found.');
            return null;
        }

        var validator = $form.validate($.extend(true, {}, {
            ignore: '', // validate hidden fields (e.g. Select2 sources)
            showErrors: function (errorMap, errorList) {
                this.defaultShowErrors();

                if ($summary.length) {
                    var messages = $.map(errorList, function (e) { return e.message; });
                    if (messages.length) {
                        $summary
                            .html('<ul>' + $.map(messages, function (m) {
                                return '<li>' + m + '</li>';
                            }).join('') + '</ul>')
                            .removeClass('d-none')
                            .attr('role', 'alert');
                    } else {
                        $summary.addClass('d-none').empty();
                    }
                }
            },
            submitHandler: function (form, event) {
                if (event) event.preventDefault();
                if ($summary.length) {
                    $summary.addClass('d-none').empty();
                }
                if (typeof options.onSubmit === 'function') {
                    options.onSubmit($form, event);
                } else {
                    form.submit();
                }
            }
        }, {
            rules: options.customRules || {},
            messages: options.customMessages || {}
        }));

        $form.data('validator', validator);
        return validator;
    }

    // ---- Map server-side validation errors onto fields ----
    // Accepts { fieldName: 'msg' } or ASP.NET Core ModelState
    // ({ 'Username': ['msg'] }) shapes. Keys are matched by name after
    // normalizing casing and dropping any dotted prefix (e.g. 'user.Username').
    function applyServerErrors(validator, errorObj) {
        if (!validator || !errorObj) return;

        var modelState = errorObj.modelState || errorObj.ModelState || errorObj;
        var errors = {};

        $.each(modelState, function (key, messages) {
            var messagesArray = $.isArray(messages) ? messages : [messages];
            if (!messagesArray.length) return;

            // Normalize: 'user.Username' -> baseName 'Username' -> fieldName 'username'
            var baseName = String(key).split('.').pop();
            var fieldName = baseName.charAt(0).toLowerCase() + baseName.slice(1);
            var $field = $(validator.currentForm)
                .find('[name="' + baseName + '"], [name="' + fieldName + '"]').first();

            if ($field.length) {
                errors[fieldName] = messagesArray[0];
            }
        });

        validator.showErrors(errors);
    }

    // ---- Unified error toast (SweetAlert2 with fallback) ----
    // `Toast` is declared as a global `const` in the layout/login scripts,
    // so it is checked by name rather than via window.
    function showValidationToast(message, title) {
        var opts = { icon: 'error', title: title || 'Unable to complete', text: message };
        if (typeof Toast !== 'undefined' && typeof Toast.fire === 'function') {
            Toast.fire(opts);
        } else if (window.Swal) {
            window.Swal.fire(opts);
        }
    }

    // ---- Public API ----
    window.initValidatedForm = initValidatedForm;
    window.applyServerErrors = applyServerErrors;
    window.showValidationToast = showValidationToast;

})(jQuery);
