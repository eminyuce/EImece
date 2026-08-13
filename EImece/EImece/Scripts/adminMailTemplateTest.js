(function (window, $) {
    "use strict";

    var KIND_LABELS = {
        String: "Metin",
        Email: "E-posta",
        Url: "URL",
        ImageUrl: "Görsel",
        Phone: "Telefon",
        Date: "Tarih",
        Number: "Sayı",
        Boolean: "Evet/Hayır"
    };

    var state = {
        templateId: 0,
        templateName: "",
        useEditor: false,
        originalSubject: "",
        config: {
            inspectUrl: "",
            previewUrl: "",
            sendUrl: "",
            defaultRecipient: ""
        }
    };

    function getToken() {
        return $("#__AjaxAntiForgeryForm input[name='__RequestVerificationToken']").val()
            || $("input[name='__RequestVerificationToken']").first().val()
            || "";
    }

    function editorBody() {
        if (window.tinymce && typeof window.tinymce.triggerSave === "function") {
            window.tinymce.triggerSave();
        }
        return $("#Body").val() || "";
    }

    function editorSubject() {
        return $("#Subject").val() || "";
    }

    function collectModelData() {
        var data = {};
        $("#mailTemplateTestFields .js-mail-template-model-value").each(function () {
            var path = $(this).attr("data-path");
            if (path) {
                data[path] = $(this).val();
            }
        });
        return data;
    }

    function buildPayload(extra) {
        var payload = {
            id: state.templateId,
            recipientEmail: $("#mailTemplateTestRecipient").val(),
            subjectOverride: $("#mailTemplateTestSubject").val(),
            modelData: collectModelData()
        };
        if (state.useEditor) {
            payload.body = editorBody();
            payload.subject = editorSubject();
        }
        if (extra) {
            $.extend(payload, extra);
        }
        return payload;
    }

    function postJson(url, payload) {
        return $.ajax({
            type: "POST",
            url: url,
            data: JSON.stringify(payload),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            headers: {
                RequestVerificationToken: getToken()
            }
        });
    }

    function showAlert(type, message) {
        var $alert = $("#mailTemplateTestAlert");
        $alert.removeClass("alert-success alert-danger alert-info")
            .addClass("alert-" + type)
            .text(message || "")
            .toggle(!!message);
    }

    function setBusy(isBusy) {
        $("#mailTemplateTestSendBtn, #mailTemplateTestPreviewBtn, #mailTemplateTestReload")
            .prop("disabled", !!isBusy);
        $("#mailTemplateTestSendBtn").toggleClass("disabled", !!isBusy);
    }

    function inputTypeForKind(kind) {
        if (kind === "Email") {
            return "email";
        }
        if (kind === "Url" || kind === "ImageUrl") {
            return "url";
        }
        return "text";
    }

    function renderFields(properties) {
        var $fields = $("#mailTemplateTestFields").empty();
        var list = properties || [];
        $("#mailTemplateTestEmptyModel").toggle(list.length === 0);
        list.forEach(function (item) {
            var kind = item.ValueKind || item.valueKind || "String";
            var path = item.Path || item.path || "";
            var value = item.SampleValue || item.sampleValue || "";
            var label = KIND_LABELS[kind] || kind;
            var $group = $("<div/>", { "class": "form-group mail-template-test-field" });
            var $label = $("<label/>").text(path + " ");
            $label.append($("<span/>", { "class": "label label-default" }).text(label));
            var isLong = (value || "").length > 80;
            var $input = $(isLong ? "<textarea/>" : "<input/>", {
                "class": "form-control js-mail-template-model-value",
                "data-path": path,
                rows: isLong ? 2 : undefined,
                type: isLong ? undefined : inputTypeForKind(kind)
            }).val(value);
            $group.append($label).append($input);
            $fields.append($group);
        });
    }

    function loadInspect() {
        showAlert("info", "Örnek veriler yükleniyor...");
        setBusy(true);
        return postJson(state.config.inspectUrl, buildPayload())
            .done(function (res) {
                if (!res || !res.success) {
                    showAlert("danger", (res && res.message) || "Şablon incelenemedi.");
                    return;
                }
                var data = res.data || {};
                state.originalSubject = data.Subject || data.subject || "";
                var name = data.Name || data.name || state.templateName || "";
                $("#mailTemplateTestTitle").text("Test E-posta" + (name ? " — " + name : ""));
                if (!$("#mailTemplateTestSubject").val()) {
                    $("#mailTemplateTestSubject").attr("placeholder", state.originalSubject || "Boş bırakırsanız şablon konusu kullanılır");
                }
                if (!$("#mailTemplateTestRecipient").val() && state.config.defaultRecipient) {
                    $("#mailTemplateTestRecipient").val(state.config.defaultRecipient);
                }
                renderFields(data.Properties || data.properties || []);
                showAlert("", "");
            })
            .fail(function (xhr) {
                var message = "Şablon incelenemedi.";
                if (xhr && xhr.status === 403) {
                    message = "Güvenlik doğrulaması başarısız. Sayfayı yenileyip tekrar deneyin.";
                }
                showAlert("danger", message);
            })
            .always(function () {
                setBusy(false);
            });
    }

    function openModal($button) {
        state.templateId = parseInt($button.attr("data-template-id"), 10) || 0;
        state.templateName = $button.attr("data-template-name") || "";
        state.useEditor = $button.attr("data-use-editor") === "true";
        $("#mailTemplateTestSubject").val("");
        $("#mailTemplateTestPreview").hide();
        $("#mailTemplateTestModal").modal("show");
        loadInspect();
    }

    function preview() {
        showAlert("info", "Önizleme hazırlanıyor...");
        setBusy(true);
        postJson(state.config.previewUrl, buildPayload())
            .done(function (res) {
                if (!res || !res.success) {
                    showAlert("danger", (res && res.message) || "Önizleme oluşturulamadı.");
                    return;
                }
                var $wrap = $("#mailTemplateTestPreview").show();
                $wrap.find(".mail-template-test-preview-subject")
                    .text("Konu: " + (res.subject || "(boş)"));
                var iframe = $wrap.find("iframe")[0];
                if (iframe) {
                    var html = res.body || "";
                    if ("srcdoc" in iframe) {
                        iframe.srcdoc = html;
                    } else if (iframe.contentWindow && iframe.contentWindow.document) {
                        var doc = iframe.contentWindow.document;
                        doc.open();
                        doc.write(html);
                        doc.close();
                    }
                }
                showAlert("", "");
            })
            .fail(function () {
                showAlert("danger", "Önizleme oluşturulamadı.");
            })
            .always(function () {
                setBusy(false);
            });
    }

    function send() {
        var recipient = ($("#mailTemplateTestRecipient").val() || "").trim();
        if (!recipient) {
            showAlert("danger", "Alıcı e-posta adresi zorunludur.");
            $("#mailTemplateTestRecipient").focus();
            return;
        }
        showAlert("info", "E-posta gönderiliyor...");
        setBusy(true);
        postJson(state.config.sendUrl, buildPayload())
            .done(function (res) {
                if (!res || !res.success) {
                    showAlert("danger", (res && res.message) || "E-posta gönderilemedi.");
                    return;
                }
                showAlert("success", res.message || "Test e-postası gönderildi.");
            })
            .fail(function (xhr) {
                var message = "E-posta gönderilemedi.";
                if (xhr && xhr.status === 403) {
                    message = "Güvenlik doğrulaması başarısız. Sayfayı yenileyip tekrar deneyin.";
                }
                showAlert("danger", message);
            })
            .always(function () {
                setBusy(false);
            });
    }

    function bind() {
        $(document).on("click", ".js-mail-template-test", function (e) {
            e.preventDefault();
            openModal($(this));
        });
        $("#mailTemplateTestReload").on("click", function () {
            loadInspect();
        });
        $("#mailTemplateTestPreviewBtn").on("click", function () {
            preview();
        });
        $("#mailTemplateTestSendBtn").on("click", function () {
            send();
        });
    }

    window.EimeceMailTemplateTest = {
        init: function (config) {
            state.config = $.extend(state.config, config || {});
            bind();
        }
    };
})(window, window.jQuery);
