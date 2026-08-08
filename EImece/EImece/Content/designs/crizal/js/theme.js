/* Crizal theme UI behaviors — complements template main.js */
(function (window, document, $) {
    'use strict';

    function setSidebarOpen(open) {
        var $sidebar = $('.offcanvas-sidebar');
        var $backdrop = $('.crizal-sidebar-backdrop');
        $sidebar.toggleClass('show', !!open);
        $backdrop.toggleClass('show', !!open);
        $('body').toggleClass('crizal-sidebar-open', !!open);
    }

    function ensureSidebarBackdrop() {
        if (!document.querySelector('.offcanvas-sidebar')) {
            return;
        }
        if (!document.querySelector('.crizal-sidebar-backdrop')) {
            var backdrop = document.createElement('div');
            backdrop.className = 'crizal-sidebar-backdrop';
            document.body.appendChild(backdrop);
        }
    }

    /* main.js applies a generic $('.owl-carousel') autoplay slider — undo + reconfigure shop carousels */
    function destroyOwl($el) {
        if (!$el || !$el.length || typeof $el.trigger !== 'function') {
            return;
        }
        if ($el.hasClass('owl-loaded')) {
            $el.trigger('destroy.owl.carousel');
        }
        $el.find('.owl-stage-outer').children().unwrap();
        $el.removeClass('owl-center owl-loaded owl-text-select-on owl-drag');
        $el.find('.owl-stage, .owl-item').children().unwrap();
        $el.find('.cloned').remove();
        $el.find('.owl-nav, .owl-dots').remove();
    }

    function initProductGallery($) {
        $('.product-gallery .product-carousel').each(function () {
            var $el = $(this);
            if (typeof $el.owlCarousel !== 'function') {
                return;
            }
            destroyOwl($el);
            $el.owlCarousel({
                items: 1,
                loop: false,
                nav: false,
                dots: false,
                margin: 0,
                autoplay: false,
                mouseDrag: true,
                touchDrag: true,
                URLhashListener: true,
                startPosition: 'URLHash'
            });
        });
    }

    function initRelatedCarousel($) {
        $('.crizal-related-carousel').each(function () {
            var $el = $(this);
            if (typeof $el.owlCarousel !== 'function') {
                return;
            }

            var options = {
                nav: false,
                dots: true,
                loop: false,
                margin: 15,
                autoplay: false,
                smartSpeed: 450,
                responsive: {
                    0: { items: 1 },
                    480: { items: 2 },
                    768: { items: 3 },
                    992: { items: 4 },
                    1200: { items: 5 }
                }
            };

            var raw = $el.attr('data-owl-carousel');
            if (raw) {
                try {
                    options = $.extend(true, options, JSON.parse(raw));
                } catch (err) { /* keep defaults */ }
            }
            options.autoplay = false;
            options.loop = false;

            destroyOwl($el);
            $el.owlCarousel(options);
        });
    }

    function initCrizalUi() {
        var body = document.body;
        if (!body || body.getAttribute('data-design') !== 'crizal') {
            return;
        }

        if (!$) {
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
                if (typeof onClickSearch === 'function') {
                    onClickSearch();
                }
            }
        });

        // Mobile category filter sidebar
        ensureSidebarBackdrop();
        $(document).off('click.crizalSidebar');
        $(document).on('click.crizalSidebar', '.crizal-open-filters', function (e) {
            e.preventDefault();
            setSidebarOpen(!$('.offcanvas-sidebar').hasClass('show'));
        });
        $(document).on('click.crizalSidebar', '.crizal-sidebar-close, .crizal-sidebar-backdrop', function (e) {
            e.preventDefault();
            setSidebarOpen(false);
        });
        $(document).on('click.crizalSidebar', '#FilterButton', function () {
            var btn = this;
            window.setTimeout(function () { try { btn.blur(); } catch (err) { /* ignore */ } }, 0);
        });

        // Product gallery thumbnails → owl carousel
        $(document).off('click.crizalGallery', '.product-gallery .product-thumbnails a');
        $(document).on('click.crizalGallery', '.product-gallery .product-thumbnails a', function (e) {
            e.preventDefault();
            var $li = $(this).closest('li');
            var index = $li.index();
            var $carousel = $(this).closest('.product-gallery').find('.product-carousel');
            $li.addClass('active').siblings().removeClass('active');
            if ($carousel.length) {
                try { $carousel.trigger('to.owl.carousel', [index, 250]); } catch (err) { /* ignore */ }
            }
        });

        initProductGallery($);
        initRelatedCarousel($);
        initHomeHero($);
        initProductTabs($);
        initProductPanels($);
        initLeaveReviewModal($);
        initNavMenuBreakpoint($);
    }

    function initNavMenuBreakpoint($) {
        var mediasize = 991;
        var syncNav = function () {
            var $nav = $('#nav');
            if (!$nav.length) {
                return;
            }
            if ($(window).width() > mediasize) {
                $nav.removeClass('open').css('display', '');
                $('.navbar-toggler').removeClass('menu-opened');
                // Collapse mobile accordion; clear inline styles from slideToggle/show
                // so desktop CSS (left: -9999px / hover) controls flyouts again.
                $nav.find('li.has-sub').removeClass('active');
                $nav.find('ul').removeClass('open').each(function () {
                    this.removeAttribute('style');
                });
            }
        };
        // Defer so this runs after nav-menu.js resizeFix (which calls ul.show()).
        $(window).off('resize.crizalNav').on('resize.crizalNav', function () {
            window.setTimeout(syncNav, 0);
        });
        syncNav();
    }

    function initLeaveReviewModal($) {
        if (typeof $.fn.modal !== 'function') {
            return;
        }

        var $modal = $('#leaveReview');
        if ($modal.length && $modal.parent()[0] !== document.body) {
            $modal.appendTo(document.body);
        }

        $(document).off('click.crizalLeaveReview', '[data-toggle="modal"][href="#leaveReview"], [data-toggle="modal"][data-target="#leaveReview"], .crizal-product-reviews__cta');
        $(document).on('click.crizalLeaveReview', '[data-toggle="modal"][href="#leaveReview"], [data-toggle="modal"][data-target="#leaveReview"], .crizal-product-reviews__cta', function (e) {
            e.preventDefault();
            var $target = $('#leaveReview');
            if ($target.length) {
                $target.modal('show');
            }
        });

        // Bootstrap data-api dismiss is not wired; close buttons need an explicit hide.
        $(document).off('click.crizalLeaveReviewDismiss', '#leaveReview [data-dismiss="modal"]');
        $(document).on('click.crizalLeaveReviewDismiss', '#leaveReview [data-dismiss="modal"]', function (e) {
            e.preventDefault();
            $('#leaveReview').modal('hide');
        });
    }

    function initProductTabs($) {
        $(document).off('click.crizalProductTabs', '.crizal-product-tabs__nav [data-toggle="tab"]');
        $(document).on('click.crizalProductTabs', '.crizal-product-tabs__nav [data-toggle="tab"]', function (e) {
            e.preventDefault();
            var $link = $(this);
            if (typeof $link.tab === 'function') {
                $link.tab('show');
            }
        });
    }

    function initProductPanels($) {
        var $root = $('#productPanels');
        if (!$root.length || typeof $.fn.collapse !== 'function') {
            return;
        }

        function syncPanelLinks() {
            $root.find('[data-toggle="collapse"]').each(function () {
                var $link = $(this);
                var target = $link.attr('href') || $link.data('target');
                var isOpen = target && $(target).hasClass('show');
                $link.toggleClass('collapsed', !isOpen);
                $link.attr('aria-expanded', isOpen ? 'true' : 'false');
            });
        }

        $root.off('shown.bs.collapse.crizalProductPanels hidden.bs.collapse.crizalProductPanels');
        $root.on('shown.bs.collapse.crizalProductPanels hidden.bs.collapse.crizalProductPanels', syncPanelLinks);

        $(document).off('click.crizalProductPanels', '#productPanels [data-toggle="collapse"]');
        $(document).on('click.crizalProductPanels', '#productPanels [data-toggle="collapse"]', function (e) {
            e.preventDefault();
            var $link = $(this);
            var target = $link.attr('href') || $link.data('target');
            if (!target) {
                return;
            }
            var $target = $(target);
            var willOpen = !$target.hasClass('show');
            $root.find('.collapse.show').not($target).collapse('hide');
            $target.collapse(willOpen ? 'show' : 'hide');
        });

        syncPanelLinks();
    }

    function initHomeHero($) {
        $('.crizal-home-hero .slider-fade3').each(function () {
            var $el = $(this);
            if (typeof $el.owlCarousel !== 'function') {
                return;
            }
            destroyOwl($el);
            $el.owlCarousel({
                items: 1,
                loop: true,
                margin: 0,
                nav: true,
                dots: true,
                navText: ["<i class='ti-angle-left'></i>", "<i class='ti-angle-right'></i>"],
                autoplay: true,
                autoplayTimeout: 6500,
                smartSpeed: 900,
                mouseDrag: false,
                touchDrag: true,
                animateIn: 'fadeIn',
                animateOut: 'fadeOut',
                responsive: {
                    0: { nav: false, dots: true },
                    768: { nav: true, dots: true }
                }
            });
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
