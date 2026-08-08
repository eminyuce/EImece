/* Crizal theme UI behaviors — complements template main.js */
(function (window, document, $) {
    'use strict';

    function initCrizalUi() {
        var body = document.body;
        if (!body || body.getAttribute('data-design') !== 'crizal') {
            return;
        }

        // Prevent main.js from swapping to missing inner logo paths
        $(window).off('scroll.crizalLogoFix');
        $(window).on('scroll.crizalLogoFix', function () {
            var $logo = $('#logo');
            if ($logo.length) {
                $logo.attr('src', '/images/logo.jpg');
            }
        });

        // Enter key submits overlay search
        $(document).on('keypress', '.search-form_input', function (e) {
            if (e.which === 13) {
                e.preventDefault();
                onClickSearch();
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
