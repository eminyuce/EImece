﻿

$('[data-add-product-cart]').each(function () {
    $(this).off("click");
    $(this).on("click", function (e) {
        e.preventDefault();
        var caller = e.target;
        var productId = $(caller).attr('data-add-product-cart');
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
 


$("#AddToCart").click(function () {
    var nProductId = $("#productId").val();
   
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
        quantity: $("#quantity").val(),
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
function bindOnRemove() {
    $('[data-shopping-item-remove]').each(function () {
        $(this).off("click");
        $(this).on("click", function (e) {
            e.preventDefault();
            var caller = e.target;
            var shoppingItemId = $(caller).attr('data-shopping-item-remove');
            var postData = JSON.stringify({ shoppingItemId: shoppingItemId });
            console.log(postData);
            ajaxMethodCall(postData, "/Payment/RemoveCart", function (data) {
                $('[data-shopping-item-row=' + shoppingItemId + ']').remove();
                $('[data-shopping-home-page-item=' + shoppingItemId + ']').remove();
                bindCalcuateTotalPrice();
                GetShoppingCartLinks();
            });
        });
    });
}
bindOnRemove();
console.log("jquery is working");
$('[data-shopping-quantity-id]').each(function () {
    $(this).off("blur");
    $(this).on("blur", function (e) {
        var caller = e.target;
        var shoppingItemId = $(caller).attr('data-shopping-quantity-id');
        var quantity = caller.value;
        $('[data-shopping-quantity-id=' + shoppingItemId + ']').val(quantity);
    });
});
function triggerUpdateQuantityMultiplePrice(e, shoppingItemId) {
    var itemPrice = $('[data-shopping-item-price=' + shoppingItemId + ']').val();
    var quantity = $('[data-shopping-quantity-id=' + shoppingItemId + ']').val();
    if (quantity == 0) return;
    var postData = JSON.stringify({ shoppingItemId: shoppingItemId, quantity: quantity });
    console.log(postData);
    ajaxMethodCall(postData, "/Payment/UpdateQuantity", function (data) {
        //  var totalPrice = parseFloat(itemPrice) * quantity;
        //  console.log(totalPrice);
        renderShoppingCartPrice(function (data) {
            $('[data-shopping-item-total-price=' + shoppingItemId + ']').html(data.TotalPrice);
        });
        bindCalcuateTotalPrice();
    });
}
$('[data-shopping-button-price]').each(function () {
    $(this).off("click");
    $(this).on("click", function (e) {
        var caller = e.target;
        var shoppingItemId = $(caller).attr('data-shopping-button-price');
        var quantity = parseInt($('[data-shopping-quantity-id=' + shoppingItemId + ']').val(), 10);
        if (isNaN(quantity)) return;
        if (quantity == 0) return;
        triggerUpdateQuantityMultiplePrice(e, shoppingItemId);
    });
});

$('[data-shopping-quantity-id]').each(function () {
    let previousValue = parseInt($(this).val(), 10);

    $(this).off('input').on('input', function (e) {
        const $input = $(this);
        const currentValue = parseInt($input.val(), 10);

        if (isNaN(currentValue)) return;

        if (currentValue == 0) return;

        const shoppingItemId = $input.attr('data-shopping-quantity-id');
        console.log(shoppingItemId);
        if (currentValue > previousValue) {
            console.log('Increased');
            // custom increase logic
            triggerUpdateQuantityMultiplePrice(e, shoppingItemId);
        } else if (currentValue < previousValue) {
            console.log('Decreased');
            // custom decrease logic
            triggerUpdateQuantityMultiplePrice(e, shoppingItemId);
        }

        previousValue = currentValue;
    });
});


function renderShoppingCartPrice(success) {
    var postData = JSON.stringify({});
    ajaxMethodCall(postData, "/Payment/renderShoppingCartPrice", success);
}

function bindCalcuateTotalPrice() {
    var grandTotalPrice = 0;
    $('[data-shopping-item-row]').each(function () {
        var shoppingItemId = $(this).attr('data-shopping-item-row');
        var itemPrice = $('[data-shopping-item-price=' + shoppingItemId + ']').val();
        var quantity = $('[data-shopping-quantity-id=' + shoppingItemId + ']').val();
        var totalPrice = parseFloat(itemPrice) * quantity;
        grandTotalPrice = grandTotalPrice + totalPrice;
    });

    renderShoppingCartPrice(function (data) {
        
        $('#CargoPrice').html(data.CargoPrice);
        $('#CargoFreeTextInfo').html(data.CargoPriceHtml);
        $('#TotalPrice').html(data.TotalPrice);
        $('#TotalPriceWithCargoPrice').html(data.TotalPriceWithCargoPrice);
        $('#HomePageTotalPrice').html(data.price);
    });
}

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