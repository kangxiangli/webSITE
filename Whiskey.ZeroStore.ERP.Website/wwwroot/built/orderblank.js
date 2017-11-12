(function () {
    /// <summary>
    /// 配货单类型
    /// </summary>
    var OrderblankType;
    (function (OrderblankType) {
        /// <summary>
        /// 直接创建配货单
        /// </summary>
        OrderblankType[OrderblankType["\u76F4\u63A5\u521B\u5EFA"] = 0] = "\u76F4\u63A5\u521B\u5EFA";
        /// <summary>
        /// 采购单到配货单
        /// </summary>
        OrderblankType[OrderblankType["\u91C7\u8D2D\u5355\u521B\u5EFA"] = 1] = "\u91C7\u8D2D\u5355\u521B\u5EFA";
    })(OrderblankType || (OrderblankType = {}));
    /// <summary>
    /// 配货单状态
    /// </summary>
    var OrderblankStatus;
    (function (OrderblankStatus) {
        /// <summary>
        /// 配货中
        /// </summary>
        OrderblankStatus[OrderblankStatus["\u914D\u8D27\u4E2D"] = 0] = "\u914D\u8D27\u4E2D";
        /// <summary>
        /// 发货中
        /// </summary>
        OrderblankStatus[OrderblankStatus["\u53D1\u8D27\u4E2D"] = 1] = "\u53D1\u8D27\u4E2D";
        /// <summary>
        /// 已撤销
        /// </summary>
        OrderblankStatus[OrderblankStatus["\u5DF2\u64A4\u9500"] = 2] = "\u5DF2\u64A4\u9500";
        /// <summary>
        /// 已完成
        /// </summary>
        OrderblankStatus[OrderblankStatus["\u5DF2\u5B8C\u6210"] = 3] = "\u5DF2\u5B8C\u6210";
    })(OrderblankStatus || (OrderblankStatus = {}));
    $(document).ready(function () {
        if (location.hash.indexOf("picking") > 0) {
            $("#tab_content li:eq(1) a").click();
        }
        function getorderblankconditions() {
            var conditions = new $.whiskey.filter.group();
            var startDate = $("#OrderblankStartDate").val();
            var endDate = $("#OrderblankEndDate").val();
            //if($(".toggle-cancel").is(":checked")){
            //    conditions.Rules.push(new $.whiskey.filter.rule("IsDeleted", "true", "equal"));
            //}
            //else{
            //    conditions.Rules.push(new $.whiskey.filter.rule("IsDeleted", "false", "equal"));
            //}
            if (startDate.length > 0 && endDate.length > 0) {
                conditions.Rules.push(new $.whiskey.filter.rule("CreatedTime", startDate + " 00:01:01", "greater"));
                conditions.Rules.push(new $.whiskey.filter.rule("CreatedTime", endDate + " 23:59:59", "less"));
            }
            // 配货单类型筛选
            var orderblankType = $("#OrderblankType option:selected").val();
            if (orderblankType && !isNaN(parseInt(orderblankType))) {
                conditions.Rules.push(new $.whiskey.filter.rule("OrderblankType", orderblankType, "equal"));
            }
            // 配货单状态筛选
            var orderblankStat = $("#Status option:selected").val();
            if (orderblankStat && !isNaN(parseInt(orderblankStat))) {
                if (orderblankStat == OrderblankStatus.已撤销) {
                    conditions.Rules.push(new $.whiskey.filter.rule("IsDeleted", "true", "equal"));
                }
            }
            $(".seach_2 input[name][name!='OrderblankStartDate'][name!='OrderblankEndDate'][name!='OrderblankType']").each(function () {
                var field = $(this).attr("Id");
                var value = $(this).val();
                if (value != null && value.length > 0) {
                    conditions.Rules.push(new $.whiskey.filter.rule(field, value, "contains"));
                }
            });
            $(".seach_2 select").each(function () {
                var field = $(this).attr("Id");
                var value = $(this).find("option:selected").val();
                if (value != undefined && value != null && value != "") {
                    conditions.Rules.push(new $.whiskey.filter.rule(field, value, "equal"));
                }
            });
            return JSON.stringify(conditions);
        }
        $.whiskey.datatable.instances[1] = $(".order_li").dataTable({
            "sDom": '<"H clearfix datatable-header text-center">rt<"F clearfix datatable-footer"<"col-md-5 info"i><"col-md-7 text-right"p>>',
            "sAjaxSource": "/Warehouses/Orderblank/List",
            "fnServerParams": function (aoData) {
                aoData.push({ name: "conditions", value: getorderblankconditions() });
            },
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                var isEnabled = aData.IsEnabled;
                if (isEnabled == false) {
                    $(nRow).css({ "color": "red" });
                }
                $("td:eq(1)", nRow).html((iDisplayIndex + 1));
                $("td:last", nRow).addClass("text-middle").css({ "width": "15%" });
                //$(nRow).addClass("treegrid-" + aData.Id + (aData.ParentId != null ? " treegrid-parent-" + aData.ParentId : ""));
                return nRow;
            },
            "fnDrawCallback": function () {
                $(".order_li .checked-all").click(function () {
                    var checkedStatus = this.checked;
                    $(".table-list tr td input[type=checkbox]").each(function () {
                        this.checked = checkedStatus;
                    });
                });
            },
            "aoColumns": [{
                    "bVisible": false,
                    "bSearchable": false,
                    "sName": "Id",
                    "mData": "Id"
                },
                {
                    "sTitle": $.whiskey.datatable.tplTitleCheckbox(),
                    "sName": "Id",
                    "bSortable": false,
                    "bSearchable": false,
                    "mData": function (data) {
                        return $.whiskey.datatable.tplListCheckbox(data.Id);
                    }
                },
                {
                    "sTitle": "编号",
                    "bSortable": false,
                    "sName": "RowNumber",
                    "mData": function (data) {
                        return "";
                    }
                },
                {
                    "sTitle": "配货单号",
                    "bSortable": false,
                    "sName": "PurchaseNumber",
                    "mData": function (data) {
                        return data.OrderBlankNumber;
                    }
                },
                {
                    "sTitle": "采购单号",
                    "bSortable": false,
                    "sName": "PurchaseNumber",
                    "mData": function (data) {
                        return data.PurchaseNumber;
                    }
                },
                {
                    "sTitle": "预约号",
                    "bSortable": false,
                    "sName": "AppointmentNumber",
                    "mData": function (data) {
                        return data.AppointmentNumber;
                    }
                },
                {
                    "sTitle": "发货店铺",
                    "bSortable": false,
                    "sName": "DeliverId",
                    "mData": function (data) {
                        if (data.OutStoreName != null)
                            return data.OutStoreName;
                        else
                            return "";
                    }
                },
                {
                    "sTitle": "发货仓库",
                    "bSortable": false,
                    "sName": "DeliverId",
                    "mData": function (data) {
                        if (data.OutStorageName != null)
                            return data.OutStorageName;
                        else
                            return "";
                    }
                },
                {
                    "sTitle": "收货店铺",
                    "bSortable": false,
                    "sName": "ReceiverId",
                    "mData": function (data) {
                        if (data.ReceiverStoreName != null)
                            return data.ReceiverStoreName;
                        else
                            return "";
                    }
                },
                {
                    "sTitle": "收货仓库",
                    "bSortable": false,
                    "sName": "ReceiverId",
                    "mData": function (data) {
                        if (data.ReceiverStorageName != null)
                            return data.ReceiverStorageName;
                        return "";
                    }
                },
                {
                    "sTitle": "采购单类型",
                    "bSortable": false,
                    "sName": "purtyp",
                    "mData": function (data) {
                        if (data.PurchaseNumber == "")
                            return "<span style='color:orange;font-size: small'>直接创建</span>";
                        else
                            return "<span style='color:orange;font-size: small'>采购单->配货单</span>";
                    }
                },
                {
                    "sTitle": "配货数量",
                    "bSortable": false,
                    "sName": "Weight",
                    "mData": function (data) {
                        var htmlPart = '<a href="javascript:void(0)" class="barcodeCount" style="cursor:pointer;">' + data.Quantity + '</a>';
                        return htmlPart;
                    }
                },
                {
                    "sTitle": "提交时间",
                    "bSortable": false,
                    "sName": "UpdatedTime",
                    "mData": function (data) {
                        if (data.CreatedTime != null)
                            return $.whiskey.tools.dateFormat(data.CreatedTime, "yyyy-MM-dd HH:mm:ss");
                        return "";
                    }
                },
                {
                    "sTitle": "配货状态",
                    "bSortable": false,
                    "sName": "Status",
                    "mData": function (data) {
                        var str = "";
                        if (data.OrderBlankNumber && data.OrderBlankNumber.length > 0) {
                            var stat = data.Status;
                            switch (stat) {
                                case OrderblankStatus.配货中: {
                                    str = "<span class='label label-info_phz'>配货中<span>";
                                    break;
                                }
                                case OrderblankStatus.发货中: {
                                    str = "<span class='label label-info'>发货中<span>";
                                    break;
                                }
                                case OrderblankStatus.已完成: {
                                    str = "<span class='label label-success'>已完成<span>";
                                    break;
                                }
                                case OrderblankStatus.已撤销: {
                                    str = "<span class='label label-danger'>已撤销<span>";
                                    break;
                                }
                                default:
                                    break;
                            }
                        }
                        return str;
                    }
                },
                {
                    "sTitle": "控制操作",
                    "bSortable": false,
                    "bSearchable": false,
                    "mData": function (data) {
                        return generateBtn(data);
                    }
                }
            ]
        });
        // 按钮委托
        //解绑
        //$(document).undelegate(".order-view,.barcodeCount,.btnRemove,.reject_ord_but,.start_ord_but,.startcheck_but,.checkedok_but","click");
        $('.toggle-cancel').switcher({
            on_state_content: "已撤销",
            off_state_content: "正常"
        }).on("click", function () {
            var isChecked = $(this).is(":checked");
            $.whiskey.datatable.instances[1].fnDraw(false);
        });
        $(".table-orderblank")
            .delegate(".order-view,.barcodeCount", "click", function () {
            var orderblankId = $(this).parents("tr").find(":checkbox").val();
            var view = new $.whiskey.web.ajaxView({
                caption: "配货单详情",
                actionUrl: "/Warehouses/Orderblank/OrderblankView",
                params: { Id: orderblankId },
                lockButton: $(this)
            });
        })
            .delegate(".btnRemove", "click", function () {
            var _id = $(this).parents("tr").find(":checkbox").val();
            //检测是否超时
            var ordernum = $(this).parents("tr").children("td").eq(2).text();
            $.post("/Warehouses/Orderblank/IsTimeout", { orderblankNum: ordernum, action: 1 /* Delete */ })
                .done(function (data) {
                if (data.ResultType != 3) {
                    $.whiskey.web.alert({
                        type: "danger",
                        par: { keyboard: false, backdrop: "static" },
                        content: data.Message
                    });
                    return;
                }
                if (data.Message == "timeout") {
                    var score = data.Data;
                    $.whiskey.web.ajaxConfirm({
                        question: "操作已超时,如继续操作,需要扣除" + score + "积分,确认继续吗?",
                        notes: "提示：此操作不可撤消",
                        success_event: function () {
                            Remove(this, _id);
                        }
                    });
                }
                else {
                    Remove(this, _id);
                }
            });
        })
            .delegate(".reject_ord_but", "click", function () {
            debugger;
            //拒绝配货
            var ordernum = $(this).parents("tr").children("td").eq(2).text();
            $.post("/Warehouses/Orderblank/IsTimeout", { orderblankNum: ordernum, action: 3 /* Reject */ })
                .done(function (data) {
                if (data.ResultType != 3) {
                    $.whiskey.web.alert({
                        type: "danger",
                        par: { keyboard: false, backdrop: "static" },
                        content: data.Message
                    });
                    return;
                }
                if (data.Message == "timeout") {
                    var score = data.Data;
                    $.whiskey.web.ajaxConfirm({
                        question: "操作已超时,如继续操作,需要扣除" + score + "积分,确认继续吗?",
                        notes: "提示：此操作不可撤消",
                        success_event: function () {
                            rejectOrderblank(ordernum);
                        }
                    });
                }
                else {
                    $.whiskey.web.ajaxConfirm({
                        question: "确定要拒绝配货吗?",
                        notes: "提示：此操作不可撤消",
                        success_event: function () {
                            rejectOrderblank(ordernum);
                        }
                    });
                }
            });
        })
            .delegate(".start_ord_but", "click", function () {
            var isClosed = $(this).data("senderclosed");
            if (isClosed) {
                $.whiskey.web.alert({
                    type: "danger",
                    par: { keyboard: false, backdrop: "static" },
                    content: '发货店铺已闭店,无法执行配货操作'
                });
                return false;
            }
            //开始配货
            var num = $(this).parents("tr").find("td:eq(2)").text();
            $.whiskey.web.load({ url: "/Warehouses/Orderblank/StartOrderblank", data: { _num: num } });
        })
            .delegate(".startcheck_but", "click", function () {
            var row = $(this).parents("tr").first();
            var ordnum = $(row).children("td").eq(2).text();
            $.whiskey.web.load({ url: "/Warehouses/Orderblank/CheckInventory?orderblankNum=" + ordnum });
            //  location.href = "/Warehouses/Orderblank/CheckInventory?orderblankNum=" + ordnum;
        })
            .delegate(".checkedok_but", "click", function () {
            //确认收货
            var num = $(this).parents("tr").children("td").eq(2).text();
            $.post("/Warehouses/Orderblank/IsTimeout", { orderblankNum: num, action: 4 /* Accept */ }).done(function (data) {
                if (data.ResultType != 3) {
                    $.whiskey.web.alert({
                        type: "danger",
                        par: { keyboard: false, backdrop: "static" },
                        content: data.Message
                    });
                    return;
                }
                if (data.Message == "timeout") {
                    var score = data.Data;
                    $.whiskey.web.ajaxConfirm({
                        question: "操作已超时,如继续操作,需要扣除" + score + "积分,确认继续吗?",
                        notes: "提示：此操作不可撤消",
                        success_event: function () {
                            confirmAccept(num);
                        }
                    });
                }
                else {
                    confirmAccept(num);
                }
            });
        });
        /**
         * 确认收货
         *
         * @param {string} orderbalnkNum
         */
        function confirmAccept(orderbalnkNum) {
            var confirm = new $.whiskey.web.ajaxConfirm({
                question: "确定收货吗？",
                notes: "提示：该操作不能撤消",
                success_event: function () {
                    $.post("/Warehouses/Orderblank/ReceptProduct", { num: orderbalnkNum }, function (da) {
                        if (da.ResultType == 3) {
                            $.whiskey.web.alert({
                                type: "success",
                                content: "操作成功",
                                par: { keyboard: false, backdrop: "static" },
                                callback: function () {
                                    $.whiskey.datatable.instances[1].fnDraw();
                                }
                            });
                            //审核成功后消息更新
                            //								var b = $(".orderblank_manage").html();
                            //                              var a=parseInt(b)-1;
                            //									$(".orderblank_manage").html(a.toString());
                            //										if(a==0){
                            //										$(".orderblank_manage").css("display","none")
                            //											}else{
                            //												$(".orderblank_manage").css("display","black")
                            //												}    
                            $.whiskey.web.updateBadge(-1, "orderblank_manage", true);
                        }
                        else {
                            $.whiskey.web.alert({
                                type: "info",
                                content: da.Message,
                                par: { keyboard: false, backdrop: "static" },
                                callback: function () {
                                }
                            });
                        }
                    });
                }
            });
        }
        function UpdateMenu() {
            $.post("/Warehouses/Orderblank/GetIncompleteOrderbalnkCount", {}, function (res) {
                if (res.ResultType == 3 && !isNaN(res.Data)) {
                    var count = parseInt(res.Data);
                    //                  if (count <= 0) {
                    //                      $(".deposit_manage").text(count).hide();
                    //                      $(".member_manage").css("display", "none");
                    //                      return;
                    //                  }
                    //                  $(".deposit_manage").text(count).show();
                    //                  $(".member_manage").css("display", "inline-block");
                    $.whiskey.web.updateBadge(count, "orderblank_manage");
                }
            });
        }
        /**
         * 拒绝收货操作
         */
        function rejectOrderblank(ordernum) {
            //拒绝配货
            $.whiskey.tools.other = new $.whiskey.web.ajaxDialog({
                caption: "拒绝配货",
                actionUrl: "/Warehouses/Orderblank/RejectOrder?odnum=" + ordernum,
                lockButton: $(this),
                successEvent: function () {
                    var _num = $("#hid_orderblankId").val();
                    var _meg = $("#ord_rejectMes").val();
                    $.post("/Warehouses/Orderblank/RejectOrderblank", { num: _num, msg: _meg }, function (da) {
                        if (da.ResultType == 3) {
                            $.whiskey.web.alert({
                                type: "success",
                                content: "操作成功"
                            });
                            //审核成功后消息更新
                            //								var b = $(".orderblank_manage").html();
                            //                              var a=parseInt(b)-1;
                            //									$(".orderblank_manage").html(a.toString());
                            //										if(a==0){
                            //										$(".orderblank_manage").css("display","none")
                            //											}else{
                            //												$(".orderblank_manage").css("display","black")
                            //												}    
                            UpdateMenu();
                        }
                        else {
                            $.whiskey.web.alert({
                                type: "danger",
                                content: da.Message,
                                par: { keyboard: false, backdrop: "static" }
                            });
                        }
                        $.whiskey.datatable.instances[1].fnDraw(false);
                    });
                },
                closeEvent: function () {
                    //return closeList();
                }
            });
        }
        $("#Print").on("click", function () {
            var list = $.whiskey.web.getIdByChecked(".table-list td input[type=checkbox]");
            if (list.length > 0) {
                var printer = $.whiskey.printer.ajaxPreview({
                    actionUrl: "/Warehouses/Orderblank/Print",
                    lockButton: $(this),
                    topMargin: "2%",
                    leftMargin: "4%",
                    contentWidth: "93.5%",
                    contentHeight: "100%",
                    params: list
                });
            }
            else {
                $.whiskey.web.alert({
                    type: "info",
                    content: "请至少选择一条数据！",
                    callback: function () {
                    }
                });
            }
        });
        $("#Export").on("click", function () {
            $.whiskey.exporter.ajaxExport({
                actionUrl: "/Warehouses/Orderblank/Export",
                lockButton: $(this),
                version: 2,
                params: {
                    conditions: getorderblankconditions()
                }
            });
        });
        $("#order_Search").on("click", function () {
            $.whiskey.datatable.instances[1].fnDraw(false);
        });
        $("#order_Clear").on("click", function () {
            $.whiskey.web.clearForm(".form-search:last");
        });
    });
    /**
     * 根据配货单状态及用户权限生成操作按钮
     *
     * @param {*} data
     * @returns {string}
     */
    function generateBtn(data) {
        var btn = "<button title='详细信息' class='btn btn-xs order-view btn-padding-right'><i class='fa fa-eye'></i></button>";
        var stat = data.Status;
        var isSenderClosed = data.isSenderClosed;
        var isReceiverStoreClosed = data.isReceiverStoreClosed;
        switch (stat) {
            case OrderblankStatus.配货中: {
                if (data.IsSender || data.IsBoth) {
                    btn += "<button data-senderclosed=\"" + (isSenderClosed ? "true" : "false") + "\"   title='\u5F00\u59CB\u914D\u8D27' style='color:green;margin-right:2px' class='btn btn-xs start_ord_but  btn-padding-right'><i class='fa fa-clipboard '></i></button>";
                    btn += "<button   title='\u64A4\u9500'  class='btn btn-xs btn-padding-right btnRemove'><i class='fa fa-trash-o'></i></button>";
                }
                break;
            }
            case OrderblankStatus.发货中: {
                if (data.IsAccept || data.IsBoth) {
                    btn += "<button title='\u5F00\u59CB\u76D8\u70B9' class='btn btn-xs startcheck_but  btn-padding-right'>\n                            <i class='fa fa-check-square'></i>\n                         </button>\n                        <button  title=\"\u62D2\u7EDD\u6536\u8D27\" style=\"margin-right:2px\" class=\"btn btn-xs reject_ord_but  btn-padding-right\">\n                            <i class=\"fa fa-life-ring\"></i>\n                        </button>\n                        <button  title='\u786E\u8BA4\u6536\u8D27' class='btn btn-xs checkedok_but  btn-padding-right'>\n                            <i class='fa fa-arrow-right'></i>\n                        </button>";
                }
                break;
            }
            case OrderblankStatus.已完成:
            case OrderblankStatus.已撤销:
            default: break;
        }
        return btn;
    }
    function Update(sender, number) {
        $.whiskey.web.load({ url: "/Warehouses/Orderblank/StartOrderblank?_num=" + number });
        //location.href = "/Warehouses/Orderblank/StartOrderblank?_num=" + number;
    }
    /**
     * 撤销配货但
     * @param sender 按钮
     * @param Id 配货单id
     */
    function Remove(sender, Id) {
        var confirm = new $.whiskey.web.ajaxConfirm({
            question: "确认要撤销配货单吗？",
            notes: "提示：数据撤销不可恢复",
            actionUrl: "/Warehouses/Orderblank/Remove",
            params: { id: Id },
            lockButton: $(sender),
            complete: function (data) {
                if (data.ResultType == 3) {
                    $.whiskey.web.alert({
                        type: "success",
                        content: "撤销成功",
                        callback: function () {
                            $.whiskey.datatable.instances[1].fnDraw(false);
                        }
                    });
                }
            }
        });
    }
}.call(this));
