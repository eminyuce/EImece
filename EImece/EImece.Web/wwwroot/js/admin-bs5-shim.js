/**
 * Bootstrap 5 compatibility for legacy adminEimece.js ($.fn.modal).
 */
(function ($, bootstrap) {
    'use strict';
    if (!$ || !bootstrap) return;

    if (typeof $.fn.modal !== 'function') {
        $.fn.modal = function (action) {
            return this.each(function () {
                var el = this;
                var instance = bootstrap.Modal.getOrCreateInstance(el);
                if (action === 'show') instance.show();
                else if (action === 'hide') instance.hide();
                else if (action === 'toggle') instance.toggle();
            });
        };
    }
})(window.jQuery, window.bootstrap);
