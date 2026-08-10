function isEmpty(str) {
    return (!str || 0 === str.length);
}

function deleteBaseContentMainImage(contentId, ImageId, contentClass, confirmationText) {
    var $modal = $("#adminDeleteConfirmModal");
    showDeleteConfirmation({
        title: $modal.attr("data-default-title"),
        message: confirmationText || $modal.attr("data-default-message"),
        deleteButtonText: $modal.attr("data-default-delete-text"),
        onConfirm: function (done) {
            var postData = JSON.stringify({
                "contentId": contentId,
                "imageId": ImageId,
                "contentClass": contentClass
            });
            ajaxMethodCall(postData, "/admin/Ajax/DeleteBaseContentMainImage", function (data) {
                $('[data-main-image-delete-id=' + ImageId + ']').html(data);
                done();
            }, function () {
                done(false);
            });
        }
    });
}

/**
 * Reusable admin delete confirmation (Bootstrap 3 modal).
 * options: { title, message, entityName, itemCount, deleteButtonText, cancelButtonText, onConfirm }
 * onConfirm(done): call done() after success, done(false) to keep the dialog open on failure.
 * If onConfirm takes no arguments, the dialog closes immediately after invoking it.
 */
function showDeleteConfirmation(options) {
    options = options || {};
    var $modal = $("#adminDeleteConfirmModal");
    if (!$modal.length) {
        if (window.confirm(options.message || options.title || "Are you sure?")) {
            if (typeof options.onConfirm === "function") {
                options.onConfirm(function () { });
            }
        }
        return;
    }

    var $dialog = $modal.find(".modal-confirm");
    var $title = $("#adminDeleteConfirmTitle");
    var $message = $("#adminDeleteConfirmMessage");
    var $entity = $("#adminDeleteConfirmEntity");
    var $count = $("#adminDeleteConfirmCount");
    var $ok = $("#adminDeleteConfirmOk");
    var $cancel = $("#adminDeleteConfirmCancel");
    var $btnLabel = $ok.find(".admin-delete-btn-label");

    var title = options.title || $modal.attr("data-default-title") || "";
    var message = options.message || $modal.attr("data-default-message") || "";
    var deleteText = options.deleteButtonText || $modal.attr("data-default-delete-text") || "";
    var cancelText = options.cancelButtonText || $modal.attr("data-default-cancel-text") || "";

    $title.text(title);
    $message.text(message);
    $btnLabel.text(deleteText);
    $cancel.text(cancelText);

    if (options.entityName) {
        $entity.text(options.entityName).prop("hidden", false).show();
    } else {
        $entity.empty().prop("hidden", true).hide();
    }

    if (options.itemCount && options.itemCount > 0) {
        $count.text("× " + options.itemCount).prop("hidden", false).show();
    } else {
        $count.empty().prop("hidden", true).hide();
    }

    var state = {
        loading: false,
        onConfirm: options.onConfirm
    };

    function setLoading(isLoading) {
        state.loading = !!isLoading;
        $dialog.toggleClass("is-loading", state.loading);
        $ok.prop("disabled", state.loading);
        $cancel.prop("disabled", state.loading);
        $modal.find(".close").prop("disabled", state.loading);
        if (state.loading) {
            $modal.attr("aria-busy", "true");
        } else {
            $modal.removeAttr("aria-busy");
        }
    }

    function closeModal() {
        setLoading(false);
        $modal.modal("hide");
    }

    function finish(success) {
        if (success === false) {
            setLoading(false);
            return;
        }
        closeModal();
    }

    $ok.off("click.adminDelete");
    $modal.off("shown.bs.modal.adminDelete");
    $modal.off("hidden.bs.modal.adminDelete");
    $modal.off("hide.bs.modal.adminDelete");
    $modal.off("keydown.adminDelete");

    $ok.on("click.adminDelete", function (e) {
        e.preventDefault();
        if (state.loading) {
            return;
        }
        if (typeof state.onConfirm !== "function") {
            closeModal();
            return;
        }
        setLoading(true);
        if (state.onConfirm.length >= 1) {
            state.onConfirm(finish);
        } else {
            state.onConfirm();
            closeModal();
        }
    });

    $modal.on("shown.bs.modal.adminDelete", function () {
        $ok.focus();
    });

    $modal.on("hide.bs.modal.adminDelete", function (e) {
        if (state.loading) {
            e.preventDefault();
        }
    });

    $modal.on("hidden.bs.modal.adminDelete", function () {
        setLoading(false);
        $ok.off("click.adminDelete");
        $modal.off("keydown.adminDelete");
        $modal.off("hide.bs.modal.adminDelete");
    });

    $modal.on("keydown.adminDelete", function (e) {
        if (state.loading) {
            return;
        }
        if (e.keyCode === 13 && !$(e.target).is("textarea,button")) {
            e.preventDefault();
            $ok.trigger("click.adminDelete");
        }
    });

    setLoading(false);
    $modal.modal("show");
}

function submitAdminDeleteForm(url, id) {
    var token = $("#__AjaxAntiForgeryForm input[name='__RequestVerificationToken']").val()
        || $("input[name='__RequestVerificationToken']").first().val();
    var $form = $("<form>").attr({
        method: "POST",
        action: url
    });
    if (token) {
        $form.append($("<input>").attr({
            type: "hidden",
            name: "__RequestVerificationToken",
            value: token
        }));
    }
    $form.append($("<input>").attr({
        type: "hidden",
        name: "Id",
        value: id
    }));
    $form.appendTo("body").trigger("submit");
}


$(document).ready(function () {
    bindSaveAdminOrderNote();
    function bindSaveAdminOrderNote() {
        $('[data-save-admin-order-note]').each(function () {
            $(this).off("click");
            $(this).on("click", handleSaveAdminOrderNote);
        });
    }
    function handleSaveAdminOrderNote(e) {
        var caller = e.target;
        var orderId = $(caller).attr('data-save-admin-order-note');
        var adminOrderNote = $('[data-textarea-admin-order-note=' + orderId + ']').val();
        var shipmentCompanyName = $('[data-order-shipment-company-name=' + orderId + ']').val();
        var shipmentTrackingNumber = $('[data-order-shipment-tracking-number=' + orderId + ']').val();

        var postData = JSON.stringify({
            "orderId": orderId,
            "adminOrderNote": adminOrderNote,
            "shipmentCompanyName": shipmentCompanyName,
            "shipmentTrackingNumber": shipmentTrackingNumber
        });
        ajaxMethodCall(postData, "/admin/Ajax/SaveAdminOrderNote", function (data) {
            $('[data-changed-order-result=' + orderId + ']').text(data);
        });
    }
 

    bindChangeOrderStatus();
    function bindChangeOrderStatus() {
        $('[data-change-order-status]').each(function () {
            $(this).off("change");
            $(this).on("change", handleChangedOrderStatus);
        });
    }
    function handleChangedOrderStatus(e) {
        var caller = e.target;
        var orderStatus = $(caller).val();
        var orderId = $(caller).attr('data-change-order-status');
        $('[data-changed-order-result="' + orderId + '"]').text("");
        var postData = JSON.stringify({ "orderId": orderId, "orderStatus": orderStatus });
        ajaxMethodCall(postData, "/admin/Ajax/ChangedOrderStatus", function (data) {
            console.log(data);
            $('[data-changed-order-result="' + orderId + '"]').text(data);
        });

    }
    bindProductDetailToolTip();
    if (window.EImece && EImece.RichTextEditor) {
        EImece.RichTextEditor.init();
    }
    bindAdminEditTabs();
    searchAutoComplete();
    function bindAdminEditTabs() {
        var $form = $(".admin-edit-form");
        if (!$form.length) {
            return;
        }

        var storageKey = "eimece.admin.editTab:" + window.location.pathname.toLowerCase();

        function activateTab(href) {
            var $link = $form.find('.admin-edit-tabs a[href="' + href + '"]');
            if ($link.length) {
                $link.tab("show");
            }
        }

        function refreshEditorsSoon() {
            window.setTimeout(function () {
                if (window.EImece && EImece.RichTextEditor && typeof EImece.RichTextEditor.refreshVisible === "function") {
                    EImece.RichTextEditor.refreshVisible();
                }
            }, 50);
        }

        $form.off("click.adminEditTabs", "[data-admin-edit-goto-content]")
            .on("click.adminEditTabs", "[data-admin-edit-goto-content]", function (e) {
                e.preventDefault();
                activateTab("#admin-edit-tab-content");
            });

        $form.off("click.adminEditTabs", "[data-admin-edit-goto-fields]")
            .on("click.adminEditTabs", "[data-admin-edit-goto-fields]", function (e) {
                e.preventDefault();
                activateTab("#admin-edit-tab-fields");
            });

        $form.off("shown.bs.tab.adminEditTabs", '.admin-edit-tabs a[data-toggle="tab"]')
            .on("shown.bs.tab.adminEditTabs", '.admin-edit-tabs a[data-toggle="tab"]', function (e) {
                var href = $(e.target).attr("href");
                try {
                    if (window.sessionStorage && href) {
                        window.sessionStorage.setItem(storageKey, href);
                    }
                } catch (err) { }

                if (href === "#admin-edit-tab-content") {
                    refreshEditorsSoon();
                }
            });

        try {
            var saved = window.sessionStorage ? window.sessionStorage.getItem(storageKey) : null;
            if (saved === "#admin-edit-tab-content" || saved === "#admin-edit-tab-fields") {
                activateTab(saved);
                if (saved === "#admin-edit-tab-content") {
                    refreshEditorsSoon();
                }
            }
        } catch (err) { }
    }
    $("input[name=checkboxGrid]").each(function () {
        $(this).off("click");
        $(this).on("click", function (e) {
            var m = $(this).is(':checked');
            if (m) {
                $(this).parents("tr:first").addClass('gridChecked');
            } else {
                $(this).parents("tr:first").removeClass('gridChecked');
            }
        });
    });
    function OrderingItem() {
        var item = this;
        item.Id = "";
        item.Position = "";
        item.IsActive = false;
        return item;
    }
    function GetSelectedOrderingValues() {
        var itemArray = new Array();
        var i = 0;
        $("input[name=gridOrdering]").each(function () {
            var id = $(this).attr("gridkey-id");
            //var m = $("input[name=checkboxGrid]").find('[gridkey-id='+id+']').is(':checked');
            //if (m) {
            var item = new OrderingItem();
            item.Id = id;
            item.Position = $(this).val();
            itemArray[i++] = item;
            //}
        });

        var jsonRequest = JSON.stringify({ "values": itemArray });
        return jsonRequest;
    }
    var YOUR_MESSAGE_STRING_CONST = $("#AdminMultiSelectDeleteConfirmMessage").text();
    $("#DeleteAll").click(function () {
        var selectedCount = GetSelectedCheckBoxValuesArray().length;
        if (selectedCount === 0) {
            showAdminAjaxError($("#CheckboxesDataTableDoesNotSelected").val() || "Lütfen en az bir kayıt seçin.");
            return;
        }
        var $deleteModal = $("#adminDeleteConfirmModal");
        showDeleteConfirmation({
            title: $deleteModal.attr("data-default-title"),
            message: YOUR_MESSAGE_STRING_CONST || $deleteModal.attr("data-bulk-message"),
            itemCount: selectedCount,
            deleteButtonText: $deleteModal.attr("data-bulk-delete-text") || $(this).text().trim(),
            onConfirm: function (done) {
                var postData = GetSelectedCheckBoxValues();
                var parsedPostData = jQuery.parseJSON(postData);
                if (parsedPostData.values.length > 0) {
                    var tableName = $("[data-gridname]").attr("data-gridname");
                    ajaxMethodCall(postData, "/admin/Ajax/Delete" + tableName + "Item", function (data) {
                        deleteItemsSuccess(data);
                        done();
                    }, function () {
                        done(false);
                    });
                } else {
                    done(false);
                }
            }
        });
    });

    $(document).on("click", "[data-admin-delete]", function (e) {
        e.preventDefault();
        var $el = $(this);
        showDeleteConfirmation({
            title: $el.attr("data-delete-title"),
            message: $el.attr("data-delete-message"),
            entityName: $el.attr("data-entity-name"),
            deleteButtonText: $el.attr("data-delete-button"),
            onConfirm: function (done) {
                submitAdminDeleteForm($el.attr("data-delete-url"), $el.attr("data-delete-id"));
                // Page navigates on successful POST; keep loading state to prevent double-submit.
            }
        });
    });

    /** @deprecated Use showDeleteConfirmation. Kept for backward compatibility. */
    function confirmDialog(message, onConfirm) {
        showDeleteConfirmation({
            message: message,
            onConfirm: onConfirm
        });
    }
    $("#OrderingAll").click(function () {
        //  console.log("OrderingAll is clicked.");
        var postData = GetSelectedOrderingValues();
        //  console.log(postData);
        var tableName = $("[data-gridname]").attr("data-gridname");
        ajaxMethodCall(postData, "/admin/Ajax/Change" + tableName + "OrderingOrState", changeOrderingSuccess);
    });

    function GetSelectedStateValues(checkboxName, state) {
        var itemArray = new Array();
        var i = 0;
        var checkboxId = 'span[name=' + checkboxName + ']';
 
        $(checkboxId).each(function () {
            var id = $(this).attr("gridkey-id");
            console.log(id);
            var m = $('input[name="checkboxGrid"]').filter('[gridkey-id="' + id + '"]').is(':checked');
            if (m) {
                var item = new OrderingItem();
                item.Id = id;
                item.Ordering = 0;
                item.IsActive = state;
                itemArray[i++] = item;
            }
        });

        return itemArray;
    }

    $("#DeselectAll").click(function () {
        var i = 0;
        $("input[name=checkboxGrid]").each(function () {
            $(this).parents("tr:first").removeClass('gridChecked');
            var m = $(this).prop('checked', false);
        });
    });
    $("#SelectAll").click(function () {
        //  console.log("SelectAll is clicked.");
        var i = 0;
        $("input[name=checkboxGrid]").each(function () {
            var selectedId = $(this).attr('gridkey-id');
            $(this).parents("tr:first").addClass('gridChecked');
            var m = $(this).prop('checked', true);
        });
    });

    $("#SetStateOffAll").click(function () {
        console.log("SetStateOffAll is clicked.");
        changeState(false);
    });
    $("#SetStateOnAll").click(function () {
        //  console.log("SetStateOnAll is clicked.");
        changeState(true);
    });
    $("#ProductStateChanged").click(function (e) {
        e.preventDefault();

        var ProductStateSelection = parseInt($("#ProductStateSelection").val(), 10);
        var ProductStateText = $("#ProductStateSelection option:selected").text();

        var selectedProductId = GetSelectedCheckBoxValuesArray();
        var postData = JSON.stringify({
            values: selectedProductId,
            ProductStateSelection: ProductStateSelection
        });

        ajaxMethodCall(postData, "/admin/Ajax/ProductStateChanged", function (data) {
            $("div[name=ProductState]").each(function () {
                var productId = $(this).attr('Product-State-Id');
                if (selectedProductId.includes(productId)) {
                    $(this).text(ProductStateText);
                }
            });
        });
    });

    function changeState(state) {
        var ppp = $("#ItemStateSelection").val();
        var selectedValues = GetSelectedStateValues("span" + ppp, state);
        if (selectedValues.length > 0) {
            var postData = JSON.stringify({ "values": selectedValues, "checkbox": ppp });
            //  console.log(postData);
            var tableName = $("[data-gridname]").attr("data-gridname");
            ajaxMethodCall(postData, "/admin/Ajax/Change" + tableName + "OrderingOrState", changeStateSuccess);
            displayMessage("hide", "");
        } else {
            displayMessage("error", $("#CheckboxesDataTableDoesNotSelected").val());
        }
    }
    $("#GridListItemSize").change(function (e) {
        var originalURL = window.location.href;
        var q = getQueryStringParameter(originalURL, "GridPageSize");
        if (!isEmpty(q)) {
            window.location.href = updateUrlParameter(originalURL, 'GridPageSize', $('#GridListItemSize option:selected').val());
        } else {
            if (hasQueryStringParameter(originalURL)) {
                window.location.href = window.location.href + "&GridPageSize=" + $('#GridListItemSize option:selected').val();
            } else {
                window.location.href = window.location.href + "?GridPageSize=" + $('#GridListItemSize option:selected').val();
            }
        }
    });

    // Optional per-upload image size override (number inputs; collapsed by default).
    var $sizeRoot = $("[data-admin-image-size]").first();
    if ($sizeRoot.length && $("#imageWidthTxt").length && $("#imageHeightTxt").length) {
        var defaultValueWidth = parseInt($("#ImageWidth").val(), 10) || parseInt($sizeRoot.attr("data-default-w"), 10) || 0;
        var defaultValueHeight = parseInt($("#ImageHeight").val(), 10) || parseInt($sizeRoot.attr("data-default-h"), 10) || 0;

        function syncImageSize(width, height) {
            width = Math.max(0, Math.min(2000, parseInt(width, 10) || 0));
            height = Math.max(0, Math.min(2000, parseInt(height, 10) || 0));
            $("#ImageWidth").val(width);
            $("#ImageHeight").val(height);
            $("#imageWidthTxt").val(width);
            $("#imageHeightTxt").val(height);
            $sizeRoot.find("[data-image-size-summary]").text(width + " × " + height + " px");
        }

        syncImageSize(defaultValueWidth, defaultValueHeight);

        $("#imageWidthTxt").on("change input", function () {
            syncImageSize(this.value, $("#imageHeightTxt").val());
        });
        $("#imageHeightTxt").on("change input", function () {
            syncImageSize($("#imageWidthTxt").val(), this.value);
        });

        $sizeRoot.on("click", "[data-image-preset]", function (e) {
            e.preventDefault();
            var $btn = $(this);
            var w, h;
            if ($btn.attr("data-image-preset") === "default") {
                w = parseInt($sizeRoot.attr("data-default-w"), 10) || defaultValueWidth;
                h = parseInt($sizeRoot.attr("data-default-h"), 10) || defaultValueHeight;
            } else {
                w = parseInt($btn.attr("data-w"), 10);
                h = parseInt($btn.attr("data-h"), 10);
            }
            syncImageSize(w, h);
            $sizeRoot.find("[data-image-preset]").removeClass("active");
            $btn.addClass("active");
        });
    }
});


function fiyatlariGuncelleGeneric(e) {
    var caller = e.target; // The button that was clicked
    var itemType = $(caller).attr('data-product-item-type');
    var itemId = $(caller).attr('data-product-item-id');
    var uniqueKey = itemType + '-' + itemId;

    const yuzde = $('[data-product-item-percentage="' + uniqueKey + '"]').val();
    const sonucDiv = $('[data-product-item-result="' + uniqueKey + '"]');

    sonucDiv.html("Fiyatlar güncelleniyor...");

    // Build payload based on item type
    var payload = {
        percentageOfIncreaseOrDecrease: parseFloat(yuzde)
    };

    // Add the appropriate ID property based on type
    if (itemType === 'ProductCategory') {
        payload.categoryId = parseInt(itemId);
    } else if (itemType === 'Brand') {
        payload.brandId = parseInt(itemId);
    } else if (itemType === 'Tag') {
        payload.tagId = parseInt(itemId);
    }

    var payloadData = JSON.stringify(payload);
    console.log("Fiyat Güncelle: " + payloadData);

    $.ajax({
        url: '/Admin/Ajax/UpdatePrices',
        type: 'POST',
        data: payloadData,
        contentType: 'application/json',
        success: function (response) {
            if (response.success) {
                sonucDiv.html(`Başarılı! ${response.affectedRows} satır güncellendi.`);
            } else {
                sonucDiv.html(`Hata: ${response.message || 'Bilinmeyen bir hata oluştu.'}`);
            }
        },
        error: function (xhr, status, error) {
            sonucDiv.html(`Hata: ${xhr.responseText || 'Fiyatlar güncellenemedi.'}`);
        }
    });
}


 
function GetSelectedCheckBoxValues() {
    var stringArray = GetSelectedCheckBoxValuesArray();
    var jsonRequest = JSON.stringify({ "values": stringArray });
    return jsonRequest;
}
function GetSelectedCheckBoxValuesArray() {
    var stringArray = new Array();
    var i = 0;
    $("input[name=checkboxGrid]").each(function () {
        var m = $(this).is(':checked');
        if (m) {
            stringArray[i++] = $(this).attr("gridkey-id");
        }
    });
    return stringArray;
}
function displayMessage(messageType, message) {
    var messagePanel = $("#ErrorMessagePanel");
    var errorMessage = $("#ErrorMessage");
    if (isEmpty(message)) {
        return;
    }
    messagePanel.fadeIn(500);
    if (messageType === "info") {
        messagePanel.attr("class", "alert alert-info");
        errorMessage.text(message);
        fadeOutAfterInterval(messagePanel);
    } else if (messageType === "error") {
        messagePanel.attr("class", "alert alert-danger");
        errorMessage.text(message);
        fadeOutAfterInterval(messagePanel);
    } else if (messageType === "hide") {
        fadeOutAfterInterval(messagePanel);
    }
}
function fadeOutAfterInterval(messagePanel) {
    var timeoutFadeOut = 2000;
    var intervalTime = 5000;
    window.setInterval(function () { // 3
        messagePanel.fadeOut(timeoutFadeOut);
    }, intervalTime);
}
function hasQueryStringParameter(originalURL) {
    if (originalURL.split('?').length > 1) {
        var qs = originalURL.split('?')[1];
        var qsArray = qs.split('&');
        return qsArray.length > 0;
    } else {
        return false;
    }
}
function getQueryStringParameter(originalURL, param) {
    if (originalURL.split('?').length > 1) {
        var qs = originalURL.split('?')[1];
        //3- get list of query strings
        var qsArray = qs.split('&');
        var flag = false;
        //4- try to find query string key
        for (var i = 0; i < qsArray.length; i++) {
            if (qsArray[i].split('=').length > 0) {
                if (param === qsArray[i].split('=')[0]) {
                    //exists key
                    return qsArray[i].split('=')[1];
                }
            }
        }
    }
    return "";
}
function updateUrlParameter(originalURL, param, value) {
    //  console.log(value);
    var windowUrl = originalURL.split('?')[0];
    var qs = originalURL.split('?')[1];
    //3- get list of query strings
    var qsArray = qs.split('&');
    var flag = false;
    //4- try to find query string key
    for (var i = 0; i < qsArray.length; i++) {
        if (qsArray[i].split('=').length > 0) {
            if (param === qsArray[i].split('=')[0]) {
                //exists key
                qsArray[i] = param + '=' + value;
            }
        }
    }

    var finalQs = qsArray.join('&');
    return windowUrl + '?' + finalQs;
    //6- prepare final url
    // window.location = windowUrl + '?' + finalQs;
}

function deleteItemsSuccess(data) {
    data.forEach(function (entry) {
        // Quote attribute value — media keys are composite (id-contentId-mod-imageType).
        var pp = $('[gridkey-id="' + entry + '"]');
        pp.closest('tr').remove();
    });

    refresh(500);
}
function changeStateSuccess(data) {
    //var parsedPostData = jQuery.parseJSON(data);
    //  console.log(data);
    data.values.forEach(function (entry) {
        if (entry.IsActive) {
            $('span[name=span' + data.checkbox + ']').filter('[gridkey-id="' + entry.Id + '"]').attr('class', 'gridActiveIcon glyphicon  glyphicon-ok-circle');
        } else {
            $('span[name=span' + data.checkbox + ']').filter('[gridkey-id="' + entry.Id + '"]').attr('class', ' gridNotActiveIcon glyphicon  glyphicon-remove-circle');
        }
    });
}
function refresh(timeElapsed) {
    setTimeout(function () {
        location.reload()
    }, timeElapsed);
}
function changeOrderingSuccess(data) {
    refresh(500);
}
function ajaxMethodCall(postData, ajaxUrl, successFunction, errorFunction) {
    $.ajax({
        type: "POST",
        url: ajaxUrl,
        data: postData,
        dataType: 'json',
        contentType: 'application/json; charset=utf-8',
        success: successFunction,
        error: function (jqXHR, exception) {
            console.error("parameters :" + postData);
            console.error("ajaxUrl :" + ajaxUrl);
            console.error("responseText :" + jqXHR.responseText);
            var message = "İşlem başarısız oldu.";
            if (jqXHR.status === 0) {
                message = "Sunucuya bağlanılamadı. Ağ bağlantınızı kontrol edin.";
                console.error('Not connect.\n Verify Network.');
            } else if (jqXHR.status === 404) {
                message = "Silme servisi bulunamadı (404): " + ajaxUrl;
                console.error('Requested page not found. [404]');
            } else if (jqXHR.status === 500) {
                message = "Sunucu hatası (500). Lütfen tekrar deneyin veya konsolu kontrol edin.";
                console.error('Internal Server Error [500].');
            } else if (exception === 'parsererror') {
                message = "Sunucu yanıtı okunamadı.";
                console.error('Requested JSON parse failed.');
            } else if (exception === 'timeout') {
                message = "İstek zaman aşımına uğradı.";
                console.error('Time out error.');
            } else if (exception === 'abort') {
                console.error('Ajax request aborted.');
                if (typeof errorFunction === "function") {
                    errorFunction(jqXHR, exception);
                }
                return;
            } else {
                console.error('Uncaught Error.\n' + jqXHR.responseText);
            }
            showAdminAjaxError(message);
            if (typeof errorFunction === "function") {
                errorFunction(jqXHR, exception);
            }
        }
    });
}

function showAdminAjaxError(message) {
    var panel = $("#ErrorMessagePanel");
    var span = $("#ErrorMessage");
    if (panel.length && span.length) {
        span.text(message);
        panel.show();
        try {
            $("html, body").animate({ scrollTop: panel.offset().top - 80 }, 200);
        } catch (e) { }
    } else {
        window.alert(message);
    }
}
function sortInputFirst(input, data) {
    var first = [];
    var others = [];
    for (var i = 0; i < data.length; i++) {
        if (data[i].text.toLowerCase().indexOf(input.toLowerCase()) == 0) {
            first.push(data[i]);
        } else {
            others.push(data[i]);
        }
    }
    first.sort();
    others.sort();
    return (first.concat(others));
}

function searchAutoComplete() {
    $("#searchTxtInput").autocomplete({
        source: function (request, response) {
            var items = new Array();
            // Use GET + actionName/controllerName (not action/controller) to avoid MVC route token collisions.
            if (request.term.length > 2) {
                $.ajax({
                    type: "GET",
                    url: "/admin/Ajax/SearchAutoComplete",
                    dataType: "json",
                    data: {
                        term: request.term,
                        actionName: $("#action").val(),
                        controllerName: $("#controller").val()
                    },
                    success: function (data) {
                        for (var i = 0; i < data.length; i++) {
                            items[i] = { text: data[i], value: data[i] };
                        }
                        response(sortInputFirst(request.term, items));
                    },
                    error: function (jqXHR, exception) {
                        console.error("SearchAutoComplete failed", jqXHR.status, jqXHR.responseText, exception);
                        response([]);
                    }
                });
            }
        },
        select: function (event, ui) {
            $("#SearchButton").click();
        }
    });
}
function bindProductDetailToolTip() {
    $('[data-product-detail]').each(function () {
        $(this).off("click");
        $(this).on("click", handleProductDetailToolTip);
    });
}
function clearProductDetailToolTip() {
    $('[data-product-detail]').each(function () {
        var productID = $(this).attr('data-product-detail');
        $('[data-product-detail-result=' + productID + ']').html("");
    });
}
function handleProductDetailToolTip(e) {
    clearProductDetailToolTip();
    var caller = e.target;
    var productID = $(caller).attr('data-product-detail');
    var postData = JSON.stringify({ "productId": productID });
    ajaxMethodCall(postData, "/Admin/Ajax/GetProductDetailToolTip", function (data) {
        $('[data-product-detail-result=' + productID + ']').html(data);
    });
}

function setPreSelectedTreeNode(preSelectedNode) {
    var productCategoryId = preSelectedNode.val();
    if (productCategoryId !== "0") {
        var textSpan = $("#Content_" + productCategoryId).text();
        $("#Content_" + productCategoryId).text("");
        $("#Content_" + productCategoryId).addClass("hover2");
        $("#Content_" + productCategoryId).append("<span id='contentInside' class='contentSelected'>" + textSpan + "</span>");
    }
}

/* Admin sidebar shell: desktop collapse + groups + mobile drawer */
(function () {
    var STORAGE_COLLAPSED = "eimece.admin.sidebarCollapsed";
    var STORAGE_GROUPS = "eimece.admin.sidebarGroups";
    var MQ_DESKTOP = "(min-width: 992px)";

    function isDesktop() {
        return window.matchMedia && window.matchMedia(MQ_DESKTOP).matches;
    }

    function safeGet(key) {
        try {
            return window.localStorage.getItem(key);
        } catch (e) {
            return null;
        }
    }

    function safeSet(key, value) {
        try {
            window.localStorage.setItem(key, value);
        } catch (e) {
            /* ignore quota / private mode */
        }
    }

    function readGroupsState() {
        try {
            var raw = safeGet(STORAGE_GROUPS);
            if (!raw) {
                return {};
            }
            var parsed = JSON.parse(raw);
            return parsed && typeof parsed === "object" ? parsed : {};
        } catch (e) {
            return {};
        }
    }

    function writeGroupsState(state) {
        safeSet(STORAGE_GROUPS, JSON.stringify(state || {}));
    }

    function setMobileDrawerOpen(isOpen) {
        var body = document.body;
        if (!body || !body.classList.contains("admin-app")) {
            return;
        }
        if (isOpen) {
            body.classList.add("sidebar-open");
        } else {
            body.classList.remove("sidebar-open");
        }
        var overlay = document.getElementById("adminSidebarOverlay");
        if (overlay) {
            overlay.setAttribute("aria-hidden", isOpen ? "false" : "true");
        }
        syncToggleAria();
    }

    function setSidebarCollapsed(collapsed) {
        var body = document.body;
        var sidebar = document.getElementById("adminSidebar");
        if (!body || !body.classList.contains("admin-app") || !sidebar) {
            return;
        }
        if (collapsed) {
            body.classList.add("sidebar-collapsed");
            sidebar.setAttribute("data-collapsed", "true");
        } else {
            body.classList.remove("sidebar-collapsed");
            sidebar.setAttribute("data-collapsed", "false");
        }
        safeSet(STORAGE_COLLAPSED, collapsed ? "1" : "0");
        syncToggleAria();
    }

    function syncToggleAria() {
        var toggle = document.getElementById("adminSidebarToggle");
        if (!toggle) {
            return;
        }
        if (isDesktop()) {
            var collapsed = document.body.classList.contains("sidebar-collapsed");
            toggle.setAttribute("aria-expanded", collapsed ? "false" : "true");
            toggle.setAttribute("title", toggle.getAttribute("data-label-toggle") || "Toggle sidebar");
        } else {
            var open = document.body.classList.contains("sidebar-open");
            toggle.setAttribute("aria-expanded", open ? "true" : "false");
            var openLabel = toggle.getAttribute("data-label-open") || "Open menu";
            var closeLabel = toggle.getAttribute("data-label-close") || "Close menu";
            toggle.setAttribute("title", open ? closeLabel : openLabel);
        }
    }

    function setGroupOpen(groupEl, isOpen, persist) {
        if (!groupEl) {
            return;
        }
        var toggle = groupEl.querySelector(".admin-nav-group-toggle");
        if (isOpen) {
            groupEl.classList.add("is-open");
        } else {
            groupEl.classList.remove("is-open");
        }
        if (toggle) {
            toggle.setAttribute("aria-expanded", isOpen ? "true" : "false");
        }
        if (persist) {
            var key = groupEl.getAttribute("data-group");
            if (key) {
                var state = readGroupsState();
                state[key] = !!isOpen;
                writeGroupsState(state);
            }
        }
    }

    function markActiveGroups() {
        var groups = document.querySelectorAll("#adminSidebar .admin-nav-group");
        for (var i = 0; i < groups.length; i++) {
            var group = groups[i];
            var hasActive = !!group.querySelector(".admin-nav-item.active");
            if (hasActive) {
                group.classList.add("has-active");
                setGroupOpen(group, true, false);
            } else {
                group.classList.remove("has-active");
            }
        }
    }

    function restoreGroupState() {
        var stored = readGroupsState();
        var groups = document.querySelectorAll("#adminSidebar .admin-nav-group[data-group]");
        for (var i = 0; i < groups.length; i++) {
            var group = groups[i];
            var key = group.getAttribute("data-group");
            if (!key || !Object.prototype.hasOwnProperty.call(stored, key)) {
                continue;
            }
            // Active route always wins: keep open if it has an active child
            if (group.classList.contains("has-active")) {
                setGroupOpen(group, true, false);
                continue;
            }
            setGroupOpen(group, !!stored[key], false);
        }
    }

    function initNavGroups() {
        var toggles = document.querySelectorAll("#adminSidebar .admin-nav-group-toggle");
        for (var i = 0; i < toggles.length; i++) {
            toggles[i].addEventListener("click", function (e) {
                e.preventDefault();
                var group = this.closest ? this.closest(".admin-nav-group") : this.parentNode;
                if (!group) {
                    return;
                }
                // On desktop collapsed mode, expand sidebar so labels + children are usable
                if (isDesktop() && document.body.classList.contains("sidebar-collapsed")) {
                    setSidebarCollapsed(false);
                    setGroupOpen(group, true, true);
                    return;
                }
                var willOpen = !group.classList.contains("is-open");
                setGroupOpen(group, willOpen, true);
            });
        }
        markActiveGroups();
        restoreGroupState();
    }

    function initAdminSidebarShell() {
        var body = document.body;
        if (!body || !body.classList.contains("admin-app")) {
            return;
        }

        var toggle = document.getElementById("adminSidebarToggle");
        var overlay = document.getElementById("adminSidebarOverlay");
        var closeBtn = document.getElementById("adminSidebarClose");
        var sidebar = document.getElementById("adminSidebar");

        // Restore desktop collapsed preference
        var storedCollapsed = safeGet(STORAGE_COLLAPSED) === "1";
        if (isDesktop()) {
            setSidebarCollapsed(storedCollapsed);
        } else {
            // Keep preference stored but do not apply icon-only on mobile
            body.classList.remove("sidebar-collapsed");
            if (sidebar) {
                sidebar.setAttribute("data-collapsed", "false");
            }
            syncToggleAria();
        }

        initNavGroups();

        if (toggle) {
            toggle.addEventListener("click", function (e) {
                e.preventDefault();
                if (isDesktop()) {
                    var nextCollapsed = !body.classList.contains("sidebar-collapsed");
                    setSidebarCollapsed(nextCollapsed);
                } else {
                    setMobileDrawerOpen(!body.classList.contains("sidebar-open"));
                }
            });
        }

        if (overlay) {
            overlay.addEventListener("click", function () {
                setMobileDrawerOpen(false);
            });
        }

        if (closeBtn) {
            closeBtn.addEventListener("click", function (e) {
                e.preventDefault();
                setMobileDrawerOpen(false);
            });
        }

        var sidebarLinks = document.querySelectorAll("#adminSidebar a.admin-nav-item");
        for (var i = 0; i < sidebarLinks.length; i++) {
            sidebarLinks[i].addEventListener("click", function () {
                if (!isDesktop()) {
                    setMobileDrawerOpen(false);
                }
            });
        }

        window.addEventListener("resize", function () {
            if (isDesktop()) {
                setMobileDrawerOpen(false);
                setSidebarCollapsed(safeGet(STORAGE_COLLAPSED) === "1");
            } else {
                body.classList.remove("sidebar-collapsed");
                if (sidebar) {
                    sidebar.setAttribute("data-collapsed", "false");
                }
                syncToggleAria();
            }
        });
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", initAdminSidebarShell);
    } else {
        initAdminSidebarShell();
    }
})();
/**
 * Product/Story tag picker: chip selection, search filter, live counts.
 * Call after AJAX injects pSelectedTags into a container.
 */
function initAdminTagPicker(root) {
    var $root = root && root.jquery ? root : $(root);
    if (!$root || !$root.length) {
        return;
    }
    var $picker = $root.find('[data-admin-tag-picker]').addBack('[data-admin-tag-picker]').first();
    if (!$picker.length) {
        return;
    }

    function refreshCounts() {
        var selectedTotal = $picker.find('.admin-tag-chip__input:checked').length;
        $picker.find('[data-tag-selected-count]').text(selectedTotal);

        $picker.find('[data-tag-category]').each(function () {
            var $cat = $(this);
            var selected = $cat.find('.admin-tag-chip__input:checked').length;
            $cat.find('[data-tag-category-selected]').text(selected);
        });
    }

    function applySearch() {
        var q = ($picker.find('[data-tag-search]').val() || '').toString().trim().toLocaleLowerCase('tr-TR');
        var anyVisible = false;

        $picker.find('[data-tag-chip]').each(function () {
            var $chip = $(this);
            var name = ($chip.attr('data-tag-name') || $chip.text() || '').toString().toLocaleLowerCase('tr-TR');
            var match = !q || name.indexOf(q) !== -1;
            $chip.toggle(match);
            if (match) {
                anyVisible = true;
            }
        });

        $picker.find('[data-tag-category]').each(function () {
            var $cat = $(this);
            var hasVisible = $cat.find('[data-tag-chip]:visible').length > 0;
            $cat.toggle(hasVisible);
        });

        $picker.find('[data-tag-no-match]').prop('hidden', !q || anyVisible);
    }

    $picker.off('.adminTagPicker');
    $picker.on('change.adminTagPicker', '.admin-tag-chip__input', function () {
        $(this).closest('[data-tag-chip]').toggleClass('is-selected', this.checked);
        refreshCounts();
    });
    $picker.on('input.adminTagPicker', '[data-tag-search]', applySearch);

    refreshCounts();
}
