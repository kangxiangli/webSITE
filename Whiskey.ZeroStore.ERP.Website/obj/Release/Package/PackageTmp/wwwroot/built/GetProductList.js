var Common;
(function (Common) {
    var datatable_install;
    $(document).ready(function () {
        var dts = $("#nwe_table_1").dataTable({
            "bScrollCollapse": true,
            "bSort": false,
            "bScrollInfinite": false,
            "sPaginationType": "full_numbers",
            "sDom": '<"H clearfix datatable-header text-center">rt<"F clearfix datatable-footer"<"col-md-5 info"i><"col-md-7 text-right"p>>',
            "sAjaxSource": "/Stores/ExchangeOrder/GetProductsCurrentUser",
            "fnServerParams": function (aoData) {
                //获取已经加到列表中的商品货号
                var productBarcodes = [];
                var selectedArr = $("#tableSelectionResult input[type=checkbox]:checked").each(function (index, elem) {
                    var $tr = $(elem).parents("tr");
                    if ($tr.treegrid("isLeaf")) {
                        var barcode = $tr.find("span.pbarcode").text();
                        productBarcodes.push(barcode);
                    }
                });
                aoData.push({ name: "existProductBarcodes", value: productBarcodes.join(',') });
                var conditions = new $.whiskey.filter.group();
                var attrName = $(".crePan #ProductName").val();
                if (attrName != "") {
                    aoData.push({ name: "productName", value: attrName });
                }
                var brand = $(".crePan #BrandId option:selected").val();
                if (brand != "" && brand != "-1")
                    conditions.Rules.push(new $.whiskey.filter.rule("Product.ProductOriginNumber.BrandId", brand, "equal"));
                //var index = ind;
                var num = $(".crePan #ProductNumber").val().trim();
                if (num != "")
                    conditions.Rules.push(new $.whiskey.filter.rule("ProductNumber", num, "equal"));
                var storeid = $("#hid_storeid").val();
                conditions.Rules.push(new $.whiskey.filter.rule("StoreId", storeid, "equal"));
                aoData.push({ name: "conditions", value: JSON.stringify(conditions) });
                aoData.push({ name: "storeId", value: storeid });
            },
            "fnRowCallback": function (nRow, aData, iDisplayIndex) {
                $(nRow).addClass("treegrid-" + aData.TreegridId);
                $(nRow).addClass("treegrid-parent-" + aData.TreegridParentId);
                return nRow;
            },
            "fnDrawCallback": function () {
                $(this).treegrid({
                    treeColumn: 2
                }).treegrid("collapseAll");
            },
            "aoColumns": [{
                    "bVisible": false,
                    "bSearchable": false,
                    "mData": "Id"
                },
                {
                    "sTitle": '选择',
                    "bSortable": false,
                    "bSearchable": false,
                    "mData": function (data) {
                        if (data.TreegridParentId) {
                            if (data.IsSelected) {
                                return "已选择";
                            }
                            return '<label class="px-single"><input type="checkbox" data-isleft="1" value="' + data.ProductBarcode + '" ></label>';
                        }
                        return '<label class="px-single"><input type="checkbox" value="' + data.ProNum + '" ></label>';
                        //return $.whiskey.datatable.tplListCheckbox(data.Id, "pdl");
                    }
                },
                {
                    "sTitle": "图片",
                    "bStoreable": false,
                    "mData": function (data) {
                        if (data.TreegridParentId) {
                            return "";
                        }
                        return "<img width='60px' class='' src='" + data.Thumbnail + "'/>";
                    }
                },
                {
                    "sTitle": "商品货号/流水号",
                    "bSortable": false,
                    "mData": function (data) {
                        if (data.TreegridParentId) {
                            return data.ProductBarcode;
                        }
                        return data.ProNum;
                    }
                },
                {
                    "sTitle": "品牌名",
                    "bSortable": false,
                    "mData": function (data) {
                        return data.Brand;
                    }
                },
                {
                    "sTitle": "款式",
                    "bSortable": false,
                    "mData": function (data) {
                        return data.Categ;
                    }
                },
                {
                    "sTitle": "颜色",
                    "bSortable": false,
                    "mData": function (data) {
                        if (data.TreegridParentId) {
                            return "";
                        }
                        return "<img title='" + data.ColoName + "' src=" + data.Colo + " width='35px' onerror='imgloaderror(this);'/>";
                    }
                },
                {
                    "sTitle": "尺码",
                    "bStoreable": false,
                    "mData": function (data) {
                        return data.Size;
                    }
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
                }, {
                    "sTitle": "<span style='color:blue'>可用</span>/库存",
                    "bStoreable": false,
                    "mData": function (data) {
                        if (data.TreegridParentId) {
                            return "";
                        }
                        return "<span style='color:blue'>" + data.EnabCou + "</span>/" + data.Cou;
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
        $(".crePan #Clear_par").click(function () {
            $(".crePan input").val("");
            $(".crePan select").find("option:eq(0)").attr("selected", "selected");
        });
        $("#nwe_table_1").on('click', "input[type=checkbox]", function () {
            var $chkbox = $(this);
            var isChecked = $chkbox.prop("checked");
            var $tr = $chkbox.parents("tr");
            if (!$tr.treegrid("isNode")) {
                return;
            }
            if (!$tr.treegrid("isLeaf")) {
                $tr.treegrid("getChildNodes").each(function (index, elem) {
                    $(elem).find("input[type=checkbox]").prop("checked", isChecked);
                });
                $tr.treegrid("expand");
            }
            else {
                if (!isChecked) {
                    $tr.treegrid("getParentNode").find("input[type=checkbox]").prop("checked", false);
                }
                else {
                    if (_.every($tr.treegrid("getParentNode").treegrid("getChildNodes").find("input[type=checkbox]"), function (elem) { return $(elem).prop("checked"); })) {
                        $tr.treegrid("getParentNode").find("input[type=checkbox]").prop("checked", true);
                    }
                }
            }
        });
    });
    function getCheckedProdNums() {
        var nums = [];
        $("#nwe_table_1 tbody tr").find(":checkbox:checked").parents("tr").each(function () {
            var t = $(this).find("td:eq(2)").text();
            nums.push(t);
        });
        return nums;
    }
})(Common || (Common = {}));
