(function (window, document, $) {
    'use strict';

    var markerClass = 'admin-required-marker';

    function labelHasMarker($label) {
        return $label.find('.' + markerClass).length > 0;
    }

    function appendMarker($label) {
        if (!$label || !$label.length || labelHasMarker($label)) {
            return;
        }

        $label.append(document.createTextNode(' '));
        $label.append($('<span>', {
            'class': markerClass,
            'aria-hidden': 'true',
            text: '*'
        }));
    }

    function markRequiredFieldLabels(root) {
        var $root = root ? $(root) : $(document);
        var selector = 'input[data-val-required], select[data-val-required], textarea[data-val-required]';

        $root.find(selector).each(function () {
            var id = this.id;
            if (!id) {
                return;
            }

            appendMarker($root.find('label[for="' + id.replace(/"/g, '\\"') + '"]'));
        });
    }

    window.egMarkAdminRequiredLabels = markRequiredFieldLabels;

    if ($) {
        $(function () {
            markRequiredFieldLabels(document);
        });
    }
}(window, document, window.jQuery));
