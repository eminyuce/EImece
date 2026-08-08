/*-----------------------------------------------------------------------------------

    Theme Name: Crizal - Multipurpose Responsive Template + Admin
    Description: Multipurpose Website Template + Admin
    Author: Chitrakoot Web
    Version: 4.0

    ---------------------------------- */
    
(function ($) {
    "use strict";
    $.fn.menumaker = function (options) {

        var settings = $.extend({
            format: "dropdown",
            sticky: false
        }, options);

        return this.each(function () {
            // Must scope to the current nav — outer $(this) is the full collection and
            // re-runs prepend/bind for every <nav> on the page (duplicate +/− buttons).
            var nav = $(this);

            if (nav.data('menumaker-init')) {
                return;
            }
            nav.data('menumaker-init', true);

            nav.find(".navbar-toggler").on('click', function () {
                $(this).toggleClass('menu-opened');
                var mainmenu = $(this).next('ul');
                if (mainmenu.hasClass('open')) {
                    mainmenu.slideToggle().removeClass('open');
                } else {
                    mainmenu.slideToggle().addClass('open');
                    if (settings.format === "dropdown") {
                        mainmenu.find('ul').show();
                    }
                }
            });

            nav.find('.navbar-nav li ul').parent().addClass('has-sub');
            nav.find('.navbar-nav li ul li').parent().addClass('sub-menu');

            var multiTg = function () {

                nav.find(".has-sub").each(function () {
                    if (!$(this).children('.submenu-button').length) {
                        $(this).prepend('<span class="submenu-button"></span>');
                    }
                });

                function toggleSubmenu($btn) {
                    var $li = $btn.closest('li.has-sub');
                    var $submenu = $li.children('ul').first();
                    if (!$submenu.length) {
                        return;
                    }
                    var opening = !$submenu.is(':visible');
                    $li.siblings('.has-sub').removeClass('active')
                        .children('ul').stop(true, true).slideUp().removeClass('open');
                    if (opening) {
                        $submenu.addClass('open').stop(true, true).slideDown();
                        $li.addClass('active');
                    } else {
                        $submenu.stop(true, true).slideUp(function () {
                            $submenu.removeClass('open');
                        });
                        $li.removeClass('active');
                    }
                }

                // Single toggle (template originally toggled twice via next+siblings).
                nav.find('.navbar-nav > li.has-sub > .submenu-button').off('click.menumaker').on('click.menumaker', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    toggleSubmenu($(this));
                });

                nav.find('.sub-menu > li.has-sub > .submenu-button').off('click.menumaker').on('click.menumaker', function (e) {
                    e.preventDefault();
                    e.stopPropagation();
                    toggleSubmenu($(this));
                });

                // Tapping the parent label should expand/collapse on mobile, not navigate to #!
                nav.find('.navbar-nav li.has-sub > a').off('click.menumaker').on('click.menumaker', function (e) {
                    if ($(window).width() > 991) {
                        return;
                    }
                    var href = ($(this).attr('href') || '').trim();
                    if (href && href !== '#' && href !== '#!' && href.indexOf('javascript:') !== 0) {
                        return;
                    }
                    e.preventDefault();
                    $(this).siblings('.submenu-button').first().trigger('click');
                });

            };

            if (settings.format === 'multitoggle') multiTg();
            else nav.addClass('dropdown');
            if (settings.sticky === true) nav.css('position', 'fixed');
            var resizeFix = function () {
                var mediasize = 991;
                if ($(window).width() > mediasize) {
                    nav.find('ul').show();
                }
            };

            resizeFix();
            return $(window).on('resize', resizeFix);

        });
    };

    $(document).ready(function () {

        $("nav").menumaker({
            format: "multitoggle"
        });

        /*------------------------------------
            Menu Selector
        --------------------------------------*/

        var urlparam = window.location.pathname.split('/');
        var menuselctor = window.location.pathname;
        if (urlparam[urlparam.length - 1].length > 0) menuselctor = urlparam[urlparam.length - 1];
        else menuselctor = urlparam[urlparam.length - 2];
        $('.navbar-nav li').find('a[href="' + menuselctor + '"]').closest('li').addClass('active').parents().eq(1).addClass('current');
        $('.navbar-nav li.has-sub ul li').find('a[href="' + menuselctor + '"]').parents().eq(4).addClass('current');
    });

    /*------------------------------------
            Toggle Search
    --------------------------------------*/

    $(".navbar-default .attr-nav").each(function () {
        $("li.search > a", this).on("click", function (e) {
            e.preventDefault();
            $(".top-search").slideToggle();
        });
    });

    $(".input-group-addon.close-search").on("click", function () {
        $(".top-search").slideUp();
    });

})(jQuery);
