

// Use currentTarget/this (not e.target): Crizal/Modern buttons wrap the label in <span>,
// so clicks on the text would otherwise miss data-add-product-cart on the <button>.
$(document).off("click.eimeceAddCart", "[data-add-product-cart]").on("click.eimeceAddCart", "[data-add-product-cart]", function (e) {
    e.preventDefault();
    var $btn = $(e.currentTarget).closest("[data-add-product-cart]");
    var productId = $btn.attr("data-add-product-cart");
    if (!productId) {
        console.warn("data-add-product-cart missing on click target");
        return;
    }
    var postData = JSON.stringify({
        productId: productId,
        quantity: 1,
        orderGuid: getOrderGuid()
    });
    console.log(postData);
    ajaxMethodCall(postData, "/Payment/AddToCart", function (data) {
        console.log(data);
        GetShoppingCartLinks();
    });
});
function getOrderGuid() {
    var orderGuid = getCookie("orderGuid");
    if (orderGuid != "") {

    } else {
        orderGuid = createUUID();
        setCookie("orderGuid", orderGuid, 365);
    }

    return orderGuid;
}

function hideAddToCartSpecError() {
    $("#addToCartSpecError").hide();
}

function showAddToCartSpecError() {
    var labels = [];
    $(".product-spec-chip-group.has-error").each(function () {
        var name = ($(this).attr("data-spec-name") || "").trim();
        if (name && labels.indexOf(name) === -1) {
            labels.push(name);
        }
    });
    var msg;
    if (labels.length === 1) {
        msg = "Lütfen " + labels[0] + " seçin.";
    } else if (labels.length > 1) {
        msg = "Lütfen " + labels.join(" ve ") + " seçin.";
    } else {
        msg = "Lütfen zorunlu ürün özelliklerini seçin.";
    }

    var $box = $("#addToCartSpecError");
    if (!$box.length) {
        $box = $('<div id="addToCartSpecError" class="pdp-spec-error" role="alert"></div>');
    }
    $box.html('<span class="pdp-spec-error__icon" aria-hidden="true">!</span><span class="pdp-spec-error__text"></span>');
    $box.find(".pdp-spec-error__text").text(msg);

    var $row = $("#AddToCart").closest(".d-flex");
    if ($row.length) {
        $row.before($box);
    } else {
        $("#AddToCart").before($box);
    }
    $box.show();
}

// Chip selector for on-sale PDP (replaces dropdown) – click to select
$(document).off("click.eimeceSpecChip", ".spec-chip[data-chip-value]").on("click.eimeceSpecChip", ".spec-chip[data-chip-value]", function (e) {
    var $chip = $(e.currentTarget);
    var $group = $chip.closest('.product-spec-chip-group');
    var $select = $group.find('select[data-product-selected-specs]');
    var val = $chip.data('chip-value');
    $group.find('.spec-chip').removeClass('is-selected').attr('aria-checked', 'false');
    $chip.addClass('is-selected').attr('aria-checked', 'true');
    if ($select.length) {
        $select.val(val).trigger('change');
    }
    $group.removeClass('has-error').find('.spec-chip-hint').addClass('d-none');
    if (!$('.product-spec-chip-group.has-error').length) {
        hideAddToCartSpecError();
    }
});

// Primary PDP button (#AddToCart). Delegated so it still works if the control is rendered after script load.
$(document).off("click.eimeceAddToCart", "#AddToCart").on("click.eimeceAddToCart", "#AddToCart", function () {
    var nProductId = $("#productId").val();
    if (!nProductId) {
        console.warn("#AddToCart clicked but #productId is missing");
        return;
    }

    var hasMissingSpec = false;
    $('[data-product-selected-specs=' + nProductId + ']').each(function () {
        if (!$(this).val()) {
            hasMissingSpec = true;
            $(this).closest('.product-spec-chip-group').addClass('has-error').find('.spec-chip-hint').removeClass('d-none');
        }
    });
    if (hasMissingSpec) {
        var $firstError = $(".product-spec-chip-group.has-error").first();
        if ($firstError.length && $firstError[0].scrollIntoView) {
            $firstError[0].scrollIntoView({ behavior: "smooth", block: "center" });
        }
        showAddToCartSpecError();
        return;
    }
    hideAddToCartSpecError();

    var selectedTotalSpecs = new Array();
    $('[data-product-selected-specs=' + nProductId + ']').each(function () {
        var obj = {
            SpecsName: $(this).attr('name'),
            SpecsValue: $(this).val() 
        };
        selectedTotalSpecs.push(obj);
    });
   
    var postData = JSON.stringify({
        productId: nProductId,
        quantity: $("#quantity").val() || 1,
        orderGuid: getOrderGuid(),
        productSpecItems: JSON.stringify({
            selectedTotalSpecs
        })
    });
    console.log(postData);
    ajaxMethodCall(postData, "/Payment/AddToCart", function (data) {
        GetShoppingCartLinks();
    });
});

function GetShoppingCartLinks() {
    var postData = JSON.stringify({});
    ajaxMethodCall(postData, "/Payment/GetShoppingCartLinks", function (data) {
        $("#ShoppingCartsLink").replaceWith(data);
        addShoppingCartsLinkDetailClick();
    });
}
function addShoppingCartsLinkDetailClick() {
    $("#ShoppingCartsLinkDetail").click(function () {
        var orderGuid = getCookie("orderGuid");
        var postData = JSON.stringify({
            orderGuid: orderGuid
        });
        console.log(postData);
        ajaxMethodCall(postData, "/Payment/GetShoppingCartSmallDetails", function (data) {
            $("#ShoppingCartsDetail").html(data);
            bindOnRemove();
        });
    });
}
function removeCart(shoppingItemId) {
    console.log(shoppingItemId);
    var postData = JSON.stringify({ shoppingItemId });
    ajaxMethodCall(postData, "/Payment/RemoveCart", function (data) {
        $('[data-shopping-item=' + shoppingItemId + ']').remove();
        console.log(data);
        if (data.TotalItemCount === 0) {
            $("#ShoppingCartsDetail").hide();
        } else {
            $("#ShoppingCartsDetail").show();
        }
        GetShoppingCartLinks();
    });
}
function showCartFeedback(message, type) {
    var $box = $("#cart-feedback");
    if (!$box.length || !message) {
        return;
    }
    $box.removeClass("d-none alert-success alert-danger alert-warning")
        .addClass(type === "error" ? "alert-danger" : "alert-success")
        .text(message)
        .show();
}

function clampCartQuantity($input) {
    var min = parseInt($input.attr("min"), 10);
    var max = parseInt($input.attr("max"), 10);
    var val = parseInt($input.val(), 10);
    if (isNaN(min)) min = 1;
    if (isNaN(max)) max = 999;
    if (isNaN(val) || val < min) val = min;
    if (val > max) val = max;
    $input.val(val);
    return val;
}

function bindOnRemove() {
    $(document).off("click.eimeceRemoveCart", "[data-shopping-item-remove]").on("click.eimeceRemoveCart", "[data-shopping-item-remove]", function (e) {
        e.preventDefault();
        var $btn = $(e.currentTarget).closest("[data-shopping-item-remove]");
        var shoppingItemId = $btn.attr("data-shopping-item-remove");
        if (!shoppingItemId) {
            return;
        }
        if ($btn.closest("[data-shopping-item-row]").length && !$btn.hasClass("is-confirming")) {
            $btn.addClass("is-confirming");
            var $label = $btn.find("span").last();
            var originalLabel = $label.length ? $label.text() : $btn.text();
            if ($label.length) {
                $label.text($btn.attr("data-confirm-label") || originalLabel);
            }
            window.setTimeout(function () {
                $btn.removeClass("is-confirming");
                if ($label.length) {
                    $label.text(originalLabel);
                }
            }, 4000);
            return;
        }
        var postData = JSON.stringify({ shoppingItemId: shoppingItemId });
        ajaxMethodCall(postData, "/Payment/RemoveCart", function (data) {
            if (!data || String(data.status).toUpperCase() === "FAILED") {
                showCartFeedback($("#cart-feedback").attr("data-error-text"), true);
                return;
            }
            $('[data-shopping-item-row=' + shoppingItemId + ']').remove();
            $('[data-shopping-home-page-item=' + shoppingItemId + ']').remove();
            GetShoppingCartLinks();
            if (data.TotalItemCount === 0 && $("[data-shopping-item-row]").length === 0) {
                window.location.reload();
                return;
            }
            bindCalcuateTotalPrice();
        });
    });
}
bindOnRemove();

function triggerUpdateQuantityMultiplePrice(e, shoppingItemId) {
    var $input = $('[data-shopping-quantity-id=' + shoppingItemId + ']');
    var quantity = clampCartQuantity($input);
    if (!quantity) return;
    var postData = JSON.stringify({ shoppingItemId: shoppingItemId, quantity: quantity });
    ajaxMethodCall(postData, "/Payment/UpdateQuantity", function (data) {
        if (!data || String(data.status).toUpperCase() === "FAILED") {
            showCartFeedback($("#cart-feedback").attr("data-error-text"), true);
            return;
        }
        if (data.LineTotal) {
            $('[data-shopping-item-total-price=' + shoppingItemId + ']').html(data.LineTotal);
        }
        bindCalcuateTotalPrice();
    });
}

$(document).off("click.eimeceQtyBtn", "[data-cart-qty-minus], [data-cart-qty-plus]")
    .on("click.eimeceQtyBtn", "[data-cart-qty-minus], [data-cart-qty-plus]", function (e) {
        e.preventDefault();
        var $btn = $(e.currentTarget);
        var shoppingItemId = $btn.attr("data-cart-qty-minus") || $btn.attr("data-cart-qty-plus");
        var $input = $('[data-shopping-quantity-id=' + shoppingItemId + ']');
        if (!$input.length) return;
        var val = clampCartQuantity($input);
        if ($btn.is("[data-cart-qty-minus]")) {
            val = Math.max(parseInt($input.attr("min"), 10) || 1, val - 1);
        } else {
            val = Math.min(parseInt($input.attr("max"), 10) || 999, val + 1);
        }
        $input.val(val);
        triggerUpdateQuantityMultiplePrice(e, shoppingItemId);
    });

$(document).off("click.eimeceUpdateQty", "[data-shopping-button-price]")
    .on("click.eimeceUpdateQty", "[data-shopping-button-price]", function (e) {
        e.preventDefault();
        var shoppingItemId = $(e.currentTarget).closest("[data-shopping-button-price]").attr("data-shopping-button-price");
        if (!shoppingItemId) return;
        triggerUpdateQuantityMultiplePrice(e, shoppingItemId);
    });

var cartQtyTimers = {};
$(document).off("change.eimeceQtyInput", "[data-shopping-quantity-id]")
    .on("change.eimeceQtyInput", "[data-shopping-quantity-id]", function (e) {
        var shoppingItemId = $(this).attr("data-shopping-quantity-id");
        clampCartQuantity($(this));
        window.clearTimeout(cartQtyTimers[shoppingItemId]);
        cartQtyTimers[shoppingItemId] = window.setTimeout(function () {
            triggerUpdateQuantityMultiplePrice(e, shoppingItemId);
        }, 250);
    });

function renderShoppingCartPrice(success) {
    var postData = JSON.stringify({});
    ajaxMethodCall(postData, "/Payment/renderShoppingCartPrice", success);
}

function escapeCartHtml(value) {
    return String(value == null ? "" : value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;");
}

function showCartFeedback(message, isError) {
    var $box = $("#cart-feedback");
    if (!$box.length || !message) return;
    $box.removeClass("d-none alert-success alert-danger")
        .addClass(isError ? "alert-danger" : "alert-success")
        .text(message);
    window.clearTimeout($box.data("hideTimer"));
    $box.data("hideTimer", window.setTimeout(function () {
        $box.addClass("d-none");
    }, 4000));
}

function updateCargoRow(data) {
    var $el = $("#CargoFreeTextInfo");
    if (!$el.length || !data) return;
    var freeLabel = $el.attr("data-cargo-free") || "";
    var cargoLabel = $el.attr("data-cargo-label") || "";
    var isFree = !data.CargoPriceInt;
    if ($el.is("tr")) {
        if (isFree) {
            $el.html('<td colspan="2" class="p-0 pt-2"><div class="alert alert-success text-center px-2 py-2 mb-0 free-cargo-banner" role="alert"><i class="fas fa-check-circle mr-1" aria-hidden="true"></i><strong>' + escapeCartHtml(freeLabel) + "</strong></div></td>");
        } else {
            $el.html('<th scope="row">' + escapeCartHtml(cargoLabel) + '</th><td class="text-end">' + escapeCartHtml(data.CargoPrice) + "</td>");
        }
    } else if (isFree) {
        $el.html('<div class="alert alert-success text-center px-2 py-2 mb-0 free-cargo-banner" role="alert"><strong>' + escapeCartHtml(freeLabel) + "</strong></div>");
    } else {
        $el.html('<div class="d-flex justify-content-between mb-0"><span>' + escapeCartHtml(cargoLabel) + ':</span><span><strong>' + escapeCartHtml(data.CargoPrice) + "</strong></span></div>");
    }
}

function bindCalcuateTotalPrice() {
    renderShoppingCartPrice(function (data) {
        if (!data) return;
        $('#CargoPrice').html(data.CargoPrice);
        updateCargoRow(data);
        $('#TotalPrice').html(data.TotalPrice);
        $('#TotalPriceWithCargoPrice').html(data.TotalPriceWithCargoPrice);
        if (data.price) {
            $('#HomePageTotalPrice').html(data.price);
        }
    });
}

function saveOrderCommentsThenGo(hrefUrl) {
    var txtArea = $("#orderComments");
    if (!txtArea.length || !hrefUrl) {
        if (hrefUrl) window.location.href = hrefUrl;
        return;
    }
    var postData = JSON.stringify({
        orderComments: txtArea.val(),
        orderGuid: txtArea.attr("data-shopping-order-guid")
    });
    ajaxMethodCall(postData, "/Payment/sendOrderComments", function () {
        window.location.href = hrefUrl;
    });
}

$(document).off("click.eimeceCheckout", "#ProceedToCheckout, #ContinueShoppingWithoutAccount")
    .on("click.eimeceCheckout", "#ProceedToCheckout, #ContinueShoppingWithoutAccount", function (e) {
        var hrefUrl = $(this).attr("href");
        if (!hrefUrl || $(this).data("busy")) {
            e.preventDefault();
            return;
        }
        e.preventDefault();
        $(this).data("busy", true).addClass("disabled");
        saveOrderCommentsThenGo(hrefUrl);
    });

$(document).off("click.eimecePdpQty", "[data-pdp-qty-minus], [data-pdp-qty-plus]")
    .on("click.eimecePdpQty", "[data-pdp-qty-minus], [data-pdp-qty-plus]", function (e) {
        e.preventDefault();
        var $btn = $(e.currentTarget);
        var inputId = $btn.attr("data-pdp-qty-minus") || $btn.attr("data-pdp-qty-plus");
        var $input = $("#" + inputId);
        if (!$input.length) return;
        var val = clampCartQuantity($input);
        if ($btn.is("[data-pdp-qty-minus]")) {
            val = Math.max(parseInt($input.attr("min"), 10) || 1, val - 1);
        } else {
            val = Math.min(parseInt($input.attr("max"), 10) || 999, val + 1);
        }
        $input.val(val);
    });

$(document).off("submit.eimeceCheckoutOnce", "form.js-checkout-form")
    .on("submit.eimeceCheckoutOnce", "form.js-checkout-form", function (e) {
        var $form = $(this);
        if ($form.data("submitted")) {
            e.preventDefault();
            return false;
        }
        $form.data("submitted", true);
        var $btns = $form.find("[type=submit], .js-checkout-submit");
        window.setTimeout(function () {
            $btns.prop("disabled", true).addClass("disabled");
        }, 0);
        return true;
    });

jQuery(function () {
    $("#btn-search").click(function () {
        console.log("eee");
        $(".error").hide();
        var hasError = false;
        var searchReg = /^[a-zA-Z0-9-]+$/;
        var searchVal = $("#Search_TextBox").val();
        if (searchVal == '') {
            $("#errorMessage").text($("#SearchRequiredErrorMessage").val());
            //  $("#Search_TextBox").after('<span class="error">' + $("#SearchRequiredErrorMessage").val() + '</span>');
            hasError = true;
        }
        //else if (!searchReg.test(searchVal)) {
        //    $("#errorMessage").text($("#SearchValidText").val());
        //    // $("#Search_TextBox").after('<span class="error">' + $("#SearchValidText").val() + '</span>');
        //    hasError = true;
        //}
        if (hasError == true) { return false; }
    });
});
function ajaxMethodCall(postData, ajaxUrl, successFunction) {
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
            if (jqXHR.status === 0) {
                console.error('Not connect.\n Verify Network.');
            } else if (jqXHR.status === 404) {
                console.error('Requested page not found. [404]');
            } else if (jqXHR.status === 500) {
                console.error('Internal Server Error [500].');
            } else if (exception === 'parsererror') {
                console.error('Requested JSON parse failed.');
            } else if (exception === 'timeout') {
                console.error('Time out error.');
            } else if (exception === 'abort') {
                console.error('Ajax request aborted.');
            } else {
                console.error('Uncaught Error.\n' + jqXHR.responseText);
            }
        }
    });
}
function setCookie(cname, cvalue, exdays) {
    var d = new Date();
    d.setTime(d.getTime() + (exdays * 24 * 60 * 60 * 1000));
    var expires = "expires=" + d.toUTCString();
    document.cookie = cname + "=" + cvalue + ";" + expires + ";path=/";
}

function getCookie(cname) {
    var name = cname + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function checkCookie() {
    var user = getCookie("username");
    if (user != "") {
        alert("Welcome again " + user);
    } else {
        user = prompt("Please enter your name:", "");
        if (user != "" && user != null) {
            setCookie("username", user, 365);
        }
    }
}
function createUUID() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}

function isEmpty(str) {
    return (!str || 0 === str.length);
}

function randomString(length, chars) {
    var mask = '';
    if (chars.indexOf('A') > -1) mask += 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    if (chars.indexOf('#') > -1) mask += '0123456789';
    var result = '';
    for (var i = length; i > 0; --i) result += mask[Math.floor(Math.random() * mask.length)];
    return result;
}


// Address dropdowns only exist on checkout/account pages — skip city Ajax elsewhere
if ($("#Cities").length) {
    // Event handler when the user selects a city
    $("#Cities").change(function (e) {
        var cityName = e.target.value;
        GetTownsByCity(cityName, null, null);
        $("#SelectedCity").val(cityName);  // Selected city
    });
    // Event handler when the user selects a town
    $("#Towns").change(function (e) {
        var townName = e.target.value;
        var cityName = $("#Cities").val();
        GetDistrictsByTown(cityName, townName);
        $("#SelectedCity").val(cityName);  // Selected city
        $("#SelectedTown").val(townName);  // Selected town
    });
    $("#Districts").change(function (e) {
        var districtName = e.target.value;
        $("#SelectedDistrict").val(districtName);
    });

    // Initially fetch cities, towns, and districts
    GetIller();
}

// Function to get all cities
function GetIller() {
    var selectedCity = $("#SelectedCity").val();  // Selected city
    var selectedTown = $("#SelectedTown").val();  // Selected town
    var selectedDistrict = $("#SelectedDistrict").val();  // Selected district

    var postData = JSON.stringify({});  // No parameters needed to get all cities
    console.log(postData);

    // Fetch the list of cities
    ajaxMethodCall(postData, "/Ajax/GetAllCities", function (data) {
        $("#Cities").empty();  // Clear existing options
        $.each(data, function (index, item) {
            var option = new Option(item.Text, item.Value);
            if (item.Value === selectedCity) {
                option.selected = true;
            }
            $("#Cities").append(option);
        });
    
        GetTownsByCity(selectedCity);
    });
}

// Function to get towns based on selected city
function GetTownsByCity(cityName) {
    var selectedTown = $("#SelectedTown").val();  // Selected town

    var postData = JSON.stringify({ cityName: cityName });
    console.log(postData);

    ajaxMethodCall(postData, "/Ajax/GetTownsByCity", function (data) {
        $("#Towns").empty();  // Clear existing options
        $.each(data, function (index, item) {
            var option = new Option(item.Text, item.Value);
            if (item.Value === selectedTown) {
                option.selected = true;
            }
            $("#Towns").append(option); // Add town to dropdown
        });

        var townName = $("#Towns").val();
        var cityName = $("#Cities").val();
        GetDistrictsByTown(cityName, townName);
    });
}

// Function to get districts based on selected town
function GetDistrictsByTown(cityName, townName) {
    var selectedDistrict = $("#SelectedDistrict").val();  // Selected district

    var postData = JSON.stringify({ cityName: cityName, townName: townName });
    console.log(postData);

    ajaxMethodCall(postData, "/Ajax/GetDistrictsByTown", function (data) {
        $("#Districts").empty();  // Clear existing options
        $.each(data, function (index, item) {
            var option = new Option(item.Text, item.Value);
            if (item.Value === selectedDistrict) {
                option.selected = true;
            }
            $("#Districts").append(option); // Add district to dropdown
        });
    });
}

$("#SubscribeEmailBtn").click(function () {
    var subscribeEmail = $("#SubscribeEmailText").val().trim();
    console.log(subscribeEmail);
    // Check if the email is empty before making the AJAX call
    if (subscribeEmail === "") {
        alert("Please enter a valid email.");
        return;  // Prevent AJAX call if the email is empty
    }
    var postData = JSON.stringify({ subscribeEmail: subscribeEmail });
    console.log(postData);
    $("#SubscribeEmailTextSuccessMessage").hide();
    ajaxMethodCall(postData, "/Ajax/SubscribeEmail", function (data) {
        if (data === "success") {
            // Show success message if the response is 'success'
            $("#SubscribeEmailTextSuccessMessage").show();
        } else {
            // Handle failure if needed
            alert(data);
        }
    });
});