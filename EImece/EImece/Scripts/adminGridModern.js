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

    var isOverlayReload = !!window.__egGridModernV;
    window.__egGridModernV = 2;

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
            $span.attr('class', 'eg-status-icon gridActiveIcon fa fa-check-circle');
            $span.attr('grid-data-value', 'True');
        } else {
            $span.attr('class', 'eg-status-icon gridNotActiveIcon fa fa-times-circle');
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
            var checked = !!this.checked;
            $tr.toggleClass('eg-row-selected', checked)
                .toggleClass('gridChecked', checked)
                .toggleClass('table-success', checked);
            if (!checked) {
                $tr.removeClass('success active info table-active');
            }
        });
    }

    function wireChrome() {
        var saved = 'compact';
        try {
            saved = window.localStorage.getItem(DENSITY_KEY) || 'compact';
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
                    ? 'eg-status-icon gridActiveIcon fa fa-check-circle'
                    : 'eg-status-icon gridNotActiveIcon fa fa-times-circle';
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


    function fixGriddlyRecordRange() {
        var walker = function (root) {
            if (!root) { return; }
            var nodes = root.querySelectorAll ? root.querySelectorAll('*') : [];
            var list = [root];
            for (var i = 0; i < nodes.length; i++) { list.push(nodes[i]); }
            for (var n = 0; n < list.length; n++) {
                var el = list[n];
                for (var c = 0; c < el.childNodes.length; c++) {
                    var node = el.childNodes[c];
                    if (node.nodeType === 3 && node.nodeValue && node.nodeValue.indexOf('through-') !== -1) {
                        node.nodeValue = node.nodeValue.replace(/through-\s*/g, '– ');
                    }
                }
            }
        };
        $('.griddly, .eg-grid, .griddly-footer, .griddly-summary').each(function () {
            walker(this);
        });
    }


    function numberGridRows(context) {
        var $root = context ? $(context) : $(document);
        var $grids = $root.is('[data-role=griddly]') ? $root : $root.find('[data-role=griddly]');
        if (!$grids.length) {
            $grids = $('[data-role=griddly]');
        }
        $grids.each(function () {
            var $g = $(this);
            var pageSize = parseInt($g.attr('data-griddly-pagesize'), 10) || 0;
            var pageNumber = 0;
            var inst = $g.data('griddly');
            if (inst && inst.options && typeof inst.options.pageNumber === 'number') {
                pageNumber = inst.options.pageNumber;
            } else {
                var $input = $g.find('input.pageNumber').first();
                if ($input.length) {
                    pageNumber = Math.max(0, (parseInt($input.val(), 10) || 1) - 1);
                }
            }
            if (pageNumber < 0) {
                pageNumber = 0;
            }
            var start = pageNumber * pageSize;
            var $rows = $g.find('table tbody.data > tr, table.eg-grid-table > tbody > tr, table.grid-table > tbody > tr');
            $rows.each(function (i) {
                var $idx = $(this).find('.eg-row-index').first();
                if (!$idx.length) {
                    return;
                }
                $idx.text(String(start + i + 1));
                $idx.removeClass('eg-row-index-guid');
            });
        });
    }

    function markModernGrids() {
        $('.griddly, .eg-grid-shell, .grid-mvc').addClass('eg-grid');
        $('.admin-grid-ops').addClass('eg-bulk-bar');
        $('.eg-grid table.grid-table > thead > tr > th, .eg-grid table.eg-grid-table > thead > tr > th, .griddly table > thead > tr > th').addClass('grid-header');
        $('[data-role=griddly]').each(function () {
            var $g = $(this);
            var n = $g.attr('data-griddly-count');
            if (n != null && n !== '') {
                $g.find('.grid-record-count-badge.griddly-recordtotal').each(function () {
                    if (!$.trim($(this).text())) {
                        $(this).text(n);
                    }
                });
            }
        });
        var saved = 'compact';
        try {
            saved = window.localStorage.getItem(DENSITY_KEY) || 'compact';
        } catch (e) { /* ignore */ }
        applyDensity(saved);
        markSecondaryGridColumns();
        fixGriddlyRecordRange();
        numberGridRows();
    }

    /**
     * Tag low-priority columns so mobile CSS can collapse them globally.
     * Keeps index / check / name / image / status / state / price / actions.
     */
    function markSecondaryGridColumns() {
        var keep = /(^|\s)(eg-col-index|eg-col-check|eg-col-name|eg-col-image|eg-col-status|eg-col-state|eg-col-price|eg-col-actions|eg-col-isservice|eg-col-isvalues|gridButtons)(\s|$)/;
        $('.eg-grid table.grid-table, .eg-grid table.eg-grid-table, .griddly table').each(function () {
            var $table = $(this);
            var $ths = $table.find('thead > tr > th');
            if ($ths.length < 7) {
                return;
            }
            $ths.each(function (idx) {
                var $th = $(this);
                var cls = $th.attr('class') || '';
                if (keep.test(cls) || $th.hasClass('gridDateClass') || $th.hasClass('smallGridColumn') || $th.hasClass('eg-col-secondary')) {
                    return;
                }
                // Hidden Id columns from Grid.Mvc — keep marking so they stay collapsed on phones
                $th.addClass('eg-col-secondary');
                $table.find('tbody > tr').each(function () {
                    $(this).children('td').eq(idx).addClass('eg-col-secondary');
                });
            });
        });
    }

    function wireLoadingState() {
        $(document).on('click', '.eg-grid .grid-header a, .eg-grid .pagination a, .eg-grid .page-link', function () {
            $(this).closest('.eg-grid').addClass('is-loading');
        });
    }

    function positionPortaledMenu($group, $menu) {
        var $btn = $group.children('.dropdown-toggle, .eg-actions-toggle').first();
        if (!$btn.length || !$menu.length || !$btn[0]) {
            return;
        }

        // Measure with fixed positioning so nested scroll/sticky containers cannot skew offset().
        $menu.css({
            display: 'block',
            position: 'fixed',
            visibility: 'hidden',
            top: 0,
            left: 0,
            right: 'auto',
            bottom: 'auto',
            minWidth: 220,
            zIndex: 2100
        });

        var btnRect = $btn[0].getBoundingClientRect();
        var menuWidth = Math.max($menu.outerWidth(), 220);
        var menuHeight = $menu.outerHeight();
        var left = btnRect.right - menuWidth;
        left = Math.max(8, Math.min(left, window.innerWidth - menuWidth - 8));

        var topBelow = btnRect.bottom + 4;
        var topAbove = btnRect.top - menuHeight - 4;
        var top = topBelow;
        if (topBelow + menuHeight > window.innerHeight - 8 && topAbove >= 8) {
            top = topAbove;
        }

        $menu.css({
            display: 'block',
            position: 'fixed',
            visibility: 'visible',
            top: top,
            left: left,
            minWidth: menuWidth,
            zIndex: 2100,
            right: 'auto',
            bottom: 'auto'
        });
    }

    function restorePortaledMenu($group) {
        var $menu = $group.data('egMenu');
        if (!$menu || !$menu.length) {
            $menu = $('body > .eg-actions-menu-portal');
        }
        if ($menu && $menu.length) {
            $menu
                .removeClass('eg-actions-menu-portal')
                .removeAttr('style')
                .appendTo($group);
        }
        $group.removeData('egMenu');
    }

    function wireActionMenus() {
        $(document).on('show.bs.dropdown', '.eg-actions.btn-group', function () {
            var $group = $(this);
            var $menu = $group.children('.dropdown-menu').first();
            if (!$menu.length) {
                return;
            }

            // Close any other portaled action menu first.
            $('.eg-actions.btn-group.show').not($group).each(function () {
                restorePortaledMenu($(this));
            });

            $group.data('egMenu', $menu);
            $menu.addClass('eg-actions-menu-portal').appendTo('body');
            // Two frames: first attach/layout, then measure final width/height.
            window.requestAnimationFrame(function () {
                positionPortaledMenu($group, $menu);
                window.requestAnimationFrame(function () {
                    positionPortaledMenu($group, $menu);
                });
            });
        });

        $(document).on('hide.bs.dropdown', '.eg-actions.btn-group', function () {
            restorePortaledMenu($(this));
        });

        $(document).on('scroll', '.eg-grid-scroll, .eg-grid .grid-wrap', function () {
            var $open = $('.eg-actions.btn-group.show');
            if ($open.length) {
                $open.removeClass('show');
                restorePortaledMenu($open);
            }
        });

        $(window).on('resize.egActionsMenu', function () {
            var $open = $('.eg-actions.btn-group.show');
            if (!$open.length) {
                return;
            }
            var $menu = $open.data('egMenu');
            if ($menu && $menu.length) {
                positionPortaledMenu($open, $menu);
            }
        });
    }

    function wireCategoryTree() {
        var $trees = $('[data-eg-category-tree]');
        if (!$trees.length) {
            return;
        }

        var mobileTreeMq = window.matchMedia('(max-width: 991px)');

        function syncMobileTreeCollapse($tree) {
            var selectedId = String($tree.attr('data-selected-id') || '0');
            var hasSelection = selectedId && selectedId !== '0';
            var collapsed = mobileTreeMq.matches && !hasSelection;
            $tree.toggleClass('is-mobile-collapsed', collapsed);
            $tree.find('> .eg-category-tree-head .eg-category-tree-toggle')
                .attr('aria-expanded', collapsed ? 'false' : 'true');
        }

        $trees.each(function () {
            var $tree = $(this);
            var selectedId = String($tree.attr('data-selected-id') || '0');
            if (selectedId && selectedId !== '0') {
                var $active = $tree.find('.eg-tree-node[data-category-id="' + selectedId + '"]');
                $active.parents('.eg-tree-node.has-children').addClass('is-open')
                    .children('.eg-tree-row').find('.eg-tree-toggle').attr('aria-expanded', 'true');
                // Collapse siblings of the path for a quieter tree when a deep node is selected.
                $tree.find('.eg-tree-node.has-children').not($active.parents('.eg-tree-node').add($active)).each(function () {
                    var $node = $(this);
                    if ($node.find('.eg-tree-node[data-category-id="' + selectedId + '"]').length === 0
                        && String($node.attr('data-category-id')) !== selectedId) {
                        // keep top-level open by default; only collapse deeper unrelated branches
                        if ($node.parents('.eg-tree-node').length > 0) {
                            $node.removeClass('is-open')
                                .children('.eg-tree-row').find('.eg-tree-toggle').attr('aria-expanded', 'false');
                        }
                    }
                });
            }
            syncMobileTreeCollapse($tree);
        });

        $(document).on('click', '[data-eg-category-tree] .eg-tree-toggle', function (e) {
            e.preventDefault();
            e.stopPropagation();
            var $btn = $(this);
            var $node = $btn.closest('.eg-tree-node');
            var open = !$node.hasClass('is-open');
            $node.toggleClass('is-open', open);
            $btn.attr('aria-expanded', open ? 'true' : 'false');
        });

        $(document).on('click', '[data-eg-category-tree] .eg-category-tree-toggle', function (e) {
            e.preventDefault();
            e.stopPropagation();
            if (!mobileTreeMq.matches) {
                return;
            }
            var $btn = $(this);
            var $tree = $btn.closest('[data-eg-category-tree]');
            var collapsed = !$tree.hasClass('is-mobile-collapsed');
            $tree.toggleClass('is-mobile-collapsed', collapsed);
            $btn.attr('aria-expanded', collapsed ? 'false' : 'true');
        });

        function onTreeMqChange() {
            $trees.each(function () {
                syncMobileTreeCollapse($(this));
            });
        }
        if (mobileTreeMq.addEventListener) {
            mobileTreeMq.addEventListener('change', onTreeMqChange);
        } else if (mobileTreeMq.addListener) {
            mobileTreeMq.addListener(onTreeMqChange);
        }
    }

    function sizeReviewNote(el) {
        if (!el) {
            return;
        }
        el.style.width = '';
        el.style.height = 'auto';
        var minH = parseFloat(window.getComputedStyle(el).minHeight) || 0;
        var next = Math.max(el.scrollHeight, minH);
        el.style.height = Math.min(next, 220) + 'px';
    }

    function sizeReviewNotes(context) {
        var root = context && context.querySelectorAll ? context : document;
        var notes = root.querySelectorAll('.eg-review-note');
        for (var i = 0; i < notes.length; i++) {
            sizeReviewNote(notes[i]);
        }
    }

    function wireOpsMore() {
        var $more = $('details.eg-ops-more');
        if (!$more.length) {
            return;
        }
        var mq = window.matchMedia('(max-width: 767px)');
        function sync() {
            $more.each(function () {
                // Desktop: keep bulk toolbar expanded. Mobile: collapse so Sil/search stay secondary.
                this.open = !mq.matches;
            });
        }
        sync();
        if (mq.addEventListener) {
            mq.addEventListener('change', sync);
        } else if (mq.addListener) {
            mq.addListener(sync);
        }
    }

    function findGridScrollElement($from) {
        var $roots = $from.closest('.js-griddly-async, .eg-grid, .griddly, .eg-grid-shell, .admin-content');
        var sel = '.griddly-scrollable-container, .eg-grid-scroll, .grid-wrap';
        var $cands = $roots.find(sel);
        if (!$cands.length) {
            $cands = $(sel);
        }
        var overflowing = $cands.filter(function () {
            return this.scrollWidth > this.clientWidth + 1;
        });
        return (overflowing.length ? overflowing : $cands).get(0) || null;
    }

    function scrollGridBy(el, delta) {
        if (!el) {
            return;
        }
        var next = el.scrollLeft + delta;
        if (typeof el.scrollTo === 'function') {
            try {
                el.scrollTo({ left: next, behavior: 'smooth' });
                return;
            } catch (err) { /* iOS old scrollTo signature */ }
        }
        if (typeof el.scrollBy === 'function') {
            try {
                el.scrollBy({ left: delta, behavior: 'smooth' });
                return;
            } catch (err2) { /* ignore */ }
        }
        el.scrollLeft = next;
    }

    function wireScrollNavButtons() {
        // Drop any prior click handlers (bundle + cache-busted overlay).
        $(document).off('click', '.eg-scroll-nav-left');
        $(document).off('click', '.eg-scroll-nav-right');
        $(document).off('click.egScrollNav');
        $(document).on('click.egScrollNav', '.eg-scroll-nav-left, .eg-scroll-nav-right', function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();
            var delta = $(this).hasClass('eg-scroll-nav-right') ? 280 : -280;
            scrollGridBy(findGridScrollElement($(this)), delta);
        });
    }

    function wireFloatingScrollbar() {
        var $containers = $('.griddly-scrollable-container, .eg-grid-scroll, .grid-wrap');
        if (!$containers.length) {
            return;
        }

        $containers.each(function () {
            var $grid = $(this);
            if ($grid.data('hasFloatingScrollbar')) {
                return;
            }
            $grid.data('hasFloatingScrollbar', true);

            var $floatWrap = $('<div class="eg-floating-scrollbar"><div class="eg-floating-scrollbar-inner"></div></div>');
            var $inner = $floatWrap.find('.eg-floating-scrollbar-inner');
            $('body').append($floatWrap);

            var isSyncingGrid = false;
            var isSyncingFloat = false;

            function updateDimensions() {
                var el = $grid[0];
                if (!el) return;
                var scrollW = el.scrollWidth;
                var clientW = el.clientWidth;
                var rect = el.getBoundingClientRect();

                // Has horizontal overflow?
                if (scrollW > clientW + 2) {
                    $inner.css('width', scrollW + 'px');
                    $floatWrap.css({
                        left: rect.left + 'px',
                        width: clientW + 'px'
                    });

                    // Is table partially on-screen, but table bottom is below viewport?
                    var windowH = $(window).height();
                    var isVisible = rect.top < windowH && rect.bottom > windowH + 30;
                    $floatWrap.toggle(isVisible);
                    if (isVisible) {
                        $floatWrap.scrollLeft($grid.scrollLeft());
                    }
                } else {
                    $floatWrap.hide();
                }
            }

            // Sync from grid to float
            $grid.on('scroll', function () {
                if (isSyncingGrid) {
                    isSyncingGrid = false;
                    return;
                }
                isSyncingFloat = true;
                $floatWrap.scrollLeft($grid.scrollLeft());
            });

            // Sync from float to grid
            $floatWrap.on('scroll', function () {
                if (isSyncingFloat) {
                    isSyncingFloat = false;
                    return;
                }
                isSyncingGrid = true;
                $grid.scrollLeft($floatWrap.scrollLeft());
            });

            // Mouse wheel / shift+wheel support on grid
            $grid.on('wheel', function (e) {
                var deltaY = e.originalEvent.deltaY;
                var deltaX = e.originalEvent.deltaX;
                if (e.shiftKey || Math.abs(deltaX) > Math.abs(deltaY)) {
                    var delta = e.shiftKey ? deltaY : deltaX;
                    $grid.scrollLeft($grid.scrollLeft() + delta);
                    e.preventDefault();
                }
            });

            $(window).on('scroll resize', updateDimensions);
            $(document).on('gridmvc.loaded griddly.loaded setfilters.griddly beforerefresh.griddly', function () {
                setTimeout(updateDimensions, 200);
            });

            updateDimensions();
        });
    }

    $(function () {
        if (isOverlayReload) {
            wireScrollNavButtons();
            markModernGrids();
            return;
        }
        markModernGrids();
        enhanceLegacyChangeStateSuccess();
        wireChrome();
        wireStatusToggles();
        wireLoadingState();
        wireActionMenus();
        wireCategoryTree();
        wireOpsMore();
        wireScrollNavButtons();
        wireFloatingScrollbar();
        sizeReviewNotes(document);
    });

    // Re-mark after Grid.Mvc and Griddly AJAX redraws
    $(document).on('gridmvc.loaded griddly.loaded refresh.griddly', function () {
        markModernGrids();
        wireFloatingScrollbar();
        sizeReviewNotes(document);
        updateSelectedCount();
    });
    window.egMarkModernGrids = markModernGrids;
    window.egUpdateSelectedCount = updateSelectedCount;
}(window, window.jQuery));
