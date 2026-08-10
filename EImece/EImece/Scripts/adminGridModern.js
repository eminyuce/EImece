/**
 * Progressive enhancements for Grid.Mvc admin lists.
 * - Per-row status toggles call the SAME /admin/Ajax/Change{grid}OrderingOrState endpoint
 *   used by bulk SetStateOnAll / SetStateOffAll (adminEimece.js).
 * - Density toggle, selected-row count, sticky-friendly actions.
 */
(function (window, $) {
    'use strict';

    if (!$ || !$.fn) {
        return;
    }

    var DENSITY_KEY = 'eimece.admin.gridDensity';

    function getGridName() {
        return $('[data-gridname]').first().attr('data-gridname') || '';
    }

    function parseBool(value) {
        if (value === true || value === false) {
            return value;
        }
        var s = String(value || '').toLowerCase();
        return s === 'true' || s === '1' || s === 'yes';
    }

    function setToggleVisual($toggle, isOn) {
        $toggle
            .toggleClass('is-on', isOn)
            .toggleClass('is-off', !isOn)
            .attr('aria-pressed', isOn ? 'true' : 'false');

        var $span = $toggle.find('span[name^="span"]').first();
        if (!$span.length) {
            return;
        }
        if (isOn) {
            $span.attr('class', 'eg-status-icon gridActiveIcon glyphicon glyphicon-ok-circle');
            $span.attr('grid-data-value', 'True');
        } else {
            $span.attr('class', 'eg-status-icon gridNotActiveIcon glyphicon glyphicon-remove-circle');
            $span.attr('grid-data-value', 'False');
        }
    }

    function toggleSingleStatus($toggle) {
        if ($toggle.hasClass('is-busy')) {
            return;
        }

        var field = $toggle.attr('data-eg-status-field');
        var $span = $toggle.find('span[name="span' + field + '"]').first();
        if (!$span.length) {
            $span = $toggle.find('span[name^="span"]').first();
        }
        if (!$span.length || !field) {
            return;
        }

        var id = $span.attr('gridkey-id');
        var current = parseBool($span.attr('grid-data-value'));
        var next = !current;
        var gridName = getGridName();
        if (!id || !gridName) {
            return;
        }

        var payload = JSON.stringify({
            values: [{ Id: id, Ordering: 0, IsActive: next }],
            checkbox: field
        });

        $toggle.addClass('is-busy');

        // Prefer shared ajax helper from adminEimece.js
        if (typeof window.ajaxMethodCall === 'function') {
            window.ajaxMethodCall(
                payload,
                '/admin/Ajax/Change' + gridName + 'OrderingOrState',
                function (data) {
                    if (typeof window.changeStateSuccess === 'function') {
                        window.changeStateSuccess(data);
                    } else {
                        setToggleVisual($toggle, next);
                    }
                    $toggle.removeClass('is-busy');
                },
                function () {
                    $toggle.removeClass('is-busy');
                }
            );
            return;
        }

        $.ajax({
            url: '/admin/Ajax/Change' + gridName + 'OrderingOrState',
            type: 'POST',
            data: payload,
            contentType: 'application/json',
            success: function (data) {
                if (typeof window.changeStateSuccess === 'function') {
                    window.changeStateSuccess(data);
                } else {
                    setToggleVisual($toggle, next);
                }
            },
            complete: function () {
                $toggle.removeClass('is-busy');
            }
        });
    }

    function applyDensity(density) {
        var mode = density === 'compact' ? 'compact' : 'comfortable';
        $('.eg-grid').toggleClass('is-compact', mode === 'compact');
        $('[data-eg-density-toggle] [data-eg-density]').each(function () {
            var active = $(this).attr('data-eg-density') === mode;
            $(this).toggleClass('is-active', active).attr('aria-pressed', active ? 'true' : 'false');
        });
        try {
            window.localStorage.setItem(DENSITY_KEY, mode);
        } catch (e) { /* ignore */ }
    }

    function updateSelectedCount() {
        var count = $('input[name="checkboxGrid"]:checked').length;
        $('[data-eg-selected-count]').each(function () {
            var $el = $(this);
            $el.find('[data-eg-selected-number]').text(count);
            if (count > 0) {
                $el.prop('hidden', false).removeAttr('hidden');
            } else {
                $el.attr('hidden', 'hidden');
            }
        });
        $('input[name="checkboxGrid"]').each(function () {
            var $tr = $(this).closest('tr');
            $tr.toggleClass('eg-row-selected', this.checked);
        });
    }

    function wireChrome() {
        var saved = 'comfortable';
        try {
            saved = window.localStorage.getItem(DENSITY_KEY) || 'comfortable';
        } catch (e) { /* ignore */ }
        applyDensity(saved);

        $(document).on('click', '[data-eg-density-toggle] [data-eg-density]', function (e) {
            e.preventDefault();
            applyDensity($(this).attr('data-eg-density'));
        });

        $(document).on('change', 'input[name="checkboxGrid"]', updateSelectedCount);
        $(document).on('click', '#SelectAll, #DeselectAll', function () {
            window.setTimeout(updateSelectedCount, 0);
        });
        updateSelectedCount();
    }

    function wireStatusToggles() {
        $(document).on('click', '[data-eg-status-toggle]', function (e) {
            e.preventDefault();
            e.stopPropagation();
            toggleSingleStatus($(this));
        });
    }

    function enhanceLegacyChangeStateSuccess() {
        if (typeof window.changeStateSuccess !== 'function' || window.changeStateSuccess.__egPatched) {
            return;
        }
        var original = window.changeStateSuccess;
        window.changeStateSuccess = function (data) {
            original(data);
            if (!data || !data.values) {
                return;
            }
            data.values.forEach(function (entry) {
                var $span = $('span[name=span' + data.checkbox + ']').filter('[gridkey-id="' + entry.Id + '"]');
                var isOn = !!entry.IsActive;
                $span.attr('grid-data-value', isOn ? 'True' : 'False');
                // Preserve eg-status-icon when present / always add for toggle wrappers
                var cls = isOn
                    ? 'eg-status-icon gridActiveIcon glyphicon glyphicon-ok-circle'
                    : 'eg-status-icon gridNotActiveIcon glyphicon glyphicon-remove-circle';
                $span.attr('class', cls);
                var $toggle = $span.closest('[data-eg-status-toggle]');
                if ($toggle.length) {
                    $toggle
                        .toggleClass('is-on', isOn)
                        .toggleClass('is-off', !isOn)
                        .attr('aria-pressed', isOn ? 'true' : 'false')
                        .removeClass('is-busy');
                }
            });
        };
        window.changeStateSuccess.__egPatched = true;
    }

    function markModernGrids() {
        $('.grid-mvc').addClass('eg-grid');
        $('.admin-grid-ops').addClass('eg-bulk-bar');
    }

    function wireLoadingState() {
        $(document).on('click', '.eg-grid .grid-header a, .eg-grid .pagination a, .eg-grid .page-link', function () {
            $(this).closest('.eg-grid').addClass('is-loading');
        });
    }

    $(function () {
        markModernGrids();
        enhanceLegacyChangeStateSuccess();
        wireChrome();
        wireStatusToggles();
        wireLoadingState();
    });
}(window, window.jQuery));
