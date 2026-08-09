/**
 * Centralized TinyMCE integration for the EImece admin panel.
 *
 * Usage:
 *   EImece.RichTextEditor.init();
 *   EImece.RichTextEditor.init('#Description');
 *   EImece.RichTextEditor.destroy('#Description');
 */
(function (window, document) {
    'use strict';

    window.EImece = window.EImece || {};

    var DEFAULT_SELECTOR = 'textarea.eimece-rich-text-editor';
    var TINYMCE_BASE = '/Content/tinymce';
    var TINYMCE_SCRIPT = TINYMCE_BASE + '/tinymce.min.js';
    var UPLOAD_URL = '/Admin/FileUpload/UploadEditorImage';

    var loadPromise = null;
    var bootstrapFocusFixBound = false;

    function getAntiForgeryToken() {
        var token = null;
        if (window.jQuery) {
            token = window.jQuery("#__AjaxAntiForgeryForm input[name='__RequestVerificationToken']").val()
                || window.jQuery("input[name='__RequestVerificationToken']").first().val();
        }
        if (!token) {
            var input = document.querySelector("#__AjaxAntiForgeryForm input[name='__RequestVerificationToken']")
                || document.querySelector("input[name='__RequestVerificationToken']");
            token = input ? input.value : null;
        }
        return token;
    }

    function resolveLanguage() {
        // CookieLanguage holds EImeceLanguage enum int (1=Turkish, 2=English, ...).
        var el = document.getElementById('CookieLanguage');
        var raw = el ? String(el.value || '').trim().toLowerCase() : '';
        if (raw === '1' || raw.indexOf('tr') === 0) {
            return 'tr';
        }
        // English is TinyMCE default; other cultures fall back to English UI.
        return null;
    }

    function ensureTinyMceLoaded() {
        if (window.tinymce) {
            return Promise.resolve(window.tinymce);
        }
        if (loadPromise) {
            return loadPromise;
        }
        loadPromise = new Promise(function (resolve, reject) {
            var existing = document.querySelector('script[data-eimece-tinymce="1"]');
            if (existing) {
                existing.addEventListener('load', function () { resolve(window.tinymce); });
                existing.addEventListener('error', reject);
                return;
            }
            var script = document.createElement('script');
            script.src = TINYMCE_SCRIPT;
            script.async = true;
            script.setAttribute('data-eimece-tinymce', '1');
            script.onload = function () { resolve(window.tinymce); };
            script.onerror = function () {
                loadPromise = null;
                reject(new Error('Failed to load TinyMCE from ' + TINYMCE_SCRIPT));
            };
            document.head.appendChild(script);
        });
        return loadPromise;
    }

    function bindBootstrapFocusFix() {
        if (bootstrapFocusFixBound || !window.jQuery) {
            return;
        }
        bootstrapFocusFixBound = true;
        // Prevent Bootstrap 3 modals from stealing focus from TinyMCE dialogs.
        window.jQuery(document).on('focusin.eimeceTinymce', function (e) {
            if (window.jQuery(e.target).closest('.tox-tinymce-aux, .tox-dialog, .tox-dialog-wrap').length) {
                e.stopImmediatePropagation();
            }
        });
    }

    function syncAllEditors() {
        if (window.tinymce && typeof window.tinymce.triggerSave === 'function') {
            window.tinymce.triggerSave();
        }
    }

    function bindFormSubmitSync() {
        if (!window.jQuery) {
            return;
        }
        window.jQuery(document)
            .off('submit.eimeceTinymce', 'form')
            .on('submit.eimeceTinymce', 'form', function () {
                syncAllEditors();
            });
    }

    function bindUnobtrusiveValidation() {
        if (!window.jQuery || !window.jQuery.validator) {
            return;
        }
        // Ensure jQuery Validate reads TinyMCE content from the underlying textarea.
        window.jQuery.validator.setDefaults({
            ignore: ':hidden:not(textarea.eimece-rich-text-editor)'
        });
    }

    function buildImagesUploadHandler() {
        return function (blobInfo, progress) {
            return new Promise(function (resolve, reject) {
                var xhr = new XMLHttpRequest();
                xhr.open('POST', UPLOAD_URL);
                xhr.withCredentials = true;

                var token = getAntiForgeryToken();
                if (token) {
                    xhr.setRequestHeader('RequestVerificationToken', token);
                }

                xhr.upload.onprogress = function (e) {
                    if (e.lengthComputable && typeof progress === 'function') {
                        progress(e.loaded / e.total * 100);
                    }
                };

                xhr.onload = function () {
                    if (xhr.status < 200 || xhr.status >= 300) {
                        reject('HTTP Error: ' + xhr.status);
                        return;
                    }
                    var json;
                    try {
                        json = JSON.parse(xhr.responseText);
                    } catch (err) {
                        reject('Invalid JSON: ' + xhr.responseText);
                        return;
                    }
                    if (!json || typeof json.location !== 'string') {
                        reject(json && json.error ? json.error : 'Upload failed');
                        return;
                    }
                    resolve(json.location);
                };

                xhr.onerror = function () {
                    reject('Image upload failed due to a XHR Transport error.');
                };

                var formData = new FormData();
                formData.append('file', blobInfo.blob(), blobInfo.filename());
                if (token) {
                    formData.append('__RequestVerificationToken', token);
                }
                xhr.send(formData);
            });
        };
    }

    function getBaseConfig(language) {
        var config = {
            base_url: TINYMCE_BASE,
            suffix: '.min',
            license_key: 'gpl',
            height: 550,
            menubar: 'file edit view insert format tools table',
            plugins: [
                'advlist', 'autolink', 'lists', 'link', 'image', 'charmap', 'preview',
                'anchor', 'searchreplace', 'visualblocks', 'code', 'fullscreen',
                'insertdatetime', 'media', 'table', 'help', 'wordcount', 'directionality'
            ],
            toolbar: [
                'undo redo | blocks | bold italic underline strikethrough | forecolor backcolor | alignleft aligncenter alignright alignjustify',
                'bullist numlist outdent indent | link image media table | removeformat | code fullscreen'
            ].join(' | '),
            block_formats: 'Paragraph=p; Heading 1=h1; Heading 2=h2; Heading 3=h3; Heading 4=h4; Heading 5=h5; Heading 6=h6; Preformatted=pre',
            branding: false,
            promotion: false,
            convert_urls: false,
            relative_urls: false,
            remove_script_host: false,
            entity_encoding: 'raw',
            valid_elements: '*[*]',
            extended_valid_elements: '*[*]',
            images_upload_handler: buildImagesUploadHandler(),
            automatic_uploads: true,
            file_picker_types: 'image',
            image_title: true,
            image_caption: true,
            table_toolbar: 'tableprops tabledelete | tableinsertrowbefore tableinsertrowafter tabledeleterow | tableinsertcolbefore tableinsertcolafter tabledeletecol',
            content_style: 'body { font-family: Helvetica, Arial, sans-serif; font-size: 14px; }',
            setup: function (editor) {
                editor.on('change keyup', function () {
                    editor.save();
                });
            }
        };

        if (language) {
            config.language = language;
            config.language_url = TINYMCE_BASE + '/langs/' + language + '.js';
        }

        return config;
    }

    function alreadyInitialized(element) {
        if (!window.tinymce || !element || !element.id) {
            return false;
        }
        return !!window.tinymce.get(element.id);
    }

    function ensureElementId(element, index) {
        if (!element.id) {
            element.id = 'eimece-rte-' + Date.now() + '-' + index;
        }
        return element.id;
    }

    function initElements(elements) {
        if (!elements || !elements.length) {
            return;
        }

        bindBootstrapFocusFix();
        bindFormSubmitSync();
        bindUnobtrusiveValidation();

        var language = resolveLanguage();
        var baseConfig = getBaseConfig(language);

        for (var i = 0; i < elements.length; i++) {
            var el = elements[i];
            if (!el || el.tagName.toLowerCase() !== 'textarea') {
                continue;
            }
            if (el.getAttribute('data-eimece-rte-init') === '1' || alreadyInitialized(el)) {
                continue;
            }

            ensureElementId(el, i);
            el.classList.add('eimece-rich-text-editor');
            el.setAttribute('data-eimece-rte-init', '1');

            var config = {};
            for (var key in baseConfig) {
                if (Object.prototype.hasOwnProperty.call(baseConfig, key)) {
                    config[key] = baseConfig[key];
                }
            }
            config.target = el;
            window.tinymce.init(config);
        }
    }

    function queryElements(selectorOrElement) {
        if (!selectorOrElement) {
            return Array.prototype.slice.call(document.querySelectorAll(DEFAULT_SELECTOR));
        }
        if (selectorOrElement.nodeType === 1) {
            return [selectorOrElement];
        }
        if (typeof selectorOrElement === 'string') {
            return Array.prototype.slice.call(document.querySelectorAll(selectorOrElement));
        }
        return [];
    }

    function destroyElements(elements) {
        if (!window.tinymce || !elements) {
            return;
        }
        for (var i = 0; i < elements.length; i++) {
            var el = elements[i];
            if (!el || !el.id) {
                continue;
            }
            var editor = window.tinymce.get(el.id);
            if (editor) {
                editor.remove();
            }
            el.removeAttribute('data-eimece-rte-init');
        }
    }

    EImece.RichTextEditor = {
        selector: DEFAULT_SELECTOR,

        init: function (selectorOrElement) {
            var elements = queryElements(selectorOrElement || DEFAULT_SELECTOR);
            if (!elements.length) {
                return Promise.resolve();
            }

            return ensureTinyMceLoaded().then(function () {
                initElements(elements);
            }).catch(function (err) {
                if (window.console && console.error) {
                    console.error('EImece.RichTextEditor failed to initialize', err);
                }
            });
        },

        destroy: function (selectorOrElement) {
            var elements = queryElements(selectorOrElement || DEFAULT_SELECTOR);
            destroyElements(elements);
        },

        triggerSave: function () {
            syncAllEditors();
        },

        /**
         * Call after a Bootstrap tab/panel that contains editors becomes visible.
         * TinyMCE needs a layout refresh when initialized inside a hidden tab.
         */
        refreshVisible: function () {
            if (!window.tinymce) {
                return this.init();
            }
            var editors = window.tinymce.editors || [];
            for (var i = 0; i < editors.length; i++) {
                var editor = editors[i];
                if (!editor) {
                    continue;
                }
                try {
                    if (typeof editor.show === 'function') {
                        editor.show();
                    }
                    editor.fire('ResizeEditor');
                    if (editor.theme && typeof editor.theme.resizeTo === 'function') {
                        // no-op for themes that support it
                    }
                    if (typeof editor.execCommand === 'function') {
                        editor.execCommand('mceAutoResize');
                    }
                } catch (err) {
                    // Ignore per-editor refresh failures.
                }
            }
            return Promise.resolve();
        }
    };
})(window, document);
