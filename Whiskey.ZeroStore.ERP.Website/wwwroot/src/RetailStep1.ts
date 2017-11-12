let codelist = [];
let camps = {};
let hashlist = new $.whiskey.hashtable(); // 队列
let memberInfoGlobal = {};

/**
 * 默认不打折,折扣值为10
 */
let DEFAULT_DISCOUNT = 10;
$(function () {
    $("#outstoreid").queryManageStore({
        defaultSelect: true,
        callback: function () {
            if ($("#outstoreid option").length == 2) {
                $("#outstoreid").parents('div.stor_li_box').hide();
            }
        }
    });
    // 监听扫码
    setInterval(function () {
        sendToQueue();
    }, 1000);

    // [选择商品]按钮handler
    $(".but_prod_sel").click(function () {
        let storeid = $("#outstoreid option:selected").val();
        //获取已经在购买列表中的货号
        let productNumberArr = [];
        let $spans = $("span.pnum");
        if ($spans.length > 0) {
            $spans.each(function (index, ele) {
                productNumberArr.push($(ele).text());
            });
        }

        if (storeid == "") {
            Info("请选择店铺！");
        } else {
            let dialog = new $.whiskey.web.ajaxDialog({
                caption: "选择商品",
                actionUrl: "/Stores/Retail/GetProductList?storeid=" + storeid + "&productNumbers=" + productNumberArr.join(","),
                lockButton: $(this),
                successEvent: function () {
                    let nums = getCheckedProdNums();
                    if (nums != null && nums.length > 0) {
                        let dat = getjsondataOfBarcode(nums);
                        getbarcodeinfoByDa(dat);
                    }
                },
                formValidator: function () {

                    let $form = $(".modal-form");
                    if (!$form.valid()) {
                        return false;
                    } else {
                        return true;
                    }
                },
                postComplete: function () {
                    $.whiskey.datatable.reset(false);
                },
            });
        }
    });


    // 会员/非会员切换设置
    $('.swithP').switcher({
        on_state_content: "非会员",
        off_state_content: "会员"
    }).on("click", function () {

        let isChecked = $(this).is(":checked");
        if (!isChecked) {
            memberInit(); // 会员
        } else {
            noMemberInit();// 非会员
        }
        $.whiskey.datatable.reset(false);
    });

    //回车登录
    $("input.memb_pwd").keyup(function (data) {
        if (data.keyCode == 13) {
            $(".but_memb_succ").trigger('click');
        }
    });

    // 默认显示非会员支付
    noMemberInit();
});

interface MemberLoginRes {
    Id: number;
    MembNum: string;
    MemberName: string;
    RealName: string;
    Balance: number;
    Score: number;
    StoreId: number;
    UniquelyIdentifies: string;
    LevelDiscount: number;
}

$(document).ready(function () {

    $(".btn-applogin").click(function () {
        let _self = this;
        if ($(_self).prop("disabled")) {
            return;
        }
        $(_self).prop("disabled", true);


        // 校验输入
        let checkRes = checkBeforeLogin(false) as any;
        if (!checkRes) { $(_self).prop("disabled", false); return; }

        let loginName = checkRes.loginName;
        let storeId = checkRes.storeId;

        $(".btn-applogin").text("推送中...");
        var data = { loginName: loginName };
        $.post("/Stores/Retail/PushAPPConfirmLogin", data)
            .done(function (res: OperationResult<any>) {
                $(_self).prop("disabled", false);
                //console.log(res);

                if (res.ResultType !== OperationResultType.Success) {
                    Info(res.Message);
                    resetAPPLoginText();
                    return;

                }
                $(_self).text("已推送,等待APP确认...");

                let memberId = res.Data.memberId;
                //console.log(memberId);
                // 查询登录状态
                setTimeout(function () {
                    queryMemberLoginStat(memberId, storeId);
                }, 0);
            })

    });

    function resetAPPLoginText() {
        $(".btn-applogin").text("app登陆");
    }

    function queryMemberLoginStat(memberId: number, storeId: number) {

        $.post("/Stores/Retail/QueryLoginStat", { memberId: memberId, storeId: storeId })
            .done(function (res: OperationResult<MemberLoginRes>) {


                if (res.ResultType !== OperationResultType.Success) {
                    resetAPPLoginText();
                    if (<any>res.Message == ConfirmLoginStat.已拒绝) {

                        Info("已拒绝");
                    }
                    return;
                }

                // 轮询
                if (!res.Data ) {
                    // 等待中
                    setTimeout(queryMemberLoginStat, 2000, memberId, storeId);
                }
                else {
                    // success
                    resetAPPLoginText();
                    loginPassProcess(res.Data);

                }

            })
    }

    // #region datatable设置

    $.whiskey.datatable.instance = $(".table-list").dataTable({
        "sDom": '<"top"<"clear">>t',  // 禁用会自动出现loading..图标，暂时留它一时
        "sAjaxSource": "/Stores/Retail/GetProductsByBarcode",
        "bDestroy": "true",
        "fnRowCallback": function (nRow: HTMLTableRowElement, aData, iDisplayIndex) {
            $(nRow).addClass("treegrid-" + aData.Id + (aData.ParentId != null ? " treegrid-parent-" + aData.ParentId : ""));
            if (aData.ParentId == "") {
                $("td:eq(1)", nRow).css({ "width": "20%", "text-align": "left" });

                $(nRow).attr("branddiscount", aData.RetailDiscount);//品牌折扣
                $(nRow).attr("productactivitydiscount", DEFAULT_DISCOUNT);//默认没有选择商品活动,打折为10
                $(nRow).attr("retailprice", aData.RetailPrice);
                $(nRow).attr("tagprice", aData.TagPrice);
            }
            else {
                $("td:eq(1)", nRow).css({ "width": "22%", "text-align": "right" });
            }
            $("td:eq(7)", nRow).css({ "width": "9%" });
            $("td:eq(8)", nRow).css({ "width": "10%" });
            $("td:eq(8)", nRow).css({ "width": "4%" });

            // 为[合计金额]所在td添加class
            $("td:eq(-3)", nRow).addClass("mongCou").css({ "color": "red" });

            // 为[数量]所在td添加class
            $("td:eq(-4)", nRow).addClass("salCou");

            // 为零售价所在td添加class
            $("td:eq(7)", nRow).addClass("price");

            return nRow;
        },
        "fnServerData": function (sSource, aoData, fnCallback) {
            $.ajax({
                "dataType": 'json',
                "type": "POST",
                "url": sSource,
                "data": aoData,
                "beforeSend": function () {
                    if (codelist.length == 0) {
                        $("#prodlis_tab tbody").html("<tr class='mes_inf'><td colspan='13'>还未购买商品……</td></tr>");
                        getConsumeCount();
                        return false;
                    }
                    $(".prodcou").attr("readonly", "readonly");
                },
                "complete": function () {
                    $(".prodcou").removeAttr("readonly");
                    $(".memb_i").removeClass("fa-chevron-down").addClass("fa-chevron-right");
                    $(".memb_i").click();
                },
                "success": function (da) {
                    // 获取到选中的库存商品
                    if (da.Other.result.ResultType != 3) {
                        codelist.pop();
                        Info(da.Other.result.Message);

                    } else {
                        fnCallback(da);
                        $("#outstoreid option[value='" + da.Other.storeid + "']").attr("selected", "selected");
                        let moncou = 0;
                        $(".pnum").each(function () {
                            let num = $(this).text();
                            let camp = $(this).parents("tr:eq(0)").find(".prodisc option:selected");
                            let campid = camp.attr("campid");
                            let campval = camp.val();
                            $(camps).attr[num] = campid;
                            $(camps).attr[num + "_val"] = campval;

                            let consum = $(this).parents("tr:eq(0)").find("td:eq(-3)").text();
                            moncou += parseFloat(consum);

                        });
                        changeSumPrice(moncou.toFixed(2));
                        $(".consume_count").val(moncou.toFixed(2));
                        getConsumeCount();
                    }

                },
                "timeout": 15000, // optional if you want to handle timeouts (which you should)
                // "error": handleAjaxError // this sets up jQuery to give me errors
            });
        },
        "fnDrawCallback": function () {
            $(".table-list").treegrid({
                initialState: "collapsed",
                saveState: true,
                treeColumn: 1,
                expanderExpandedClass: 'treegrid-expander-expanded',
                expanderCollapsedClass: 'treegrid-expander-collapsed'
            });

            // 遍历每一行,检测是否有商品活动
            let $trs = $(".table-list tr");

            // 优先应用首个商品活动折扣
            $.each($trs, function (index, nRow) {
                let $select = $("td:eq(-5)", nRow).find("select");
                var option = $select.find("option:eq(0)");
                if (option.data("campaignid")) {
                    $select.trigger("change");
                }
            });
        },
        "fnServerParams": function (aoData) {
            let conditions = new $.whiskey.filter.group();
            let tli = [];

            // 使用弹出层里选中的商品货号搜索
            $.each(codelist, function (i, v) {
                if (v != "")
                    tli.push(v);
            });
            if (codelist.length > 0) {
                conditions.Rules.push(new $.whiskey.filter.rule("ProductBarcode", tli, "in"));
            }
            let storeid = $("#outstoreid option:selected").val();

            if (storeid != null) {
                aoData.push({ name: "storeId", value: storeid });
            }


            aoData.push({ name: "conditions", value: JSON.stringify(conditions) });
            aoData.push({ name: "barcode", value: codelist });


        },
        "aoColumns": [
            {
                "bVisible": false,
                "bSearchable": false,
                "sName": "Id",
                "mData": "Id"
            },
            {
                "sTitle": '已选择',
                "sName": "Id",
                "bSortable": false,
                "bSearchable": false,
                "mData": function (data) {
                    return '<label class="px-single"><input type="checkbox" value="' + data.Id + '" class="px te_1_che" checked="checked" disabled="disabled"><span class="lbl"></span></label>';
                }
            },
            {
                "sTitle": "货号/流水号",
                "bSortable": false,
                "sName": "ProductNumber",
                "mData": function (data) {
                    if (data.ParentId == "")
                        return "<span class='pnum' style='color:blue;text-align:left'>" + data.ProductNumber + "</span>";
                    return "<span class='pbarcode' style='text-align:right'>" + data.ProductNumber + "</span>";
                },
            },
            {
                "sTitle": "图片",
                "bSortable": false,
                "sName": "ThumbnailPath",
                "mData": function (data) {
                    if (data.ParentId == "") {
                        return "";
                    }
                    if (!data.ThumbnailPath || data.ThumbnailPath.length == 0) {
                        return "无";
                    }
                    return "<div class='thumbnail-img_five_box'><div class='thumbnail-img_five'><div class='thumbnail-img_f'><img class='popimg' src='" + data.ThumbnailPath + "'/></div></div></div>";
                },
            },
            {
                "sTitle": "品牌",
                "bSortable": false,
                "sName": "BrandName",
                "mData": function (data) {
                    return data.BrandName;
                },
            },
            {
                "sTitle": "尺码",
                "bSortable": false,
                "sName": "SizeName",
                "mData": function (data) {
                    return data.SizeName;
                },
            },
            {
                "sTitle": "颜色",
                "bSortable": false,
                "sName": "ColorName",
                "mData": function (data) {
                    return "<img style='width:35px' src='" + data.IconPath + "' title='" + data.ColorName + "'/>";
                }
            },
            {
                "sTitle": "吊牌价(￥)",
                "bSortable": false,
                "sName": "TagPrice",
                "mData": function (data) {
                    let span = (data.TagPrice as number).toFixed(2);
                    return span;
                }
            },
            {
                "sTitle": "零售价(￥)",
                "bSortable": false,
                "sName": "OrderGuid",
                "mData": function (data) {
                    let span = generateRetailPriceSpanText(data.RetailPrice, data.RetailDiscount, DEFAULT_DISCOUNT, DiscountType.BrandDiscount);
                    return span;
                }
            },
            {
                "sTitle": "活动",
                "bSortable": false,
                "sName": "OrderGuid",
                "mData": function (data) {

                    if (data.ParentId == "") {
                        return getComp(data.SalesCampaign);
                    }
                    return "";
                }
            },
            {
                "sTitle": "数量",
                "bSortable": false,
                "sName": "MemberId",
                "mData": function (data) {
                    if (data.ParentId == "")
                        return getProCou(data.CurCou, data.Cou, 1);
                    return "";
                }
            },
            {
                "sTitle": "合计金额(￥)",
                "bSortable": false,
                "sName": "MemberId",
                "mData": function (data): any {
                    if (data.ParentId == "") {
                        let money = parseFloat(getCountMone(data.RetailPrice, DEFAULT_DISCOUNT, data.CurCou));
                        return money.toFixed(2);
                    }
                    return "";
                }
            },
            {
                "sTitle": "<span style='color:blue'>本店</span>/总库存",
                "bSortable": false,
                "sName": "OrderStatus",
                "mData": function (data) {
                    if (data.ParentId == "") {

                        return data.Cou + "/" + data.AllCou;
                    }
                    return "";
                }
            },
            {
                "sTitle": "操作",
                "bSortable": false,
                "sName": "Notes",
                "mData": function (data) {
                    return "<img class='remove_ico' width='30px' src='/Theme/default/img/nav_icon/sc.png'/>";

                }
            }
        ],
        "initComplete": function (settings, json) {
            $('.cur_selectpicker').selectpicker();
            $('.cur_selectpicker').selectpicker('refresh');
            $(".checked-all").click(function () {

            });
        }

    });

    // #endregion

    // table默认显示
    $("#prodlis_tab tbody").html("<tr class='mes_inf'><td colspan='13'>还未购买商品……</td></tr>");

    function getSelectStore(): number {
        let storeId = parseInt($("#outstoreid option:selected").val());
        if (!storeId || storeId <= 0 || isNaN(storeId)) {
            return storeId = -1;
        }
        return storeId;
    }

    // 搜索区->[商品条码]输入框
    $("#ProductNumber").keyup(function (event) {
        if (event.keyCode == 13) {
            let tx = $("#ProductNumber").val();
            let storeid = $("#outstoreid option:selected").val();
            if (storeid == "") {
                Info("请选择店铺！");
            } else if (tx.length > 0) {

                referData();
            }


        }
    });

    // 搜索区->[确定]按钮
    $(".but_prod_succ").click(function () {
        let storeid = $("#outstoreid option:selected").val();
        if (storeid == "") {
            Info("请选择店铺！");
        } else if ($("#ProductNumber").val().trim().length > 0) {

            referData();
        }
    });

    $(".container").delegate(".prodcou", "change", function () {
        let count = parseInt($(this).val());
        count = Math.max(1, count);
        $(this).val(count);
        // 改变商品数量
        let da = getjsondataOfBarcode();
        getbarcodeinfoByDa(da);
    }).delegate(".remove_ico", "click", function () {
        // 移除按钮
        removeRow(this);
    });

    // [选择会员]按钮
    $(".but_memb_sel").click(function () {
        let dialog = new $.whiskey.web.ajaxDialog({
            caption: "选择会员",
            actionUrl: "/Subjects/Collocation/MembList",
            diacl: "dia",
            lockButton: $(this),
            successEvent: function () {
                let t = $("input:radio:checked").parents("tr").children("td:eq(1)").text().trim();
                if (t != "") {
                    $("#membNum").val(t);
                    $("#membName").val("");
                    $(".memb_pwd").val("");
                    $(".curr_mon").val("");
                    $(".score_cou").val("");
                    $(".collocation_num").val("");
                    $(".collocation_name").val("");
                    $(".cardmoney_consum").val("");
                    $(".score_consum").val("");
                    $(".cash_consum").val("");
                    $(".swipcard_consum").val("");
                    $(".erase_consum").val("");
                    $(".retumoney_consum").val("");
                    $(".coupon option:gt(0)").remove();
                }
            },

        });

    });

    /**
     * 登录校验
     */
    function checkBeforeLogin(checkPass = true): any {

        let storeId = getSelectStore();
        let loginName = $("#membNum").val().trim() as string;
        let pass = $(".memb_pwd").val().trim() as string;

        if (storeId <= 0) {
            Info("请选择店铺！");
            return false;
        }
        if (!loginName || loginName.length <= 0) {
            Info("请填写会员登录名");
            return false;
        }
        if (checkPass) {

            if (!pass || pass.length <= 0) {
                Info("请填写会员登录密码");
                return false;
            }
        }
        return {
            loginName: loginName,
            pass: pass,
            storeId: storeId
        }
    }


    // 会员区->[确定]按钮
    $(".but_memb_succ").click(function () {
        var checkRes = checkBeforeLogin();
        if (!checkRes) { return; }


        $(this).attr("disabled", "disabled");
        const url = '/Retail/MemberValid';
        let postData = {
            loginName: checkRes.loginName,
            passwd: checkRes.pass,
            storeId: checkRes.storeId
        };
        $.post(url, postData).done((res: OperationResult<MemberLoginRes>) => {

            $(".but_memb_succ").removeAttr("disabled");

            if (res.ResultType != OperationResultType.Success) {
                Info(res.Message);
                $("#membNum").attr("datnum", 0);
                $("#membName").val("");
                $(".curr_mon").val(0);
                $(".score_cou").val(0);
                return;
            }
            debugger;
            loginPassProcess(res.Data);

        });//end post


    });


    function loginPassProcess(data: MemberLoginRes) {
        // 登录成功
        let memberProfile = data
        memberInfoGlobal = data; //获取会员信息
        $.whiskey.web.alert({
            type: "success",
            content: "会员登录成功",
            callback: function () {
            }
        });
        debugger;
        $("#hidden_memberId").val(data.Id);
        $("#membNum").attr("datnum", memberProfile.MembNum);  // 会员卡号
        $("#membName").val(memberProfile.RealName);           // 会员名字
        $(".curr_mon").val(memberProfile.Balance.toFixed(2));            // 储值余额
        $(".score_cou").val(memberProfile.Score.toFixed(2));             // 积分余额


        // 刷新商品活动
        $.whiskey.datatable.reset(false);
        $("#timer").show();
        timer(300, function () {
            location.reload();
        });
    }

    // 选择搭配师
    $(".collocation_succ").click(function () {
        let num = $(".collocation_num").val().trim();
        $.post("/Collocation/GetCollcationInfo", { num: num }, function (da) {
            if (da.ResultType != 3) {
                Info("该搭配师账户不存在或已注销！");
            } else {
                $(".collocation_num").attr("coll_num", da.Data.Numb);
                $(".collocation_name").val(da.Data.name);
            }
        });

    });


    // #region 通用处理

    $("#Print").on("click", function () {
        let list = $.whiskey.web.getIdByChecked(".table-list td input[type=checkbox]");
        if (list.length > 0) {
            let printer = $.whiskey.printer.ajaxPreview({
                actionUrl: "/Stores/Retail/Print",
                lockButton: $(this),
                topMargin: "2%",
                leftMargin: "4%",
                contentWidth: "93.5%",
                contentHeight: "100%",
                params: list
            });
        }
        else {
            Info("请至少选择一条数据！");
        }
    });

    $("#Export").on("click", function () {
        let list = $.whiskey.web.getIdByChecked(".table-list td input[type=checkbox]");
        if (list.length > 0) {
            let printer = $.whiskey.exporter.ajaxExport({
                actionUrl: "/Stores/Retail/Export",
                lockButton: $(this),
                fileName: "新导出文件",
                topMargin: 10,
                leftMargin: 10,
                contentWidth: "98%",
                contentHeight: "100%",
                params: list
            });
        } else {
            Info("请至少选择一条数据！");
        }
    });


    // 批量移除列表
    $("#RemoveAll").on("click", function () {
        let list = $.whiskey.web.getIdByChecked(".table-list td input[type=checkbox]");
        if (list.length > 0) {
            let confirm = new $.whiskey.web.ajaxConfirm({
                question: "确认要将这些商品移除吗吗？",
                notes: "提示：数据移动到回收站后，可以重新添加",
                success_event: function () {
                    RemoveRows();
                },
                params: list,
                complete: function () {

                }
            });
        } else {
            Info("请至少选择一条数据！");
        }
    });


    // #endregion

    // 切换店铺
    $("#outstoreid").change(function () {
        let storeid = $(this).find("option:selected").val();

        // 判断是否已经选了商品,切换后会清空选中的商品
        let allcou = $("tbody tr").not(".mes_inf").length;
        if (allcou > 0) {
            let confirm = new $.whiskey.web.ajaxConfirm({
                question: "更换店铺可能会导致下面的商品丢失？",
                notes: "提示：如果新的店铺中没有指定的商品，则原有的商品会被移除！",
                lockButton: $("#outstoreid"),
                success_event: function () {
                    codelist = [];
                    $("#prodlis_tab tbody").html("<tr class='mes_inf'><td colspan='13'>还未购买商品……</td></tr>");
                    refreshMemberLogin();
                },
                cancel_event: function () {
                    let stoid = $("#outstoreid").attr("origsel");
                    $("#outstoreid option[value='" + stoid + "']").prop("selected", true);
                }
            });
        } else {
            $("#outstoreid").attr("origsel", storeid);
            refreshMemberLogin();
        }

    });
    function refreshMemberLogin() {
        if (isMemb()) {
            $("#membNum").val('');
            $(".memb_pwd").val('');
            $("#membName").val('');
            $(".curr_mon").val('');
            $(".score_cou").val('');
        }
    }

    /**
     * 加载step2视图
     */
    $("#btnStep2").click(function () {
        //获取总金额
        let totalPrice = parseFloat($(".moncou_span").text());
        let storeId = $("#outstoreid").val();
        if (!storeId || isNaN(storeId)) {
            Info("请选择店铺！");
            return;
        }

        var products = getProductsInfo();
        if (products && products.length <= 0) {
            Info("请选择商品！");
            return;
        }
        let campIds = _.map(products, x => x.CampId);
        let memberId = isMemb() ? $("#hidden_memberId").val() : '';
        let postData = { totalPrice: totalPrice, storeId: storeId, campIds: campIds.join(','), memberId: memberId };
        $.post('/Stores/Retail/Step2', postData, function (res) {
            $(".step1").hide();
            $(".step2").append(res);
        }, "html");
        //location.href = '/Stores/Retail/Step2?totalPrice=' + totalPrice;
        return;
    });

});



/**
 * 计算本行商品的合计金额
 * 
 * @param {any} sender 每行活动列表select标签
 */
function calcMoney(sender: HTMLSelectElement): CalcMoneyRes {
    //<option data-memberdiscount="7" data-nomemeberdiscount="8" data-campaigntype="2">莲湖店商品活动</option>
    let option = $(sender).find("option:selected")[0] as HTMLOptionElement;
    let tagPrice = parseFloat($(sender).parents("tr").attr("tagprice"));
    let retailPrice = parseFloat($(sender).parents("tr").attr("retailprice"));
    let brandDiscount = parseFloat($(sender).parents("tr").attr("branddiscount"))
    let productCount = parseInt($(sender).parents("tr").find(".prodcou").val());

    if (option.text.indexOf("不参与活动") > 0 && option.value == "10") {
        //没有选择活动,不打折, 小计 = 零售价*数量
        let total = parseFloat(getCountMone(retailPrice, DEFAULT_DISCOUNT, productCount));
        return {
            RetailPrice: retailPrice,
            TotalPrice: total,
            ProductActivityDiscount: DEFAULT_DISCOUNT,
            BrandDiscount: brandDiscount,
            DiscountType: DiscountType.BrandDiscount
        }
    }
    else {
        // 活动可用的情况下,零售价需要重新计算(吊牌价*活动折扣), 小计 = 新零售价*活动折扣*数量
        let isMember = isMemb();
        let discount = 10;
        let memberdiscount = parseFloat($(option).data("memberdiscount"));
        let nomemeberdiscount = parseFloat($(option).data("nomemeberdiscount"));
        let campaigntype = parseInt($(option).data("campaigntype")) as SalesCampaignType;

        //校验活动是否适用当前用户类型
        if (isMember && campaigntype == SalesCampaignType.NoMemberOnly) {
            alert("该活动仅非会员可用");
            $(sender).val(DEFAULT_DISCOUNT);

            //不打折, 小计 = 零售价*数量
            let total = parseFloat(getCountMone(retailPrice, 10, productCount));
            return {
                RetailPrice: retailPrice,
                TotalPrice: total,
                ProductActivityDiscount: DEFAULT_DISCOUNT,
                BrandDiscount: brandDiscount,
                DiscountType: DiscountType.BrandDiscount
            }
        }

        if (!isMember && campaigntype == SalesCampaignType.MemberOnly) {
            alert("该活动仅会员可用");
            $(sender).val(DEFAULT_DISCOUNT);
            //不打折, 小计 = 零售价*数量
            let total = parseFloat(getCountMone(retailPrice, 10, productCount));
            return {
                RetailPrice: retailPrice,
                TotalPrice: total,
                ProductActivityDiscount: DEFAULT_DISCOUNT,
                BrandDiscount: brandDiscount,
                DiscountType: DiscountType.BrandDiscount
            }
        }



        // 得到折扣
        switch (campaigntype) {
            case SalesCampaignType.MemberOnly:
                {
                    discount = memberdiscount;
                    break;
                }
            case SalesCampaignType.NoMemberOnly:
                {
                    discount = nomemeberdiscount;
                    break;
                }
            case SalesCampaignType.EveryOne:
                {
                    discount = isMember ? memberdiscount : nomemeberdiscount;
                    break;
                }
            default: discount = DEFAULT_DISCOUNT;
        }

        // 计算1件商品打折后的零售价
        retailPrice = parseFloat(getCountMone(tagPrice, discount, 1))
        // 计算多件商品小计金额
        let total = parseFloat(getCountMone(tagPrice, discount, productCount));
        return {
            RetailPrice: retailPrice,
            TotalPrice: total,
            ProductActivityDiscount: discount,
            BrandDiscount: brandDiscount,
            DiscountType: DiscountType.ProductActivityDiscount
        }

    }
    //let num = $(this).parents("tr").find(".pnum").text();
    //let campid = camp.attr("campid");
    //let campval = camp.val();
    //camps.attr[num] = campid;
    //camps.attr[num + "_val"] = campval;
}

function saleCampaignHandler(sender: HTMLSelectElement) {
    let trElement = $(sender).parents("tr")[0]
    let rowTotalMonedTD = $(sender).parents("tr").find(".mongCou")[0] as HTMLTableDataCellElement;
    let retailPriceTD = $(sender).parents("tr").find(".price")[0] as HTMLTableDataCellElement;

    // 重新计算本行商品的零售价,小计 
    let calcRes = calcMoney(sender);

    // 更新商品活动折扣
    $(trElement).attr("productactivitydiscount", calcRes.ProductActivityDiscount);

    // 更新零售价及折扣展示
    let text = generateRetailPriceSpanText(calcRes.RetailPrice, calcRes.BrandDiscount, calcRes.ProductActivityDiscount, calcRes.DiscountType);
    retailPriceTD.innerHTML = text;

    // 同步子项的零售价展示
    var parentId = /treegrid-p(\d+)/.exec(trElement.className)[1];
    var childrenClassName = "treegrid-parent-p" + parentId;
    $(trElement).nextAll("tr." + childrenClassName).each(function (index, elem) {
        $(elem).find(".price")[0].innerHTML = text;
    })

    // 更新小计金额
    rowTotalMonedTD.innerText = calcRes.TotalPrice.toFixed(2);


    // 更新所有商品的合计金额
    let moncou = updateProductTotalPrice();
    changeSumPrice(moncou.toFixed(2));
}

/**
 * 得到活动
 * 
 * @param {GetBarcodeRes[]} saleCampaignArr
 * @param {any} [campid]
 * @returns {string}
 */
function getComp(saleCampaignArr: GetBarcodeRes[], campid?): string {
    let camp = `<select class='prodisc form-control cur_selectpicker' onchange='saleCampaignHandler(this)' style='width:200px;'>`;


    if (saleCampaignArr.length > 0) {
        for (let i = 0; i < saleCampaignArr.length; i++) {
            camp += `<option data-memberdiscount='${saleCampaignArr[i].MemberDiscount}' 
            data-nomemeberdiscount='${saleCampaignArr[i].NoMmebDiscount}' 
            data-campaigntype='${saleCampaignArr[i].SalesCampaignType}' 
            data-campaignid='${saleCampaignArr[i].Id}'>
                        ${saleCampaignArr[i].CampaignName}
                    </option>`;
        }
    }
    else {
        camp += `<option value='${DEFAULT_DISCOUNT}'>-不参与活动-</option>`;
    }
    camp += "</select>";
    return camp;
}

// 得到商品数量
function getProCou(cur, max, min) {
    if (cur == undefined)
        cur = 1;
    if (max == undefined)
        max = 1;
    if (min == undefined)
        min = 1;
    return "<input class='prodcou' style='width:50px' type='number' max='" + max + "' min='" + min + "' value='" + cur + "'>";
}

// 获取所有购买商品信息
function getProductsInfo(): ProductInfo[] {
    let li: ProductInfo[] = [];

    //找到所有的parent节点计算
    $(".pnum").parents("tr").each(function () {
        let che = $(this).find($(".te_1_che"));
        if ($(che).is(":checked")) {
            let num = $(this).find(".pnum").text(); // 商品编号
            let option = $(this).find(".prodisc").find("option:selected");
            let quantity = $(this).find(".prodcou").val(); // 数量

            let className = ".treegrid-parent-" + $(che).val();
            let barcodes = [];
            $(className).each(function () {
                let chec = $(this).find(".te_1_che");
                if ($(chec).is(":checked")) {
                    let code = $(this).find(".pbarcode").text();
                    barcodes.push(code);
                }
            });
            var hasCamp = $(option).data("campaignid");
            if (!hasCamp || isNaN(parseInt(hasCamp)) || parseInt(hasCamp) <= 0) {
                //未参与活动
                li.push({
                    ProdNum: num,
                    CampId: null,
                    Quantity: quantity,
                    CampType: null,
                    Barcodes: barcodes,
                    CampDiscount: null
                });
            }
            else {
                let campaignId = parseInt($(this).find(".prodisc").find("option:selected").data("campaignid")); //活动id
                let campaignType = parseInt($(this).find(".prodisc").find("option:selected").data("campaigntype")); //活动类型
                let CampDiscount = parseFloat($(this).attr("productactivitydiscount"));
                li.push({
                    ProdNum: num,
                    CampId: campaignId,
                    Quantity: quantity,
                    CampType: campaignType,
                    Barcodes: barcodes,
                    CampDiscount: CampDiscount
                });
            }

        }
    });
    if (li.length == 0) {
        return null;
    }
    return li;
}

/**
 * 是否会员
 * 
 * @returns {boolean}
 */
function isMemb(): boolean {
    let membnum = $("#membNum").attr("datnum");
    let ismem = !$(".swithP").is(":checked");
    ismem = membnum != undefined && membnum != "" && ismem;
    return ismem;
}

// 获取 会员信息
function getMemberInfo() {
    if (isMemb()) {
        let menum = $("#membNum").attr("datnum");
        let collnum = $(".collocation_num").val();
        return { MemberNum: menum, CollNum: collnum };
    } else {
        return null;
    }
}


//根据返回的数据重新加载活动
function refleSalecamps(camps) {
    let membnum = $("#membNum").attr("datnum");
    let opts = "<option value='10'>-不参加活动-</option>";
    if (camps.Data.length != 0) {

        for (let j = 0; j < camps.Data.length; j++) {
            let cm = camps.Data[j];
            let cmname = cm.CampaignName;
            let cmid = cm.Id;
            let cmtype = cm.SalesCampaignType;
            let cmmemdis = cm.MemberDiscount;
            let cmnomedis = cm.NoMmebDiscount;
            let othercash = cm.OtherCashCoupon;
            let ismem = isMemb(); //!$(".swithP").is(":checked");
            if (cmmemdis != undefined && membnum != undefined && membnum != "" && ismem) {
                let name = cmname + "[会员]";
                if (cmnomedis != undefined) {
                    if (cmmemdis == cmnomedis)
                        opts += "<option campid=" + cmid + " campTy='1' value=" + cmmemdis + ">" + name + "</option>";
                    else {
                        opts += "<option campid=" + cmid + " campTy='1' value=" + cmmemdis + ">" + name + "</option>";
                        opts += "<option campid=" + cmid + " campTy='0' value=" + cmmemdis + ">" + cmname + "</option>";
                    }
                } else {
                    opts += "<option campid=" + cmid + " campTy='1' value=" + cmmemdis + ">" + name + "</option>";
                }
            } else {
                opts += "<option campty='0' campid=" + cmid + " value=" + cmnomedis + ">" + cmname + "</option>";
            }
        }

    }
    return opts;
}





function getjsondataOfBarcode(nums?) {
    let da = [];
    let curtr = $(".pnum").parents("tr");
    for (let i = 0; i < curtr.length; i++) {
        let pnum = $(curtr[i]).find(".pnum").text(); // 商品编号
        let cou = $(curtr[i]).find(".prodcou").val(); // 数量
        let curval = $(curtr[i]).find(".te_1_che").val();
        // treegrid-parent-p13791
        let chilCl = ".treegrid-parent-" + curval;
        let barcodes = [];
        $(chilCl).each(function () {
            let code = $(this).find(".pbarcode").text();
            if (barcodes.length < cou && cou != 0) {
                barcodes.push(code);
            } else {
                let ind = codelist.indexOf(code);
                codelist.splice(ind, 1);
            }
            if (cou == 0) {
                $(this).remove();
            }
        });
        da.push({ ProductNumb: pnum, ExisBarcode: barcodes, NeedCou: cou });
    }
    if (nums != undefined && nums.length > 0) {
        for (let i = 0; i < nums.length; i++) {
            da.push({ ProductNumb: nums[i], ExisBarcode: [], NeedCou: 1 });
        }
    }
    return da;

}

// 根据条码和商品编号获取商品信息
// pnums:[a,b,c]  a:[d]  d:{ProductNumb:"xxxxx",ExisBarcode:["xxxxx001","xxxxx002"],NeedCou:3}
function getbarcodeinfoByDa(pnums) {
    if (pnums != undefined && pnums.length > 0) {
        let storeId = $("#outstoreid option:selected").val();
        let postData = {
            retailMods: pnums,
            storeId: storeId
        }
        $.post("/Stores/Retail/GetBarcodes", postData, function (da) {

            for (let i = 0; i < da.length; i++) {
                let ind = codelist.indexOf(da[i]);
                if (ind == -1)
                    codelist.push(da[i]);
            }
            if (codelist.length > 0)
                $.whiskey.datatable.reset(false);
        });
    }
}




// 计算价格
function getConsumeCount() {
    let cou = $("#prodlis_tab tbody tr .pnum").length;
    let trs = $("#prodlis_tab tbody .pnum").parents("tr");
    let checou = trs.length;
    let procou = 0;
    trs.each(function () {
        let coun = $(this).children().eq(-4).find("input").val();
        procou += parseInt(coun);
    });
    if (cou > 0) {

        $(".couinf").show().html("").html("共" + cou + "款，已勾选" + checou + "款，总计" + procou + "件");
    }
    else {
        $(".couinf").hide();
    }

    // 修改合计金额展示
    let moncou = updateProductTotalPrice();
    changeSumPrice(moncou.toFixed(2));

}

// 刷新商品条码
function referData(num?) {
    if (num == undefined) {
        num = $("#ProductNumber").val();
    }
    EnterQue(num);
}


// [清空]处理
function clear_context(sender) {

    let confirm = new $.whiskey.web.ajaxConfirm({
        question: "确认要清空结算信息吗？",
        notes: "提示：当前页面结算信息都会被清空！",
        lockButton: $(sender),
        "success_event": function () {
            $(".memb-cu").find("input").val("");
            $("#membNum").removeAttr("datnum");
            $.retail.member.remove();
        }
    });
}

// 详细信息
function View(sender, Id) {
    let view = new $.whiskey.web.ajaxView({
        caption: "详细信息",
        actionUrl: "/Stores/Retail/View",
        params: { Id: Id },
        lockButton: $(sender),
    });
}

// 删除一条
function Delete(sender, Id) {
    let confirm = new $.whiskey.web.ajaxConfirm({
        question: "确认要彻底删除这条数据吗？",
        notes: "提示：数据彻底删除后不可恢复，请谨慎操作！",
        actionUrl: "/Stores/Retail/Delete",
        params: { Id: Id },
        lockButton: $(sender),
        complete: function () {
            $.whiskey.datatable.reset(true);
        }
    });
}


/**
 * 计算商品总金额，basePrice:单价，discount:折扣，quantity:件数
 * 
 * @param {any} basePrice
 * @param {any} discount
 * @param {any} quantity
 * @returns {number}
 */
function getCountMone(basePrice, discount, quantity): string {

    if (!(isNaN(basePrice) && isNaN(discount) && isNaN(quantity))) {
        let cou = (basePrice * discount / 10 * quantity) + 0.001 + "";
        cou = parseFloat(cou).toFixed(2);
        return cou;
    }
    else {
        return "";
    }
}

// 计算商品折扣，保留两位小数
function getdiscount(t1, t2) {
    let re = (t1 / t2 * 10) + 0.001 + "";
    re = re.substring(0, re.indexOf('.')) + re.substr(re.indexOf('.'), 3);
    return parseFloat(re);
}

// 将优惠券数据压入队列
function EnterQue(scanNumber) {

    if (scanNumber.length > 0) {
        var barcodeArr = scanNumber.split(',');
        if (barcodeArr.length <= 0) {
            return;
        }
        $("#ProductNumber").val("").focus();
        barcodeArr.forEach(function (value, index, arr) {
            var uuid = $.whiskey.tools.UUID(32, 16);
            hashlist.put(uuid, value);
        });

    }
}

// 将队列中的优惠券数据发往服务端验证
function sendToQueue() {
    if (hashlist.size() > 0) {
        let globalUUID = hashlist.getFirst(0);
        let scanNumber = hashlist.getFirst(1);
        if (codelist.indexOf(scanNumber) == -1)
            codelist.push(scanNumber);
        hashlist.remove(globalUUID);
        let setting = $.whiskey.datatable.instance.fnSettings();
        setting.sAjaxSource = "/Stores/Retail/GetProductsByBarcode";
        $.whiskey.datatable.instance.fnSettings(setting);
        $.whiskey.datatable.reset(false);

    }
}

function RemoveRows() {
    let ches = $("#prodlis_tab tbody .px:checked");
    let barcodes = [];
    for (let i = 0; i < ches.length; i++) {
        let ch = ches[i];
        let code = $(ch).parents("tr:eq(0)").find(".pbarcode").text();
        if (code != undefined && code != "") {
            barcodes.push(code);
            let ind = codelist.indexOf(code);
            codelist.splice(ind, 1);
        }
    }
    releseInventoryLock(barcodes);
}
function removeRow(send) {
    let confirm = new $.whiskey.web.ajaxConfirm({
        question: "确认要移除该商品吗？",
        notes: "提示：可以在\"选择商品\"中重新添加",
        success: function () {
            // treegrid-parent-p12313
            let barcode = [];
            let cl = $(send).parents("tr:eq(0)").attr("class");
            let reg = /treegrid-parent-(.*)/gi;
            let parid = reg.exec(cl)[1]; // treegrid-p2703
            let parow = $(".treegrid-" + parid);
            let prval = $(parow).find(".prodcou").val();
            let res = (parseInt(prval) - 1) < 0 ? 0 : (parseInt(prval) - 1);
            $(parow).find(".prodcou").val(res);

            let code = $(send).parents("tr:eq(0)").find(".pbarcode").text();
            if (code != undefined && code != "")
                barcode.push(code);

            let curval = $(send).parents("tr:eq(0)").find(".te_1_che").val();
            let chilcl = ".treegrid-parent-" + curval;
            $(chilcl).each(function () {
                let te = $(this).find(".pbarcode").text();
                if (te != "")
                    barcode.push(te);
            });
            if (barcode.length > 0) {
                for (let i = 0; i < barcode.length; i++) {
                    let cd = barcode[i];
                    let ind = codelist.indexOf(cd);
                    if (ind > -1)// &&codelist.length>1)
                        codelist.splice(ind, 1);
                }
                let codli = $(".pbarcode").length;

                releseInventoryLock(barcode);
            }



        }
    });
}

// 释放库存,刷新列表
function releseInventoryLock(barcodes) {
    if (barcodes.length > 0) {
        $.post('/Stores/Retail/ReleseInventoryLock', { barcodes: barcodes.join(',') }).done(function (res) {
            // 重新请求数据
            $.whiskey.datatable.reset(false);
        });
    }
}

// 修改合计金额
function changeSumPrice(price) {
    $(".moncou_span").text(price);
}

/**
 * 计算列表中所有商品总金额
 * 
 * @returns {number}
 */
function updateProductTotalPrice(): number {
    let moncou = 0;
    $("#prodlis_tab tbody .pnum").parents("tr").each(function () {
        if ($(this).find(":checkbox").is(":checked")) {
            let mon = $(this).find(".mongCou").text();
            moncou += parseFloat(mon);
        }
    });
    return moncou;
}






// 会员结算界面初始化
function memberInit() {

    $(".lab-mi").removeClass("col-md-3").addClass("col-md-1");
    $(".cou-consum").removeClass("col-md-5").addClass("col-md-8");
    $(".scan").parent().removeClass("col-md-3").addClass("col-md-4");
    $(".memb-cl").show();
    $(".mes_warn").show();
}

// 非会员界面初始化
function noMemberInit() {
    $(".memb-cl").hide();
    $(".mes_warn").hide();
    $(".lab-mi").removeClass("col-md-1").addClass("col-md-3");
    $(".cou-consum").removeClass("col-md-8").addClass("col-md-5");
    $(".scan").parent().removeClass("col-md-4").addClass("col-md-3");
    memberInfoGlobal = {};
}




function Info(content: string, callback?: Function): void {
    var option = {
        type: "info",
        content: content,
        callback: callback
    };
    $.whiskey.web.alert(option);
}


/**
 * 生成零售价及折扣展示td内容
 * 
 * @param {number} retailPrice
 * @param {number} brandDiscount
 * @param {number} productActivityDiscount
 * @param {DiscountType} discountType
 * @returns
 */
function generateRetailPriceSpanText(retailPrice: number, brandDiscount: number, productActivityDiscount: number, discountType: DiscountType) {


    let spanHtml = retailPrice.toFixed(2);

    switch (discountType) {
        case DiscountType.BrandDiscount: {
            let discou = getDiscountText(brandDiscount);
            spanHtml += discou;
            break;
        }
        case DiscountType.ProductActivityDiscount: {
            let discou = getDiscountText(productActivityDiscount);
            spanHtml += discou;
            break;
        }

    }
    return spanHtml;

}


function getDiscountText(discount: number) {
    let txt = "";
    if (discount != 10) {
        txt = `<span style='color:red'>[${discount}折]</span>`;
    }
    return txt;
}

function timer(seconds: number, callback) {
    if (window["GLOBAL_RETAIL_TIMER_HANDLE"]) {
        clearInterval(window["GLOBAL_RETAIL_TIMER_HANDLE"]);
    }

    window["GLOBAL_RETAIL_TIMER_HANDLE"] = window.setInterval(function () {

        var day: number | string = 0,

            hour: number | string = 0,

            minute: number | string = 0,

            second: number | string = 0;//时间默认值
        if (seconds == 0) {
            callback();
        }

        if (seconds > 0) {

            day = Math.floor(seconds / (60 * 60 * 24));

            hour = Math.floor(seconds / (60 * 60)) - (day * 24);

            minute = Math.floor(seconds / 60) - (day * 24 * 60) - (hour * 60);

            second = Math.floor(seconds) - (day * 24 * 60 * 60) - (hour * 60 * 60) - (minute * 60);

        }

        if (minute <= 9) minute = '0' + minute;

        if (second <= 9) second = '0' + second;

        $('#day_show').html(day + "天");

        $('#hour_show').html('<s id="h"></s>' + hour + '时');

        $('#minute_show').html('<s></s>' + minute + '分');

        $('#second_show').html('<s></s>' + second + '秒');

        seconds--;


    }, 1000);

}