/* Crizal theme UI behaviors only — no business logic */
(function (window, document, $) {
    'use strict';

    function initCrizalUi() {
        var body = document.body;
        if (!body || body.getAttribute('data-design') !== 'crizal') {
            return;
        }

        // Smooth scroll for in-page anchors
        $(document).on('click', 'a.crizal-scroll-top, .scroll-to-top-btn', function (e) {
            var href = this.getAttribute('href');
            if (href && href.charAt(0) === '#') {
                var target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                }
            }
        });

        // Mobile: close navbar after link click
        $(document).on('click', '.crizal-navbar .navbar-nav .nav-link:not(.dropdown-toggle)', function () {
            var collapse = document.querySelector('.crizal-navbar .navbar-collapse.show');
            if (collapse && window.jQuery && $.fn.collapse) {
                $(collapse).collapse('hide');
            }
        });
    }

    if (window.jQuery) {
        $(initCrizalUi);
    } else if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initCrizalUi);
    } else {
        initCrizalUi();
    }
})(window, document, window.jQuery);
