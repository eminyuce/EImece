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
 * Reusable admin delete confirmation (Bootstrap 5 modal).
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
        $modal.find(".btn-close").prop("disabled", state.loading);
        if (state.loading) {
            $modal.attr("aria-busy", "true");
        } else {
            $modal.removeAttr("aria-busy");
        }
    }

    function closeModal() {
        setLoading(false);
        if (window.bootstrap && bootstrap.Modal && $modal[0]) {
            bootstrap.Modal.getOrCreateInstance($modal[0]).hide();
        } else if ($modal.modal) {
            $modal.modal("hide");
        }
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
        if (e.key === "Enter" && !$(e.target).is("textarea,button")) {
            e.preventDefault();
            $ok.trigger("click.adminDelete");
        }
    });

    setLoading(false);
    if (window.bootstrap && bootstrap.Modal && $modal[0]) {
        bootstrap.Modal.getOrCreateInstance($modal[0]).show();
    } else if ($modal.modal) {
        $modal.modal("show");
    }
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
        $(document)
            .off("click.adminOrderNote", "[data-save-admin-order-note]")
            .on("click.adminOrderNote", "[data-save-admin-order-note]", handleSaveAdminOrderNote);
    }
    function handleSaveAdminOrderNote(e) {
        var caller = e.currentTarget || e.target;
        var orderId = $(caller).attr("data-save-admin-order-note");
        var adminOrderNote = $("[data-textarea-admin-order-note=" + orderId + "]").val();
        var shipmentCompanyName = $("[data-order-shipment-company-name=" + orderId + "]").val();
        var shipmentTrackingNumber = $("[data-order-shipment-tracking-number=" + orderId + "]").val();

        var postData = JSON.stringify({
            "orderId": orderId,
            "adminOrderNote": adminOrderNote,
            "shipmentCompanyName": shipmentCompanyName,
            "shipmentTrackingNumber": shipmentTrackingNumber
        });
        ajaxMethodCall(postData, "/admin/Ajax/SaveAdminOrderNote", function (data) {
            $("[data-changed-order-result=" + orderId + "]").text(data);
        });
    }

    bindChangeOrderStatus();
    function bindChangeOrderStatus() {
        $(document)
            .off("change.adminOrderStatus", "[data-change-order-status]")
            .on("change.adminOrderStatus", "[data-change-order-status]", handleChangedOrderStatus);
    }
    function handleChangedOrderStatus(e) {
        var caller = e.currentTarget || e.target;
        var orderStatus = $(caller).val();
        var orderId = $(caller).attr("data-change-order-status");
        $("[data-changed-order-result=\"" + orderId + "\"]").text("");
        var postData = JSON.stringify({ "orderId": orderId, "orderStatus": orderStatus });
        ajaxMethodCall(postData, "/admin/Ajax/ChangedOrderStatus", function (data) {
            var $badge = $("[data-eg-status-badge=\"" + orderId + "\"]");
            if ($badge.length) {
                var label = $(caller).find("option:selected").text();
                $badge.text(label);
                $badge.attr("class", "eg-state-badge eg-order-status-badge eg-order-status-" + orderStatus);
                $badge.attr("title", label);
            }
            $("[data-changed-order-result=\"" + orderId + "\"]").text(data);
        });
    }

    bindAdminOrderNoteModal();
    function bindAdminOrderNoteModal() {
        $(document)
            .off("click.adminOrderNoteModal", "[data-eg-open-order-note]")
            .on("click.adminOrderNoteModal", "[data-eg-open-order-note]", function (e) {
                e.preventDefault();
                openAdminOrderNoteModal(this);
            });
        $(document)
            .off("click.adminOrderNoteSave", "[data-eg-note-save]")
            .on("click.adminOrderNoteSave", "[data-eg-note-save]", function (e) {
                e.preventDefault();
                saveAdminOrderNoteModal();
            });
    }

    function getAdminOrderNoteModal() {
        return $("#adminOrderNoteModal");
    }

    function openAdminOrderNoteModal(trigger) {
        var $btn = $(trigger);
        var $modal = getAdminOrderNoteModal();
        if (!$modal.length) {
            return;
        }
        var orderId = $btn.attr("data-order-id");
        var orderNumber = $btn.attr("data-order-number") || "";
        $modal.data("orderId", orderId);
        $modal.find("[data-eg-note-textarea]").val($btn.attr("data-note") || "");
        $modal.find("[data-eg-note-company]").val($btn.attr("data-company") || "");
        $modal.find("[data-eg-note-tracking]").val($btn.attr("data-tracking") || "");
        $modal.find("[data-eg-note-order-label]").text(orderNumber ? ("Sipariş " + orderNumber) : "");
        $modal.find("[data-eg-note-save-result]").text("");
        if (window.bootstrap && bootstrap.Modal && $modal[0]) {
            bootstrap.Modal.getOrCreateInstance($modal[0]).show();
        } else if ($modal.modal) {
            $modal.modal("show");
        }
        window.setTimeout(function () {
            if ($btn.attr("data-eg-focus") === "cargo") {
                $modal.find("[data-eg-note-company]").trigger("focus");
            } else {
                $modal.find("[data-eg-note-textarea]").trigger("focus");
            }
        }, 150);
    }

    function saveAdminOrderNoteModal() {
        var $modal = getAdminOrderNoteModal();
        var orderId = $modal.data("orderId");
        if (!orderId) {
            return;
        }
        var note = $modal.find("[data-eg-note-textarea]").val() || "";
        var company = $modal.find("[data-eg-note-company]").val() || "";
        var tracking = $modal.find("[data-eg-note-tracking]").val() || "";
        var $save = $modal.find("[data-eg-note-save]");
        $save.prop("disabled", true);
        var postData = JSON.stringify({
            "orderId": orderId,
            "adminOrderNote": note,
            "shipmentCompanyName": company,
            "shipmentTrackingNumber": tracking
        });
        ajaxMethodCall(postData, "/admin/Ajax/SaveAdminOrderNote", function (data) {
            $save.prop("disabled", false);
            $modal.find("[data-eg-note-save-result]").text(data || "");
            applyAdminOrderNoteToRow(orderId, note, company, tracking);
            if (window.bootstrap && bootstrap.Modal && $modal[0]) {
                var inst = bootstrap.Modal.getInstance($modal[0]);
                if (inst) { inst.hide(); }
            } else if ($modal.modal) {
                $modal.modal("hide");
            }
        }, function () {
            $save.prop("disabled", false);
        });
    }

    function applyAdminOrderNoteToRow(orderId, note, company, tracking) {
        var $btns = $("[data-eg-open-order-note][data-order-id=\"" + orderId + "\"]");
        $btns.attr("data-note", note).attr("data-company", company).attr("data-tracking", tracking);
        var preview = (note || "").replace(/\s+/g, " ").trim();
        var $noteCell = $btns.filter(".eg-co-note-edit").closest(".eg-co-note-cell");
        if ($noteCell.length) {
            var $preview = $noteCell.find(".eg-co-note-preview, .eg-co-note-empty");
            if (preview) {
                if ($preview.hasClass("eg-co-note-empty")) {
                    $preview.removeClass("eg-co-note-empty").addClass("eg-co-note-preview");
                }
                $preview.attr("title", note).text(preview);
            } else {
                $preview.removeClass("eg-co-note-preview").addClass("eg-co-note-empty").removeAttr("title").text("Not yok");
            }
        }
        var cargoLine = "";
        var c = (company || "").trim();
        var t = (tracking || "").trim();
        if (c && t) { cargoLine = c + " · " + t; }
        else if (c) { cargoLine = c; }
        else if (t) { cargoLine = t; }
        var $cargoCell = $btns.filter(".eg-co-cargo-edit").closest(".eg-co-cargo-cell");
        if ($cargoCell.length) {
            var $line = $cargoCell.find(".eg-co-cargo-line, .eg-co-cargo-empty");
            if (cargoLine) {
                $line.removeClass("eg-co-cargo-empty").addClass("eg-co-cargo-line").attr("title", cargoLine).text(cargoLine);
            } else {
                $line.removeClass("eg-co-cargo-line").addClass("eg-co-cargo-empty").removeAttr("title").text("Kargo yok");
            }
        }
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
            if (!$link.length) {
                return;
            }
            if (window.bootstrap && bootstrap.Tab) {
                bootstrap.Tab.getOrCreateInstance($link[0]).show();
            } else if ($link.tab) {
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

        $form.off("shown.bs.tab.adminEditTabs", '.admin-edit-tabs a[data-bs-toggle="tab"]')
            .on("shown.bs.tab.adminEditTabs", '.admin-edit-tabs a[data-bs-toggle="tab"]', function (e) {
                var href = $(e.target).attr("href");
                try {
                    if (window.sessionStorage && href) {
                        window.sessionStorage.setItem(storageKey, href);
                    }
                } catch (err) {
                    console.warn("Could not persist admin edit tab.", err);
                }

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
        } catch (err) {
            console.warn("Could not restore admin edit tab from sessionStorage.", err);
        }
    }
    function syncGridCheckboxRow($checkbox) {
        var $tr = $checkbox.closest("tr");
        var checked = $checkbox.prop("checked") === true;
        $tr.toggleClass("eg-row-selected", checked)
            .toggleClass("gridChecked", checked)
            .toggleClass("table-success", checked);
        if (!checked) {
            $tr.removeClass("success active info table-active");
        }
    }
    function syncAllGridCheckboxRows() {
        $("input[name=checkboxGrid]").each(function () {
            syncGridCheckboxRow($(this));
        });
        if (typeof window.egUpdateSelectedCount === "function") {
            window.egUpdateSelectedCount();
        }
    }
    $(document)
        .off("change.egGridCheck", "input[name=checkboxGrid]")
        .on("change.egGridCheck", "input[name=checkboxGrid]", function () {
            syncGridCheckboxRow($(this));
            if (typeof window.egUpdateSelectedCount === "function") {
                window.egUpdateSelectedCount();
            }
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
    $("#DeleteAll").on("click", function () {
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
                var parsedPostData = JSON.parse(postData);
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
    $("#OrderingAll").on("click", function () {
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

    $("#DeselectAll").on("click", function () {
        $("input[name=checkboxGrid]").prop("checked", false);
        syncAllGridCheckboxRows();
    });
    $("#SelectAll").on("click", function () {
        $("input[name=checkboxGrid]").prop("checked", true);
        syncAllGridCheckboxRows();
    });

    $("#SetStateOffAll").on("click", function () {
        console.log("SetStateOffAll is clicked.");
        changeState(false);
    });
    $("#SetStateOnAll").on("click", function () {
        //  console.log("SetStateOnAll is clicked.");
        changeState(true);
    });
    $("#ProductStateChanged").on("click", function (e) {
        e.preventDefault();

        var ProductStateSelection = Number.parseInt($("#ProductStateSelection").val(), 10);
        var ProductStateText = $("#ProductStateSelection option:selected").text();

        var selectedProductId = GetSelectedCheckBoxValuesArray();
        var postData = JSON.stringify({
            values: selectedProductId,
            ProductStateSelection: ProductStateSelection
        });

        ajaxMethodCall(postData, "/admin/Ajax/ProductStateChanged", function (data) {
            var stateNames = ["NONE","ProductInStock","ProductOutOfStock","PreOrder","Discontinued","Backorder","ComingSoon","LimitedStock","Reserved","AwaitingRestock","NotForSale"];
            var stateClasses = stateNames.map(function (n) { return "eg-state-" + n; }).join(" ");
            var nextClass = "eg-state-" + (stateNames[ProductStateSelection] || "NONE");
            $("div[name=ProductState]").each(function () {
                var productId = $(this).attr('Product-State-Id');
                if (selectedProductId.includes(productId) || selectedProductId.includes(Number(productId))) {
                    $(this).removeClass(stateClasses).addClass(nextClass).attr("title", ProductStateText).text(ProductStateText);
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
    $("#GridListItemSize").on("change", function (e) {
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
        var defaultValueWidth = Number.parseInt($("#ImageWidth").val(), 10) || Number.parseInt($sizeRoot.attr("data-default-w"), 10) || 0;
        var defaultValueHeight = Number.parseInt($("#ImageHeight").val(), 10) || Number.parseInt($sizeRoot.attr("data-default-h"), 10) || 0;

        function syncImageSize(width, height) {
            width = Math.max(0, Math.min(2000, Number.parseInt(width, 10) || 0));
            height = Math.max(0, Math.min(2000, Number.parseInt(height, 10) || 0));
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
                w = Number.parseInt($sizeRoot.attr("data-default-w"), 10) || defaultValueWidth;
                h = Number.parseInt($sizeRoot.attr("data-default-h"), 10) || defaultValueHeight;
            } else {
                w = Number.parseInt($btn.attr("data-w"), 10);
                h = Number.parseInt($btn.attr("data-h"), 10);
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
        payload.categoryId = Number.parseInt(itemId, 10);
    } else if (itemType === 'Brand') {
        payload.brandId = Number.parseInt(itemId, 10);
    } else if (itemType === 'Tag') {
        payload.tagId = Number.parseInt(itemId, 10);
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
        var $span = $('span[name=span' + data.checkbox + ']').filter('[gridkey-id="' + entry.Id + '"]');
        if (entry.IsActive) {
            $span.attr('class', 'eg-status-icon gridActiveIcon fa-solid fa-circle-check');
            $span.attr('grid-data-value', 'True');
        } else {
            $span.attr('class', 'eg-status-icon gridNotActiveIcon fa-solid fa-circle-xmark');
            $span.attr('grid-data-value', 'False');
        }
        // Sync modern pill/switch wrappers when present (Products reference + reusable toggles).
        var $toggle = $span.closest('[data-eg-status-toggle]');
        if ($toggle.length) {
            $toggle
                .toggleClass('is-on', !!entry.IsActive)
                .toggleClass('is-off', !entry.IsActive)
                .attr('aria-pressed', entry.IsActive ? 'true' : 'false')
                .removeClass('is-busy');
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
            $("#SearchButton").trigger("click");
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
    var productCategoryId = preSelectedNode && preSelectedNode.val ? preSelectedNode.val() : "0";
    if (productCategoryId !== "0") {
        var $legacy = $("#Content_" + productCategoryId);
        if ($legacy.length) {
            var textSpan = $legacy.text();
            $legacy.text("");
            $legacy.addClass("hover2");
            $legacy.append("<span id='contentInside' class='contentSelected'>" + textSpan + "</span>");
        }
    }
    var $picker = $("[data-admin-tree-picker]").first();
    if ($picker.length) {
        adminTreeSyncPicker($picker, { skipValidation: true });
    } else {
        adminTreeMarkPicked(productCategoryId);
    }
}

function adminTreeHidden($picker) {
    var hiddenId = $picker && $picker.attr ? $picker.attr("data-hidden-id") : "";
    if (hiddenId) {
        var $byId = $("#" + hiddenId);
        if ($byId.length) {
            return $byId;
        }
    }
    var $fromGroup = $picker.closest(".form-group").find("input[type='hidden']").first();
    if ($fromGroup.length) {
        return $fromGroup;
    }
    return $picker.prevAll("input[type='hidden']").first();
}

function adminTreeMarkPicked(id) {
    var $tree = $("[data-eg-category-tree]");
    if (!$tree.length) {
        return;
    }
    $tree.find(".eg-tree-link.is-active, .eg-tree-node.is-active").removeClass("is-active");
    var numericId = parseInt(id, 10);
    if (numericId > 0) {
        var $node = $tree.find("[data-category-id='" + numericId + "']").first();
        $node.addClass("is-active");
        $node.children(".eg-tree-row").find(".eg-tree-link").addClass("is-active");
    }
}

function adminTreeSetIcon($picker, state) {
    var $icon = $picker.find("[data-tree-picker-icon]");
    $icon.removeClass("fa-solid fa-house fa-solid fa-folder-open fa-solid fa-circle-exclamation");
    if (state === "child") {
        $icon.addClass("fa-solid fa-folder-open");
    } else if (state === "empty") {
        $icon.addClass("fa-solid fa-circle-exclamation");
    } else {
        $icon.addClass("fa-solid fa-house");
    }
}

function adminTreeSyncPicker($picker, options) {
    if (!$picker || !$picker.length) {
        return;
    }
    var $hidden = adminTreeHidden($picker);
    var id = parseInt($hidden.val(), 10) || 0;
    var $name = $picker.find(".admin-tree-picker__name");
    var $badge = $picker.find("[data-tree-picker-badge]");
    var $rootBtn = $picker.find("[data-tree-root-btn]");
    var mode = $picker.attr("data-mode") || "parent";
    var rootText = $picker.attr("data-root-text") || "Ana kategori";
    var emptyText = $picker.attr("data-empty-text") || "";
    var skipValidation = options && options.skipValidation;
    $picker.removeClass("is-root is-child is-empty");
    if (id > 0) {
        $picker.addClass("is-child");
        $badge.text("Seçilen");
        $rootBtn.prop("disabled", false);
        adminTreeSetIcon($picker, "child");
    } else if (mode === "required") {
        $picker.addClass("is-empty");
        $badge.text("Seçilmedi");
        if (!$name.text().trim()) {
            $name.text(emptyText);
        }
        $rootBtn.prop("disabled", true);
        adminTreeSetIcon($picker, "empty");
    } else {
        $picker.addClass("is-root");
        $badge.text(rootText);
        $name.text(rootText);
        $rootBtn.prop("disabled", true);
        adminTreeSetIcon($picker, "root");
    }
    adminTreeMarkPicked(id);
    if (!skipValidation && $hidden.length && typeof $hidden.valid === "function") {
        try {
            var $form = $hidden.closest("form");
            if ($form.length && $form.data("validator")) {
                $hidden.valid();
            }
        } catch (ignore) { }
    }
}

function adminTreePick(id, name, $hidden, sameSelectionMessage) {
    var currentId = $("#Id").val();
    if (sameSelectionMessage && currentId && String(id) === String(currentId)) {
        alert(sameSelectionMessage);
        return;
    }
    if ($hidden && $hidden.length) {
        $hidden.val(id);
    }
    var $picker = $hidden && $hidden.length
        ? $hidden.closest(".form-group").find("[data-admin-tree-picker]").first()
        : $("[data-admin-tree-picker]").first();
    $picker.find(".admin-tree-picker__name").text(name || "");
    adminTreeSyncPicker($picker);
}

$(document).on("click", "[data-tree-root-btn]", function () {
    var $picker = $(this).closest("[data-admin-tree-picker]");
    var $hidden = adminTreeHidden($picker);
    $hidden.val("0");
    adminTreeSyncPicker($picker);
});

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

    var mobileScrollLockY = 0;

    function lockBodyScroll() {
        var body = document.body;
        if (!body) {
            return;
        }
        mobileScrollLockY = window.pageYOffset || document.documentElement.scrollTop || 0;
        body.style.position = "fixed";
        body.style.top = "-" + mobileScrollLockY + "px";
        body.style.left = "0";
        body.style.right = "0";
        body.style.width = "100%";
    }

    function unlockBodyScroll() {
        var body = document.body;
        if (!body) {
            return;
        }
        var y = mobileScrollLockY || 0;
        body.style.position = "";
        body.style.top = "";
        body.style.left = "";
        body.style.right = "";
        body.style.width = "";
        if (y) {
            window.scrollTo(0, y);
        }
        mobileScrollLockY = 0;
    }

    function setMobileDrawerOpen(isOpen) {
        var body = document.body;
        if (!body || !body.classList.contains("admin-app")) {
            return;
        }
        var wasOpen = body.classList.contains("sidebar-open");
        if (isOpen) {
            body.classList.add("sidebar-open");
            if (!wasOpen && !isDesktop()) {
                lockBodyScroll();
            }
        } else {
            body.classList.remove("sidebar-open");
            if (wasOpen) {
                unlockBodyScroll();
            }
        }
        var overlay = document.getElementById("adminSidebarOverlay");
        if (overlay) {
            overlay.setAttribute("aria-hidden", isOpen ? "false" : "true");
        }
        var sidebar = document.getElementById("adminSidebar");
        if (sidebar) {
            sidebar.setAttribute("aria-hidden", isOpen || isDesktop() ? "false" : "true");
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

    function isMegaOpen() {
        return !!(document.body && document.body.classList.contains("admin-mega-open"));
    }

    function getMegaToggle() {
        return document.getElementById("adminMegaMenuOpen") || document.getElementById("adminSidebarToggle");
    }

    var megaAnimTimer = null;

    function megaAnimMs() {
        if (window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches) {
            return 0;
        }
        return 220;
    }

    function setMegaMenuOpen(isOpen, options) {
        var body = document.body;
        var panel = document.getElementById("adminMegaMenu");
        var overlay = document.getElementById("adminSidebarOverlay");
        if (!body || !panel) {
            return;
        }
        options = options || {};
        var focusGroup = options.focusGroup || "";
        var focusSearch = !!options.focusSearch;

        if (megaAnimTimer) {
            window.clearTimeout(megaAnimTimer);
            megaAnimTimer = null;
        }

        if (!isOpen && !body.classList.contains("admin-mega-open") && !panel.classList.contains("is-open")) {
            panel.setAttribute("hidden", "hidden");
            body.classList.remove("admin-mega-backdrop");
            panel.setAttribute("aria-hidden", "true");
            if (overlay && !body.classList.contains("sidebar-open")) {
                overlay.setAttribute("aria-hidden", "true");
            }
            syncToggleAria();
            return;
        }

        if (isOpen) {
            panel.removeAttribute("hidden");
            body.classList.add("admin-mega-backdrop");
            if (overlay) {
                overlay.setAttribute("aria-hidden", "false");
            }
            // Force closed styles to paint before adding .is-open so the transition runs.
            void panel.offsetWidth;
            body.classList.add("admin-mega-open");
            panel.classList.add("is-open");
            panel.setAttribute("aria-hidden", "false");
            if (!isDesktop()) {
                setMobileDrawerOpen(false);
            }
            lockBodyScroll();
            window.setTimeout(function () {
                var input = document.getElementById("adminSidebarSearchInput");
                if (focusSearch && input) {
                    input.focus();
                }
                if (focusGroup) {
                    var card = panel.querySelector('[data-mega-group="' + focusGroup + '"]');
                    if (card && card.scrollIntoView) {
                        card.scrollIntoView({ block: "nearest" });
                    }
                }
            }, 40);
        } else {
            body.classList.remove("admin-mega-open");
            panel.classList.remove("is-open");
            panel.setAttribute("aria-hidden", "true");
            megaAnimTimer = window.setTimeout(function () {
                megaAnimTimer = null;
                if (isMegaOpen()) {
                    return;
                }
                panel.setAttribute("hidden", "hidden");
                body.classList.remove("admin-mega-backdrop");
                if (overlay && !body.classList.contains("sidebar-open")) {
                    overlay.setAttribute("aria-hidden", "true");
                }
                unlockBodyScroll();
            }, megaAnimMs());
        }
        syncToggleAria();
    }

    function syncToggleAria() {
        var toggle = getMegaToggle();
        if (!toggle) {
            return;
        }
        var open = isMegaOpen();
        toggle.setAttribute("aria-expanded", open ? "true" : "false");
        var openLabel = toggle.getAttribute("data-label-open") || "Open menu";
        var closeLabel = toggle.getAttribute("data-label-close") || "Close menu";
        toggle.setAttribute("title", open ? closeLabel : openLabel);
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
        var triggers = document.querySelectorAll("[data-open-mega]");
        for (var i = 0; i < triggers.length; i++) {
            triggers[i].addEventListener("click", function (e) {
                e.preventDefault();
                var group = this.getAttribute("data-open-mega") || "";
                var focusSearch = this.getAttribute("data-mega-focus-search") === "true";
                setMegaMenuOpen(true, { focusGroup: group, focusSearch: focusSearch });
            });
        }
        markActiveGroups();
    }

    function normalizeSearchText(str) {
        if (!str) return "";
        return str
            .replace(/İ/g, "i")
            .replace(/I/g, "i")
            .toLowerCase()
            .replace(/ı/g, "i")
            .replace(/ü/g, "u")
            .replace(/ö/g, "o")
            .replace(/ş/g, "s")
            .replace(/ç/g, "c")
            .replace(/ğ/g, "g")
            .trim();
    }

    function initSidebarMenuSearch() {
        var searchInput = document.getElementById("adminSidebarSearchInput");
        var searchClear = document.getElementById("adminSidebarSearchClear");
        var searchEmpty = document.getElementById("adminSidebarSearchEmpty");
        var mega = document.getElementById("adminMegaMenu");

        if (!searchInput || !mega) {
            return;
        }

        function filterMenu() {
            var query = normalizeSearchText(searchInput.value);
            var isSearching = query.length > 0;

            if (searchClear) {
                searchClear.style.display = isSearching ? "flex" : "none";
            }

            var items = mega.querySelectorAll(".admin-mega-item");
            var totalVisibleItems = 0;

            for (var i = 0; i < items.length; i++) {
                var item = items[i];
                var itemText = normalizeSearchText(item.textContent);
                var itemTitle = normalizeSearchText(item.getAttribute("title") || "");
                var card = item.closest ? item.closest(".admin-mega-card") : null;
                var cardTitleEl = card ? card.querySelector(".admin-mega-card-title") : null;
                var cardTitle = normalizeSearchText(cardTitleEl ? cardTitleEl.textContent : "");
                var itemMatch = !isSearching || itemText.indexOf(query) >= 0 || itemTitle.indexOf(query) >= 0 || cardTitle.indexOf(query) >= 0;

                if (itemMatch) {
                    item.style.display = "";
                    totalVisibleItems++;
                    if (isSearching) {
                        item.classList.add("search-highlight");
                    } else {
                        item.classList.remove("search-highlight");
                    }
                } else {
                    item.style.display = "none";
                    item.classList.remove("search-highlight");
                }
            }

            var cards = mega.querySelectorAll(".admin-mega-card");
            for (var c = 0; c < cards.length; c++) {
                var cardEl = cards[c];
                var visibleInCard = cardEl.querySelectorAll(".admin-mega-item:not([style*='display: none'])").length;
                cardEl.style.display = (!isSearching || visibleInCard > 0) ? "" : "none";
            }

            if (searchEmpty) {
                searchEmpty.style.display = (isSearching && totalVisibleItems === 0) ? "block" : "none";
            }
        }

        searchInput.addEventListener("input", filterMenu);

        searchInput.addEventListener("keydown", function (e) {
            if (e.key === "Escape" || e.key === "Esc") {
                if (searchInput.value) {
                    searchInput.value = "";
                    filterMenu();
                    e.stopPropagation();
                    return;
                }
                setMegaMenuOpen(false);
            } else if (e.key === "Enter") {
                e.preventDefault();
                var firstVisible = mega.querySelector(".admin-mega-item:not([style*='display: none'])");
                if (firstVisible && firstVisible.href) {
                    window.location.href = firstVisible.href;
                }
            }
        });

        if (searchClear) {
            searchClear.addEventListener("click", function (e) {
                e.preventDefault();
                searchInput.value = "";
                filterMenu();
                searchInput.focus();
            });
        }
    }

    function initAdminSidebarShell() {
        var body = document.body;
        if (!body || !body.classList.contains("admin-app")) {
            return;
        }

        var toggle = getMegaToggle();
        var overlay = document.getElementById("adminSidebarOverlay");
        body.classList.remove("sidebar-collapsed");
        body.classList.remove("sidebar-open");

        initNavGroups();
        initSidebarMenuSearch();

        var megaCloseBtn = document.getElementById("adminMegaMenuClose");
        if (megaCloseBtn) {
            megaCloseBtn.addEventListener("click", function (e) {
                e.preventDefault();
                setMegaMenuOpen(false);
            });
        }

        if (toggle) {
            toggle.addEventListener("click", function (e) {
                e.preventDefault();
                if (isMegaOpen()) {
                    setMegaMenuOpen(false);
                } else {
                    setMegaMenuOpen(true, { focusSearch: true });
                }
            });
        }

        if (overlay) {
            overlay.addEventListener("click", function () {
                setMegaMenuOpen(false);
                setMobileDrawerOpen(false);
            });
        }

        document.addEventListener("keydown", function (e) {
            if (e.key !== "Escape" && e.key !== "Esc") {
                return;
            }
            if (isMegaOpen()) {
                setMegaMenuOpen(false);
            }
        });

        setMegaMenuOpen(false);

        window.addEventListener("resize", function () {
            if (isMegaOpen()) {
                setMegaMenuOpen(false);
            }
            body.classList.remove("sidebar-collapsed");
            body.classList.remove("sidebar-open");
            unlockBodyScroll();
            syncToggleAria();
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
            var match = !q || name.includes(q);
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

function syncAdminValidationSummaryTone() {
    $("[data-admin-success-message]").each(function () {
        var $el = $(this);
        var success = ($el.attr("data-admin-success-message") || "").trim();
        var messages = $el.find("li").map(function () {
            return $(this).text().replace(/\s+/g, " ").trim();
        }).get().filter(function (t) { return t.length > 0; });
        if (!messages.length) {
            return;
        }
        var allSuccess = !!success && messages.every(function (t) { return t === success; });
        $el.toggleClass("alert-success", allSuccess);
        $el.toggleClass("alert-danger", !allSuccess);
    });
}

$(document).on("invalid-form.validate", "form", function () {
    window.setTimeout(syncAdminValidationSummaryTone, 0);
});

$(function () {
    syncAdminValidationSummaryTone();
});

