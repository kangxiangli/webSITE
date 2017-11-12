function getStatText(stat) {
    if (stat == 0) {
        return "可退";
    }
    else if (stat == 1) {
        return "已退";
    }
    else if (stat == 2) {
        return "已换货";
    }
    return "";
}
var ReturnProcessor = (function () {
    /**
     * Creates an instance of ReturnProcessor
     *
     *
     * @memberOf ReturnProcessor
     */
    function ReturnProcessor() {
        /**
         *  刷卡+现金之和
         *
         * @private
         *
         * @memberOf ReturnProcessor
         */
        this.cashAndSwipCardTotal = 0;
        this._retailNumber = $("#hidRetailNumber").val();
    }
    ReturnProcessor.prototype.Init = function () {
        this.getRetailInventory();
    };
    ReturnProcessor.prototype.isNumber = function (keycode) {
        if ((keycode >= 48 && keycode <= 57) || (keycode >= 96 && keycode <= 105) || keycode == 8 || keycode == 110 || keycode == 46 || keycode == 190) {
            return true;
        }
        return false;
    };
    ReturnProcessor.prototype.numberFormat = function (da) {
        var reg = /^\d+$/;
        if (reg.test(da)) {
            return da + ".00";
        }
        else
            return da;
    };
    /**
     * 生成复选框,如果是整单退,默认都是勾选的,而且不允许修改
     * @param productBarcode 流水号
     * @param isReturn 是否已经退货
     * @param UseCouponOrStoreActivity 是否使用了优惠券或参与了店铺活动
     */
    ReturnProcessor.prototype.generateCheckBox = function (productBarcode, isReturn, UseCouponOrStoreActivity) {
        if (isReturn === 1 || isReturn == 2) {
            return "";
        }
        if (UseCouponOrStoreActivity) {
            return "<input type=\"checkbox\" class=\"chk-item\" checked=\"checked\" disabled=\"disabled\" value=\"" + productBarcode + "\" />";
        }
        else {
            return "<input type=\"checkbox\" class=\"chk-item\" value=\"" + productBarcode + "\" />";
        }
    };
    /**
     * 获取订单下的库存商品列表
     */
    ReturnProcessor.prototype.getRetailInventory = function () {
        var _self = this;
        var tbody = document.querySelector("#tbl tbody");
        $.post('/Stores/Return/GetRetailInventory', { retailNumber: _self._retailNumber })
            .done(function (res) {
            // 确定是否只能整单退货
            _self.UseCouponOrStoreActivity = res.Data.UseCouponOrStoreActivity;
            if (_self.UseCouponOrStoreActivity) {
                $(".chkall").prop("checked", true).prop("disabled", "true");
            }
            var inventoryList = res.Data.InventoryList;
            //生成表格
            var str = "";
            for (var i = 0; i < inventoryList.length; i++) {
                str += "<tr>";
                str += "<td>" + _self.generateCheckBox(inventoryList[i].ProductBarcode, inventoryList[i].IsReturn, _self.UseCouponOrStoreActivity) + "</td>";
                //str += `<td>${inventoryList[i].Id}</td>`;
                str += "<td>" + inventoryList[i].ProductBarcode + "</td>";
                str += "<td>" + _self.generateImg(inventoryList[i].ThumbnailPath) + " </td>";
                str += "<td>" + inventoryList[i].ProductRetailPrice + "</td>";
                str += "<td>" + getStatText(inventoryList[i].IsReturn) + "</td>";
                str += "</tr>";
            }
            $("#tbl tbody").html(str);
        }).done(function () {
            _self.BindEvent();
            _self.OnSelectChange();
        });
    };
    /**
     * 生成图片标签
     */
    ReturnProcessor.prototype.generateImg = function (path) {
        if (path && path.length > 0) {
            return "<div class='thumbnail-img_five_box'>\n                        <div class='thumbnail-img_five'>\n                            <div class='thumbnail-img_f'>\n                                <img class='popimg' onerror=\"imgloaderror(this);\" src='" + path + "' />\n                            </div>\n                        </div>\n                    </div>";
        }
        return "无";
    };
    /**
     * //获取被选中的checkbox
     *
     * @returns {string[]}
     *
     * @memberOf ReturnProcessor
     */
    ReturnProcessor.prototype.getCheckedBarcodes = function () {
        var selectedProductBarcodes = [];
        var selectedCheckboxes = $("#tbl tbody input[type=checkbox]:checked");
        selectedCheckboxes.each(function (index, elem) {
            selectedProductBarcodes.push(elem.value.trim());
        });
        return selectedProductBarcodes;
    };
    // 修改选择的退货商品
    ReturnProcessor.prototype.OnSelectChange = function () {
        var _self = this;
        var selectedProductBarcodes = _self.getCheckedBarcodes();
        //计算应退各项数值
        if (selectedProductBarcodes.length > 0) {
            var postData = { retailNumber: _self._retailNumber, productBarcodes: selectedProductBarcodes.join(',') };
            console.log(postData);
            var url = "";
            if (_self.UseCouponOrStoreActivity) {
                url = "/Stores/Return/GetWholeReturnCou";
            }
            else {
                url = "/Stores/Return/GetReturnMoney";
            }
            $.post(url, postData, function (da) {
                if (da.ResultType == 3) {
                    $("#ReturnMoneyCoun").val(_self.numberFormat(da.Data.ReturnMoneyCou));
                    $("#ReturnCardValue").val(_self.numberFormat(da.Data.ReturnCardValue));
                    $("#ReturnGetScore").val(da.Data.ReturnGetScore);
                    $("#ReturnScore").val(da.Data.ReturnScore);
                    $("#ReturnSwipCard").val(_self.numberFormat(da.Data.ReturnSwipCard));
                    $("#ReturnCash").val(_self.numberFormat(da.Data.ReturnCash));
                    $("#Admin").val(da.Data.Admin);
                    $("#CouponMon").val(da.Data.Coupon.toFixed(2));
                    $("#EraseMoney").val(da.Data.EraseMoney.toFixed(2));
                    $("#StoreActivityDiscount").val(da.Data.StoreActivityDiscount.toFixed(2));
                    $("#ReturnCount").val(da.Data.ReturnCount); //得到退货数量
                    _self.cashAndSwipCardTotal = da.Data.ReturnSwipCard + da.Data.ReturnCash;
                }
                else {
                    var $_1 = jQuery;
                    $_1.whiskey.web.alert({
                        type: "info",
                        content: "当前页面存在异常，请关闭重新打开",
                        callback: function () {
                        }
                    });
                }
            });
        }
        else {
            // 重置控件值
            $("#ReturnMoneyCoun").val("0.00");
            $("#ReturnCardValue").val("0.00");
            $("#ReturnGetScore").val("0");
            $("#ReturnScore").val("0");
            $("#ReturnSwipCard").val("0.00");
            $("#ReturnCash").val("0.00");
            $("#StoreActivityDiscount").val("0.00");
        }
    };
    /**
     * 绑定一堆事件
     *
     *
     * @memberOf ReturnProcessor
     */
    ReturnProcessor.prototype.BindEvent = function () {
        var _self = this;
        // regist checkbox select change event handler
        var onSelectChange = this.OnSelectChange.bind(this);
        $("#tbl tbody input[type=checkbox]").click(onSelectChange);
        // datepicker
        $("#ReturnTime").datepicker({
            showOtherMonths: true
        });
        var date = new Date();
        var de = date.toLocaleDateString();
        $("#ReturnTime").val(de).blur(function () {
            if ($(this).val().trim() == "") {
                $(this).val(de);
            }
        });
        $(".chkall").click(function () {
            var checked = $(this).prop("checked");
            $("#tbl tbody input[type=checkbox]").prop("checked", checked);
            onSelectChange();
        });
        $("#ReturnSwipCard,#ReturnCash").keyup(function (even) {
            var te = $(this).val();
            te = te.replace(/^[^0-9]$/, "").trim();
            $(this).val(te);
        }).blur(function () {
            var tx = $(this).val().trim();
            if (tx.indexOf(".") == tx.length - 1) {
                tx = tx + "00";
                $(this).val(tx);
            }
            var reg = /^\d+[.]?\d+$/;
            if (!reg.test(tx)) {
                $(this).val("0.00");
            }
            var reg = /^0(?!\.)\d+/;
            if (reg.test(tx)) {
                $(this).val(tx.substr(1));
            }
        });
        $("#ReturnSwipCard,#ReturnCash").blur(function () {
            var err = "";
            var returnScore = parseFloat($("#ReturnScore").val());
            var returnCardValue = parseFloat($("#ReturnCardValue").val());
            var swipcaredreturn = parseFloat($("#ReturnSwipCard").val().trim());
            var cashreturn = parseFloat($("#ReturnCash").val().trim());
            var returnErase = parseFloat($("#EraseMoney").val());
            var returnCouponMon = parseFloat($("#CouponMon").val());
            var returnStoreActivityDiscount = parseFloat($("#StoreActivityDiscount").val());
            var returnMoneyCoun = parseFloat($("#ReturnMoneyCoun").val());
            //var maxErase = $("#hid_maxera").val();
            var total = returnScore + returnCardValue + swipcaredreturn + cashreturn + returnErase + returnCouponMon + returnStoreActivityDiscount;
            if (total !== returnMoneyCoun) {
                err = "退款明细之和与退款总额不一致";
            }
            else if (swipcaredreturn + cashreturn > returnMoneyCoun) {
                err = "刷卡应退和现金应退之和不应该大于退款总额";
            }
        });
    };
    /**
     * 判断是否有错误
     */
    ReturnProcessor.prototype.hasError = function () {
        var note = $(".returnProd_pg #Note").val().trim();
        if (note.length == 0) {
            $(".returnProd_pg").parents(".modal-content")
                .find("button[data-bb-handler='success']")
                .attr("disabled", "disabled");
            return false;
        }
        else {
            $(".returnProd_pg").parents(".modal-content")
                .find("button[data-bb-handler='success']")
                .removeAttr("disabled");
        }
        return true;
    };
    ReturnProcessor.prototype.getReturnDetailFromWindow = function () {
        var returnScore = parseFloat($("#ReturnScore").val());
        var returnCardValue = parseFloat($("#ReturnCardValue").val());
        var returnSwipCard = parseFloat($("#ReturnSwipCard").val().trim());
        var returnCash = parseFloat($("#ReturnCash").val().trim());
        var returnErase = parseFloat($("#EraseMoney").val());
        var returnCouponMon = parseFloat($("#CouponMon").val());
        var returnStoreActivityDiscount = parseFloat($("#StoreActivityDiscount").val());
        var returnMoneyCoun = parseFloat($("#ReturnMoneyCoun").val());
        var returnGetScore = parseFloat($("#ReturnGetScore").val());
        var returnCount = parseFloat($("#ReturnCount").val());
        var swipeCardType = parseInt($("#SwipeCardType").val());
        var returnObj = {};
        returnObj.Coupon = returnCouponMon;
        returnObj.EraseMoney = returnErase;
        returnObj.ReturnCardValue = returnCardValue;
        returnObj.ReturnCash = returnCash;
        returnObj.ReturnCount = returnCount;
        returnObj.ReturnGetScore = returnGetScore;
        returnObj.ReturnMoneyCou = returnMoneyCoun;
        returnObj.ReturnScore = returnScore;
        returnObj.ReturnSwipCard = returnSwipCard;
        returnObj.StoreActivityDiscount = returnStoreActivityDiscount;
        returnObj.SwipeCardType = swipeCardType;
        return returnObj;
    };
    ReturnProcessor.prototype.createReturnProducts = function () {
        var _self = this;
        var postData = {};
        postData.productBarcodes = this.getCheckedBarcodes();
        postData.returnDetail = this.getReturnDetailFromWindow();
        postData.Returntime = $("#ReturnTime").val();
        postData.Note = $("#Note").val().trim();
        if (!postData.Note || postData.Note.length <= 0) {
            $.whiskey.web.alert({
                type: "info",
                content: '请填写退货原因',
                callback: function () {
                }
            });
            return false;
        }
        postData.retailNumber = this._retailNumber;
        //console.log(JSON.stringify(postData));
        var url = "/Stores/Return/Create";
        $.post(url, { postData: JSON.stringify(postData) }).done(function (da) {
            var $ = jQuery;
            if (da.ResultType == 3) {
                $.whiskey.web.alert({
                    type: "success",
                    content: "退货完成！",
                    callback: function () {
                        location.href = '/Stores/Returned/Index';
                    }
                });
                return true;
            }
            else {
                $.whiskey.web.alert({
                    type: "info",
                    content: da.Message,
                    callback: function () {
                    }
                });
                return false;
            }
        });
    };
    return ReturnProcessor;
}());
