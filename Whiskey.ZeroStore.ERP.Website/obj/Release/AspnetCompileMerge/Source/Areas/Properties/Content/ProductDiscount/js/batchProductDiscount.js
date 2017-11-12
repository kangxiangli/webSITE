
var bat_hashlist = new $.whiskey.hashtable();
$(document).ready(function () {
    $.whiskey.datatable.instance = $(".addprod_li").dataTable(
    {
        "aaSorting": [[0, 'desc']],
        "sDom": 't<"clearfix datatable-footer"<"col-md-3"><"col-md-7 text-right">>',
        "fnDrawCallback": function () {
            $('.edit-text').editable({
                type: "text",
                pk: "",
                tpl: "<input type='text' class='text-center' style='width: 80px'>",
                ajaxOptions: {
                    //dataType: 'json'

                },
                //url: "@Url.Action("SetAmount")",
                validate: function (value) {
                    if ($.trim(value) == '') return '提示：商品数量不能为空！';
                },
                title: "请输入新的商品数量",
                success: function (data, newValue) {
                    if (data.ResultType == 3) {
                        $(".scan-valid-count").text(data.Data.validCount);
                        //$.whiskey.datatable.reset(true);
                    }
                },
                error: function (errors) {
                }
            });
            $(".checked-all").click(function () {
                var checkedStatus = this.checked;
                $(".table tr td input[type=checkbox]").each(function () {
                    this.checked = checkedStatus;
                });

            });
            $(".table tr td input[type=checkbox]").checked = true;
        },
        "aoColumns": [
            {
                "sTitle": $.whiskey.datatable.tplTitleCheckbox(),
                "bSortable": false,
                "bSearchable": false,
                "sName": "Id",
                "mData": "Id"
            },
            {
                "sTitle": "<span style='color: #5ebd5e'>款号</span>/货号",
                "bSortable": false,
                "bSearchable": false,
                "sName": "ProductNumber",

            },
            {
                "sTitle": "品牌",
                "bSortable": false,
                "bSearchable": false,
                "sName": "Brand",

            },
            {
                "sTitle": "产品分类",
                "bSortable": false,
                "bSearchable": false,
                "sName": "Category",
            },
            {
                "sTitle": "尺寸",
                "bSortable": false,
                "bSearchable": false,
                "sName": "Size",

            },
            {
                "sTitle": "图片",
                "sName": "Thumbnail",
                "bSortable": false,
                "bSearchable": false,
            }
            ,

            //{
            //    "sTitle": "数量",
            //    "sName": "Thumbnail",
            //    "bSortable": false,
            //    "bSearchable": false,

            //},
            {
                "sTitle": "操作",
                "bSortable": false,
                "bSearchable": false,
                "mdata": function (data) {

                    return $.whiskey.datatable.tpldelete(data.id);
                }
            }
        ]

    });
    $("#RemoveAll").on("click", function () {
        var list = $.whiskey.web.getIdByChecked(".table-list td input[type=checkbox]");
        if (list.length > 0) {
            var confirm = new $.whiskey.web.ajaxConfirm({
                question: "确认要移除这些商品吗？",
                notes: "提示：此操作会将商品从服务器缓存中移除",
                // actionUrl: "@Url.Action("Remove")",
                success_event: function () {
                    var chelist = $(".table-list .te_1_che:checked");
                    deleCurPage();
                    chelist.each(function () {
                        $(this).parents("tr").remove();
                        //123
                    });
                },
                params: list,
                complete: function () {
                    //$.whiskey.datatable.reset(true);

                }
            });
        } else {
            $.whiskey.web.alert({
                type: "info",
                content: "请至少选择一条数据！",
                callback: function () {
                }
            });
        }
    });

    $(".scan-number").blur(function () {
        $(this).addClass("input-validation-error");
    }).focus(function () {
        $(this).removeClass("input-validation-error");
    });

    $(".scan-number").keyup(function (event) {
        if (event.keyCode == 13) {
            if (!intervalState()) return false;
            var scanNumber = $(".scan-number").val();
            if (scanNumber.trim() != "")
                EnterQue(scanNumber);
        }
    });

    $("#sear-ok").click(function () {
        var scanNumber = $(".scan-number").val();
        if (scanNumber.trim() != "")
            EnterQue(scanNumber);
    });

    setIntervalSend();

    $(window).on('beforeunload', function (e) {
        if (bat_hashlist.size() > 0) {
            var message = "系统检测到队列中有（" + bat_hashlist.size() + "条数据）还未提交到服务器，刷新或关闭浏览器会导致这些数据丢失！！";
            e.returnValue = message;
            return message;
        }
        //如果表中的数据还未入库，给出提示：
        var issave = $(".batchproduct #isSave").val();
        if (issave == 1) {
            var mes = "下表中数据尚未保存到折扣方案，关闭浏览器会导致数据丢失！";
            e.returnValue = mes;
            return mes;
        }
    }).on("unload", function () {
        $(".batchproduct #Create").attr("disabled", "disabled");
        $(".batchproduct #isSave").val(0);
        $.post("/Properties/ProductDiscount/UnloadCurPage", {}, function () { });
    });

    // getStoreInf();
    // loadStoreData();
    registChange();
    $("body").delegate(".scan-invalid", "click", function () {
        if ($(".scan-invalid-count").html() == "0") {
            $.whiskey.web.alert({
                type: "info",
                content: "无效数量为0！",
                callback: function () {
                }
            });
            return false;
        }
        $.whiskey.tools.other("0");
        showValid(1);
        return false;
    });
    //查看无效列表
    $(".scan-invalid").on("click", function () {
        if ($(".scan-invalid-count").html() == "0") {
            $.whiskey.web.alert({
                type: "info",
                content: "无效数量为0！",
                callback: function () {
                }
            });
            return false;
        }
        $.whiskey.tools.other("0");
        showValid(1);
        return false;
    });

    //查看有效列表
    $("body").delegate(".scan-valid", "click", function () {
        if ($(".scan-valid-count").html() == "0") {
            $.whiskey.web.alert({
                type: "info",
                content: "有效数量为0！",
                callback: function () {
                }
            });
            return false;
        }
        $.whiskey.tools.other("1");
        showValid(0);
        return false;
    });

    $(".scan-valid").click(function () {
        if ($(".scan-valid-count").html() == "0") {
            $.whiskey.web.alert({
                type: "info",
                content: "有效数量为0！",
                callback: function () {
                }
            });
            return false;
        }
        $.whiskey.tools.other("1");
        showValid(0);
        return false;
    });

    $("#Quantity").blur(function () {
        var regex = /^\d+$/;
        var count = $("#Quantity").val();
        if (!regex.test(count)) {
            $(this).parent("div").addClass("has-error");
        } else {
            $(this).parent("div").removeClass("has-error");
        }
    });

    $(".batchproduct #Create").click(function () {
        var url = "/Properties/ProductDiscount/BathcProductCreate";
        var saleDiscount = $(".batchproduct #saleDiscount").val().trim();
        var wholesaleDiscount = $(".batchproduct #wholesaleDiscount").val().trim();
        var purchaseDiscount = $(".batchproduct #purchaseDiscount").val().trim();
        var discountName = $(".batchproduct #DiscountName").val().trim();
        var notes = $(".batchproduct #Notes").val().trim();
        var discouTyp = $(".switcher_hidd").is(":checked") ? "2" : "1";

        var err = "";
        if (saleDiscount != "") {
            var dis = parseFloat(saleDiscount);
            if (dis <= 0 || dis > 10) {
                err = "零售折扣在1~10之间";
            }
        } else {
            err = "零售折扣不为空";
        }
        if (wholesaleDiscount != "") {
            var dis = parseFloat(wholesaleDiscount);
            if (dis <= 0 || dis > 10) {
                err = "批发折扣在1~10之间";
            }
        } else {
            err = "批发折扣不为空";
        }
        if (purchaseDiscount != "") {
            var dis = parseFloat(purchaseDiscount);
            if (dis <= 0 || dis > 10) {
                err = "采购折扣在1~10之间";
            }
        } else {
            err = "采购折扣不为空";
        }
        if (discountName == "") {
            err = "折扣方案名不为空";
        }
        if (err.length > 0) {
            $.whiskey.web.alert({
                type: "info",
                content: err,
                callback: function () {
                }
            });
        } else {

            var par = { DiscountName: discountName, RetailDiscount: saleDiscount, WholesaleDiscount: wholesaleDiscount, PurchaseDiscount: purchaseDiscount, Description: notes, DiscountType: discouTyp };
            var arr = [];
            $(".batchproduct tbody tr :checkbox").each(function () {
                arr.push($(this).val());
            });
            if (arr.length > 0) {
                $.post(url, { ids: arr, dto: par }, function (dat) {
                    if (dat.ResultType == 3) {
                        $(".batchproduct #isSave").val(0);
                        $.whiskey.web.alert({
                            type: "success",
                            content: "折扣方案添加成功,可以在\"商品折扣管理\"的\"未启用\"中查看",
                            callback: function () {
                                location.href = "/Properties/ProductDiscount/Index";
                            }
                        });
                    } else {
                        $.whiskey.web.alert({
                            type: "info",
                            content: dat.Message,
                            callback: function () {

                            }
                        });
                    }
                });
            } else {
                $.whiskey.web.alert({
                    type: "info",
                    content: "请选择商品",
                    callback: function () {
                    }
                });
            }

            //$.post(url, { ids: null, dto: par }, function (da) {
            //    if (da.ResultType == 0) {
            //        //缓存数据失效

            //    }
            //});
        }
    });

    $(".vali_vad").blur(function () {

        if ($(this).val() == null || $(this).val() == "") {
            $(this).parent().addClass("has-error");
        } else {
            $(this).parent().removeClass("has-error");
        }
    });
    //在弹出层中选择商品
    $("#selec_prod_list").click(function () {
        var dialog = new $.whiskey.web.ajaxDialog({
            caption: "选择商品",
            successTit: "确定",
            noneheader: true,
            actionUrl: "/Products/Product/GetProductList",
            successEvent: function () {
                getAllCheck();
            },
            lockButton: $(this),
            formValidator: function () {
                var $form = $(".modal-form");
                if (!$form.valid()) {
                    $(".modal-dialog").parent("div").animate({ scrollTop: 20 }, 500);
                    return false;
                } else {
                    return true;
                }
            },
            postComplete: function () {
                $.whiskey.datatable.reset(false);
                return true;
            },
        });

    });

    //批量导入，
    $("#selec_prodBatch_list").click(function () {


        var dialog = new $.whiskey.web.ajaxDialog({
            caption: "批量导入",
            successTit: "确定",
            successEvent: select_check_Access,
            actionUrl: "/Products/Product/BatchImport",
            noneheader: true,
            lockButton: $(this),
            methType: "post",
            //uploadUrl: "/Warehouses/AddProduct/ExcelFileUpload",
            formValidator: function () {
                var $form = $(".modal-form");
                if (!$form.valid()) {
                    $(".modal-dialog").parent("div").animate({ scrollTop: 20 }, 500);
                    return false;
                } else {
                    return true;
                }
            },
            postComplete: function () {
                // $.whiskey.datatable.reset(false);
                return true;
            },
        });

    });

    _swith = new $('.switcher_hidd').switcher({
        //theme: 'square',
        on_state_content: "货号",
        off_state_content: "款号"
    });
    $('.switcher_hidd').on("click", function () {
        var ch = $(this).is(":checked");
        //如果存在数据没有保存，提示：
        var issave = $(".batchproduct #isSave").val();
        var trs = $(".batchproduct tbody tr").length;
        if (issave == 1 && trs > 0) {
            return new $.whiskey.web.ajaxConfirm({
                question: "确认要切换吗？",
                notes: "提示：下表中数据尚未保存，切换后数据将会清空",
                // actionUrl: "@Url.Action("Remove")",
                success_event: function () {
                    $(".batchproduct tbody tr").remove();
                    switcherMesg(ch);
                    swithcerResetCou();
                    //将显示的数量置0
                    $(".batchproduct .scan-queue-count").text(0);
                    $(".batchproduct .scan-valid-count").text(0);
                    $(".batchproduct .scan-invalid-count").text(0);
                },
                cancel_event: function () {
                    ch = !ch;
                    if (ch) {
                        $('.switcher_hidd').prop("checked", true);
                        $('.switcher_hidd').parents(".switcher:first").addClass("checked");
                    } else {
                        $('.switcher_hidd').removeAttr("checked");
                        $('.switcher_hidd').parents(".switcher:first").removeClass("checked");
                    }
                    switcherMesg(ch);
                }
            });
        } else {
            //将显示的数量置0
            swithcerResetCou();
        }
        switcherMesg(ch);
    }).click().click();
    $("#myTab li a[href$='deta_tab']").parent().click(function () {
        $(".switcher_rig").show();
    });

    $("#myTab li a[href$='bigcate_tab']").parent().click(function () {
        $(".switcher_rig").hide();
    });


});
window.onload = function () {
    $(".bootstrap-switch-container").css("hieght", '43px');
}
var invel;
function swithcerResetCou() {
    var qucou = parseInt($(".batchproduct .scan-queue-count").text());
    var valcou = parseInt($(".batchproduct .scan-valid-count").text());
    var invalcou = parseInt($(".batchproduct .scan-invalid-count").text());

    if (qucou != 0 || valcou != 0 || invalcou != 0) {
        bat_hashlist.clear();
        $(".batchproduct #isSave").val(0);
        $.post("/Properties/ProductDiscount/UnloadCurPage", {}, function () { });
    }

    $(".batchproduct .scan-queue-count").text(0);
    $(".batchproduct .scan-valid-count").text(0);
    $(".batchproduct .scan-invalid-count").text(0);
}
function switcherMesg(ische) {
    var mesg = '请输入款号';
    if (ische) {
        mesg = "请输入商品货号";
        $("#left_content .panel-warning").removeClass("panel-warning").addClass("panel-danger");
    } else {
        $("#left_content .panel-danger").removeClass("panel-danger").addClass("panel-warning");
    }
    $(".batchproduct #ScanNumber").attr("placeholder", mesg);

}
function setIntervalSend() {
    invel = setInterval(function () {
        sendToQueue();
    }, 1000);

}

function clearIntervalSend() {
    clearInterval(invel);
    invel = undefined;
}

function intervalState() {
    if (!invel) {
        $.whiskey.web.alert({
            type: "info",
            content: "没有权限！请确认登录或重新刷新页面",
            callback: function () {
            }
        });
        return false;
    }
    return true;
}

//将数据压入队列
function EnterQue(scanNumber) {
    if (scanNumber.length > 0) {
        var barcodeArr = scanNumber.split(',');
        if (barcodeArr.length <= 0) {
            return;
        }
        barcodeArr.forEach(function (value, index, arr) {
            var uuid = $.whiskey.tools.UUID(32, 16);
            hashlist.put(uuid, value);
            $(".scan-queue-count").text(hashlist.size());
            $(".scan-number").val("");
            $(".scan-number").focus();
        });

    }
}

//将队列中的数据发往服务端验证
function sendToQueue() {
    if (bat_hashlist.size() > 0) {
        //禁用 确认、删除按钮
        $("#Create").attr("disabled", "disabled");
        $("#RemoveAll").attr("disabled", "disabled");
        var globalUUID = bat_hashlist.getFirst(0);
        var scanNumber = bat_hashlist.getFirst(1);
        var isbigNumb = !$(".switcher_hidd").is(":checked");

        $.ajax({
            type: "POST",
            data: { uuid: globalUUID, number: scanNumber, isbigNumb: isbigNumb },
            async: false,
            url: "/Properties/ProductDiscount/AddToScan",
            success: function (data) {
                refrshValidDat(data);
                bat_hashlist.remove(data.Data.uuid);
                if (data.ResultType == 3) {
                    $(".batchproduct #isSave").val(1);
                    addRow_cur(data.Other, isbigNumb);


                } else {
                    // clearIntervalSend();
                    bat_hashlist.remove(globalUUID);
                    $.whiskey.web.alert({
                        type: "info",
                        content: data.Message,
                        callback: function () {

                        }
                    });
                }

            }
        });

    } else {
        $("#Create").removeAttr("disabled");
        $("#RemoveAll").removeAttr("disabled");
    }
}


function refrshValidDat(data) {
    $(".scan-queue-count").animate({
        opacity: "0.3",
    }, 'slow', function () {
        $(".scan-queue-count").animate({
            opacity: "1.0",
        }, 'fast', function () {
            $(".scan-queue-count").text(bat_hashlist.size());
        });
    });

    if ($(".scan-valid-count").text() != data.Data.validCou) {
        $(".scan-valid-count").animate({
            opacity: "0.3",
        }, 'slow', function () {
            $(".scan-valid-count").animate({
                opacity: "1.0",
            }, 'fast', function () {
                $(".scan-valid-count").text(data.Data.validCou);
            });
        });
    }

    if ($(".scan-invalid-count").text() != data.Data.invalidCou) {
        $(".scan-invalid-count").animate({
            opacity: "0.3",
        }, 'slow', function () {
            $(".scan-invalid-count").animate({
                opacity: "1.0",
            }, 'fast', function () {
                $(".scan-invalid-count").text(data.Data.invalidCou);
            });
        });
    }
}

//将批量导入中选中的元素压入队列
function select_check_Access() {
    if (!intervalState()) return false;
    $(".td_lef:checked").each(function () {

        EnterQue($(this).val());
    });
}

//显示有效和无效列表
function showValid(_ind) {
    if (_ind == undefined || _ind == "")
        _ind = 0;
    var dialog = new $.whiskey.web.ajaxDialog({
        caption: "录入列表",
        successTit: "确定",
        actionUrl: "/Properties/ProductDiscount/GetValidList?actid=" + _ind,
        successEvent: function () { },
        lockButton: $(this),
        //formValidator: function () {
        //    var $form = $(".modal-form");
        //    if (!$form.valid()) {
        //        $(".modal-dialog").parent("div").animate({ scrollTop: 20 }, 500);
        //        return false;
        //    } else {
        //        return true;
        //    }
        //},
        postComplete: function () {
            //$.whiskey.datatable.reset(false);
            //return true;
        },
    });
}

//在弹出层中当勾选了复选框，单击“确定”时触发
function getAllCheck() {
    if (!intervalState()) return false;
    var checks = $(".pdl:checked");
    var nums = [];
    for (var i = 0; i < checks.length; i++) {
        // var _id = $(checks[i]).val();
        var num = $(checks[i]).parents("td").nextAll("td").eq(1).html();
        nums.push(num);
    }
    //如果已经存在就直接累加
    var ts = $(".addprod_li tbody .proCou");
    if (ts.length > 0) {
        ts.each(function () {
            var _num = $(this).parents("tr").find("td:eq(2)").text().trim();
            var inde = nums.indexOf(_num);
            if (inde > -1) {
                nums.splice(inde, 1);
                $(this).val(parseInt($(this).val()) + 1);
            }

        });
    };
    for (var i = 0; i < nums.length; i++) {
        EnterQue(nums[i]);
    }
};
//在页面加载时注册必要的事件
function registChange() {
    $("#StoreID").change(function () {
        var sto_id = $("#StoreID option:selected").val();
        var t = changeStore(sto_id);


    });
    var sto_id = $("#StoreID option:selected").val();
    var t = changeStore(sto_id);


}

//当店铺下拉菜单发生改变时触发
function store_sel_change(sender) {
    sender = "#" + sender;
    var sto_id = $(sender).children("option:selected").val();
    var storage_str = changeStore(sto_id);
    var flag = $(sender).attr("flagattr");
    var condi = ".storage_sel_str[flagattr='" + flag + "']";
    $(condi).html("").html(storage_str);
}

//动态添加一行
function addRow_cur(da, isbignum) {

    //var jsonda = $.parseJSON(da);
    var jsonda = da;
    var res = "";
    if (jsonda != undefined && jsonda != null) {
        // 是否已经存在
        var nums = $($.whiskey.datatable.instance[0]).children("tbody").find("." + jsonda.ProductNumber + "");
        if (nums.length > 0) {
            var couem = $(nums).parents("tr").find(".proCou");
            var cou = $(couem).val();
            $(couem).val(parseInt(cou) + 1);
        } else {

            res += "<tr><td>" + $.whiskey.datatable.tplListCheckbox(jsonda.Id) + "</td>";
            if (isbignum) {
                res += "<td class='" + jsonda.ProductNumber + "' style='color: #5ebd5e'>" + jsonda.ProductNumber + "</td>";
            } else {
                res += "<td class='" + jsonda.ProductNumber + "'>" + jsonda.ProductNumber + "</td>";
            }

            res += "<td>" + jsonda.Brand + "</td><td>" + jsonda.Category + "</td>";
            //<td>" + jsonda.ProductName + "</td>
            res += "<td>" + jsonda.Size + "</td>";
            //res += "</td>" + jsonda.Season + "</td>"+"<td>" + jsonda.Color + "</td>"
            res += "<td><img style='width:46px' class='img-thumbnail img-responsive' src='" + jsonda.Thumbnail + "'/></td>";

            var onlyflag = guid();
            //res += "<td style='width:10%'><input style='width:100%' disabled='disabled' class='proCou form-control' flagattr='" + onlyflag + "' type='text'  Value='1' /></td>";
            res += "<td style='width:15%'>" + deleDat() + review() + "</td>";
            res += "</tr>";
            appendToContent(res);
        }

    }
    //if (res != "")
    //    $.whiskey.tools.arraylist.add(res);

}

//强数据动态的添加到table中，如果超过了指定的条数，就不再添加，只保留到arrlist中
function appendToContent(res) {
    var showleng = $(".sel_num option:selected").val(); //每页显示的数据条数
    var rowleng = $($.whiskey.datatable.instance[0]).children("tbody").children("tr").length; //当前已经显示数据行数

    //if (rowleng < showleng) {
    //    $($.whiskey.datatable.instance[0]).children("tbody").append(res);

    //}
    //var store = $("#StoreID option:selected").val();
    $($.whiskey.datatable.instance[0]).children("tbody").append(res);
    //刷新分页
    var cur = $(".pagination .active a").attr("p");
    if (cur == undefined || cur == null)
        cur = 1;
    var t = parseInt(cur);
    refreshPageing(t);

}

//刷新分页
function refreshPageing(cur) {
    cur = parseInt(cur);
    var rowcount = $.whiskey.tools.arraylist.leng();
    var showleng = $(".sel_num option:selected").val(); //每页显示的条数
    if (rowcount > 0 && showleng > 0) {
        var pagecoun = Math.ceil(rowcount / showleng);
        var pagestr = "<ul class='pagination'><li><a p='1' class='star' href='#'>首页</a></li><li><a p=" + (cur + 1) + " href='#'>下一页</a></li>";
        if (cur + 1 > pagecoun) {
            pagestr = "<ul class='pagination'><li><a p='1' class='star' href='#'>首页</a></li><li class='disabled'><a p=" + (cur + 1) + " href='#'>下一页</a></li>";
        }
        if (pagecoun <= 10) {
            for (var i = 1; i <= pagecoun; i++) {
                if (cur == i)
                    pagestr += "<li class='active'><a p=" + i + " href='#'>" + i + "</a></li>";
                else
                    pagestr += "<li><a p=" + i + " href='#'>" + i + "</a></li>";
            }
        } else {
            var start = (cur - 5) < 1 ? 1 : (cur - 5);
            var end = (cur + 5) > pagecoun ? pagecoun : (cur + 5);
            if (start > 1) {
                pagestr += "……";
            }
            for (var i = start; i < end; i++) {
                pagestr += "<li><a p=" + i + " href='#'>" + i + "</a></li>";
            }
            if (end < pagecoun) {
                pagestr += "……";
            }
        }

        if (cur - 1 < 1) {
            pagestr += "<li class='disabled'><a p=" + (cur - 1) + " href='#'>上一页</a></li><li><a p=" + pagecoun + " class='end' href='#'>末页</a></li></ul>";
        } else {
            pagestr += "<li><a p=" + (cur - 1) + " href='#'>上一页</a></li><li><a p=" + pagecoun + " class='end' href='#'>末页</a></li></ul>";
        }
        $("#DataTables_Table_0_wrapper .dataTables_paginate").html("").html(pagestr);
    }
}

//刷新表中的数据 cur:当前显示的页码，rowcount:每页显示的行数
function refreshTableData(cur, rowcount) {
    cur = parseInt(cur);
    rowcount = parseInt(rowcount);
    if (cur == undefined || cur == null || cur == "") {
        cur = 1;
    }
    if (rowcount == undefined || cur == null || cur == "") {
        rowcount = 10;
    }
    var cou = $.whiskey.tools.arraylist.leng();
    var strInd = (cur - 1) * rowcount;
    var endInd = strInd + rowcount;
    var rows = $.whiskey.tools.arraylist.getRang(strInd, endInd);
    $($.whiskey.datatable.instance[0]).children("tbody").html("").html(rows.join());
}

function getStorage() {
    var storeid = $("#StoreID option:selected").val();
    if (storeid == null || storeid == "")
        return;
    $.post("/Storage/GetStorage", { storeId: storeid, title: "请下拉选择" }, function (da) {
        $("#StorageID").html("");
        $("#StorageID").append(getOptions(da, "该店铺下没有关联的仓库"));
    })
}

function changeStore(_id) {
    var storags = "";
    if (_id == undefined || _id == "" || _id == "-1") {
        $("#StorageID").html("").html(storags);
    } else {

        //var da = $.whiskey.tools.json();
        $.post("/Warehouses/AddProduct/GetEnableAddProductStorageById/", { id: _id }, function (da) {
            if (da.length > 0) {

                for (var j = 0; j < da.length; j++) {
                    if (da[j].Other == "1") {
                        storags += "<option selected='selected' value='" + da[j].Value + "'>" + da[j].Text + "</option>";
                    } else
                        storags += "<option value='" + da[j].Value + "'>" + da[j].Text + "</option>";
                }
                $("#StorageID").html("").html(storags);
            }
        });
    }
}

//获取当前选中行在arrlist中的索引
function getrowIndex(sender) {
    var rows = $("tbody tr[class!='bg_gray']");
    for (var i = 0; i < rows.length; i++) {
        if (rows[i] == sender) {
            var showleng = parseInt($(".sel_num option:selected").val()); //每页条数
            var pagein = parseInt($("li[class='active'] a").attr("p")); //当前为第几页
            return (pagein - 1) * showleng + i;
        }
    }
}

//批量入库 dat:[ProduId=22&StorCou=120&StoreId=12&StorageId=2&Fid=1,
//             ProduId=22&StorCou=120&StoreId=12&StorageId=2&Fid=2
//            ] ,
//删除当前页数据，并加载新数据
function deleCurPage() {
    var trs = $("tbody tr").each(function () {
        deleCurRow(this);
        var showleng = $(".sel_num option:selected").val();
        refreshTableData(1, showleng);
        refreshPageing(1);
    });
}

//删除指定行
function deleCurRow(row) {
    var rid = getrowIndex(row);
    $.whiskey.tools.arraylist.dele(rid);
    $(row).remove();
    var cou = $("tbody tr").length;
    if (cou == 0) {
        //更新表数据
        var showleng = $(".sel_num option:selected").val(); //每页显示的条数
        var cur = $(".pagination .active a").attr("p");
        refreshTableData(cur, showleng);
        refreshPageing(cur);
    }
}

//ispage :是否是整页入库
function addInventory(dat, ispage) {
    $.post("/Warehouses/Inventory/AddInventory", { da: dat.join("|") }, function (d) {
        if (d.ResultType == 3) {
            $("#modal_di").modal('hide');
            $.whiskey.web.alert({
                type: "success",
                par: { keyboard: false, backdrop: 'static' },
                content: "入库成功！",
                callback: function () {
                    if (ispage) { //整页入库时清空当前页面，加载下一页

                        deleCurPage();
                        //$($.whiskey.datatable.instance[0]).children("tbody").html("");//清空当前页

                    } else { //逐条数据入库时，将当前数据行背景置灰，整行不勾选，不可操作
                        for (var i = 0; i < dat.length; i++) {
                            //var reg = /^[a-z]+=(\d+)+&.+$/i;
                            //var t=reg.exec(dat[i]);
                            //if (t !== null) {
                            //    var productId = parseInt(t[1]);
                            //}
                            //var chec= $(":checkbox[value='" + productId + "']");
                            var chectr = $.whiskey.tools.arraylist.other();
                            $(chectr).addClass("bg_gray");
                            $(chectr).find(":button").attr("disabled", "disabled");
                            $(chectr).find("input").attr("disabled", "disabled");
                            $(chectr).find("select").attr("disabled", "disabled");
                            $(chectr).find(":checkbox").prop("checked", false).attr("disabled", "disabled");
                            var ind = getrowIndex(chectr);
                            $.whiskey.tools.arraylist.dele(ind);

                        }
                    }
                    var coulen = $.whiskey.tools.arraylist.leng();
                    if (coulen == 0) {
                        $.post("/Warehouses/AddProduct/UnloadPg", {}, function () { })
                        $.whiskey.tools.arraylist.clear(); //清空全部数据
                        location.href = "/Warehouses/Inventory/Index";
                    } else {
                        if (ispage) {
                            var showleng = $(".sel_num option:selected").val(); //每页显示的条数
                            refreshPageing(1);
                            refreshTableData(1, showleng);

                        }
                    }
                }
            });
        } else {
            $.whiskey.web.alert({
                type: "warning",
                content: "入库失败！",
                callback: function () {
                }
            });
        }
    })
}

//返回键值对数据   a
function addInventoryAcc(checkSend) {
    var detr = $(checkSend).parents("tr:eq(0)");

    var proId = $(checkSend).val(); //商品id

    var store = detr.find(".store_sel_str option:selected").val(); //商店id
    var storage = detr.find(".storage_sel_str option:selected").val(); //仓库id
    var num = detr.find(".proCou").val(); // $(det[8]).children().val(); //入库数量
    var num_n = parseInt(num);
    var descript = $("#Notes").val();
    if (num_n == null || num_n == 0 || num_n == "") {
        $.whiskey.web.alert({
            type: "warning",
            content: "入库数量不能为空！",
            callback: function () {
            }
        });
        var top = $(det[8]).offset().top;
        $(detr[8]).addClass("has-error");
        $("body").animate({ scrollTop: top / 2 }, 500);
        return null;
    } else {
        $(detr[8]).removeClass("has-error");
        return { ProduId: proId, StorCou: num, StoreId: store, StorageId: storage, Descriptions: descript };
        // {ProduId:22,StorCou:120,StoreId:12,StorageId:2}

    }
}

$(function () {
    $(".sel_num").html("<select><option value='5'>5条</option><option value='10'>10条</option><option value='15'>15条</option><option value='25'>25条</option><option value='50'>50条</option></select>")
    $("body").delegate(":button[title='删除']", "click", function () {
        deleCurRow($(this).parents("tr")[0]);
    }).delegate(".checked-all", "click", function () {
        if ($(this).is(":checked")) {

            $(".te_1_che").prop("checked", "checked");
        } else
            $(".te_1_che").prop("checked", false);

    }).delegate("input[type='number']", "blur", function () {
        var num = $(this).val();
        if (num == 0 || num == "") {
            var top = $(this).offset().top;
            var winheig = $(window).height();
            var offsethei = top - winheig;
            $(this).parent().addClass("has-error");
            $("body").animate({ scrollTop: top + offsethei }, 500);
        } else {
            $(this).parent().removeClass("has-error")
        }

    }).delegate(":button[title='预览']", "click", function () {
        var id = $(this).parents("td").prevAll("td:last").children().children(":checkbox").val();
        var isorig = $(".switcher_hidd").is(":checked");
        var url = "/Products/Product/View";
        if (!isorig) {
            url = "/Products/Product/OrgView";
        }

        var view = new $.whiskey.web.ajaxView({
            caption: "详细信息",
            actionUrl: url,
            params: { Id: id },
            lockButton: $(this),
        });

    });
    $("body").delegate("#DataTables_Table_0_wrapper .dataTables_paginate a", "click", function () {
        var cur = $(this).attr("p");
        var showleng = $(".sel_num option:selected").val(); //每页显示的数据条数
        refreshPageing(cur); //刷新分页按钮
        refreshTableData(cur, showleng); //刷新表中的数据
    });
    $(".sel_num").change(function () {
        var leng = $(".sel_num option:selected").val();
        refreshTableData(1, parseInt(leng));
        refreshPageing(1);
    });

});

