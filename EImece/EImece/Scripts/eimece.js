
function GetShoppingCartLinks() {
    var postData = JSON.stringify({});
    ajaxMethodCall(postData, "/Payment/GetShoppingCartLinks", function (data) {
        $("#ShoppingCartsLink").empty();
        $("#ShoppingCartsLink").html(data);
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