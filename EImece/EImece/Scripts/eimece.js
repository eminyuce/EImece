function isEmpty(str) {
    return (!str || 0 === str.length);
}

$(document).ready(function () {
    bindCKEDITOR();
    searchAutoComplete();
    function bindCKEDITOR() {
        $('[data-ckeditor-field]').each(function () {
            CKEDITOR.replace(this);
        });
    }
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
        console.log("DeleteAll is clicked.");
        confirmDialog(YOUR_MESSAGE_STRING_CONST, function () {
            var postData = GetSelectedCheckBoxValues();
            var parsedPostData = jQuery.parseJSON(postData);
            if (parsedPostData.values.length > 0) {
                var tableName = $("[data-gridname]").attr("data-gridname");
                console.log("Delete" + tableName + "Item");
                ajaxMethodCall(postData, "/Ajax/Delete" + tableName + "Item", deleteItemsSuccess);
            }
        });
        
    });

    function confirmDialog(message, onConfirm) {
        var fClose = function () {
            modal.modal("hide");
        };
        var modal = $("#confirmModal");
        modal.modal("show");
        $("#confirmMessage").empty().append(message);
        $("#confirmOk").one('click', onConfirm);
        $("#confirmOk").one('click', fClose);
        $("#confirmCancel").one("click", fClose);
    }
    $("#OrderingAll").click(function () {
        console.log("OrderingAll is clicked.");
        var postData = GetSelectedOrderingValues();
        console.log(postData);
        var tableName = $("[data-gridname]").attr("data-gridname");
        ajaxMethodCall(postData, "/Ajax/Change" + tableName + "OrderingOrState", changeOrderingSuccess);
    });

    function GetSelectedStateValues(checkboxName, state) {
        var itemArray = new Array();
        var i = 0;
        $('span[name=' + checkboxName + ']').each(function () {
            var id = $(this).attr("gridkey-id");
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

    var defaultValueWidth = $('#ImageWidth').val();
    var defaultValueHeight = $('#ImageHeight').val();

    var handle1 = $("#sliderWidthHandle");
    handle1.text(defaultValueWidth);
    $("#sliderWidth").slider({
        min: 0,
        max: 2000,
        value: defaultValueWidth,
        step: 10,
        create: function () {
            handle1.text($(this).slider("value"));
        },
        slide: function (event, ui) {
            handle1.text(ui.value);
            $('#imageWidthTxt').val(ui.value);
        },
        change: function (event, ui) {
            $('#imageWidthTxt').val(ui.value);
        }
    });

    var handle2 = $("#sliderHeightHandle");
    handle2.text(defaultValueHeight);
    $("#sliderHeight").slider({
        min: 0,
        max: 2000,
        value: defaultValueHeight,
        step: 10,
        create: function () {
            handle2.text($(this).slider("value"));
        },
        slide: function (event, ui) {
            handle2.text(ui.value);
            $('#imageHeightTxt').val(ui.value);
        },
        change: function (event, ui) {
            $('#imageHeightTxt').val(ui.value);
        }
    });
    $("#imageHeightTxt").val(defaultValueHeight);
    $("#imageHeightTxt").change(function () {
        var value = this.value;
        console.log(value);
        $("#sliderHeight").slider("value", parseInt(value));
        handle2.text(parseInt(value));
    });
    $("#imageWidthTxt").val(defaultValueWidth);
    $("#imageWidthTxt").change(function () {
        var value = this.value;
        console.log(value);
        $("#sliderWidth").slider("value", parseInt(value));
        handle1.text(parseInt(value));
    });
     


});

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

function deleteItemsSuccess(data) {

    data.forEach(function (entry) {
        var pp = $('[gridkey-id=' + entry + ']');
        pp.parent().parent().remove();
    });

    refresh();
}
function changeStateSuccess(data) {
    //var parsedPostData = jQuery.parseJSON(data);
    console.log(data);
    data.values.forEach(function (entry) {
        if (entry.IsActive) {
            $('span[name=span' + data.checkbox + ']').filter('[gridkey-id="' + entry.Id + '"]').attr('style', 'color:green;  font-size:2em;').attr('class', 'glyphicon  glyphicon-ok-circle');
        } else {
            $('span[name=span' + data.checkbox + ']').filter('[gridkey-id="' + entry.Id + '"]').attr('style', 'color:red;  font-size:2em;').attr('class', 'glyphicon  glyphicon-remove-circle');
        }


    });
}
function refresh() {

    setTimeout(function () {
        location.reload()
    }, 100);
}
function changeOrderingSuccess(data) {
    console.log(data);
    refresh();
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
            console.log("auto complate");
            var items = new Array();
            

            var jsonRequest = JSON.stringify({ "term": request.term, "action": $("#action").val(), "controller": $("#controller").val() });
            console.log(jsonRequest);
            if (request.term.length > 2) {
                ajaxMethodCall(jsonRequest, "/Ajax/SearchAutoComplete", function (data) {
                    for (var i = 0; i < data.length ; i++) {
                        items[i] = { text: data[i], value: data[i] };
                    }
                    console.log(items);
                    response(sortInputFirst(request.term, items));
                });

            }

        },
        select: function (event, ui) {
            $("#SearchButton").click();

        }
    });

}