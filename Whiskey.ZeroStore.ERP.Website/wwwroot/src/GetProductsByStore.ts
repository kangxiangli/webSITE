var datatable_install;
$(document).ready(function () {

    var dts = $("#nwe_table_1").dataTable({
        "bScrollCollapse": true,
        "bSort": false,
        "bScrollInfinite": false,
        "sPaginationType": "full_numbers",
        "sDom": '<"H clearfix datatable-header text-center">rt<"F clearfix datatable-footer"<"col-md-5 info"i><"col-md-7 text-right"p>>',
        "sAjaxSource": "/Properties/SalesCampaign/GetProductsByStore",
        "fnServerParams": function (aoData: FormField[]) {

            var conditions = new $.whiskey.filter.group();

            // 按商品名称搜索
            var productName = $("#ProductName").val();//.trim();
            if (productName != "") {
                aoData.push({
                    name: 'ProductName',
                    value: productName
                });
            }

            // 按品牌搜索,选择自营品牌或外部品牌时brandId为-1
            var brand = $("#BrandId option:selected").val();
            if (brand != "" && brand != "-1") {
                aoData.push({ name: "brandId", value: brand });
            }
            var SeasonId = $("#SeasonId option:selected").val();
            if (SeasonId != "" && SeasonId != "-1") {
                aoData.push({ name: "SeasonId", value: SeasonId });
            }

            var bigProdNum = $("#BigProdNum").val().trim();
            if (bigProdNum != "") {
                aoData.push({ name: "bigProdNum", value: bigProdNum });
            }
            var categoryId = parseInt($("#CategoryId").val())
            if (!isNaN(categoryId) && categoryId > 0) {
                aoData.push({ name: "categoryId", value: categoryId.toString() });
            }



        },
        "fnRowCallback": function (nRow, aData, iDisplayIndex) {
            var isEnabled = aData.IsEnabled;
            if (isEnabled == false) {
                $(nRow).css({ "color": "red" });
            }
            //  $("td:eq(1)", nRow).addClass("text-left").css({ "width": "26%" });
            $("td:last", nRow).addClass("text-left").css({ "width": "10%" });
            $(nRow).addClass("treegrid-" + aData.Id + (aData.ParentId != null ? " treegrid-parent-" + aData.ParentId : ""));
            return nRow;
        },
        "aoColumns": [{
            "bVisible": false,
            "bSearchable": false,
            "mData": "Id"
        },
        {
            "sTitle": `<a class="chk-all">全选</a>`,
            "bSortable": false,
            "bSearchable": false,
            "mData": function (data) {
                // 对已经加入到数组中的款号，显示checked状态
                if (checkedNumbers.indexOf(data.ProNum) !== -1) {
                    return `<input type="checkbox" checked value="${data.Id}" data-bigprodnumber="${data.ProNum}">`;
                }
                else {
                    return `<input type="checkbox" value="${data.Id}" data-bigprodnumber="${data.ProNum}">`;
                }
            }
        },
        {
            "sTitle": "商品款号",
            "bSortable": false,
            "mData": function (data) {
                return data.ProNum;
            },
        },
        {
            "sTitle": "品牌名",
            "bSortable": false,
            "mData": function (data) {
                return data.Brand;
            },
        },
        {
            "sTitle": "品类",
            "bSortable": false,
            "mData": function (data) {
                return data.CategoryName;
            },
        },
        {
            "sTitle": "季节",
            "bStoreable": false,
            "mData": function (data) {
                return data.Seaso;
            }
        },
        {
            "sTitle": "吊牌价(￥)",
            "bStoreable": false,
            "mData": function (data) {
                return data.TagPrice;
            }
        },
        {
            "sTitle": "图片",
            "bStoreable": false,

            "mData": function (data) {
                return "<div class='thumbnail-img_five_box'> <div class='thumbnail-img_five'> <div class='thumbnail-img_f'><img onerror='imgloaderror(this);' src='" + data.Thumbnail + "' class='popimg'/> </div></div></div>";
            }
        }
        ]

    });
    $(dts).addClass("table table-striped");
    //$.whiskey.datatable.instances[0]=dts;
    $.whiskey.datatable.instances[0] = dts;
    //datatable_install = $.whiskey.datatable.instance;
    //alert("hi");
    $("#Search_par").click(function () {
        $.whiskey.datatable.reset(false, $.whiskey.datatable.instances[0]);

    });
    $("#AttributeNameOrNum").keyup(function (event) {
        if (event.keyCode == 13) {
            // alert("hi");
            $.whiskey.datatable.reset(false, $.whiskey.datatable.instances[0]);
            event.stopPropagation();
        }
    });
    $("#Clear_par").click(function () {
        $(".prod_search_pan").find("input").val("");
        $(".prod_search_pan").find("select").children("option:eq(0)").attr("selected", "selected");

        //$("#DepartmentId option").eq(0).attr("selected", true);
    });
    // $(".bootbox-close-button:last").hide();

    $(".storeProducts").parents(".modal-content").find(".bootbox-close-button").click(function () {
        $(".partiCreat").parents(".modal-content").show();
    });

    // 根据款号选中状态同步到数组中
    function addOrRemoveNum(bigProdNum: string, checked: boolean): void {
        if (!bigProdNum || bigProdNum.length <= 0) {
            return;
        }
        let indexInArr = checkedNumbers.indexOf(bigProdNum);
        //移除或新增
        if (checked && indexInArr === -1) {
            checkedNumbers.push(bigProdNum);
        }
        else if (!checked && indexInArr !== -1) {
            checkedNumbers.splice(indexInArr, 1);
        }
    }

    $("#nwe_table_1").off("click", "input[type=checkbox]")
        .on("click", "input[type=checkbox]", function () {
            //选择后将bigprodnum存储在数组中
            let checked = $(this).prop("checked");
            let bigprodnum = $(this).data("bigprodnumber");
            addOrRemoveNum(bigprodnum, checked);
        });
    $("#nwe_table_1").off("click", ".chk-all")
        .on("click", ".chk-all", function () {
            //选择后将bigprodnum存储在数组中
            if (_.every($("#nwe_table_1 tbody input[type=checkbox]"), (chkbox: HTMLInputElement) => chkbox.checked)) {
                // 全部取消勾选
                $("#nwe_table_1 tbody input[type=checkbox]").each((index, elem) => {
                    let bigprodnum = $(elem).data("bigprodnumber");
                    let checked = false;
                    $(elem).prop("checked", checked);
                    addOrRemoveNum(bigprodnum, checked);
                });

            }
            else {
                // 全部勾选
                $("#nwe_table_1 tbody input[type=checkbox]").each((index, elem) => {
                    let checked = true;
                    let bigprodnum = $(elem).data("bigprodnumber");
                    $(elem).prop("checked", checked);
                    addOrRemoveNum(bigprodnum, checked);
                });

            }
        });
});

