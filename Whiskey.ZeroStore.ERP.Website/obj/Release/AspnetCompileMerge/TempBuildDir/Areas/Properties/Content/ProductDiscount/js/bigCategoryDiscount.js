
$(document).ready(function () {



    $.whiskey.datatable.instance = $(".table-list-bigca").dataTable({
        "bScrollCollapse": false,
        "bStateSave": true,
        "sDom": 't<"clearfix datatable-footer"<"col-md-3 text-left"l><"col-md-7 text-right"p>>',
        "sAjaxSource": "/",
        "fnServerParams": function (aoData) {
            var conditions = new $.whiskey.filter.group();
            var startDate = $(".start-date").val();
            var endDate = $(".end-date").val();

            if (startDate.length > 0 && endDate.length > 0) {
                conditions.Rules.push(new $.whiskey.filter.rule("CreatedTime", startDate + " 00:01:01", "greater"));
                conditions.Rules.push(new $.whiskey.filter.rule("CreatedTime", endDate + " 23:59:59", "less"));
            }


            $(".form-search .Coll").each(function () {

                var field = $(this).attr("id");
                var value = $(this).val();
                if (value != null && value.length > 0 && value != "-1") {
                    if (value.indexOf(",") == 0)
                        value = value.substr(1);
                    if (value.endsWith(',')) {
                        value = value.substr(0, value.length - 1);
                    }
                    var valarr = value.split(",");
                    var daarr = [];
                    $.each(valarr, function (i, v) {
                        if (v != "") {
                            var t = parseInt(v);
                            daarr.push(t);
                        }
                    });
                    if (daarr.length > 0)
                        conditions.Rules.push(new $.whiskey.filter.rule(field, daarr, "in"));
                }
            });
            aoData.push({ name: "conditions", value: JSON.stringify(conditions) });
            //$.whiskey.tools.other(conditions);
        },
        "aLengthMenu": [10],

        "fnPreDrawCallback": function (oSettings) {
            //alert("hi");
        },
        "fnRowCallback": function (nRow, aData, iDisplayIndex) {
            var isEnabled = aData.IsEnabled;
            if (isEnabled == false) {
                $(nRow).css({ "color": " #5ebd5e" });
            }


            //var ch = $('.swit_sel .checked');
            //if (ch != undefined && ch != null && ch.length > 0) {

            //    var savda = $.whiskey.tools.json();
            //    var starInd=1;
            //    var len=0;

            //    if (savda != null && savda != "") {
            //        var daarr = savda.split('|');
            //        if (daarr != null && daarr != "" && daarr.length > 0) {
            //            starInd = parseInt(daarr[0]);
            //            len = parseInt(daarr[1]);
            //        }
            //    }

            //    $("td:eq(1)", nRow).text(starInd + len + iDisplayIndex);
            //    var lenda=$(nRow).find("td:eq(3)").text();
            //    var reg=/^\d+$/;
            //    if(reg.test(lenda)){
            //        len=lenda;
            //    }
            //    $.whiskey.tools.json($(nRow).find("td:eq(1)").text() + "|" +len );
            //}
            //else
            $("td:eq(0)", nRow).text(iDisplayIndex + 1);
            $("td:eq(2)", nRow).css({ "width": "12%", "padding-left": "0" });
            //$("td:eq(4)", nRow).css({ "float": "left","width":"13%" });
            $("td:last", nRow).addClass("text-middle").css({ "width": "13%" });
            $("td:last button", nRow).css({ "margin": "2px" });
            $(nRow).addClass("treegrid-" + aData.Id + (aData.ParentId != null ? " treegrid-parent-" + aData.ParentId : ""));
            return nRow;
        },
        "fnFooterCallback": function () {

        },

        "aoColumns": [
            {
                "bVisible": false,
                "bSearchable": false,
                "sName": "Id",
                "mData": "Id"
            },

            {
                "sTitle": "排序",
                "bSortable": false,
                "sName": "Number",
                "mData": function (data) {
                    return "";
                }
            },


            {
                "sTitle": "编号",
                "bSortable": false,
                "sName": "ProductNumber",
                "mData": function (data) {
                    return "<span class='_pnum' style='margin-left:-23px'>" + data.ProductNumber + "</span>"
                }
            },
            {
                "sTitle": "商品图片",
                "sName": "ThumbnailPath",
                "bSortable": false,
                "bSearchable": false,
                "mData": function (data) {
                    if (data.ThumbnailPath == null || data.ThumbnailPath == "null" || data.ThumbnailPath == "")
                        return "";
                    else
                        return '<div style="display:block;width:80px;height:80px;overflow:hidden;border:1px solid #eaeaea;margin:0 auto 0 auto;"><img src="' + data.ThumbnailPath + '" style="margin:2px;max-width:72px;" /></div>';;
                }
            },
            {
                "sTitle": "品牌",
                "bSortable": false,
                "sName": "BrandName",
                "mData": function (data) {
                    return data.BrandName;
                }
            },
            {
                "sTitle": "款式",
                "bSortable": false,
                "sName": "BrandName",
                "mData": function (data) {
                    return data.CategoryName;
                }
            },
            {
                "sTitle": "季节",
                "bSortable": false,
                "sName": "SeasonName",
                "mData": function (data) {
                    return data.SeasonName;
                }
            },
            {
                "sTitle": "尺码",
                "bSortable": false,
                "sName": "SizeName",
                "mData": function (data) {
                    return data.SizeName;
                }
            },
            {
                "sTitle": "颜色",
                "bSortable": false,
                "sName": "ColorName",
                "mData": function (data) {
                    if (data.ParentId == "") {
                        return ""
                    } else {
                        var st = "<img src='" + data.ColorImg + "' title='" + data.ColorName + "' style='width:40px;margin:0 auto;'>";

                        return st;
                    }

                }
            },

            {
                "sTitle": "折扣状态",
                "bSortable": false,
                "sName": "TagPrice",
                "mData": function (data) {
                    if (data.ParentId == "") {
                        return ""
                    } else {
                        if (data.UseDefaultDiscount) {
                            return "默认";
                        } else return "自定义";
                    }
                },
            },
            {
                "sTitle": "吊牌价",
                "bSortable": false,
                "sName": "TagPrice",
                "mData": function (data) {
                    return data.TagPrice;
                },
            },

            {
                "sTitle": "控制操作",
                "bSortable": false,
                "bSearchable": false,
                "mData": function (data) {
                    //var controller = $.whiskey.datatable.controller(function() {
                    var controller = "";
                    var isDeleted = data.IsDeleted;
                    var reg = /[par|org](\d+)/;
                    var res = reg.exec(data.Id);
                    if (res != null && res != "" && res.length == 2) {
                        if (!isDeleted) {
                            controller += $.whiskey.datatable.tplView(data.Id);
                            return controller;
                            //return "";
                        } else {
                            controller += $.whiskey.datatable.tplView(data.Id);

                            return controller;
                        }
                    }
                    if (!isDeleted) {
                        controller += $.whiskey.datatable.tplView(data.Id);

                    } else {
                        controller += $.whiskey.datatable.tplView(data.Id);

                    }
                    var st = "<button title='查看商品明细' type='button' onclick='searDetails(this)' class='btn btn-xs  btn-padding-right' style='margin: 2px;'><i class='fa fa-th-list'></i> </button>";
                    return controller + st;
                    //});
                    //return controller;
                }
            }
        ]

    });

    //初始化
    // initDropdown();

});

function initEvetn() {

    $(".table-list-bigca").delegate(".treegrid-expander", "click", function () {
        var send = $(this);
        var bignum = $(this).parent().text();
        var t = $(".pcou").text();
        var reg = /\d+/;
        var cou = parseInt(reg.exec(t)[0]);
        if (cou > 100) {

            var _id = $(send).parents("tr:first").find(":checkbox").val();
            var t = $(send).attr("lodat");
            if (t != "ld") {
                $(".loading_hid_img").removeAttr("hidden");
                var par = $.whiskey.tools.other();
                if (par != undefined && par != null) {
                    for (var i = 0; i < par.Rules.length; i++) {
                        if (par.Rules[i].Field == "BigProdNum") {
                            par.Rules.splice(i, 1);
                        }
                    }
                }
                par.Rules.push(new $.whiskey.filter.rule("BigProdNum", bignum, "equal"));
                //var setting = $.whisk.datatable.instance.fnSettings();
                $.post("/Products/Product/GetChildByBigNum", { "conditions": JSON.stringify(par) }, function (dat) {
                    if (dat != "") {
                        var resu = "";
                        for (var i = 0; i < dat.length; i++) {
                            var da = dat[i];
                            resu += '<tr parentId="' + _id + '"><td class="text-right"><label class="px-single"><input type="checkbox" value="' + da.Id + '" class="px te_1_che " checked="checked"><span class="lbl"></span></label></td>';
                            var chilStar = $(send).parents("tr:first").children().eq(1).text();
                            resu += "<td>" + (parseInt(chilStar) + i + 1) + "</td><td></td><td></td><td>" + da.ProductNumber + "</td>";
                            resu += '<td><div style="display:block;width:80px;height:80px;overflow:hidden;border:1px solid #eaeaea;margin:0 auto 0 auto;"><img src="' + da.ThumbnailPath + '" style="margin:2px;max-width:74px;"></div></td>';
                            resu += "<td>" + da.BrandName + "</td>";
                            resu += "<td>" + da.CategoryName + "</td>";
                            resu += "<<td>" + da.SeasonName + "</td>";
                            resu += "<td>" + da.SizeName + "</td>";
                            resu += "<td>" + da.ColorName + "</td>";
                            if (da.UseDefaultDiscount) {
                                resu += "<td>默认折扣</td>";
                            } else {
                                resu += "<td>自定义折扣</td>";
                            }
                            resu += "<td>" + da.TagPrice + "</td>";
                            resu += "<td>" + da.RetailPrice + "</td>";
                            resu += "<td>" + da.WholesalePrice + "</td>";
                            resu += '<td class="text-middle" style="width: 15%;"><button id="View" title="查看详细信息" type="button" onclick="View(this,' + da.Id + ');" class="btn btn-xs  btn-padding-right"><i class="fa fa-eye"></i> </button><button id="Verify" title="审核数据" type="button" onclick="Verify(this,' + da.Id + ');" class="btn btn-xs  btn-padding-right"><i class="fa fa-key"></i> </button><button id="Update" title="修改数据" type="button" onclick="Update(this,' + da.Id + ');" class="btn btn-xs  btn-padding-right"><i class="fa fa-pencil"></i> </button><button id="Remove" title="将数据移动至回收站" type="button" onclick="Remove(this,' + da.Id + ');" class="btn btn-xs  btn-padding-right"><i class="fa fa-trash-o"></i> </button></td>';
                            resu + "</tr>";
                        }
                        $(send).parents("tr:first").after(resu);
                        $(".loading_hid_img").attr("hidden", "hidden");
                    }
                })
                //setting.sAjaxSource = "/Products/Product/GetChildByBigNum";
                //$.whisk.datatable.instance.fnSettings(setting);
                //$.whiskey.datatable.reset();
                $(send).attr("lodat", "ld");
            }

            if ($(send).attr("class").indexOf("treegrid-expander-collapsed") > 0) {
                $(send).removeClass("treegrid-expander-collapsed").addClass("treegrid-expander-expanded");
                $(send).parents("tr:first").nextAll("[parentId='" + _id + "']").show();
            } else {
                $(send).removeClass("treegrid-expander-expanded").addClass("treegrid-expander-collapsed");

                $(send).parents("tr:first").nextAll("[parentId='" + _id + "']").hide();
            }
        }

    });
    $(".bigcatedicou .dis").keyup(function () {
        var t = $(this).val().replace(/[^0-9.]/gi, "");
        if (parseFloat(t) > 10)
            t = 10;
        $(this).val(t);
    });
    $("#moda_but_succ").click(function () {
        var disname = $("#discou_name").val().trim();
        disname == "" ?
            $("#discou_name").parent("div").addClass("has-error") :
            $("#discou_name").parent("div").removeClass("has-error");
        if (disname != "") {
            $("#moda_but_succ").attr("disabled", "disabled");
            saveDis();
        }

    });
    $(".bigcatedicou #Create").on("click", function () {
       
        var isajx = false;
        $(".form-search .Coll").each(function () {
            if ($(this).val() != "") isajx = true;
        });
        if (!isajx) {
            $.whiskey.web.alert({
                type: "info",
                content: "请选择条件",
                callback: function () {
                }
            });
        } else {

            var isemp = false;
            $(".bigcatedicou .dis").each(function () {
                if ($(this).val() == "") isemp = true;
            });
            if (isemp) {
                $.whiskey.web.alert({
                    type: "info",
                    content: "折扣不能为空",
                    callback: function () {
                    }
                });
            } else {
                var branname = $("#Brands option:selected").text();
                var catename = $("#Categories option:selected").text();
                var sizename = $("#Sizes option:selected").text();
                var seasoname = $("#Seasons option:selected").text();
                var colorname = $("#Colors option:selected").text();
                var name = branname + catename + sizename + seasoname + colorname;
                name = name.replace(/[^a-zA-Z0-9]/gi,"");

                $("#discou_name").val(name);
                $("#productNum").modal({ keyboard: false, backdrop: "static" }).css({
                    "margin-top": function () {
                        return $(window).height() / 4;
                    }
                }).on('hide.bs.modal', function () {
                    $("#Create").removeAttr("disabled");
                    $("#moda_but_succ").removeAttr("disabled");
                    $("[data-dismiss='modal']").removeAttr("disabled");
                    $("#product_org_id").val("");

                });

            }
        }
        // $(this).attr("disabled", "disabled");
    });
    $(document).on("keyup", function (e) {
        var code = e.which | e.keycode;
        if (code == 13) {
            $("#moda_but_succ").click();
        }
    });
    $(".bigcatedicou #RemoveAll").on("click", function () {
        var list = $.whiskey.web.getIdByChecked(".table-list-bigca td input[type=checkbox]");
        if (list.length > 0) {
            var ids = [];
            for (var i = 0; i < list.length; i++) {
                var t = list[i];
                if (t.value.indexOf("par") == -1) {
                    ids.push(t.value);
                }
            }
            var isdele = !$(".trusher").is(":checked");
            var tit = "确认要将这些数据移至回收站吗？";
            var desc = "提示：数据移动到回收站后，随时可以从回收站中将其恢复";
            if (isdele) {
                tit = "确认要将这些数据从回收站移除吗？";
                desc = "提示：数据将会从回收站中永久移除，不支持恢复,请谨慎操作";
            }
        }
    });
    $("#Search").on("click", function () {
        var isajx = false;
        $(".form-search .Coll").each(function () {
            if ($(this).val() != "") isajx = true;
        });
        if (isajx) {
            var dt = $.whiskey.datatable.instance;
            var setting = dt.fnSettings();
            setting.sAjaxSource = "/Properties/ProductDiscount/GetProducts";

            dt.fnSettings(setting);
            $.whiskey.datatable.reset(true);
        } else {
            $.whiskey.web.alert({
                type: "info",
                content: "请选择查询条件",
                callback: function () {
                }
            });
        }
    });
}
function saveDis() {
    var li = new Object();
    $(".form-search .Coll").each(function () {
        var name = $(this).attr("name");
        var value = $(this).val();
        $(li).attr(name, value);
    });
    var startTime = $(".bigcatedicou #StartDate").val();
    var endtime = $(".bigcatedicou #EndDate").val();
    li.StartTime = startTime;
    li.EndTime = endtime;

    //零售折扣
    li.RetailDiscount = $("#RetailDisco").val().trim();
    //批发折扣
    li.WholesaleDiscount = $("#WholesaleDisco").val().trim();
    //采购折扣
    li.PurchaseDiscount = $("#PurchaseDisco").val().trim();
    //折扣名
    li.DiscountName = $("#discou_name").val().trim();
    $.post("/Properties/ProductDiscount/Create", { dto: li }, function (da) {
        if (da.ResultType == 3) {
            $("#productNum").modal('hide');
           window.location.href = "/Properties/ProductDiscount/Index";
            $.whiskey.web.alert({
                type: "success",
                content: "添加成功",
                callback: function () {
                }
            });
           
        }
    });
}
function padleft(st) {
    var _st = (st).toString(32);
    var t = "000";
    return t.substr(0, t.length - _st.length) + st;
}
function initDropdown(dropOj, da) {
    var droptarget = dropOj;
    if (dropOj == undefined)
        droptarget = $(".multiselect");
    droptarget.multiselect({
        nonSelectedText: "下拉选择",
        buttonWidth: "100%",
        buttonClass: 'col-md-12 btn',
        enableFiltering: true,
        enableCaseInsensitiveFiltering: true,
        filterPlaceholder: '快速检索',
        onChange: function (option, checked) {
            var hida = $(option).parents(".colld").find(".Coll");
            var arr = $(hida).val().split(",");
            var selda = $(option).val();
            if (checked) {
                arr.push(selda);
            } else {
                var ind = arr.indexOf(selda);
                arr.splice(ind, 1);
            }
            $(hida).val(arr.join(","));
        },
        onDropdownHidden: function (event) {
            var targ = $(event.target).parents(".colld:eq(0)").find(".Coll");
            var name = $(targ).attr("id");
            var val = $(targ).val();
            seleChang(name, val);
        }
    });
    if (da != undefined)
        droptarget.multiselect('dataprovider', da);
}


function seleChang(name, val) {
    if (name == "CategoryId") {
        if (val.startsWith(",")) {
            val = val.substr(1);
        }
        if (val.endsWith(",")) {
            val = val.substr(0, val.length - 1);
        }
        var catids = val.split(",");
        if (catids.length > 0) {
            $.post("/Properties/ProductDiscount/GetSizesByCategoryIds", { ids: catids }, function (da) {
                initDropdown($(".bigcatedicou #Sizes"), da);
            });
        } else {
            $(".bigcatedicou #Sizes").multiselect('destroy');
        }
    }
}
//搭配属性变动时，修改关联二级、
function otherColl(cruId) {
    if (cruId == "OneCollo") {
        reloadTwoColl();
    } else if (cruId == "TwoCollo") {
        reloadThreeColl();
    } else if (cruId == "ThreeCollo") {

    }

}

//修改二级
function reloadTwoColl() {
    var parids = $("#OneCollo").parent().find(".Coll").val();
    $.post("GetProductAttriByParentIds", { parIds: parids }, function (da) {
        /*
        var options = [
 {label: 'Option 1', title: 'Option 1', value: '1', selected: true},
 {label: 'Option 2', title: 'Option 2', value: '2'},
 {label: 'Option 3', title: 'Option 3', value: '3', selected: true},
 {label: 'Option 4', title: 'Option 4', value: '4'},
 {label: 'Option 5', title: 'Option 5', value: '5'},
 {label: 'Option 6', title: 'Option 6', value: '6', disabled: true}
];
        */
        if (da != null && da.length > 0) {
            var res = [];
            for (var i = 0; i < da.length; i++) {
                var td = da[i];
                res.push({ label: td.Text, title: td.Text, value: td.Value, });
            }
            $("#TwoCollo").html("").html(res);
            initDropdown($("#TwoCollo"), res);
        }
    });

    reloadThreeColl();
}

//修改三级
function reloadThreeColl() {
    //
    reloadForColl();

}
