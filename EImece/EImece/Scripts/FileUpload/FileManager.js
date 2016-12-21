/*
    4-2015 
    Trimmed down version of the original File Manager to exclude Handlebars support
    Other than all original functionality remains in tact
*/


var lib_FileManager = (function () {

    var _managers = [];

    //target is a jquery object
    function FileManager(target, options) {
        this._managerId = _managers.length;
        this.baseId = options.baseId || -1;
        this.files = [];
        this.managerSelector = "[data-filelist='" + (this._managerId) + "']";  //using an internal data id system for each manager
        this.loaderClass = options.loaderClass || '';
        this.useLoader = (typeof options.useLoader !== 'undefined') ? options.useLoader : false;

        //add, remove, reorder definitions
        if (options.hasOwnProperty("add")) {
            this.addConfig = {
                urlAdd: options.add.urlAdd || '',
                onSuccess: options.add.onSuccess || _baseOnSuccess,
                onError: options.add.onError || _baseOnError,
                append: (typeof options.add.append !== 'undefined') ? options.add.append : true  //should the add function append to the list or prepend to the list 
            };
        }
        if (options.hasOwnProperty("remove")) {
            this.removeConfig = {
                urlRemove: options.remove.urlRemove || '',
                onSuccess: options.remove.onSuccess || _baseOnSuccess,
                onError: options.remove.onError || _baseOnError
            };
        }
        if (options.hasOwnProperty("reorder")) {
            this.reorderConfig = {
                urlReorder: options.reorder.urlReorder || '',
                onSuccess: options.reorder.onSuccess || _baseOnSuccess,
                onError: options.reorder.onError || _baseOnError
            };
        }

        /*
        if (options.hasOwnProperty("model")) {
            var tempmodel = options.model;

            //set the printformat handler if it isn't custom
            for (var dex = 0; dex < tempmodel.length; dex++) {
                if (typeof tempmodel[dex].printformat == 'undefined') {
                    tempmodel[dex].printformat = _basePrintFormat;
                }
            }

            this.model = tempmodel;
        }*/


        options.onStart = options.onStart || function () { };
        var onStartSuccess = options.onStart;

        //bind the managerid to the already rendered starter
        $(target).attr("data-filelist-orig", (this._managerId));
        $(target).find("[data-filelist]").first().attr('data-filelist', this._managerId);

        //are we preloading data
        //preload data model before rendering the list
        //could probably offload this to it's own fnction so the constructor isn't too bloated
        if (options.hasOwnProperty("preload")) {
            var preloadmethod = options.preload.method || "local"; //default local

            //prep for ajax retrieval
            var truethis = this;
            $.ajax({
                url: options.preload.url || '',
                type: 'GET',
                dataType: 'JSON',
                async: true,
                success: function (data) { //data array retrieved
                    //we just append the data to the already rendered starter
                    for (var predex = 0; predex < data.length; predex++) {
                        //$(truethis.managerSelector).append(data[dex]);
                        if ($(truethis.managerSelector)[0].tagName == 'TABLE') {
                            $(truethis.managerSelector).children("tbody").append(data[predex]);
                        }
                        else {
                            $(truethis.managerSelector).append(data[predex]);
                        }
                    }

                    onStartSuccess(truethis);
                    _hideManager(false, truethis);
                        
                },
                error: function (requestE, status, error) {
                    //don't know what to do just yet, still pondering the flow
                },
            });  //end of ajax
            
        }
        else {
            //no model to speak of so no preload or it's been done locally
            $("[data-filelist-orig='" + this._managerId + "']").after('<div data-filelist-loader="' + this._managerId + '"></div>');
            _hideManager(false, this);
            onStartSuccess(this);
            //non handlebars should already have the starter rendered
        }

        _managers.push(this);
    }
    FileManager.prototype.add = function (data, uniquecallback) {  //data is the data model presumably
        _hideManager(true, this);

        this.files.push(data);

        //var onSuccess = this.addConfig.onSuccess;
        //var onError = this.addConfig.onError;    

        data.baseid = this.baseId;

        if (this.addConfig.urlAdd.length > 0) {
            var params = data;
            var truethis = this;

            $.ajax({
                url: this.addConfig.urlAdd,
                type: 'POST',
                data: params,
                dataType: "JSON",
                async: true,
                success: function (data) {
                    //note that because we allow for custom container definition, that we have to be smart
                    //about where the eventual markup gets appended to.  Example, a table will always append
                    //data into the tbody tag, not the table tag while divs/sections/lists will append
                    //to the direct container, no plans for custom handlers there...yet, that's a ways off
                    _addMarkupToList(data, truethis, truethis.addConfig.append)

                    truethis.addConfig.onSuccess(data, "FileManager_Add");

                    if (typeof uniquecallback !== 'undefined') {
                        uniquecallback(data);
                    }

                    _hideManager(false, truethis);
                },
                error: function (requestE, status, error) {
                    truethis.addConfig.onError(requestE, status, error, "FileManager_Add");
                    _hideManager(false, truethis);
                },
            });
        }
        else {
            //local add only
            //have to integrate the url if it's defined
            try {
                this.addlocal(data, this.addConfig.append, uniquecallback);
                this.addConfig.onSuccess(data, "FileManager_Add");
            }
            catch (err) {
                this.addConfig.onError("", "", err, "FileManager_Add");
            }

            _hideManager(false, this);
        }
    };

    /*
        Add locally only, don't trigger an ajax call to add the items to the db
        Will accept a single model or a collection of models to add to the list
        doappend is a bool that determines if the elements should be appended or prepended to the list
    */
    FileManager.prototype.addlocal = function (data, doappend, uniquecallback) {
        _hideManager(true, this);

        try {
            if (Array.isArray(data)) {
                for (var dex = 0; dex < data.length; dex++) {
                    _localAdd(data[dex], this, doappend);
                }
            }
            else {
                _localAdd(data, this, doappend);
            }

            if (typeof uniquecallback !== 'undefined') {
                uniquecallback(data);
            }
        }
        catch (err) { }

        _hideManager(false, this);
    };

    FileManager.prototype.placeholderAdd = function (data, doappend, uniquecallback) {
        //instead of doing a normal add, we instead render a placeholder item,
        //marking it with a temporary identifer specified by the data attribute
        //when we are ready to actually populate the real values, the user will
        //call the replace function to replace the placeholder with the proper markup
        //render
        if ($(this.managerSelector)[0].tagName == 'TABLE') {
            if (doappend) {
                $(this.managerSelector).children("tbody").append(data.markup);
            }
            else {
                $(this.managerSelector).children("tbody").prepend(data.markup);
            }
        }
        else {
            if (doappend) {
                $(this.managerSelector).append(data.markup);
            }
            else {
                $(this.managerSelector).prepend(data.markup);
            }
        }

        if (typeof uniquecallback !== 'undefined') {
            uniquecallback();
        }

        //data should contain the markup to insert
        //this means that the caller must already have rendered out the markup
    };
    FileManager.prototype.placeholderError = function (data, uniquecallback) {
        var truethis = this;
        $(this.managerSelector).find("[data-placeholder='" + data.placeholderId + "']").replaceWith(data.markup);
        $(this.managerSelector).find("[data-placeholder='" + data.placeholderId + "'] [queueupload-btn-remove]").on("click", function () {
            $(truethis.managerSelector).find("[data-placeholder='" + data.placeholderId + "']").fadeOut("slow", function () {
                $(this).remove();
            });
        });

        if (typeof uniquecallback !== 'undefined') {
            uniquecallback();
        }
    };
    FileManager.prototype.placeholderReplace = function (data, uniquecallback) {
        //essentially an add call but instead of appending/prepending the render, replace it
        //from the placeholder
        //the data must include the model to use or markup to append (markup = model as far as values are concerned)
        //as well as the placeholderid to search and replace
        var markup = "";
        if (this.engine == 'handlebars') {
            if (typeof this.model != 'undefined') {
                //check the manager.model  for any handling we have to do
                for (var dex = 0; dex < this.model.length; dex++) {
                    if (this.model[dex].type == 'datetime') {
                        data.model = _datetimeHandler(data.model, this.model[dex].name);
                    }
                    if (this.model[dex].type == 'bool') {
                        data.model = _boolHandler(data.model, this.model[dex].name);
                    }

                    data.model[this.model[dex].name] = this.model[dex].printformat(data.model[this.model[dex].name]);
                }
            }

            var templatedata = {
                model: data.model
            };

            markup = this.addConfig.templateAdd(templatedata);
        }
        else {
            markup = data.model;
        }

        $(this.managerSelector).find("[data-placeholder='" + data.placeholderId + "']").replaceWith(markup);
        this.addConfig.onSuccess(data, "FileManager_Add");

        if (typeof uniquecallback !== 'undefined') {
            uniquecallback(data);
        }
    };

    /* 
        //Remove Function - Remove a file from the list
        caller: event object
    */
    FileManager.prototype.remove = function (caller, uniquecallback) {
        //$(this.managerSelector).hide();
        //$("[data-filelist-loader='" + this._managerId + "']").addClass(this.loaderClass);
        _hideManager(true, this);

        var fileid = $(caller).closest("[data-file]").attr('data-file');

        //local event callbacks

        if (this.removeConfig.urlRemove.length > 0) {
            var params = { targetid: fileid, baseid: this.baseId };
            var truethis = this;

            $.ajax({
                url: this.removeConfig.urlRemove,
                type: 'POST',
                data: params,
                dataType: 'JSON',
                async: true,
                success: function (data) {
                    //remove the file from the list
                    var item = $(caller).closest("[data-file]");
                    $(item).fadeOut("slow", function () {
                        $(this).remove();
                    });
                    truethis.removeConfig.onSuccess(data, "FileManager_Remove");

                    if (typeof uniquecallback !== 'undefined') {
                        uniquecallback(data);
                    }

                    _hideManager(false, truethis);
                },
                error: function (requestE, status, error) {
                    truethis.removeConfig.onError(requestE, status, error, "FileManager_Remove");
                    //$(truethis.managerSelector).show();
                    _hideManager(false, truethis);
                },
            });
        }//end of if urlRemove is not blank
        else {
            try {
                //just remove the element in question, it's just a local removal
                var item = $(caller).closest("[data-file]");
                $(item).hide(600, "swing", function () {
                    $(item).remove();
                });

                if (typeof uniquecallback !== 'undefined') {
                    uniquecallback();
                }

                //removed, now do the callback
                this.removeConfig.onSuccess(undefined, "FileManager_Remove");
            }
            catch (err) {
                this.removeConfig.onError("", "", err, "FileManager_Remove");
            }

            //$("[data-filelist-loader='" + this._managerId + "']").removeClass(this.loaderClass);
            //$(this.managerSelector).show();
        }
    };//end of prototype function remove

    /*
        Update Function - Update an existing entry with a new version of the model
        data - the updated model to use or the markup to replace with
        id - the file identifier value (the value of the model attribute used in the add template to identify the entry
    */
    FileManager.prototype.update = function (data, id, uniquecallback) {
        $("[data-file='" + id + "']").replaceWith(data); //replace the entry with new markup

        if (typeof uniquecallback !== 'undefined') {
            uniquecallback(data);
        }
    };

    /* 
        //Reorder Function - Change the order of Files by a position of 1
        direction: direction string (up or down), 
        event: event caller
    */
    FileManager.prototype.reorder = function (direction, caller, uniquecallback) {
        direction = direction || 'down';

        //trace up the parents to find the data-file attribute container, this contains the fileid
        var fileid = $(caller).closest("[data-file]").attr('data-file');
        //$("[data-filelist-loader='" + this._managerId + "']").addClass(this.loaderClass);
        var movingUp = (direction == 'up') ? true : false;
        var currentfile = $(caller).closest("[data-file]");
        //non context bound copies

        if (this.reorderConfig.urlReorder.length > 0) {
            //if the caller doesn't have the disabled class then start the actual work
            if (!$(caller).hasClass('btnDisabled')) {
                _hideManager(true, this); //hide the manager ui until the process is finished

                var params = { baseid: this.baseId, targetid: fileid, movingUp: movingUp };
                var truethis = this;

                $.ajax({
                    url: this.reorderConfig.urlReorder,
                    data: params,
                    type: 'POST',
                    dataType: 'JSON',
                    async: true,
                    success: function (data) {
                        //do the swap here
                        if (movingUp) {
                            //swap with previous sibling
                            currentfile.insertBefore(currentfile.prev());
                        }
                        else {
                            //swap with next sibling
                            currentfile.insertAfter(currentfile.next());
                        }

                        truethis.reorderConfig.onSuccess(data, "FileManager_Reorder");

                        if (typeof uniquecallback !== 'undefined') {
                            uniquecallback();
                        }

                        _hideManager(false, truethis);
                    },
                    error: function (requestE, status, error) {
                        truethis.reorderConfig.onError(requestE, status, error, "FileManager_Reorder");
                        _hideManager(false, truethis);
                    },
                }); //end of ajax

            } //end of if event has btnDisabled class
        }
        else {
            //just a local order change for the sake of aesthetics
            try {
                _hideManager(true, this); //hide the manager ui until the process is finished

                if (movingUp) {
                    //swap with previous sibling
                    currentfile.insertBefore(currentfile.prev());
                }
                else {
                    //swap with next sibling
                    currentfile.insertAfter(currentfile.next());
                }

                if (typeof uniquecallback !== 'undefined') {
                    uniquecallback();
                }

                this.reorderConfig.onSuccess(undefined, "FileManager_Reorder");
            }
            catch (err) {
                this.reorderConfig.onError("", "", err, "FileManager_Reorder");
            }



            _hideManager(false, this);
        }
    };  //end of prototype function reorder



    function _hideManager(hidestatus, manager) {
        if (manager.useLoader === true) {
            if (hidestatus === true) {
                //hide
                $(manager.managerSelector).hide();
                $("[data-filelist-loader='" + manager._managerId + "']").addClass(manager.loaderClass);
            }
            else {
                //show
                $(manager.managerSelector).show();
                $("[data-filelist-loader='" + manager._managerId + "']").removeClass(manager.loaderClass);
            }
        }
    }

    function _localAdd(newinput, manager, doappend) {
        try {
            //add a file to the list
            _addMarkupToList(newinput, manager, doappend);
        }
        catch (err) {
            console.log(err);
        }
    }


    function _addMarkupToList(data, manager, doappend) {
        //can get the tagtype by doing a selector $().tagName which is in all caps when returned
        if ($(manager.managerSelector)[0].tagName == 'TABLE') {
            if (doappend) {
                $(manager.managerSelector).children("tbody").append(data);
            }
            else {
                $(manager.managerSelector).children("tbody").prepend(data);
            }
        }
        else {
            if (doappend) {
                $(manager.managerSelector).append(data);
            }
            else {
                $(manager.managerSelector).prepend(data);
            }
        }
    }

    function disableAllOrderButtons(target) {
        $(target + " a.btnListOrder").each(function () {
            if (!this.hasClass("btnDisabled")) {
                $(this).addClass("btnDisabled");
            }
        });
    }

    function resetListOrderButtons(target) {
        $(target + " a.btnListOrder").each(function () {
            $(this).removeClass("btnDisabled");
        });

        $(target + " a.btnListOrder").first().addClass("btnDisabled");
        $(target + " a.btnListOrder").last().addClass("btnDisabled");
    }


    function _basePrintFormat(value) {
        return value;
    }

    function _baseOnSuccess(data, eventname) {
        //do nothing for now, console logs are cumbersome for this case
        //console.log("Custom success event callback not defined for " + eventname);
    }
    function _baseOnError(requestE, status, error, eventname) {
        //console.log("Custom error event callback not defined for " + eventname + ".  Using base callback.");
        console.log(eventname + ":Error: " + error);
    }


    return {
        init: function (target, options) {
            var managerscreated = [];

            $(target).each(function () {
                var manager = new FileManager(this, options);
                managerscreated.push(manager);
            });

            return managerscreated;
        },
        get: {
            manager: function (target) {
                var managersretrieved = [];
                $(target + "[data-filelist-orig]").each(function () {
                    var dex = $(this).attr("data-filelist-orig");
                    managersretrieved.push(_managers[dex]);
                })

                return managersretrieved;
            },
        },
        update: {
            order: function (direction, caller) {
                direction = direction || 'down';
                //trace up the parents to find the closest data-filelist attribute
                var managerid = $(caller).closest("[data-filelist]").attr("data-filelist");
                _managers[managerid].reorder(direction, caller);
            },
            remove: function (caller) {
                var managerid = $(caller).closest("[data-filelist]").attr("data-filelist");
                _managers[managerid].remove(caller);
            }
        }

    };

}());

