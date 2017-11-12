var checkedNumbers = [];
/**
 * uploader 配置
 */
$(function () {
    var uploader = new window.plupload.Uploader({
        runtimes: 'silverlight,html4,html5,flash',
        browse_button: 'btnBatchUpload',
        url: '/Properties/SalesCampaign/ExcelFileUpload',
        flash_swf_url: '~/Content/plupload-2.1.8/js/Moxie.swf">~/Content/plupload-2.1.8/js/Moxie.swf',
        filters: {
            mime_types: [
                { title: "txt", extensions: "txt" },
                { title: "excel", extensions: "xls,xlsx" }
            ],
            max_file_size: '400kb',
            prevent_duplicates: true
        }
    });
    uploader.init();
    uploader.bind('FilesAdded', function (uploader, files) {
        uploader.start();
    });
    uploader.bind('BeforeUpload', function (uploader, file) {
        $("#up_state").html("正在上传……");
    }); //
    uploader.bind('FileUploaded', function (uploader, file, obj) {
        var res = JSON.parse($(obj.response).text());
        if (res.ResultType === 3 && res.Data.length > 0) {
            initTable(res.Data);
        }
    });
    $('#start_upload').click(function () {
        uploader.start();
    });
});
$(function () {
    // 店铺多选
    // 活动店铺
    var storeIds = $("#hideStoreIds").val();
    if (storeIds && storeIds.length > 0) {
        debugger;
        var memberArr = storeIds.split(',');
        $('#CampaignStore').queryAllStore({ selected: memberArr });
    }
    else {
        $("#CampaignStore").queryAllStore();
    }
    // 非会员折扣禁用输入
    //$("#nomembDisc").attr("disabled", "disabled");
    // 删除按钮委托
    $(document).delegate(".clre", "click", function () {
        $(this).parents("tr").remove();
        // 修改总数
        var totalCount = $("#tblCampCreate tbody tr").length;
        $("#productCount").text(totalCount);
        numCount--;
        if (0 == numCount % pageCount) {
            preLink();
            firstLink();
            nextText();
            lastText();
        }
        reloadTable();
    });
    // 活动的会员/非会员限制选择修改时,禁用相关的输入框
    $(".membonl").change(function () {
        var ty = $(this).find("option:selected").val();
        if (ty == 0) {
            $("#membDisc").attr("disabled", "disabled");
            $("#nomembDisc").removeAttr("disabled");
        }
        else if (ty == 1) {
            $("#membDisc").removeAttr("disabled");
            $("#nomembDisc").attr("disabled", "disabled");
        }
        if (ty == 2) {
            $("#membDisc").removeAttr("disabled");
            $("#nomembDisc").removeAttr("disabled");
        }
    });
    // 活动的日期选择控件
    $(".start-date,.end-date").datepicker({
        todayBtn: true
    });
    // 选择商品按钮
    $("#btn_sel").click(function () {
        $(".partiCreat").parents(".modal-content").hide();
        var storeId = $("#base #CampaignStore option:selected").val();
        if (storeId != null && storeId.length > 0) {
            var dialog = new $.whiskey.web.ajaxDialog({
                caption: "选择商品",
                actionUrl: "/Properties/SalesCampaign/GetProductsByStore",
                getParams: { storeId: storeId },
                successEvent: function () {
                    //选择商品之后,点击提交按钮callback
                    initTable(getCheckedProdNums());
                },
                closeEvent: function () {
                    $(".partiCreat").parents(".modal-content").show();
                },
                lockButton: $(this),
                formValidator: function () {
                    var $form = $(".modal-form");
                    if (!$form.valid()) {
                        return false;
                    }
                    else {
                        return true;
                    }
                },
                complete: function () {
                    $.whiskey.datatable.reset(false);
                    return true;
                }
            });
        }
        else {
            $.whiskey.web.alert({
                type: "info",
                content: "请选择店铺！",
                callback: function () {
                }
            });
            $(".partiCreat").parents(".modal-content").show();
            return false;
        }
    });
    // 选择商品/批量上传切换
    $('.swithP').switcher({
        //theme: 'square',
        on_state_content: "选择商品",
        off_state_content: "批量上传"
    }).on("click", function () {
        var ch = $(this).is(":checked");
        if (ch) {
            $("#btn_sel").show();
            $("#btnBatchUpload").hide();
        }
        else {
            $("#btn_sel").hide();
            $("#btnBatchUpload").show();
        }
    });
});
// 获取选择的商品货号
function getCheckedProdNums() {
    return checkedNumbers;
}
// post操作
function dataValiPost() {
    var errcou = 0;
    var da = [];
    //活动编号
    var campaignNumber = $("#hideCampaignNumber").val();
    if (campaignNumber && campaignNumber.length > 0) {
        da.push({ CampaignNumber: campaignNumber.trim() });
    }
    //活动名称
    if ($(".clwid #CampaignName").val().trim().length == 0) {
        $(".clwid #CampaignName").attr("title", "活动名不为空").css({ "border": "2px solid red" }).parents("div:eq(0)").addClass("has-error");
        errcou += 1;
    }
    else {
        $(".clwid #CampaignName").css({ "border": "" }).parents("div:eq(0)").removeClass("has-error");
        da.push({ CampaignName: $(".clwid #CampaignName").val().trim() });
    }
    //活动店铺
    var store = $(".clwid #CampaignStore option:selected");
    if (store.length == 0) {
        $("button[class^='multiselect']").attr("title", "所属店铺不为空").css("border", "2px solid red").parents("div:eq(0)").addClass("has-error");
        errcou += 1;
    }
    else {
        $("button[class^='multiselect']").css("border", "").parents("div:eq(0)").removeClass("has-error");
        if (store.val() == "all") {
            var _ids = [];
            $(".clwid #CampaignStore option").not("option[value='all']").each(function () {
                var stid = $(this).val();
                _ids.push(stid);
                da.push({ StoreIds: _ids.join(',') });
            });
        }
        else {
            var ids = [];
            store.each(function () {
                var stid = $(this).val();
                ids.push(stid);
                da.push({ StoreIds: ids.join(',') });
            });
        }
    }
    //活动时间
    var starTime = $(".clwid #StartDate").val();
    var endTime = $(".clwid #EndDate").val();
    if (starTime.length == 0) {
        $(".clwid #StartDate").attr("title", "开始时间不为空").css({ "border": "2px solid red" }).parents("div:eq(0)").addClass("has-error");
        errcou += 1;
    }
    else {
        $(".clwid #StartDate").css("border", "").parents("div:eq(0)").removeClass("has-error");
        da.push({ CampaignStartTime: starTime });
    }
    if (endTime.length == 0) {
        $(".clwid #EndDate").attr("title", "结束时间不为空").css("border", "2px solid red").parents("div:eq(0)").addClass("has-error");
        errcou += 1;
    }
    else {
        $(".clwid #EndDate").css("border", "").parents("div:eq(0)").removeClass("has-error");
        if (Date.parse(starTime) > Date.parse(endTime)) {
            $(".clwid #EndDate").attr("title", "结束时间不能晚于开始时间").css("border", "2px solid red").parents("div:eq(0)").addClass("has-error");
            errcou += 1;
        }
        else {
            $(".clwid #EndDate").attr("title", "").css("border", "").parents("div:eq(0)").removeClass("has-error");
            da.push({ CampaignEndTime: endTime });
        }
    }
    //会员商品折扣
    var dis = parseFloat($(".clwid #membDisc").val());
    if (dis <= 0 || dis > 10) {
        $(".clwid #membDisc").attr("title", "折扣在1-10之间").css("border", "2px solid red").parents("div:eq(0)").addClass("has-error");
        errcou += 1;
    }
    else {
        $(".clwid #membDisc").attr("title", "折扣在1-10之间").css("border", "").parents("div:eq(0)").removeClass("has-error");
        da.push({ MemberDiscount: dis });
    }
    //非会员商品折扣
    var ndis = parseFloat($(".clwid #nomembDisc").val());
    if (ndis <= 0 || ndis > 10) {
        $(".clwid #nomembDisc").attr("title", "折扣在1-10之间").css("border", "2px solid red").parents("div:eq(0)").addClass("has-error");
        errcou += 1;
    }
    else {
        $(".clwid #nomembDisc").attr("title", "折扣在1-10之间").css("border", "").parents("div:eq(0)").removeClass("has-error");
        da.push({ NoMmebDiscount: ndis });
    }
    var ty = $(".membonl option:selected").val();
    da.push({ SalesCampaignType: ty });
    // 获取活动描述
    var descri = $(".clwid #Descript").val().trim();
    if (descri.length == 0) {
        $(".clwid #Descript").attr("title", "活动描述不为空").css("border", "2px solid red").parents("div:eq(0)").addClass("has-error");
        errcou += 1;
    }
    else {
        $(".clwid #Descript").attr("title", "活动描述不为空").css("border", "").parents("div:eq(0)").removeClass("has-error");
        da.push({ Descript: descri });
    }
    // 获取商品款号
    if ($(".clwid tbody tr").length == 0) {
        $(".clwid #btn_sel").attr("title", "请选择参与本次活动的商品").css("border", "2px solid red").parents("div:eq(0)").addClass("has-error");
        errcou += 1;
    }
    else {
        $(".clwid #btn_sel").attr("title", "").css("border", "").parents("div:eq(0)").removeClass("has-error");
        var pns = [];
        $(".clwid tbody tr").each(function () {
            var t = $(this).find("td:eq(2)").text().trim();
            pns.push(t);
        });
        da.push({ prod: pns.join(',') });
    }
    // 是否可以同时参与其他的活动
    if ($("#otherCamp").is(":checked")) {
        da.push({ OtherCampaign: true });
    }
    //是否可以同时和其他的代金券一起使用
    if (!$("#cashCoupon").is(":checked")) {
        da.push({ OtherCashCoupon: false });
    }
    if (errcou == 0) {
        return { err: 0, da: da };
    }
    return { err: 1 };
}
// 初始化table
function initTable(bigProdNums) {
    $(".partiCreat").parents(".modal-content").show();
    // 与已选择的款号比较去重
    if (bigProdNums != undefined && bigProdNums.length > 0) {
        var totalCount = $("#tblCampCreate tbody tr").length;
        $("#tblCampCreate tbody tr").each(function () {
            var pnum = $(this).find("td").eq(2).text().trim();
            var ind = bigProdNums.indexOf(pnum);
            if (ind != -1) {
                bigProdNums.splice(ind, 1); // 删除重复项
            }
        });
        if (bigProdNums.length == 0)
            return false;
        //totalCount += bigProdNums.length;
        // 请求新选择的款号信息,追加到table中
        $.post("/Properties/SalesCampaign/GetProductsByNums", { bigProdNums: bigProdNums }, function (da) {
            var trs = "";
            if (da.ResultType == 3) {
                for (var i = 0; i < da.Data.length; i++) {
                    var t = da.Data[i];
                    trs += "<tr>";
                    trs += "<td>" + t.Id + "</td>";
                    trs += "<td>" + t.ProductName + "</td>";
                    trs += "<td>" + t.BigProdNum + "</td>";
                    trs += "<td>" + t.BrandName + "</td>";
                    trs += "<td>" + t.CategoryName + "</td>";
                    trs += "<td>" + t.SeasonName + "</td>";
                    trs += "<td><div class='thumbnail-img_five_box'> <div class='thumbnail-img_five'> <div class='thumbnail-img_f'><img class='popimg' src='" + t.ThumbnailPath + "'/></div></div></div></td>";
                    trs += "<td><a href=\"javascript:;\" class=\"clre\">\u5220\u9664</a></td>";
                    trs += "</tr>";
                }
                $("#tblCampCreate tbody").append(trs);
                numCount += da.Data.length;
                reloadTable();
                totalCount += da.Data.length;
                $("#productCount").text(totalCount.toString());
            }
        });
    }
}
