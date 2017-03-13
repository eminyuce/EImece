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
        } else if (!searchReg.test(searchVal)) {
            $("#errorMessage").text($("#SearchValidText").val());
            // $("#Search_TextBox").after('<span class="error">' + $("#SearchValidText").val() + '</span>');
            hasError = true;
        }
        if (hasError == true) { return false; }
    });
});