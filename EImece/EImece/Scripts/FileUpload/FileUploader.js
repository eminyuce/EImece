var lib_FileUploader = (function(){

    var storedObj = [];

    function init(target, options) {
        var item = new FileUploader(target, options);

        //execute other initialization procedures (adding markup to the target, event binding, etc etc)
        generateMarkup(item);

        //store it in the storedObj assoc array object using the target as the key
        storedObj[target] = item;

        return item;
    }

    function FileUploader(target, options) {
        this.queueFile = [];
        this.queueUrl = [];

        this.actionUrl = options.actionUrl || "";
        if (this.actionUrl.trim() == "") {
            throw "Must specify an action url";
        }
        this.useDragDrop = options.useDragDrop || true;

        this.onUploadSuccess = options.onUploadSuccess || function (data) { };
        this.onUploadError = options.onUploadError || function (data) { };

        this.loaderClass = options.loaderClass || "sonic-loader";

        this.target = target || "";
        if (this.target == "") {
            throw "Must specify a selector target";
        }
    }
    FileUploader.prototype.allowDrop = function (e) {
        e.preventDefault();
    };
    FileUploader.prototype.dragDropped = function (e) {
        e.preventDefault();

        // fetch FileList object
        var files = e.target.files || e.dataTransfer.files;

        var request = {
            caller: this
        };

        // handle all File objects (for now we will only handle the first file out of simplicity, multiples will involve queue work)
        if (files.length > 0) {
            request.isUrl = false;
            request.data = files[0];
        }
        else {
            request.isUrl = true;
            request.data = e.dataTransfer.getData("text");
        }

        var result = initializeUpload(request);
    };
    FileUploader.prototype.manualSubmit = function (type, options) {
        //manual file/url submission
        var data = null;
        //depending on the type, grab the different input types from the manual upload area of the container
        //then construct the calls necessary to make the manual uploads happen
        if (type == "file") {
            //console.log($(this.target).find("input[name='file']")[0].files);
            data = $(this.target).find("input[name='file']")[0].files[0];
        }
        else if(type == 'url'){
            //console.log($(this.target).find("input[name='url']").val());
            data = $(this.target).find("input[name='url']").val();
            if (data.trim() == '') {
                data = null;
            }
        }

        if (data != null) {
            var request = {
                caller: this,
                isUrl: (type == 'url') ? true : false,
                data: data
            };

            initializeUpload(request);
        }
        else {
            throw ("No valid File/URL Specified");
        }
    }

    function initializeUpload(options) {
        //using a base xmlhttprequest is just easier than going through a framework/library
        var xhr = new XMLHttpRequest();

        //check browser for upload compatibility for XMLhttpRequest object
        if (xhr.upload) {
            $(options.caller.target).find(".FileUploader-form, .FileUploader-dragdrop-container").css("opacity", 0);
            $(options.caller.target).find(".FileUploader-loader").addClass(options.caller.loaderClass);
            //FileUploader-dv-container
            //FileUploader-dragdrop-container



            // file received/failed event binder
            xhr.onreadystatechange = function (e) {
                if (xhr.readyState == 4) {  //finished process and got response
                    try{
                        var response = JSON.parse(xhr.response);  //response should be in json format to begin with
                    }
                    catch (err) {
                        options.caller.onUploadError("File Data Not Found");
                        $(options.caller.target).find(".FileUploader-form, .FileUploader-dragdrop-container").css("opacity", 1);
                        $(options.caller.target).find(".FileUploader-loader").removeClass(options.caller.loaderClass);
                    }
                    //leave room for additional attribute binding for the event response but define data to be the original api response
                    var data = {
                        data: response
                    };

                    $(options.caller.target).find("input[name='url']").val("");
                    $(options.caller.target).find("input[name='file']").val("");

                    if (xhr.status == 200) { //success OK
                        options.caller.onUploadSuccess(data);
                    }
                    else {
                        options.caller.onUploadError(data);
                    }

                    $(options.caller.target).find(".FileUploader-form, .FileUploader-dragdrop-container").css("opacity", 1);
                    $(options.caller.target).find(".FileUploader-loader").removeClass(options.caller.loaderClass);
                }//end of xhr readystate 4
            };//end of onreadystatechange event binder

            // start upload
            xhr.open("POST", options.caller.actionUrl, true);

            //take advantage of the FormData API which handles appending of multi-part data into a singular form request object
            var formData = new FormData();
            if (options.isUrl) {
                formData.append('url', options.data);
            }
            else {
                formData.append('file', options.data);
            }

            //post the request data
            xhr.send(formData);
        }//end of upload compatibility check

    }//end of initializeUpload function

    function generateMarkup(caller) {
        $(caller.target).empty();
        
        $(caller.target).append('<div class="FileUploader-dv-container">' +
                                '<div class="FileUploader-loader"></div>' +
                                '<form action="" method="post" enctype="multipart/form-data" class="FileUploader-form">' +
                                '<span>Submit a File: </span>' +
                                '<input type="file" name="file" class="FileUploader-input-file"/>'+
                                '<a class="FileUploader-inputbtn-file k-button" onclick="lib_FileUploader.manualSubmit(\'file\', \'' + caller.target + '\')">Add File</a><br/>' +
                                '<span>Submit a URL: </span>' +
                                '<input type="text" name="url" class="FileUploader-input-url" placeholder="Type URL Here"/>' +
                                '<a class="FileUploader-inputbtn-url k-button" onclick="lib_FileUploader.manualSubmit(\'url\', \'' + caller.target + '\')">Add URL</a>');

        if (caller.useDragDrop) {
            //include drag/drop markup and event bindings
            $(caller.target).append('<div class="FileUploader-dragdrop-container" '+
                                  'ondrop="lib_FileUploader.eventDropped(event, \'' + caller.target + '\')" ' +
                                  'ondragover="lib_FileUploader.allowDrop(event)">' +
                                  '<span class="FileUploader-dragdrop-message">Drag and Drop File Here</span></div>');
        }

        $(caller.target).append('</form></div>');
    }


    return{
        allowDrop: function (e) {
            e.preventDefault();
        },
        eventDropped: function (e, calltarget) {
            storedObj[calltarget].dragDropped(e);
        },
        manualSubmit:function(type, calltarget){
            storedObj[calltarget].manualSubmit(type);
        },
        init: function (target, options) {
            return init(target, options);
        }
    };


}());
