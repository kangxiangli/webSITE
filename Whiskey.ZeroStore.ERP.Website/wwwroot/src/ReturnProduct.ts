interface GetRetailInventoryRes {
    /**
     *库存列表 
      */
    InventoryList: InventoryList[];

    /**
      * 是否只能整单退货
      */
    UseCouponOrStoreActivity: boolean;
}

interface GetReturnCouRes {

    /**
     * 总计返回值
     */
    ReturnMoneyCou: number;

    /**
     * 退还现金
     */
    ReturnCash: number;

    /**
     * 退还积分
     */
    ReturnScore: number;

    /**
     * 退还储值
     */
    ReturnCardValue: number;

    /**
     * 退还刷卡
     */
    ReturnSwipCard: number;

    /**
    *刷卡类型
    */
    SwipeCardType: number;

    /**
     * 扣除获取积分
     */
    ReturnGetScore: number;

    /**
     * 抹零扣除
     */
    EraseMoney: number;

    /**
     * 优惠券扣除
     */
    Coupon: number;

    /**
     * 店铺活动扣除
     */
    StoreActivityDiscount: number;

    /**
     * 经办人
     */
    Admin: string;


    /**
     * 退货数量
     * 
     * @type {number}
     * @memberOf GetReturnCouRes
     */
    ReturnCount: number;

}

interface InventoryList {

    /**
    * 库存id
    */
    InventoryId: number;

    /**
     * 库存流水号
     */
    ProductBarcode: string;

    /**
     * 商品缩略图
     */
    ThumbnailPath: string;

    /**
     * 销售价格
     */
    ProductRetailPrice: number;

    /**
     * 是否已经退货
     */
    IsReturn: number;


}

interface CreateReturnReq {

    /**
     * 零售单号
     * 
     * @type {number}
     * @memberOf CreateReturnReq
     */
    retailNumber: string;
    /**
    *库存流水号
    */
    productBarcodes: string[];

    /**
    *退款各项明细
    *
    *@type     {GetReturnCouRes}
    *@memberOf CreateReturnReq
    */
    returnDetail: GetReturnCouRes;

    /**
     * 退货原因
     * 
     * @type {string}
     * @memberOf CreateReturnReq
     */
    Note: string;

    /**
     * 退货日期
     * 
     * @type {Date}
     * @memberOf CreateReturnReq
     */
    Returntime: Date;
}
function getStatText(stat:number){
        if(stat==0){
            return "可退"
        }
        else if(stat==1){
            return "已退"
        }
        else if(stat ==2){
            return "已换货"
        }
        return "";
        
    }
class ReturnProcessor {

    /**
     * 零售订单id
     * 
     * 
     * @memberOf ReturnProcessor
     */
    private readonly _retailNumber: string;

    /**
     * 是否只能整单退货
     * 
     * @private
     * @type {boolean}
     * @memberOf ReturnProcessor
     */
    private UseCouponOrStoreActivity: boolean;

    /**
     *  刷卡+现金之和
     * 
     * @private
     * 
     * @memberOf ReturnProcessor
     */
    private cashAndSwipCardTotal = 0;


    /**
     * Creates an instance of ReturnProcessor
     * 
     * 
     * @memberOf ReturnProcessor
     */
    constructor() {
        this._retailNumber = $("#hidRetailNumber").val();
    }


    Init() {
        this.getRetailInventory();
    }

    isNumber(keycode) {
        if ((keycode >= 48 && keycode <= 57) || (keycode >= 96 && keycode <= 105) || keycode == 8 || keycode == 110 || keycode == 46 || keycode == 190) {
            return true;
        }
        return false;
    }
    numberFormat(da) {
        var reg = /^\d+$/;
        if (reg.test(da)) {
            return da + ".00";
        } else return da;
    }
    
    /**
     * 生成复选框,如果是整单退,默认都是勾选的,而且不允许修改
     * @param productBarcode 流水号
     * @param isReturn 是否已经退货
     * @param UseCouponOrStoreActivity 是否使用了优惠券或参与了店铺活动
     */
    generateCheckBox(productBarcode: string, isReturn: number, UseCouponOrStoreActivity: boolean): string {
        if (isReturn === 1 || isReturn==2) {
            return "";
        }
        if (UseCouponOrStoreActivity) {
            return `<input type="checkbox" class="chk-item" checked="checked" disabled="disabled" value="${productBarcode}" />`;

        }

        else {
            return `<input type="checkbox" class="chk-item" value="${productBarcode}" />`;
        }
    }
    /**
     * 获取订单下的库存商品列表
     */
    getRetailInventory() {
        let _self = this;
        var tbody = document.querySelector("#tbl tbody")
        $.post('/Stores/Return/GetRetailInventory', { retailNumber: _self._retailNumber })
            .done(function (res: OperationResult<GetRetailInventoryRes>) {
                // 确定是否只能整单退货
                _self.UseCouponOrStoreActivity = res.Data.UseCouponOrStoreActivity;
                if (_self.UseCouponOrStoreActivity) {
                    $(".chkall").prop("checked", true).prop("disabled", "true");
                }
                let inventoryList = res.Data.InventoryList;
                //生成表格
                var str = "";
                for (let i = 0; i < inventoryList.length; i++) {

                    str += "<tr>";
                    str += `<td>${_self.generateCheckBox(inventoryList[i].ProductBarcode, inventoryList[i].IsReturn, _self.UseCouponOrStoreActivity)}</td>`;
                    //str += `<td>${inventoryList[i].Id}</td>`;
                    str += `<td>${inventoryList[i].ProductBarcode}</td>`;
                    str += `<td>${_self.generateImg(inventoryList[i].ThumbnailPath)} </td>`;
                    str += `<td>${inventoryList[i].ProductRetailPrice}</td>`;
                    str += `<td>${getStatText(inventoryList[i].IsReturn)}</td>`;
                    str += "</tr>";
                }
                $("#tbl tbody").html(str);


            }).done(function () {
                _self.BindEvent();
                _self.OnSelectChange();
            })
    }

    /**
     * 生成图片标签
     */
    generateImg(path: string): string {

        if (path && path.length > 0) {
            return `<div class='thumbnail-img_five_box'>
                        <div class='thumbnail-img_five'>
                            <div class='thumbnail-img_f'>
                                <img class='popimg' onerror="imgloaderror(this);" src='${path}' />
                            </div>
                        </div>
                    </div>`;
        }
        return "无";
    }
    /**
     * //获取被选中的checkbox
     * 
     * @returns {string[]}
     * 
     * @memberOf ReturnProcessor
     */
    getCheckedBarcodes(): string[] {
        let selectedProductBarcodes: string[] = [];
        let selectedCheckboxes = $("#tbl tbody input[type=checkbox]:checked");
        selectedCheckboxes.each((index, elem) => {
            selectedProductBarcodes.push((elem as HTMLInputElement).value.trim());
        });
        return selectedProductBarcodes;
    }

    // 修改选择的退货商品
    OnSelectChange() {
        let _self = this;
        let selectedProductBarcodes: string[] = _self.getCheckedBarcodes();

        //计算应退各项数值

        if (selectedProductBarcodes.length > 0) {

            let postData = { retailNumber: _self._retailNumber, productBarcodes: selectedProductBarcodes.join(',') };
            console.log(postData);
            let url: string = "";
            if (_self.UseCouponOrStoreActivity) {
                url = "/Stores/Return/GetWholeReturnCou";
            }
            else {
                url = "/Stores/Return/GetReturnMoney";
            }
            $.post(url, postData, function (da: OperationResult<GetReturnCouRes>) {

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
                    $("#ReturnCount").val(da.Data.ReturnCount);//得到退货数量

                    _self.cashAndSwipCardTotal = da.Data.ReturnSwipCard + da.Data.ReturnCash
                } else {
                    let $ = jQuery as any;
                    $.whiskey.web.alert({
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
    }



    /**
     * 绑定一堆事件
     * 
     * 
     * @memberOf ReturnProcessor
     */
    BindEvent() {
        let _self = this;
        // regist checkbox select change event handler
        let onSelectChange = this.OnSelectChange.bind(this);
        $("#tbl tbody input[type=checkbox]").click(onSelectChange);

        // datepicker
        $("#ReturnTime").datepicker({
            showOtherMonths: true,
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
            let total = returnScore + returnCardValue + swipcaredreturn + cashreturn + returnErase + returnCouponMon + returnStoreActivityDiscount;

            if (total !== returnMoneyCoun) {
                err = "退款明细之和与退款总额不一致";
            }
            else if (swipcaredreturn + cashreturn > returnMoneyCoun) {
                err = "刷卡应退和现金应退之和不应该大于退款总额";
            }

        });
    }

    /**
     * 判断是否有错误
     */
    hasError(): boolean {

        var note = $(".returnProd_pg #Note").val().trim();
        if (note.length == 0) {
            $(".returnProd_pg").parents(".modal-content")
                .find("button[data-bb-handler='success']")
                .attr("disabled", "disabled");

            return false;
        } else {
            $(".returnProd_pg").parents(".modal-content")
                .find("button[data-bb-handler='success']")
                .removeAttr("disabled");
        }
        return true;
    }

    getReturnDetailFromWindow(): GetReturnCouRes {
        let returnScore = parseFloat($("#ReturnScore").val());
        let returnCardValue = parseFloat($("#ReturnCardValue").val());
        let returnSwipCard = parseFloat($("#ReturnSwipCard").val().trim());
        let returnCash = parseFloat($("#ReturnCash").val().trim());
        let returnErase = parseFloat($("#EraseMoney").val());
        let returnCouponMon = parseFloat($("#CouponMon").val());
        let returnStoreActivityDiscount = parseFloat($("#StoreActivityDiscount").val());
        let returnMoneyCoun = parseFloat($("#ReturnMoneyCoun").val());
        let returnGetScore = parseFloat($("#ReturnGetScore").val());
        let returnCount = parseFloat($("#ReturnCount").val());
        let swipeCardType = parseInt($("#SwipeCardType").val());

        let returnObj = {} as GetReturnCouRes;
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

    }
    createReturnProducts(): boolean {
        var _self = this;
        let postData = {} as CreateReturnReq;
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
            let $ = jQuery as any;
            if (da.ResultType == 3) {
                $.whiskey.web.alert({
                    type: "success",
                    content: "退货完成！",
                    callback: function () {
                        location.href = '/Stores/Returned/Index';
                    }
                });
                return true;
            } else {
                $.whiskey.web.alert({
                    type: "info",
                    content: da.Message,
                    callback: function () {
                    }
                });
                return false;
            }


        });

    }


}

