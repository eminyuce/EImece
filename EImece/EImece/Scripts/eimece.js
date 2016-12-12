function isEmpty(str) {
    return (!str || 0 === str.length);
}

$(document).ready(function () {
    $("#DeselectAll").click(function () {
        console.log("DeselectAll is clicked.");
        var i = 0;
        $("input[name=checkboxGrid]").each(function () {
            $(this).parent().parent().removeClass('gridChecked');
            var m = $(this).prop('checked', false);
        });
    });
    $("#SelectAll").click(function () {
        console.log("SelectAll is clicked.");
        var i = 0;
        $("input[name=checkboxGrid]").each(function () {
            $(this).parent().parent().addClass('gridChecked');
            var m = $(this).prop('checked', true);
        });
    });

    $("#SetStateOffAll").click(function () {
        console.log("SetStateOffAll is clicked.");
        changeState(false);
    });
    $("#SetStateOnAll").click(function () {
        console.log("SetStateOnAll is clicked.");
        changeState(true);
    });
    function changeState(state) {
        var ppp = $("#ItemStateSelection").val();
        var selectedValues = GetSelectedStateValues("span" + ppp, state);
        if (selectedValues.length > 0) {
            var postData = JSON.stringify({ "values": selectedValues, "checkbox": ppp });
            console.log(postData);
            var tableName = $("[data-gridname]").attr("data-gridname");
            ajaxMethodCall(postData, "/Ajax/Change" + tableName + "OrderingOrState", changeStateSuccess);
            displayMessage("hide", "");

        } else {
            displayMessage("error", "Checkboxes on the grid does not selected");
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
    function displayMessage(messageType, message) {
        var messagePanel = $("#ErrorMessagePanel");
        var errorMessage = $("#ErrorMessage");
        messagePanel.show();
        if (messageType == "info") {
            messagePanel.attr("class", "alert alert-info");
            errorMessage.text(message);
        } else if (messageType == "error") {
            messagePanel.attr("class", "alert alert-danger");
            errorMessage.text(message);
        } else if (messageType == "hide") {
            messagePanel.hide();
        }
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
                    if (param == qsArray[i].split('=')[0]) {
                        //exists key
                        return qsArray[i].split('=')[1];
                    }
                }
            }

        }
        return "";
    }
    function updateUrlParameter(originalURL, param, value) {
        console.log(value);
        var windowUrl = originalURL.split('?')[0];
        var qs = originalURL.split('?')[1];
        //3- get list of query strings
        var qsArray = qs.split('&');
        var flag = false;
        //4- try to find query string key
        for (var i = 0; i < qsArray.length; i++) {
            if (qsArray[i].split('=').length > 0) {
                if (param == qsArray[i].split('=')[0]) {
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
                } else if (jqXHR.status == 404) {
                    console.error('Requested page not found. [404]');
                } else if (jqXHR.status == 500) {
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
});