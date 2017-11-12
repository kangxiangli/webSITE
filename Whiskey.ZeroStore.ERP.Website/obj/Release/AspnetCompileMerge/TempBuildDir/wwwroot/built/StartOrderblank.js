/**
 * 配货页面
 */
(function () {
    var $tableInstance = null;
    var hashlist = new $.whiskey.hashtable();
    var uuid = $.whiskey.tools.UUID();
    $(function () {
        var num = $("#hid-ordernum_dat").val();
        $tableInstance = $(".pur_list_tab_cre").dataTable({
            "aaSorting": [[0, 'desc']],
            "sAjaxSource": "/Warehouses/Orderblank/OrderblankViewList",
            "sDom": 'it<"F clearfix datatable-footer"<"col-md-2"l>r<"col-md-3"><"col-md-7 text-right"p>>',
            "fnServerParams": function (aoData) {
                var conditions = new $.whiskey.filter.group();
                aoData.push({ name: "OrderblankNumber", value: num });
                aoData.push({ name: "uuid", value: uuid });
                aoData.push({ name: "conditions", value: JSON.stringify(conditions) });
            },
            "fnDrawCallback": function () {
                $(".checked-all").click(function () {
                    var checkedStatus = this.checked;
                    $(".table-list tr td input[type=checkbox]").each(function () {
                        this.checked = checkedStatus;
                    });
                });
                $(".table-list").treegrid({
                    saveState: true,
                    treeColumn: 2,
                    expanderExpandedClass: 'treegrid-expander-expanded',
                    expanderCollapsedClass: 'treegrid-expander-collapsed'
                });
            },
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $(nRow).addClass("treegrid-" + aData.Id + (aData.ParentId != null ? " treegrid-parent-" + aData.ParentId : ""));
                $("td:eq(1)", nRow).html(iDisplayIndex + 1);
                return nRow;
            },
            "aoColumns": [{
                    "bVisible": false,
                    "bSearchable": false,
                    "sName": "UpdateTime",
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
                    "sTitle": "自增编号",
                    "bSortable": false,
                    "sName": "",
                    "mData": function (da) {
                        return "";
                    }
                },
                {
                    "sTitle": "商品编号",
                    "bSortable": false,
                    "sName": "ProductNumber",
                    "mData": function (da) {
                        if (da.ParentId == "")
                            return da.Number;
                        return "";
                    }
                },
                {
                    "sTitle": "流水号",
                    "bSortable": false,
                    "sName": "ProductName",
                    "mData": function (da) {
                        if (da.ParentId != "")
                            return da.Number;
                        return "";
                    }
                },
                {
                    "sTitle": "品牌",
                    "bSortable": false,
                    "sName": "Brand",
                    "mData": function (da) {
                        if (da.Brand == undefined)
                            return "";
                        return da.Brand;
                    }
                },
                {
                    "sTitle": "尺码",
                    "bSortable": false,
                    "sName": "Size",
                    "mData": function (da) {
                        if (da.Size == undefined)
                            return "";
                        return da.Size;
                    }
                },
                {
                    "sTitle": "季节",
                    "bSortable": false,
                    "sName": "Season",
                    "mData": function (da) {
                        if (da.Season == undefined)
                            return "";
                        return da.Season;
                    }
                },
                {
                    "sTitle": "颜色",
                    "bSortable": false,
                    "sName": "Color",
                    "mData": function (da) {
                        if (da.Color == undefined)
                            return "";
                        return da.Color;
                    }
                },
                {
                    "sTitle": "图片",
                    "sName": "Thumbnail",
                    "bSortable": false,
                    "bSearchable": false,
                    "mData": function (da) {
                        if (da.ParentId != "")
                            return "";
                        return "<div class='thumbnail-img_five_box'><div class='thumbnail-img_five'><div class='thumbnail-img_f'><img class='' src=" + da.Thumbnail + " onerror='imgloaderror(this);'></div></div></div>";
                    }
                },
                {
                    "sTitle": "配货数量",
                    "sName": "Amount",
                    "bSortable": false,
                    "bSearchable": false,
                    "mData": function (da) {
                        if (da.ParentId == "")
                            return da.Quantity;
                        return "";
                    }
                },
                {
                    "sTitle": "操作",
                    "sName": "Access",
                    "bSortable": false,
                    "bSearchable": false,
                    "mData": function (da) {
                        return "<button class='form-control remove_but' style='color:red' title='删除'><i class='fa fa-life-ring'></i></button>";
                    }
                },
            ]
        });
        /**
         * checkbox全选,反选
         */
        $(".pur_list_tab_cre").on("click", "input[type=checkbox]", function () {
            var row = $(this).parents("tr")[0];
            var isCheck = $(this).is(":checked");
            if ($(row).treegrid("isLeaf")) {
                // 获取父级节点
                var $parentNode = $(row).treegrid("getParentNode");
                var $parentCheckbox = $parentNode.find("input[type=checkbox]");
                if (isCheck) {
                    //判断是否所有子节点均选中了
                    //获取同级节点
                    var $childCheckboxes = [];
                    $.each($parentNode.treegrid("getChildNodes"), function (i, e) {
                        $childCheckboxes.push($(e).find("input[type=checkbox]")[0]);
                    });
                    var flag = true;
                    $.each($childCheckboxes, function (index, elem) {
                        if (!($(elem).is(":checked"))) {
                            flag = false;
                        }
                    });
                    $parentCheckbox.prop("checked", flag);
                }
                else {
                    $parentCheckbox.prop("checked", false);
                }
            }
            else {
                var $childCheckboxes_1 = [];
                $.each($(row).treegrid("getChildNodes"), function (i, e) {
                    $childCheckboxes_1.push($(e).find("input[type=checkbox]")[0]);
                });
                $.each($childCheckboxes_1, function (index, elem) {
                    $(elem).prop("checked", isCheck);
                });
            }
        });
        /**
         * 队列监听
         */
        setInterval(function () {
            sendToQueue();
        }, 1000);
        //初始禁用右侧元素
        alldisable();
        //单条移除
        $(document).delegate(".remove_but", "click", function () {
            var row = $(this).parents("tr");
            $.whiskey.web.ajaxConfirm({
                question: "确定要移除选该商品吗",
                notes: "",
                success_event: function () {
                    removeRow(row);
                },
                cancel_event: function () {
                }
            });
        });
        $(window).on("beforeunload", function (e) {
            if (hashlist.size() > 0) {
                var mes = "检测到还有" + hashlist.size() + "条数据正在校验，刷新或者关闭浏览器会丢失这些数据";
                //e.returnValue = mes;
                return mes;
            }
            var t = $.whiskey.tools.other();
            if (t != undefined && t != null && t.length > 0) {
                t = t.substr(1, t.length - 1);
                var mes = "检测到新增数据，编号：" + t + "未保存，刷新或者关闭浏览器会丢失这些数据";
                //e.returnValue = mes;
                return mes;
            }
            var sta = $.whiskey.tools.status();
            if (sta == 1) {
                var mes = "检测到数据已经修改，刷新或者关闭浏览器会丢失这些修改数据";
                //e.returnValue = mes;
                return mes;
            }
        });
        $(document).delegate(".pur_list_tab_cre input", "change", function () {
            $.whiskey.tools.status(1); // 设置修改状态
        });
        //[批量导入]按钮
        $("#selec_prodBatch_list").click(function () {
            var dialog = new $.whiskey.web.ajaxDialog({
                caption: "批量导入",
                successTit: "确定",
                successEvent: select_check_Access,
                actionUrl: "/Warehouses/Orderblank/BatchImport",
                noneheader: true,
                lockButton: $(this),
                methType: "post",
                formValidator: function () {
                    var $form = $(".modal-form");
                    if (!$form.valid()) {
                        $(".modal-dialog").parent("div").animate({ scrollTop: 20 }, 500);
                        return false;
                    }
                    else {
                        return true;
                    }
                },
                postComplete: function () {
                    return true;
                }
            });
        });
        //将批量导入中选中的元素压入队列
        function select_check_Access() {
            //var checks = $(".pdl:checked");
            var orderblankId = $("#orderblankId").val();
            var num = $("#hid-ordernum_dat").val();
            var tableData = window.gloablDataTableObj.data();
            var barcodes = [];
            for (var i = 0; i < tableData.length; i++) {
                barcodes.push(tableData[i].value);
            }
            $.ajax({
                url: "/Warehouses/Orderblank/MultitudeVaild",
                type: "POST",
                data: { id: orderblankId, nums: barcodes.join(","), orderblanknum: num, uuid: uuid },
                success: function (data) {
                    updateQueueCount(hashlist.size());
                    updateValidCount(data.Data.validCount);
                    updateInvalidCount(data.Data.invalidCount);
                    if (data.ResultType == 3) {
                        $tableInstance.api().draw(false);
                    }
                }
            });
        }
        //将队列中的数据发往服务端验证
        function sendToQueue() {
            if (hashlist.size() > 0) {
                disableSaveBtn();
                var entryId = hashlist.getFirst(0);
                var scanNumber = hashlist.getFirst(1);
                var num = $("#hid-ordernum_dat").val();
                var exist = 0;
                $(".pur_list .dt_num").each(function () {
                    if ($(this).html().trim() == scanNumber)
                        exist = 1;
                });
                $.ajax({
                    type: "POST",
                    data: { entryId: entryId, barcode: scanNumber, orderblanknum: num, uuid: uuid },
                    async: false,
                    url: "/Warehouses/Orderblank/StartOrderblankValid",
                    success: function (data) {
                        if (data.ResultType == 3) {
                            $tableInstance.api().draw(false);
                        }
                        //处理完毕，从队列中移除
                        hashlist.remove(data.Data.entryId);
                        updateQueueCount(hashlist.size());
                        updateValidCount(data.Data.validCount);
                        updateInvalidCount(data.Data.invalidCount);
                    }
                });
            }
            else {
                if ($.whiskey.tools.json() != "1") {
                    $("#save_pur").removeAttr("disabled");
                    $("#saveord_notsend").removeAttr("disabled");
                }
                var t = $.whiskey.tools.status;
            }
        }
        //查看无效配货
        $(".scan-invalid").click(function () {
            var view = new $.whiskey.web.ajaxView({
                caption: "无效列表",
                actionUrl: "/Warehouses/Orderblank/InValid?uuid=" + uuid
            });
        });
        //查看有效配货
        $(".scan-valid").click(function () {
            var view = new $.whiskey.web.ajaxView({
                caption: "有效列表",
                actionUrl: "/Warehouses/Orderblank/VaildView?uuid=" + uuid
            });
        });
        //批量移除
        $("#removeall_ord").click(function () {
            var $checkboxes = $($tableInstance).children("tbody").find(":checkbox:checked");
            if ($checkboxes.length <= 0) {
                $.whiskey.web.alert({
                    type: "info",
                    par: { keyboard: false, backdrop: "static" },
                    content: "请勾选需要删除的行",
                    callback: function () {
                    }
                });
                return;
            }
            $.whiskey.web.ajaxConfirm({
                question: "确定要移除选中的商品吗",
                notes: "提示：此操作中移除该商品",
                success_event: function () {
                    //配货单号
                    var num = $("#hid-ordernum_dat").val();
                    var barcodes = [];
                    $checkboxes.each(function (index, elem) {
                        // 获取tr
                        var row = $(this).parents("tr");
                        // 获取选中的流水号
                        if ($(row).treegrid('isLeaf')) {
                            var barcode = $(row).treegrid("getNodeId");
                            barcodes.push($(elem).val());
                        }
                    });
                    removeBarcodes(barcodes, num, uuid);
                },
                cancel_event: function () {
                }
            });
        });
        $("#ScanNumber").keyup(function (e) {
            if (e.keyCode == 13)
                $("#sear-ok").click();
        });
        $("#sear-ok").click(function () {
            var scanNumber = $(".scan-number").val();
            if (scanNumber.trim() != "")
                EnterQue(scanNumber);
        });
        //保存配货单并配货
        $("#save_pur").click(function () {
            //检测是否超时
            $.post("/Warehouses/Orderblank/IsTimeout", { orderblankNum: $("#hid-ordernum_dat").val(), action: 2 /* Delivery */ })
                .done(function (data) {
                if (data.ResultType != 3) {
                    $.whiskey.web.alert({
                        type: "danger",
                        par: { keyboard: false, backdrop: "static" },
                        content: "data.Message"
                    });
                }
                if (data.Message == "timeout") {
                    var score = data.Data;
                    $.whiskey.web.ajaxConfirm({
                        question: "配货已超时,如继续操作,需要扣除" + score + "积分,确认继续吗?",
                        notes: "提示：此操作不可撤消",
                        success_event: function () {
                            Createorder(1);
                        }
                    });
                }
                else {
                    $.whiskey.web.ajaxConfirm({
                        question: "确定要保存配货单并开始配货吗",
                        notes: "提示：此操作不可撤消",
                        success_event: function () {
                            Createorder(1);
                        }
                    });
                }
            });
        });
        //保存但是不配货
        $("#saveord_notsend").click(function () {
            $.whiskey.web.ajaxConfirm({
                question: "确定要保存配货吗",
                notes: "提示：此操作不可撤消",
                success_event: function () {
                    Createorder(0);
                },
                cancel_event: function () {
                }
            });
        });
    });
    //是否修改了
    function isedit() {
        var t = $.whiskey.tools.other();
        var sta = $.whiskey.tools.status();
        if (t == undefined || t == null || t == "") {
            if (sta != 1) {
                return false;
            }
            return true;
        }
        return true;
    }
    //创建配货单
    function Createorder(issend) {
        disableSaveBtn();
        var ordNum = $("#hid-ordernum_dat").val();
        var notes = $("#cre_Notes").val().trim();
        var sta = $.whiskey.tools.status();
        $.post("/Warehouses/Orderblank/SaveOrderblankAndSend", { num: ordNum, notes: notes, send: issend, uuid: uuid }, function (da) {
            $("#save_pur").removeAttr("disabled");
            $("#saveord_notsend").removeAttr("disabled");
            if (da.ResultType == 3) {
                var tit = "";
                if (issend == 1) {
                    tit = "配货完成";
                }
                else {
                    tit = "保存成功";
                }
                $.whiskey.web.ajaxConfirm({
                    question: tit,
                    notes: "提示：是否需要回到配货管理页面",
                    success_event: function () {
                        $.whiskey.tools.json("-1");
                        $.whiskey.tools.other("");
                        $.whiskey.tools.status(0);
                        location.href = "/Warehouses/Purchase/Index#picking";
                    }
                });
            }
            else {
                $.whiskey.web.alert({
                    type: "info",
                    content: da.Message,
                    par: { keyboard: false, backdrop: "static" },
                    callback: function () {
                        $.whiskey.tools.json("-1");
                    }
                });
            }
        });
    }
    function deleDat() {
        return "<button display='inline' type='button' style='color:red'  margin-right:3px' title='移除' class='btn btn_remove fa fa-life-ring'></button>";
    }
    function removeBarcodes(barcodesFromUser, orderblankNum, uuid) {
        $.post("/Warehouses/Orderblank/RemoveBarcodeFromOrderblank", { barcodesFromUser: barcodesFromUser, orderblankNum: orderblankNum, uuid: uuid }).done(function (da) {
            if (da.ResultType == 3) {
                updateQueueCount(hashlist.size());
                updateValidCount(da.Data.validCount);
                updateInvalidCount(da.Data.invalidCount);
                $tableInstance.api().draw(false);
                $.whiskey.tools.status(1);
            }
            else {
                $.whiskey.web.alert({
                    type: "info",
                    content: "删除失败，请刷新页面重新操作",
                    par: { keyboard: false, backdrop: "static" }
                });
            }
        });
    }
    /**
     * 移除选中的行
     *
     * @param {any} trElement
     */
    function removeRow(trElement) {
        var orderblankNum = $("#hid-ordernum_dat").val();
        var barcodes = [];
        if ($(trElement).treegrid("isNode")) {
            if (!$(trElement).treegrid("isLeaf")) {
                //父节点删除
                var $childCheckboxes = [];
                $.each($(trElement).treegrid("getChildNodes"), function (i, e) {
                    var code = $(e).treegrid("getNodeId");
                    barcodes.push(code);
                });
            }
            else {
                //按流水号删除
                var barcode = $(trElement).find(":checkbox").val();
                barcodes.push(barcode);
            }
            if (barcodes.length > 0) {
                removeBarcodes(barcodes, orderblankNum, uuid);
            }
        }
    }
    //禁用右侧
    function alldisable() {
        $("#whestr input").attr("disabled", "disabled");
    }
    //启用左侧
    function alleabled() {
        $("#left_content input").removeAttr("disabled");
        $("#left_content").css("opacity", 1);
    }
    function storeWithStorage(storeId, bindstorag) {
        if (storeId != undefined && storeId != null && storeId != "") {
            var storgId;
            var storag;
            if ($(bindstorag).attr("id") == "lin_outStorageId") {
                storag = $("#lin_buyStorageId option:selected").val();
            }
            if ($(bindstorag).attr("id") == "lin_buyStorageId") {
                storag = $("#lin_option:selected").val();
            }
            $.post("/Warehouses/Storage/GetStorages", { storeId: storeId }, function (da) {
                var opts = "<option value=''>选择仓库</option>";
                if (da != null && da != "") {
                    for (var i = 0; i < da.length; i++) {
                        var dat = da[i];
                        if (dat.IsDefaultStorage) {
                            if (storag != undefined && dat.Id == storag) {
                                opts += "<option disabled='disabled' value=" + dat.Id + " selected='selected'>" + dat.Name + "</option>";
                            }
                            else
                                opts += "<option value=" + dat.Id + " selected='selected'>" + dat.Name + "</option>";
                        }
                        else {
                            if (storag != undefined && dat.Id == storag) {
                                opts += "<option disabled='disabled' value=" + dat.Id + ">" + dat.Name + "</option>";
                            }
                            else
                                opts += "<option value=" + dat.Id + ">" + dat.Name + "</option>";
                        }
                    }
                }
                $(bindstorag).html("").html(opts);
            });
        }
    }
    function leftallenable() {
        var outstorag = $("#lin_outStorageId option:selected").val();
        var instorag = $("#inStoreId option:selected").val();
        var buystorag = $("#lin_buyStorageId option:selected").val();
        if (outstorag != "" && instorag != "" && buystorag != undefined && buystorag != "") {
            alleabled();
        }
        else {
            alldisable();
        }
    }
    /**
     * 更新队列数量
     *
     * @param {number} count
     */
    function updateQueueCount(count) {
        $(".scan-queue-count").animate({
            opacity: "0.3"
        }, 'slow', function () {
            $(".scan-queue-count").animate({
                opacity: "1.0"
            }, 'fast', function () {
                $(".scan-queue-count").text(count);
            });
        });
    }
    /**
     * 更新有效数量
     *
     * @param {number} validCount
     */
    function updateValidCount(validCount) {
        if (parseInt($(".scan-valid-count").text()) != validCount) {
            $(".scan-valid-count").animate({
                opacity: "0.3"
            }, 'slow', function () {
                $(".scan-valid-count").animate({
                    opacity: "1.0"
                }, 'fast', function () {
                    $(".scan-valid-count").text(validCount);
                });
            });
        }
    }
    /**
     *  更新无效数量
     *
     * @param {number} invalidCount
     */
    function updateInvalidCount(invalidCount) {
        if (parseInt($(".scan-invalid-count").text()) != invalidCount) {
            $(".scan-invalid-count").animate({
                opacity: "0.3"
            }, 'slow', function () {
                $(".scan-invalid-count").animate({
                    opacity: "1.0"
                }, 'fast', function () {
                    $(".scan-invalid-count").text(invalidCount);
                });
            });
        }
    }
    /**
     * 禁用发货/保存按钮
     */
    function disableSaveBtn() {
        $("#save_pur").attr("disabled", "disabled");
        $("#saveord_notsend").attr("disabled", "disabled");
    }
    /**
     * 更改配货数量
     *
     * @param {any} numb
     */
    function updateCount(numb) {
        $(".pur_list .dt_num").each(function () {
            var num = $(this).html().trim();
            if (num == numb) {
                var co = $(this).parents("tr").children(".pro_coun").children("input");
                var sourCou = parseInt($(co).val().trim()) + 1;
                $(co).val(sourCou);
            }
        });
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
    /**
     * 将选择的流水号提交给后台
     */
    function multitudeVaild() {
        var checks = $(".pdl:checked");
        if (checks.length !== 0) {
            var barcodeArr = [];
            for (var i = 0; i < checks.length; i++) {
                var tr = $(checks[i]).parents("tr")[0];
                if ($(tr).treegrid("isLeaf")) {
                    var barcode = $(tr).treegrid("getNodeId");
                    if (barcode != "") {
                        barcodeArr.push(barcode);
                    }
                }
            }
            if (barcodeArr.length !== 0) {
                var id = $("#orderblankId").val();
                var orderblankNum = $("#hid-ordernum_dat").val();
                $.ajax({
                    url: "/Warehouses/Orderblank/MultitudeVaild",
                    type: "POST",
                    data: { id: id, nums: barcodeArr.join(","), orderblanknum: orderblankNum, uuid: uuid },
                    success: function (data) {
                        updateQueueCount(hashlist.size());
                        updateValidCount(data.Data.validCount);
                        updateInvalidCount(data.Data.invalidCount);
                        if (data.ResultType == 3) {
                            $tableInstance.api().draw(false);
                        }
                    }
                });
            }
        }
    }
    var cur_div_form;
    /**
     * //在弹出层中选择商品
     */
    $("#selec_prod_list").click(function () {
        var orderblankNum = $("#hid-ordernum_dat").val();
        var dialog = new $.whiskey.web.ajaxDialog({
            caption: "选择商品",
            successTit: "确定",
            className: "box-dg",
            actionUrl: "/Warehouses/Orderblank/GetProductList?orderblankNum=" + orderblankNum,
            noneheader: true,
            successEvent: function () {
                multitudeVaild();
                //console.log(cur_div_form);
                $(cur_div_form).show();
            },
            lockButton: $(this),
            formValidator: function () {
                var $form = $(".modal-form");
                if (!$form.valid()) {
                    $(".modal-dialog").parent("div").animate({ scrollTop: 20 }, 500);
                    return false;
                }
                else {
                    return true;
                }
            },
            closeEvent: function () {
                $(cur_div_form).show();
            },
            beforeSend: function () {
                cur_div_form = $("form[0fashion='fashion-team']").parents(".modal-content");
                $(cur_div_form).hide();
            },
            postComplete: function () {
                $(cur_div_form).show();
                $tableInstance.api().draw(false);
                return true;
            }
        });
    });
    /**
     * 在弹出层中当勾选了复选框，单击“确定”时触发
     */
    function getAllCheck() {
        var checks = $(".pdl:checked");
        var arrayNumber = [];
        for (var i = 0; i < checks.length; i++) {
            var num = $(checks[i]).parents("td").nextAll("td").eq(1).html();
            if (num !== "" && num !== null && num !== undefined) {
                arrayNumber.push(num);
            }
        }
        // 提交选择的条码
        if (arrayNumber.length > 0) {
            $.post("/Warehouses/Orderblank/AddProduct", { Numbers: arrayNumber }).done(function (data) {
                if (data.ResultType != 3) {
                    $.whiskey.web.alert({
                        type: "danger",
                        content: data.Data.Message,
                        callback: function () {
                            return false;
                        }
                    });
                }
            });
        }
    }
}.call(this));
