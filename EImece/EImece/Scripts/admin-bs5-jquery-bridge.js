/**
 * jQuery plugin shims for Bootstrap 5 (modal/tab/tooltip/collapse/dropdown).
 * Keeps vendor scripts (Griddly, inline admin views) working without BS3 jQuery plugins.
 */
(function (window, $) {
    "use strict";
    if (!$ || !$.fn || !window.bootstrap) {
        return;
    }

    function eachComponent(collection, Component, fn) {
        return collection.each(function () {
            var instance = Component.getOrCreateInstance(this);
            fn.call(this, instance);
        });
    }

    if (!$.fn.modal) {
        $.fn.modal = function (action) {
            var Component = window.bootstrap.Modal;
            if (!Component) {
                return this;
            }
            if (action && typeof action === "object") {
                return this.each(function () {
                    Component.getOrCreateInstance(this, action);
                    if (action.show === true) {
                        Component.getOrCreateInstance(this).show();
                    }
                });
            }
            if (action === "show") {
                return eachComponent(this, Component, function (i) { i.show(); });
            }
            if (action === "hide") {
                return eachComponent(this, Component, function (i) { i.hide(); });
            }
            if (action === "toggle") {
                return eachComponent(this, Component, function (i) { i.toggle(); });
            }
            return this.each(function () {
                Component.getOrCreateInstance(this);
            });
        };
    }

    if (!$.fn.tab) {
        $.fn.tab = function (action) {
            var Component = window.bootstrap.Tab;
            if (!Component) {
                return this;
            }
            if (action === "show") {
                return eachComponent(this, Component, function (i) { i.show(); });
            }
            return this;
        };
    }

    if (!$.fn.tooltip) {
        $.fn.tooltip = function (opts) {
            var Component = window.bootstrap.Tooltip;
            if (!Component) {
                return this;
            }
            var options = (opts && typeof opts === "object") ? opts : {};
            return this.each(function () {
                Component.getOrCreateInstance(this, options);
            });
        };
    }

    if (!$.fn.collapse) {
        $.fn.collapse = function (action) {
            var Component = window.bootstrap.Collapse;
            if (!Component) {
                return this;
            }
            if (action === "show") {
                return eachComponent(this, Component, function (i) { i.show(); });
            }
            if (action === "hide") {
                return eachComponent(this, Component, function (i) { i.hide(); });
            }
            if (action === "toggle") {
                return eachComponent(this, Component, function (i) { i.toggle(); });
            }
            return this;
        };
    }

    if (!$.fn.dropdown) {
        $.fn.dropdown = function (action) {
            var Component = window.bootstrap.Dropdown;
            if (!Component) {
                return this;
            }
            if (action === "show") {
                return eachComponent(this, Component, function (i) { i.show(); });
            }
            if (action === "hide") {
                return eachComponent(this, Component, function (i) { i.hide(); });
            }
            if (action === "toggle") {
                return eachComponent(this, Component, function (i) { i.toggle(); });
            }
            return this;
        };
    }
}(window, window.jQuery));
