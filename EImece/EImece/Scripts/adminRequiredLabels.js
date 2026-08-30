(function (window, document, $) {
    'use strict';

    var markerClass = 'admin-required-marker';

    function labelTextEndsWithAsterisk($label) {
        var text = $.trim($label.text());
        return text.length > 0 && text.charAt(text.length - 1) === '*';
    }

    function labelHasMarker($label) {
        if ($label.find('.' + markerClass).length > 0) {
            return true;
        }

        if (labelTextEndsWithAsterisk($label)) {
            return true;
        }

        // Older AdminLabelFor builds appended the marker as a sibling after </label>.
        var $orphan = $label.next('.' + markerClass);
        if ($orphan.length) {
            $label.append(document.createTextNode(' '));
            $label.append($orphan.detach());
            return true;
        }

        return false;
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
