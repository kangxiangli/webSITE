
(function ($) {
    $.showpopover = $.showpopover || { version: 1.0 };
})(jQuery);

(function ($) {

    //异步加载
    $.showpopover.ajaxLoading = function (options, wrapper) {
        var img = $("<div style=\"background-image:url('/content/images/loader_bg.png');padding:3px;\"><img id=\"progressImgage\"  src=\"/content/images/ajax_loader.gif\" /></div>"); //Loading小图标
        var mask = $("<div id=\"maskOfProgressImage\"></div>").addClass("mask").addClass("hide");
        var PositionStyle = "fixed";
        if (wrapper != null && wrapper != "" && wrapper != undefined) {
            $(wrapper).css("position", "relative").append(img).append(mask);
            PositionStyle = "absolute";
        }
        else {
            $("body").append(img).append(mask);
        }
        img.css({
            "z-index": "2000",
            "display": "none"
        })
        mask.css({
            "position": PositionStyle,
            "top": "0",
            "right": "0",
            "bottom": "0",
            "left": "0",
            "z-index": "1000",
            "background-color": "#000000",
            "display": "none"
        });
        var complete = options.complete;
        options.complete = function (httpRequest, status) {
            img.hide();
            mask.hide();
            if (complete) {
                complete(httpRequest, status);
            }
        };
        options.error = function (XMLHttpRequest, textStatus, errorThrown) {
            $.whiskey.web.alert({
                type: "danger",
                content: "服务器请求发生错误：" + errorThrown + "，错误代码：" + XMLHttpRequest.status,
                callback: function () {
                }
            });
        };
        options.async = true;
        img.show().css({
            "position": PositionStyle,
            "top": "40%",
            "left": "50%",
            "margin-top": function () { return -1 * img.height() / 2; },
            "margin-left": function () { return -1 * img.width() / 2; }
        });
        mask.show().css("opacity", "0.1");
        $.ajax(options);

    };

    $.showpopover.web = {
        //对话框
        ajaxDialog: function (options) {
            var _id = guid();
            $.showpopover.ajaxLoading({
                // cache: false,
                type: "get",
                url: options.actionUrl,// + "?r=" + Math.random()
                data: options.getParams,
                beforeSend: function () {
                    $(options.lockButton).attr("disabled", "disabled");
                    if (typeof (options.beforeSend) == "function") {
                        options.beforeSend();
                    }

                },
                complete: function (result) {
                    $(options.lockButton).removeAttr("disabled");
                },
                success: function (data) {
                    var formHeader = "<form id='" + _id + "' 0fashion='fashion-team' class=\"modal-form form-horizontal dropzone\" action=\"" + options.actionUrl + "\" enctype=\"multipart\/form-data\">";
                    var formBody = data;
                    var formFooter = "</form>";
                    var mesg = formHeader + formBody + formFooter;
                    if (options.noneheader)
                        mesg = formBody;
                    if (typeof (options.getComplete) == "function") {
                        options.getComplete();
                    }
                    var succe_but_tit = options.successTit;
                    if (succe_but_tit == undefined) {
                        succe_but_tit = "提交";
                    }
                    var succe_but_event = options.successEvent;
                    if (typeof (succe_but_event) != "function") {
                        succe_but_event = success_event;
                    }

                    options.formModel = bootbox.dialog({
                        message: mesg,
                        // message:formBody,
                        title: options.caption,
                        buttons: {
                            success: {
                                label: succe_but_tit,
                                icon: "fa-check",
                                className: "btn-primary",
                                callback: function () {
                                    return succe_but_event(options);
                                }
                            },
                            cancel: {
                                label: "关闭",
                                icon: "fa-close",
                                className: "btn-default",
                                callback: function () {
                                    if (typeof (options.closeEvent) == "function") {
                                        options.closeEvent();
                                    }
                                }
                            }
                        }
                    });
                }
            });
        },

    };

})(jQuery);

function success_event(options) {
    $.whiskey.web.formVerify(".modal-form");
    if (typeof (options.formValidator) == "function") {
        if (!options.formValidator()) {
            return false;
        }
    }
    $.whiskey.ajaxLoading({
        type: "POST",
        url: options.actionUrl,
        data: $(".modal-form").serialize() + "&" + JSON.stringify(options.postParams),
        beforeSend: function () {
            $(".modal-footer .btn-primary").attr("disabled", "disabled");
        },
        complete: function (data) {
            $(".modal-footer .btn-primary").removeAttr("disabled");
        },
        success: function (data) {
            if (typeof (data) == 'object') {
                if (data.ResultType == 3) {
                    if (typeof (options.postComplete) == "function") {
                        if (options.postComplete(data)) {
                            options.formModel.modal('hide');
                        }
                    } else {
                        options.formModel.modal('hide');
                    }
                    if (options.showCompletePrompt) {
                        $.whiskey.web.alert({
                            type: "success",
                            content: data.Message,
                            callback: function () {
                                if (options.returnUrl != undefined) {
                                    window.location = options.returnUrl;
                                } else {
                                    bootbox.hideAll();
                                }
                            }
                        });

                    }

                    return true;
                } else {
                    $.whiskey.web.alert({
                        type: "danger",
                        content: data.Message,
                        callback: function () {
                        }
                    });
                }
            }
            else {
                $.whiskey.web.alert({
                    type: "info",
                    content: data,
                    callback: function () {
                    }
                });
            }
        }
    });
    return false;
}

