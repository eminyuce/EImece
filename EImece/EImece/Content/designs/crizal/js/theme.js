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
                nav: true,
                dots: true,
                loop: false,
                margin: 16,
                autoplay: false,
                smartSpeed: 450,
                responsive: {
                    0: { items: 2 },
                    576: { items: 3 },
                    768: { items: 4 },
                    992: { items: 5 },
                    1200: { items: 6 }
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

        document.addEventListener('error', function (e) {
            var t = e.target;
            if (!t || t.tagName !== 'IMG') {
                return;
            }
            if (t.closest && t.closest('.product-gallery')) {
                t.onerror = null;
                t.classList.add('is-broken');
                t.removeAttribute('src');
            }
        }, true);

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
        initChatFabSafeArea();
        initProductTabs($);
        initProductPanels($);
        initProductShortDescription($);
        initPageThemeLightbox();
        initLeaveReviewModal($);
        initNavMenuBreakpoint($);
        initMobileSubmenuCleanup($);
        initMobileNavOpenClass($);
    }

    /**
     * WhatsApp / chat FABs are often injected with inline position:fixed;left:…;bottom:…
     * and cover the newsletter email field on ~390px. Park them bottom-right under 768px.
     */
    function initChatFabSafeArea() {
        function parkRight(el) {
            if (!el) {
                return;
            }
            el.style.setProperty('left', 'auto', 'important');
            el.style.setProperty('right', '1rem', 'important');
            el.style.setProperty('bottom', '1rem', 'important');
        }

        function reposition() {
            if (window.innerWidth > 767 || !document.body) {
                return;
            }

            var known = document.querySelectorAll(
                '#WAButton, .floating-wpp, .whatsapp-button, .whatsapp-float, .wa-float, .blantershow-chat,' +
                ' a[href*="wa.me"], a[href*="api.whatsapp.com"], [id*="whatsapp"], [id*="WhatsApp"],' +
                ' [class*="whatsapp"], [class*="WhatsApp"]'
            );
            for (var i = 0; i < known.length; i++) {
                parkRight(known[i]);
            }

            // Late-injected widgets are usually direct body children
            var kids = document.body.children;
            for (var k = 0; k < kids.length; k++) {
                var el = kids[k];
                if (!el || el.tagName === 'SCRIPT' || el.tagName === 'STYLE' || el.tagName === 'LINK') {
                    continue;
                }
                var style = window.getComputedStyle(el);
                if (style.position !== 'fixed') {
                    continue;
                }
                var rect = el.getBoundingClientRect();
                var nearBottomLeft = rect.bottom > window.innerHeight - 96
                    && rect.left < 96
                    && rect.width <= 88
                    && rect.height <= 88
                    && rect.width >= 36
                    && rect.height >= 36;
                if (nearBottomLeft) {
                    parkRight(el);
                }
            }
        }

        reposition();
        window.setTimeout(reposition, 800);
        window.setTimeout(reposition, 2000);
        window.addEventListener('resize', reposition);
    }

    function initMobileSubmenuCleanup($) {
        // Remove duplicate +/− controls if an older menumaker run created extras.
        $('#nav li.has-sub').each(function () {
            $(this).children('.submenu-button').slice(1).remove();
        });
    }

    function syncCrizalNavOpenClass($) {
        var open = $('#nav').hasClass('open');
        $('body').toggleClass('crizal-nav-open', open);
        $('.navbar-toggler').attr('aria-expanded', open ? 'true' : 'false');
    }

    function initMobileNavOpenClass($) {
        // Fallback for browsers without :has(); CSS uses both selectors.
        syncCrizalNavOpenClass($);
        $(document).off('click.crizalNavOpen', '.navbar-toggler');
        $(document).on('click.crizalNavOpen', '.navbar-toggler', function () {
            // menumaker toggles .open synchronously; sync after that handler.
            window.setTimeout(function () { syncCrizalNavOpenClass($); }, 0);
        });
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
            syncCrizalNavOpenClass($);
        };
        // Defer so this runs after nav-menu.js resizeFix (which calls ul.show()).
        $(window).off('resize.crizalNav').on('resize.crizalNav', function () {
            window.setTimeout(syncNav, 0);
        });
        syncNav();
    }

    function initLeaveReviewModal($) {
        var $modal = $('#leaveReview');
        if (!$modal.length) {
            return;
        }

        if ($modal.parent()[0] !== document.body) {
            $modal.appendTo(document.body);
        }

        var $form = $modal.is('form') ? $modal : $modal.find('form').first();
        var emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]{2,}$/;

        function showModal() {
            if (typeof $.fn.modal === 'function') {
                $modal.modal('show');
                return;
            }
            if (window.bootstrap && window.bootstrap.Modal) {
                window.bootstrap.Modal.getOrCreateInstance($modal[0]).show();
            }
        }

        function hideModal() {
            if (typeof $.fn.modal === 'function') {
                $modal.modal('hide');
                return;
            }
            if (window.bootstrap && window.bootstrap.Modal) {
                var instance = window.bootstrap.Modal.getInstance($modal[0]);
                if (instance) {
                    instance.hide();
                }
            }
        }

        function messages() {
            return {
                required: $form.attr('data-msg-required') || 'This field is required.',
                name: $form.attr('data-msg-name') || $form.attr('data-msg-required') || 'This field is required.',
                email: $form.attr('data-msg-email') || $form.attr('data-msg-required') || 'This field is required.',
                emailFormat: $form.attr('data-msg-email-format') || 'Please enter a valid email address.',
                subject: $form.attr('data-msg-subject') || $form.attr('data-msg-required') || 'This field is required.',
                review: $form.attr('data-msg-review') || $form.attr('data-msg-required') || 'This field is required.',
                rating: $form.attr('data-msg-rating') || $form.attr('data-msg-required') || 'This field is required.',
                captcha: $form.attr('data-msg-captcha') || 'Please answer the security question.',
                recaptcha: $form.attr('data-msg-recaptcha') || 'Please complete the captcha.'
            };
        }

        function errorTarget($input) {
            var name = $input.attr('name');
            var $group = $input.closest('.crizal-review-field, .crizal-review-captcha, .captcha-container, .recaptcha-container, .form-group');
            var $msg = $();
            if (name) {
                $msg = $group.find("[data-valmsg-for='" + name + "']");
            }
            if (!$msg.length) {
                $msg = $group.find('.crizal-review-modal__error').first();
            }
            return $msg;
        }

        function showFieldError($input, message) {
            $input.addClass('is-invalid input-validation-error').attr('aria-invalid', 'true');
            var $group = $input.closest('.crizal-review-field, .crizal-review-captcha');
            $group.addClass('is-invalid');
            var $msg = errorTarget($input);
            if ($msg.length) {
                $msg.text(message)
                    .removeClass('field-validation-valid d-none')
                    .addClass('field-validation-error is-visible');
            }
        }

        function clearFieldError($input) {
            $input.removeClass('is-invalid input-validation-error').removeAttr('aria-invalid');
            var $group = $input.closest('.crizal-review-field, .crizal-review-captcha');
            $group.removeClass('is-invalid');
            var $msg = errorTarget($input);
            if ($msg.length) {
                $msg.text('')
                    .removeClass('field-validation-error is-visible')
                    .addClass('field-validation-valid');
            }
        }

        function fieldValue($input) {
            return $.trim($input.val() || '');
        }

        function findField(key) {
            return $form.find('[data-review-field="' + key + '"]').filter('input, textarea, select');
        }

        function validateOneField($input) {
            var msg = messages();
            var key = ($input.attr('data-review-field') || '').toLowerCase();
            var value = fieldValue($input);
            var type = ($input.attr('type') || '').toLowerCase();

            if (type === 'hidden' && key !== 'rating') {
                return true;
            }

            if (key === 'rating') {
                var rating = parseInt(value, 10);
                if (!rating || rating < 1 || rating > 5) {
                    showFieldError($input, msg.rating);
                    $form.find('.crizal-review-stars').addClass('is-invalid');
                    return false;
                }
                clearFieldError($input);
                $form.find('.crizal-review-stars').removeClass('is-invalid');
                return true;
            }

            if (key === 'email' || type === 'email') {
                if (!value) {
                    showFieldError($input, msg.email);
                    return false;
                }
                if (!emailPattern.test(value)) {
                    showFieldError($input, msg.emailFormat);
                    return false;
                }
                clearFieldError($input);
                return true;
            }

            if (key === 'captcha' || $input.attr('name') === 'Captcha') {
                if (!value) {
                    showFieldError($input, msg.captcha);
                    $form.find('.crizal-review-captcha').addClass('is-invalid');
                    return false;
                }
                clearFieldError($input);
                $form.find('.crizal-review-captcha').removeClass('is-invalid');
                return true;
            }

            if ($input.prop('required') || $input.attr('data-val-required') || key) {
                if (!value) {
                    var requiredMessage = msg[key] || $input.attr('data-val-required') || msg.required;
                    showFieldError($input, requiredMessage);
                    return false;
                }
                clearFieldError($input);
            }

            return true;
        }

        function validateReviewForm() {
            var msg = messages();
            var valid = true;
            var $firstInvalid = null;

            $form.find('input, textarea, select').each(function () {
                var $input = $(this);
                var type = ($input.attr('type') || '').toLowerCase();
                var fieldKey = ($input.attr('data-review-field') || '').toLowerCase();
                if (type === 'hidden' && fieldKey !== 'rating') {
                    return;
                }
                if (type === 'submit' || type === 'button' || type === 'reset') {
                    return;
                }
                if (!validateOneField($input)) {
                    valid = false;
                    if (!$firstInvalid) {
                        $firstInvalid = fieldKey === 'rating' ? $form.find('.crizal-review-stars__btn').first() : $input;
                    }
                }
            });

            var $captcha = $form.find('input[name="Captcha"]');
            if ($captcha.length && !validateOneField($captcha)) {
                valid = false;
                if (!$firstInvalid) {
                    $firstInvalid = $captcha;
                }
            }

            var $recaptcha = $form.find('.g-recaptcha');
            if ($recaptcha.length) {
                var recaptchaValue = '';
                try {
                    if (window.grecaptcha && typeof window.grecaptcha.getResponse === 'function') {
                        recaptchaValue = window.grecaptcha.getResponse();
                    }
                } catch (err) {
                    recaptchaValue = '';
                }
                var $recaptchaMsg = $form.find('.recaptcha-container .crizal-review-modal__error, .recaptcha-container [data-valmsg-for]').first();
                if (!recaptchaValue) {
                    $form.find('.crizal-review-captcha, .recaptcha-container').addClass('is-invalid');
                    if ($recaptchaMsg.length) {
                        $recaptchaMsg.text(msg.recaptcha)
                            .removeClass('field-validation-valid d-none')
                            .addClass('field-validation-error is-visible');
                    }
                    valid = false;
                } else if ($recaptchaMsg.length) {
                    $recaptchaMsg.text('').removeClass('field-validation-error is-visible').addClass('field-validation-valid');
                }
            }

            var $alert = $form.find('.crizal-review-modal__alert').first();
            if ($alert.length && $alert.attr('data-server-error') !== 'true') {
                if (!valid) {
                    $alert.removeClass('d-none').text(msg.required);
                } else {
                    $alert.addClass('d-none').text('');
                }
            }

            if (!valid && $firstInvalid && $firstInvalid.length) {
                try {
                    $firstInvalid.trigger('focus');
                    var el = $firstInvalid.get(0);
                    if (el && typeof el.scrollIntoView === 'function') {
                        el.scrollIntoView({ block: 'center', behavior: 'smooth' });
                    }
                } catch (err) { /* ignore */ }
            }

            return valid;
        }

        function paintStars(value, hoverValue) {
            $modal.find('.crizal-review-stars__btn').each(function () {
                var star = parseInt($(this).attr('data-rating'), 10) || 0;
                $(this).toggleClass('is-on', star <= (value || 0) && !hoverValue);
                $(this).toggleClass('is-hover-on', !!hoverValue && star <= hoverValue);
                $(this).attr('aria-pressed', star <= (value || 0) ? 'true' : 'false');
            });
            $modal.find('.crizal-review-stars').toggleClass('is-hover', !!hoverValue);
        }

        function currentRating() {
            var value = parseInt($('#review-rating').val(), 10);
            return value >= 1 && value <= 5 ? value : 0;
        }

        $(document).off('click.crizalLeaveReview', '[data-toggle="modal"][href="#leaveReview"], [data-toggle="modal"][data-target="#leaveReview"], [data-bs-toggle="modal"][data-bs-target="#leaveReview"], .crizal-product-reviews__cta');
        $(document).on('click.crizalLeaveReview', '[data-toggle="modal"][href="#leaveReview"], [data-toggle="modal"][data-target="#leaveReview"], [data-bs-toggle="modal"][data-bs-target="#leaveReview"], .crizal-product-reviews__cta', function (e) {
            e.preventDefault();
            showModal();
        });

        $(document).off('click.crizalLeaveReviewDismiss', '#leaveReview [data-dismiss="modal"], #leaveReview [data-bs-dismiss="modal"]');
        $(document).on('click.crizalLeaveReviewDismiss', '#leaveReview [data-dismiss="modal"], #leaveReview [data-bs-dismiss="modal"]', function (e) {
            e.preventDefault();
            hideModal();
        });

        $modal.off('click.crizalReviewStars', '.crizal-review-stars__btn');
        $modal.on('click.crizalReviewStars', '.crizal-review-stars__btn', function () {
            var value = parseInt($(this).attr('data-rating'), 10) || 0;
            $('#review-rating').val(value);
            paintStars(value);
            validateOneField($('#review-rating'));
        });
        $modal.off('mouseenter.crizalReviewStars mouseleave.crizalReviewStars', '.crizal-review-stars__btn');
        $modal.on('mouseenter.crizalReviewStars', '.crizal-review-stars__btn', function () {
            paintStars(currentRating(), parseInt($(this).attr('data-rating'), 10) || 0);
        });
        $modal.on('mouseleave.crizalReviewStars', '.crizal-review-stars', function () {
            paintStars(currentRating());
        });
        paintStars(currentRating());

        $form.off('submit.crizalReview');
        $form.on('submit.crizalReview', function (e) {
            $form.addClass('was-validated');
            if (!validateReviewForm()) {
                e.preventDefault();
                e.stopImmediatePropagation();
                return false;
            }
            return true;
        });

        $form.off('click.crizalReviewSubmit', '[type="submit"]');
        $form.on('click.crizalReviewSubmit', '[type="submit"]', function (e) {
            $form.addClass('was-validated');
            if (!validateReviewForm()) {
                e.preventDefault();
                e.stopImmediatePropagation();
                return false;
            }
            return true;
        });

        $form.off('blur.crizalReview input.crizalReview change.crizalReview', 'input, textarea, select');
        $form.on('blur.crizalReview change.crizalReview', 'input, textarea, select', function () {
            validateOneField($(this));
        });
        $form.on('input.crizalReview', 'input, textarea, select', function () {
            var $input = $(this);
            if ($input.hasClass('is-invalid') || $input.hasClass('input-validation-error') || $form.hasClass('was-validated')) {
                validateOneField($input);
            }
        });

        if ($form.attr('data-open-on-error') === 'true' || $modal.find('.crizal-review-modal__alert:not(.d-none)').filter(function () {
            return $.trim($(this).text()).length > 0;
        }).length) {
            window.setTimeout(showModal, 150);
        }

        $modal.off('shown.bs.modal.crizalReview');
        $modal.on('shown.bs.modal.crizalReview', function () {
            var $name = findField('name').add($form.find('input[name$=".Name"], input[name="Name"]')).first();
            if ($name.length) {
                $name.trigger('focus');
            }
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

    function initProductShortDescription($) {
        var $blocks = $('.crizal-product-shortdesc');
        if (!$blocks.length) {
            return;
        }

        $blocks.each(function () {
            var $block = $(this);
            var full = $block.find('.crizal-product-shortdesc__full')[0];
            var $preview = $block.find('.crizal-product-shortdesc__preview');
            var $btn = $block.find('.crizal-product-shortdesc__toggle');
            if (!full || !$btn.length) {
                return;
            }

            function syncToggle(isOpen) {
                $block.toggleClass('is-expanded', isOpen);
                $preview.toggleClass('d-none', isOpen);
                $btn.toggleClass('is-open', isOpen);
                $btn.attr('aria-expanded', isOpen ? 'true' : 'false');
                var label = isOpen ? $btn.attr('data-less-text') : $btn.attr('data-continue-text');
                $btn.find('.crizal-product-shortdesc__toggle-label').text(label);
            }

            full.addEventListener('shown.bs.collapse', function () { syncToggle(true); });
            full.addEventListener('hidden.bs.collapse', function () { syncToggle(false); });

            $btn.off('click.crizalShortDesc').on('click.crizalShortDesc', function (e) {
                e.preventDefault();
                if (window.bootstrap && window.bootstrap.Collapse) {
                    window.bootstrap.Collapse.getOrCreateInstance(full).toggle();
                    return;
                }
                var willOpen = !full.classList.contains('show');
                full.classList.toggle('show', willOpen);
                syncToggle(willOpen);
            });
        });
    }

    function initPageThemeLightbox() {
        document.querySelectorAll('[data-pt-lightbox]').forEach(function (gallery) {
            var modalId = gallery.getAttribute('data-pt-lightbox');
            var modalEl = document.getElementById(modalId);
            if (!modalEl) {
                return;
            }
            var carouselEl = modalEl.querySelector('.carousel');

            var pendingIdx = 0;
            var pointerStart = null;
            modalEl.addEventListener('shown.bs.modal', function () {
                if (carouselEl && window.bootstrap && window.bootstrap.Carousel) {
                    window.bootstrap.Carousel.getOrCreateInstance(carouselEl, { interval: false }).to(pendingIdx);
                }
            });

            gallery.addEventListener('pointerdown', function (e) {
                pointerStart = { x: e.clientX, y: e.clientY };
            });

            gallery.addEventListener('click', function (e) {
                var item = e.target.closest('[data-pt-slide]');
                if (!item || !gallery.contains(item)) {
                    return;
                }
                if (item.closest('.product-thumbnails')) {
                    return;
                }
                if (pointerStart && (Math.abs(e.clientX - pointerStart.x) > 8 || Math.abs(e.clientY - pointerStart.y) > 8)) {
                    e.preventDefault();
                    return;
                }
                e.preventDefault();
                pendingIdx = parseInt(item.getAttribute('data-pt-slide'), 10) || 0;
                showLightbox(modalEl, carouselEl, pendingIdx);
            });
        });

        document.addEventListener('click', function (e) {
            var trigger = e.target.closest('[data-pt-lightbox-single]');
            if (!trigger) {
                return;
            }
            var href = trigger.getAttribute('href');
            if (!href) {
                return;
            }
            e.preventDefault();
            var modalEl = ensureSingleLightbox();
            var img = modalEl.querySelector('.pt-lightbox__img');
            if (img) {
                img.src = href;
                img.alt = (trigger.querySelector('img') && trigger.querySelector('img').alt) || '';
            }
            showLightbox(modalEl, null, 0);
        });

        document.addEventListener('click', function (e) {
            var closeBtn = e.target.closest('.pt-lightbox__close');
            if (closeBtn) {
                var modal = closeBtn.closest('.pt-lightbox');
                if (modal) {
                    e.preventDefault();
                    e.stopPropagation();
                    if (window.bootstrap && window.bootstrap.Modal) {
                        var bsModal = window.bootstrap.Modal.getInstance(modal) || window.bootstrap.Modal.getOrCreateInstance(modal);
                        if (bsModal) {
                            bsModal.hide();
                        }
                    } else if (window.jQuery) {
                        window.jQuery(modal).modal('hide');
                    }
                }
            }
        });

        function showLightbox(modalEl, carouselEl, idx) {
            if (modalEl.parentElement !== document.body) {
                document.body.appendChild(modalEl);
            }
            if (window.bootstrap && window.bootstrap.Modal) {
                if (carouselEl && window.bootstrap.Carousel) {
                    window.bootstrap.Carousel.getOrCreateInstance(carouselEl, { interval: false }).to(idx);
                }
                window.bootstrap.Modal.getOrCreateInstance(modalEl).show();
                return;
            }
            if (window.jQuery) {
                var $ = window.jQuery;
                if (carouselEl) {
                    $(carouselEl).carousel(idx);
                }
                $(modalEl).modal('show');
            }
        }

        function ensureSingleLightbox() {
            var existing = document.getElementById('ptSingleLightbox');
            if (existing) {
                return existing;
            }
            var wrap = document.createElement('div');
            wrap.id = 'ptSingleLightbox';
            wrap.className = 'modal fade pt-lightbox';
            wrap.setAttribute('tabindex', '-1');
            wrap.setAttribute('aria-hidden', 'true');
            wrap.innerHTML =
                '<div class="modal-dialog modal-dialog-centered modal-xl">' +
                '<div class="modal-content">' +
                '<button type="button" class="pt-lightbox__close" data-bs-dismiss="modal" data-dismiss="modal" aria-label="Close">&times;</button>' +
                '<div class="modal-body p-0"><img class="pt-lightbox__img" alt=""></div>' +
                '</div></div>';
            document.body.appendChild(wrap);
            return wrap;
        }
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

    function markHeroSlidePlaceholder(slide) {
        if (!slide) {
            return;
        }
        slide.classList.add('crizal-home-hero__slide--placeholder');
        slide.style.backgroundImage = 'none';
        slide.removeAttribute('data-background');
    }

    function applyHeroBackground(slide, url) {
        if (!slide || !url) {
            return;
        }
        var safe = String(url).replace(/'/g, '%27');
        slide.style.backgroundImage = "url('" + safe + "')";
        slide.setAttribute('data-background', url);
        slide.classList.remove('crizal-home-hero__slide--placeholder');
    }

    /**
     * Probe hero slide backgrounds. Missing media must never surface as a visible
     * broken <img> / filename dump (owl forces .owl-item img display:block).
     */
    function ensureHeroSlideMedia(slide) {
        if (!slide || !window.Image) {
            return;
        }
        var primary = slide.getAttribute('data-background')
            || (slide.style && slide.style.backgroundImage
                ? String(slide.style.backgroundImage).replace(/^url\(["']?/, '').replace(/["']?\)$/, '')
                : '');
        var fallback = slide.getAttribute('data-hero-fallback') || '';
        if (!primary) {
            markHeroSlidePlaceholder(slide);
            return;
        }

        var probe = new Image();
        probe.onload = function () {
            applyHeroBackground(slide, primary);
        };
        probe.onerror = function () {
            if (fallback && fallback !== primary) {
                var probe2 = new Image();
                probe2.onload = function () {
                    applyHeroBackground(slide, fallback);
                };
                probe2.onerror = function () {
                    markHeroSlidePlaceholder(slide);
                };
                probe2.src = fallback;
                return;
            }
            markHeroSlidePlaceholder(slide);
        };
        probe.src = primary;
    }

    function initHomeHero($) {
        $('.crizal-home-hero .crizal-home-hero__slide').each(function () {
            ensureHeroSlideMedia(this);
        });

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
