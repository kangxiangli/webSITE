
//yxk 2015-9-14
//提供视图页面需要频繁使用的功能



//da:[{Text:'张三',Value='1'},{Text:'李四',Value:'2'}]
//emptyMess:当da为null时显示的信息
function getOptions(da, emptyMess) {
    var te = "";
    if (da != null & da != "") {
        $.each(da, function (i, v) {
            te += "<option value='" + da[i].Value + "'>" + da[i].Text + "</option>";
        }
       )
    }
    else {
        if (emptyMess != "")
            te += "<option>" + emptyMess + "</option>";
    }
    return te;
}


//数据校验
//对单个控件校验数字
function numberValid(contr) {
    var conval = $(contr).val().trim();
    var reg = /^\d+\.?\d+$/;
    if (!reg.test(conval)) {
        if (conval.length != 0) {
            $(contr).parent().addClass("has-error");
            $.whiskey.web.alert({ type: "warning", content: "输入的内容只能为数字", callback: function () { } })
        }
        else
            $(contr).parent().removeClass("has-error")
    }
    else {
        $(contr).parent().removeClass("has-error")
    }
}

//只能输入数字
function numberValidOverride(contr) {
    var conval = $(contr).val().trim();
    var reg = /^[0-9]*[0-9]$/
    if (!reg.test(conval)) {
        if (conval.length != 0) {
            $(contr).parent().addClass("has-error");
            conval = conval.replace(/[^\d]/g, "");
            $(contr).val(conval);
            $.whiskey.web.alert({ type: "warning", content: "输入的内容只能为数字", callback: function () { } })
        }
        else
            $(contr).parent().removeClass("has-error")
    }
    else {
        $(contr).parent().removeClass("has-error")
    }
}

//仓库类型：
var glo_storageType_1 = [
    {
        Value:"0",
        Text:"默认仓库"
       

    },
    {
        Value:"1",
        Text:"新建仓库"
    }
];
function GetAllStorageType() {
    return glo_storageType_1;
}
function GetStorageTypeByVal(_val) {
    glo_storageType_1.each(function (i, v) {
        if (glo_storageType_1[i].Value == _val)
            return glo_storageType_1[i].Text;
    });
    return null;
}
function guid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
// 生成guid值
function guid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
        return v.toString(16);
    });
}
function verifyDat() {
    return "<button display='inline'  style='color:green; margin-right:3px' title='确定入库' type='button' class='btn  fa fa-clipboard '></button>";
}
function deleDat() {
   
    return "<button display='inline' type='button' style='color:red'  margin-right:3px' title='删除' class='btn btn_remove fa fa-life-ring'></button>";
    
}
function review() {
    
    return "<button display='inline' type='button'  style='margin-right:3px' title='预览' class='btn  glyphicon glyphicon-eye-open'></button>";
}

function getDatetime() {
    var date = new Date();
    var year = date.getFullYear();
    var month = date.getMonth();
    var day = date.getDate();
    return (year + "/" + month + "/" + day);
}
function showTooTip(bo,da) {
    if (bo == true || bo == "true") {
        return "<label style='float:left' class='px-single'><button display='inline' attr='"+da+"'  style='color:green; margin-right:3px' type='button' class='btn che_1  fa fa-clipboard '></button></label>";

        
    }
    else {

        return "<label style='float:left' class='px-single'><button display='inline' attr='" + da + "'  style='color:red; margin-right:3px'  type='button' class='btn che_1 fa fa-life-ring'></button></label>";
    }
}







//动态添加一行
function addRow(da) {

    var jsonda = $.parseJSON(da);
    if (jsonda != undefined && jsonda != null) {
        var res = "";
        res += "<tr class='dt_row'><td class='dt_td dt_ch'>" + $.whiskey.datatable.tplListCheckbox(jsonda.Id) + "</td><td class='dt_td dt_num'>" + jsonda.ProductNumber + "</td><td class='dt_td'>" + jsonda.ProductName + "</td>";
        res += "<td class='dt_td'>" + jsonda.Brand + "</td><td class='dt_td'>" + jsonda.Size + "</td>";
        res += "<td class='dt_td'>" + jsonda.Season + "</td><td class='dt_td'>" + jsonda.Color + "</td>"
        res += "<td class='dt_td'><img class='img-thumbnail img-responsive' src='" + jsonda.Thumbnail + "'/></td><td class='dt_td'>" + jsonda.WholesalePrice + "</td><td class='dt_td pro_coun'><input style='width:100%' type='number' value='1' min='0' title='采购数量不能小于1件' class='form-control'></td>";

        res += "<td class='dt_td' style='width:15%'>" + deleDat() + verifyDat() + review() + "</td>"
        res += "</tr>";

        $($.whiskey.datatable.instance[0]).children("tbody").append(res);
    }

}
$("#sear-ok").click(function () {
    var scanNumber = $(".scan-number").val();
    EnterQue(scanNumber);
});