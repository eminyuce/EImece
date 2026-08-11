/**
 * Visual drag-drop builder for product-spec TemplateXml.
 * Depends on jQuery + jQuery UI Sortable/Draggable.
 */
(function (window, $) {
    'use strict';

    if (!$ || !$.fn) {
        return;
    }

    var TYPE_LABELS = {
        textbox: 'Metin',
        textarea: 'Metin alanı (HTML)',
        dropdown: 'Açılır liste',
        radio: 'Radyo düğmeleri',
        checkbox: 'Evet / Hayır',
        multiselect: 'Çoklu seçim',
        datetime: 'Tarih / Saat',
        group: 'Grup'
    };

    var FIELD_TYPES = {
        textbox: true,
        textarea: true,
        dropdown: true,
        radio: true,
        checkbox: true,
        multiselect: true,
        datetime: true
    };

    function normalizeFieldType(type) {
        type = (type || 'textbox').toLowerCase();
        if (type === 'date') {
            return 'datetime';
        }
        if (type === 'select') {
            return 'dropdown';
        }
        if (type === 'bool' || type === 'boolean') {
            return 'checkbox';
        }
        if (type === 'checkboxes' || type === 'multicheckbox') {
            return 'multiselect';
        }
        if (type === 'text' || type === 'input') {
            return 'textbox';
        }
        return FIELD_TYPES[type] ? type : 'textbox';
    }

    var SAMPLES = [
        {
            name: 'Giyim Özellikleri',
            position: 1,
            explain: 'Giyim örneği: Renk/Beden için Değer listelerinde “Renkler” ve “Bedenler” oluşturun.',
            xml: '<component>\n  <group name="Giyim Özellikleri">\n    <dropdown name="Renk" values="Renkler" />\n    <dropdown name="Beden" values="Bedenler" />\n    <textbox name="Malzeme" display="Kumaş / Malzeme" />\n    <checkbox name="Yıkamaya Uygun" />\n  </group>\n</component>'
        },
        {
            name: 'Elektronik Özellikleri',
            position: 2,
            explain: 'Elektronik örneği: Marka/Model metin; Garanti ve Ağırlık birimli.',
            xml: '<component>\n  <group name="Teknik Özellikler">\n    <textbox name="Marka" />\n    <textbox name="Model" />\n    <textbox name="Garanti" unit="ay" />\n    <textbox name="Ağırlık" unit="kg" />\n    <dropdown name="Renk" values="Renkler" />\n    <checkbox name="Kutu İçeriği Tam mı?" />\n  </group>\n</component>'
        },
        {
            name: 'Ev & Mobilya Özellikleri',
            position: 3,
            explain: 'Mobilya örneği: Ölçü birimleri cm/kg.',
            xml: '<component>\n  <group name="Ürün Ölçüleri">\n    <textbox name="Genişlik" unit="cm" />\n    <textbox name="Yükseklik" unit="cm" />\n    <textbox name="Derinlik" unit="cm" />\n    <textbox name="Ağırlık" unit="kg" />\n    <dropdown name="Renk" values="Renkler" />\n    <textbox name="Malzeme" />\n  </group>\n</component>'
        },
        {
            name: 'Kozmetik Özellikleri',
            position: 4,
            explain: 'Kozmetik örneği: Cilt Tipi listesi gerekir.',
            xml: '<component>\n  <group name="Kozmetik Bilgileri">\n    <textbox name="Hacim" unit="ml" />\n    <dropdown name="Cilt Tipi" values="Cilt Tipleri" />\n    <textbox name="İçerik Özeti" display="Ana içerik" />\n    <checkbox name="Paraben İçermez" />\n    <checkbox name="Hayvan Deneyi Yok" />\n  </group>\n</component>'
        },
        {
            name: 'Ayakkabı Özellikleri',
            position: 5,
            explain: 'Ayakkabı örneği: Numara ve Renk listeleri gerekir.',
            xml: '<component>\n  <group name="Ayakkabı Özellikleri">\n    <dropdown name="Numara" values="Ayakkabı Numaraları" />\n    <dropdown name="Renk" values="Renkler" />\n    <textbox name="Taban" />\n    <textbox name="Materyal" />\n    <checkbox name="Su Geçirmez" />\n  </group>\n</component>'
        },
        {
            name: 'Açıklama, Durum ve Tarih',
            position: 6,
            explain: 'Yeni bileşenler: HTML metin alanı, radyo, çoklu seçim, tarih/saat (time=false ile sadece tarih).',
            xml: '<component>\n  <group name="Ek Bilgiler">\n    <textarea name="Aciklama" display="Açıklama" html="true" />\n    <radio name="Durum" values="Yeni, İkinci El, Yenilenmiş" />\n    <multiselect name="Ozellikler" display="Özellikler" values="Su Geçirmez, Hafif, Dayanıklı" />\n    <datetime name="Randevu" display="Randevu Tarihi" time="true" />\n    <datetime name="Teslim" display="Teslim Tarihi" time="false" />\n  </group>\n</component>'
        }
    ];

    function escapeXml(value) {
        return String(value == null ? '' : value)
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;');
    }

    function attr(el, name) {
        if (!el || !el.getAttribute) {
            return '';
        }
        var v = el.getAttribute(name);
        return v == null ? '' : v;
    }

    function AdminTemplateBuilder(options) {
        this.$root = $(options.root);
        this.$canvas = this.$root.find('[data-tb-canvas]');
        this.$validation = this.$root.find('[data-tb-validation]');
        this.listNames = options.listNames || [];
        this.listsUrl = options.listsUrl || '/admin/lists/';
        this.onChange = typeof options.onChange === 'function' ? options.onChange : null;
        this._uid = 0;
        this._init();
    }

    AdminTemplateBuilder.prototype._init = function () {
        var self = this;
        this._bindPalette();
        this._bindToolbar();
        this._initGroupSortable();

        this.$canvas.on('click', '[data-tb-delete-group]', function (e) {
            e.preventDefault();
            var $group = $(this).closest('.tb-group');
            if (self.$canvas.find('.tb-group').length <= 1) {
                window.alert('En az bir grup kalmalıdır.');
                return;
            }
            if (window.confirm('Bu grup ve içindeki alanlar silinsin mi?')) {
                $group.remove();
                self._ensureEmptyHint();
                self._emitChange();
            }
        });

        this.$canvas.on('click', '[data-tb-delete-field]', function (e) {
            e.preventDefault();
            $(this).closest('.tb-field').remove();
            self._emitChange();
        });

        this.$canvas.on('input change', 'input, select', function () {
            self._emitChange();
        });
    };

    AdminTemplateBuilder.prototype._emitChange = function () {
        if (this.onChange) {
            this.onChange(this.toXml());
        }
    };

    AdminTemplateBuilder.prototype._nextId = function () {
        this._uid += 1;
        return 'tb' + this._uid;
    };

    AdminTemplateBuilder.prototype._bindPalette = function () {
        var self = this;
        this.$root.find('[data-tb-add]').on('click', function (e) {
            e.preventDefault();
            var type = $(this).data('tb-add');
            if (type === 'group') {
                self.addGroup('Yeni Grup');
            } else {
                var $target = self.$canvas.find('.tb-fields').last();
                if (!$target.length) {
                    self.addGroup('Ürün Özellikleri');
                    $target = self.$canvas.find('.tb-fields').last();
                }
                self.addField($target, type, {});
            }
            self._emitChange();
        });

        if ($.fn.draggable) {
            this.$root.find('.tb-palette__item').draggable({
                helper: 'clone',
                revert: 'invalid',
                zIndex: 1000,
                connectToSortable: false
            });
        }

        this.$canvas.droppable({
            accept: '.tb-palette__item[data-tb-add="group"]',
            hoverClass: 'tb-canvas--drop-hover',
            drop: function (event, ui) {
                var type = ui.draggable.data('tb-add');
                if (type === 'group') {
                    self.addGroup('Yeni Grup');
                    self._emitChange();
                }
            }
        });
    };

    AdminTemplateBuilder.prototype._bindToolbar = function () {
        var self = this;
        this.$root.find('[data-tb-sample]').on('click', function (e) {
            e.preventDefault();
            if (self.hasContent()) {
                if (!window.confirm('Mevcut şablon üzerine örnek yazılacak. Devam edilsin mi?')) {
                    return;
                }
            }
            var sample = SAMPLES[Math.floor(Math.random() * SAMPLES.length)];
            self.fromXml(sample.xml);
            self.$root.trigger('tb:sample', [sample]);
            self._emitChange();
        });

        this.$root.find('[data-tb-clear]').on('click', function (e) {
            e.preventDefault();
            if (!window.confirm('Tüm grup ve alanlar temizlensin mi?')) {
                return;
            }
            self.clear();
            self.addGroup('Ürün Özellikleri');
            self._emitChange();
        });
    };

    AdminTemplateBuilder.prototype._initGroupSortable = function () {
        var self = this;
        if (!$.fn.sortable) {
            return;
        }
        this.$canvas.sortable({
            items: '> .tb-group',
            handle: '.tb-group__drag',
            axis: 'y',
            tolerance: 'pointer',
            update: function () {
                self._emitChange();
            }
        });
    };

    AdminTemplateBuilder.prototype._initFieldSortable = function ($fields) {
        var self = this;
        if (!$.fn.sortable) {
            return;
        }
        $fields.sortable({
            items: '> .tb-field',
            handle: '.tb-field__drag',
            connectWith: '[data-tb-canvas] .tb-fields',
            placeholder: 'tb-field-placeholder',
            tolerance: 'pointer',
            receive: function (event, ui) {
                // Palette drop: convert helper clone into real field card
                var $item = ui.item;
                if ($item.hasClass('tb-palette__item')) {
                    var type = $item.data('tb-add');
                    var $card = self._buildFieldEl(type, {});
                    $item.replaceWith($card);
                    self._emitChange();
                }
            },
            update: function () {
                self._emitChange();
            }
        });

        // Allow dropping palette field chips onto field lists
        $fields.droppable({
            accept: '.tb-palette__item[data-tb-add!="group"]',
            hoverClass: 'tb-fields--drop-hover',
            greedy: true,
            drop: function (event, ui) {
                if (!ui.draggable.hasClass('tb-palette__item')) {
                    return;
                }
                var type = ui.draggable.data('tb-add');
                if (type && type !== 'group') {
                    self.addField($(this), type, {});
                    self._emitChange();
                }
            }
        });
    };

    AdminTemplateBuilder.prototype._ensureEmptyHint = function () {
        this.$canvas.find('.tb-empty').remove();
        if (!this.$canvas.find('.tb-group').length) {
            this.$canvas.append(
                '<div class="tb-empty">Henüz grup yok. Paletten <strong>Grup</strong> ekleyin veya örnek yükleyin.</div>'
            );
        }
    };

    AdminTemplateBuilder.prototype._valuesSelectHtml = function (selected) {
        var html = '<select class="form-control input-sm tb-field-values" title="Değer listesi adı (values)">';
        html += '<option value="">— Liste seçin —</option>';
        var found = false;
        var i;
        for (i = 0; i < this.listNames.length; i++) {
            var n = this.listNames[i];
            var sel = selected && String(selected).toLowerCase() === String(n).toLowerCase() ? ' selected' : '';
            if (sel) {
                found = true;
            }
            html += '<option value="' + escapeXml(n) + '"' + sel + '>' + escapeXml(n) + '</option>';
        }
        if (selected && !found) {
            html += '<option value="' + escapeXml(selected) + '" selected>' + escapeXml(selected) + ' (kayıtlı değil)</option>';
        }
        html += '</select>';
        html += '<input type="text" class="form-control input-sm tb-field-values-custom" placeholder="veya liste adı yazın" value="' +
            (selected && !found ? escapeXml(selected) : '') + '" style="' + (selected && !found ? '' : 'display:none;') + '" />';
        return html;
    };

    AdminTemplateBuilder.prototype._bindValuesSelect = function ($el) {
        $el.find('.tb-field-values').on('change', function () {
            var v = $(this).val();
            var $custom = $el.find('.tb-field-values-custom');
            if (!v) {
                $custom.show().focus();
            } else {
                $custom.hide().val('');
            }
        });
    };

    AdminTemplateBuilder.prototype._buildFieldEl = function (type, data) {
        type = normalizeFieldType(type);
        data = data || {};
        var id = this._nextId();
        var label = TYPE_LABELS[type] || type;
        var htmlChecked = data.html === false || data.html === 'false' ? '' : ' checked';
        var timeChecked = data.time === false || data.time === 'false' ? '' : ' checked';
        var $el = $(
            '<div class="tb-field" data-type="' + type + '" data-tb-id="' + id + '">' +
            '  <span class="tb-field__drag" title="Sürükle"><span class="glyphicon glyphicon-move"></span></span>' +
            '  <span class="tb-field__badge">' + escapeXml(label) + '</span>' +
            '  <div class="tb-field__cols">' +
            '    <div class="tb-field__col">' +
            '      <label>Alan adı (name)</label>' +
            '      <input type="text" class="form-control input-sm tb-field-name" placeholder="Örn: Renk" value="' + escapeXml(data.name || '') + '" />' +
            '    </div>' +
            '    <div class="tb-field__col tb-col-display">' +
            '      <label>Görünen etiket (display)</label>' +
            '      <input type="text" class="form-control input-sm tb-field-display" placeholder="İsteğe bağlı" value="' + escapeXml(data.display || '') + '" />' +
            '    </div>' +
            '    <div class="tb-field__col tb-col-unit">' +
            '      <label>Birim (unit)</label>' +
            '      <input type="text" class="form-control input-sm tb-field-unit" placeholder="kg, cm…" value="' + escapeXml(data.unit || '') + '" />' +
            '    </div>' +
            '    <div class="tb-field__col tb-col-values">' +
            '      <label>Liste (values) <a href="' + escapeXml(this.listsUrl) + '" target="_blank" class="tb-lists-link">Listeler</a></label>' +
            '      <div class="tb-values-wrap"></div>' +
            '    </div>' +
            '    <div class="tb-field__col tb-col-html">' +
            '      <label class="tb-flag-label">' +
            '        <input type="checkbox" class="tb-field-html"' + htmlChecked + ' /> HTML kabul et' +
            '      </label>' +
            '    </div>' +
            '    <div class="tb-field__col tb-col-time">' +
            '      <label class="tb-flag-label">' +
            '        <input type="checkbox" class="tb-field-time"' + timeChecked + ' /> Saat göster' +
            '      </label>' +
            '    </div>' +
            '  </div>' +
            '  <button type="button" class="btn btn-default btn-xs tb-field__delete" data-tb-delete-field title="Alanı sil">&times;</button>' +
            '</div>'
        );

        if (type === 'checkbox') {
            $el.find('.tb-col-unit, .tb-col-values, .tb-col-html, .tb-col-time').hide();
        } else if (type === 'textbox') {
            $el.find('.tb-col-values, .tb-col-html, .tb-col-time').hide();
        } else if (type === 'textarea') {
            $el.find('.tb-col-unit, .tb-col-values, .tb-col-time').hide();
        } else if (type === 'dropdown' || type === 'radio' || type === 'multiselect') {
            $el.find('.tb-col-unit, .tb-col-html, .tb-col-time').hide();
            $el.find('.tb-values-wrap').html(this._valuesSelectHtml(data.values || ''));
            this._bindValuesSelect($el);
        } else if (type === 'datetime') {
            $el.find('.tb-col-unit, .tb-col-values, .tb-col-html').hide();
        }

        return $el;
    };

    AdminTemplateBuilder.prototype._buildGroupEl = function (name, fields) {
        var self = this;
        var id = this._nextId();
        var $group = $(
            '<div class="tb-group" data-type="group" data-tb-id="' + id + '">' +
            '  <div class="tb-group__header">' +
            '    <span class="tb-group__drag" title="Grubu sürükle"><span class="glyphicon glyphicon-move"></span></span>' +
            '    <label class="tb-group__label">Grup adı</label>' +
            '    <input type="text" class="form-control tb-group-name" value="' + escapeXml(name || 'Ürün Özellikleri') + '" />' +
            '    <button type="button" class="btn btn-default btn-sm" data-tb-delete-group title="Grubu sil">' +
            '      <span class="glyphicon glyphicon-trash"></span> Sil' +
            '    </button>' +
            '  </div>' +
            '  <div class="tb-fields"></div>' +
            '  <div class="tb-group__hint">Alan eklemek için üstteki bileşenlere tıklayın veya buraya sürükleyin.</div>' +
            '</div>'
        );
        var $fields = $group.find('.tb-fields');
        this._initFieldSortable($fields);
        if (fields && fields.length) {
            fields.forEach(function (f) {
                $fields.append(self._buildFieldEl(f.type, f));
            });
        }
        return $group;
    };

    AdminTemplateBuilder.prototype.addGroup = function (name, fields) {
        this.$canvas.find('.tb-empty').remove();
        var $group = this._buildGroupEl(name, fields || []);
        this.$canvas.append($group);
        return $group;
    };

    AdminTemplateBuilder.prototype.addField = function ($fields, type, data) {
        if (!$fields || !$fields.length) {
            return null;
        }
        var $card = this._buildFieldEl(type, data || {});
        $fields.append($card);
        return $card;
    };

    AdminTemplateBuilder.prototype.clear = function () {
        this.$canvas.empty();
        this._ensureEmptyHint();
        this.hideValidation();
    };

    AdminTemplateBuilder.prototype.hasContent = function () {
        return this.$canvas.find('.tb-group').length > 0 ||
            this.$canvas.find('.tb-field').length > 0;
    };

    AdminTemplateBuilder.prototype.fromXml = function (xml) {
        var self = this;
        this.clear();
        xml = (xml || '').trim();
        if (!xml) {
            this.addGroup('Ürün Özellikleri');
            return true;
        }

        var doc;
        try {
            doc = new DOMParser().parseFromString(xml, 'text/xml');
        } catch (e) {
            this.addGroup('Ürün Özellikleri');
            return false;
        }

        if (doc.querySelector('parsererror')) {
            this.addGroup('Ürün Özellikleri');
            return false;
        }

        var groups = doc.getElementsByTagName('group');
        if (!groups.length) {
            this.addGroup('Ürün Özellikleri');
            return true;
        }

        for (var i = 0; i < groups.length; i++) {
            var g = groups[i];
            var gName = attr(g, 'name') || ('Grup ' + (i + 1));
            var fields = [];
            var children = g.children || g.childNodes;
            for (var j = 0; j < children.length; j++) {
                var el = children[j];
                if (!el.tagName) {
                    continue;
                }
                var tag = el.tagName.toLowerCase();
                if (!FIELD_TYPES[tag] && tag !== 'date' && tag !== 'checkboxes' && tag !== 'multicheckbox') {
                    continue;
                }
                var fieldType = normalizeFieldType(tag);
                var timeAttr = attr(el, 'time');
                var htmlAttr = attr(el, 'html');
                var includeTime = true;
                if (tag === 'date' || timeAttr === 'false' || timeAttr === '0' || timeAttr === 'no') {
                    includeTime = false;
                }
                fields.push({
                    type: fieldType,
                    name: attr(el, 'name'),
                    display: attr(el, 'display'),
                    unit: attr(el, 'unit'),
                    values: attr(el, 'values'),
                    html: htmlAttr === 'false' || htmlAttr === '0' || htmlAttr === 'no' ? false : true,
                    time: includeTime
                });
            }
            this.addGroup(gName, fields);
        }
        this._ensureEmptyHint();
        return true;
    };

    AdminTemplateBuilder.prototype._fieldValues = function ($field) {
        var $sel = $field.find('.tb-field-values');
        var $custom = $field.find('.tb-field-values-custom');
        if ($sel.length && $sel.val()) {
            return ($sel.val() || '').trim();
        }
        if ($custom.length && $custom.is(':visible')) {
            return ($custom.val() || '').trim();
        }
        if ($custom.length && ($custom.val() || '').trim()) {
            return ($custom.val() || '').trim();
        }
        return '';
    };

    AdminTemplateBuilder.prototype.toXml = function () {
        var self = this;
        var lines = ['<component>'];
        this.$canvas.find('.tb-group').each(function () {
            var $g = $(this);
            var gName = ($g.find('.tb-group-name').val() || '').trim() || 'Grup';
            lines.push('  <group name="' + escapeXml(gName) + '">');
            $g.find('.tb-fields > .tb-field').each(function () {
                var $f = $(this);
                var type = normalizeFieldType($f.data('type'));
                var name = ($f.find('.tb-field-name').val() || '').trim();
                if (!name) {
                    return;
                }
                var display = ($f.find('.tb-field-display').val() || '').trim();
                var unit = ($f.find('.tb-field-unit').val() || '').trim();
                var values = self._fieldValues($f);
                var attrs = ' name="' + escapeXml(name) + '"';
                if (display) {
                    attrs += ' display="' + escapeXml(display) + '"';
                }
                if ((type === 'textbox' || type === 'textarea') && unit) {
                    attrs += ' unit="' + escapeXml(unit) + '"';
                }
                if ((type === 'dropdown' || type === 'radio' || type === 'multiselect') && values) {
                    attrs += ' values="' + escapeXml(values) + '"';
                }
                if (type === 'textarea') {
                    attrs += ' html="' + ($f.find('.tb-field-html').is(':checked') ? 'true' : 'false') + '"';
                }
                if (type === 'datetime') {
                    attrs += ' time="' + ($f.find('.tb-field-time').is(':checked') ? 'true' : 'false') + '"';
                }
                lines.push('    <' + type + attrs + ' />');
            });
            lines.push('  </group>');
        });
        lines.push('</component>');
        return lines.join('\n');
    };

    AdminTemplateBuilder.prototype.validate = function () {
        var errors = [];
        var $groups = this.$canvas.find('.tb-group');
        if (!$groups.length) {
            errors.push('En az bir grup ekleyin.');
        }
        var fieldCount = 0;
        var self = this;
        $groups.each(function (gi) {
            var $g = $(this);
            var gName = ($g.find('.tb-group-name').val() || '').trim();
            if (!gName) {
                errors.push('Grup ' + (gi + 1) + ': grup adı boş olamaz.');
            }
            $g.find('.tb-fields > .tb-field').each(function (fi) {
                fieldCount += 1;
                var $f = $(this);
                var type = ($f.data('type') || '').toLowerCase();
                var name = ($f.find('.tb-field-name').val() || '').trim();
                if (!name) {
                    errors.push('Grup "' + (gName || (gi + 1)) + '", alan ' + (fi + 1) + ': alan adı (name) zorunlu.');
                }
                if (type === 'dropdown' || type === 'radio' || type === 'multiselect') {
                    var values = self._fieldValues($f);
                    if (!values) {
                        var kindLabel = type === 'radio' ? 'radyo' : (type === 'multiselect' ? 'çoklu seçim' : 'açılır liste');
                        errors.push('Alan "' + (name || (fi + 1)) + '": ' + kindLabel +
                            ' için values (liste adı veya A, B, C) seçin/yazın.');
                    }
                }
            });
        });
        if ($groups.length && fieldCount === 0) {
            errors.push('En az bir alan ekleyin (metin, metin alanı, açılır liste, radyo, çoklu seçim, tarih veya evet/hayır).');
        }
        return errors;
    };

    AdminTemplateBuilder.prototype.showValidation = function (errors) {
        if (!errors || !errors.length) {
            this.hideValidation();
            return;
        }
        var html = '<strong>Şablon kaydedilemedi:</strong><ul style="margin:6px 0 0 18px;">';
        errors.forEach(function (e) {
            html += '<li>' + escapeXml(e) + '</li>';
        });
        html += '</ul>';
        this.$validation.html(html).show();
    };

    AdminTemplateBuilder.prototype.hideValidation = function () {
        this.$validation.hide().empty();
    };

    AdminTemplateBuilder.SAMPLES = SAMPLES;

    window.AdminTemplateBuilder = AdminTemplateBuilder;
})(window, window.jQuery);
