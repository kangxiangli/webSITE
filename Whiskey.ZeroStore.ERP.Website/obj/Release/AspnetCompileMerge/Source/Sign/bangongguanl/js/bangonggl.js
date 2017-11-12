/**
 * Created by xiuxia on 2016/9/23.
 */
//办公管理
var wh = $(window).height();
$(".sixBox").height(wh - $(".til_bak").height());
$(".sixBox>div").height($(".sixBox").height() / 3);
var inerHg = $(".sixBox>div>a").height(),
    inerWh = $(".sixBox>div>a").width();
$(".sixBox>div>a").css("top", '50%').css("margin-top", -inerHg / 2).css("left", '50%').css("margin-left", -inerWh / 2);
$(".sixBox>div>a").click(function () {
    $(this).addClass("smallBig");
    setTimeout(function () {
        $(".sixBox>div>a").removeClass("smallBig");
    }, 1000);
});
$(".bangonggl .back_tx").click(function () {
    window.location.href = "../dk.html";
});

//提交离职申请
$("#leave_btn").click(function () {
    var result = checkResignation();
    if (result == false) {
        return false;
    } else {
        showDialog("您确定提交离职申请吗？");
    }
});
//点击确定按钮离职调用函数
function leavaFn() {
    var adminId = localStorage.AdminId;
    $("#ResignationId").attr("value", adminId);
    var entity = $("#leave_form").serializeObject();
    $.xiaodie.ajax({
        url: "/admin/ApplyResignation",
        type: "post",
        data: entity,
        success: function (data) {
            if (data.ResultType == 3) {
                getResignation();
            } else {
                showMsg(data.Message);
            }
        }
    });
}

getResignation();
//离职状态返回 0表示审核中，1表示审核通过，2表示审核不通过
function getResignation() {
    var adminId = localStorage.AdminId;
    $.xiaodie.ajax({
        url: "/admin/GetResignation",
        type: "post",
        data: { AdminId: adminId },
        success: function (data) {
            if (data.ResultType == 3) {
                var adminDate = data.Data;
                var dateTime = adminDate.ResignationDate;
                var beLeave = adminDate.ResignationReason;
                var PersonnelVt = adminDate.PersonnelVerifyType; //人事
                var TechniqueVt = adminDate.TechniqueVerifyType;  //技术
                var ManagerVt = adminDate.ManagerVerifyType;  //经理
                if (dateTime == '' || dateTime == null || dateTime == undefined) {
                    $("#leave_form").show();
                    $(".leave_info").hide();
                } else {
                    $("#leave_form").hide();
                    $(".leave_info").show();
                    $(".leave_info .dateTm").val(dateTime);
                    $(".leave_info .leaveBf").html(beLeave);

                    //离职申请
                    var pass = ".pass", audit = ".audit", notPass = ".notPass";
                    getstate(PersonnelVt, pass);
                    getstate(TechniqueVt, audit);
                    getstate(ManagerVt, notPass);
                }
            } else {
                showMsg(data.Message);
            }
        }
    })
}


//校验离职申请
function checkResignation() {
    var dateTime = $("#leaveOffice").val(),
        because = $("#leave_because").val();
    if (dateTime == null || dateTime == '' || dateTime == undefined) {
        showMsg("离职日期有误");
        return false;
    }
    if (because == null || because == '' || because == undefined) {
        showMsg("请填写离职原因");
        return false;
    } else {
        if (because.length > 50) {
            showMsg("离职原因不能超过50个字符");
            return false;
        } else {
            return true;
        }
    }
}

//离职审核状态
function getstate(state, cls) {
    $(cls + " a").remove();
    if (state == 0) {
        $(cls).append('<a href="#" class="tooltip-test" data-toggle="tooltip" title="审核中"><img src="../img/audit.png" alt="审核中"/></a>');
    } else if (state == 1) {
        $(cls).append('<a href="#" class="tooltip-test" data-toggle="tooltip" title="通过"><img src="../img/pass.png" alt="审核中"/></a>');
    } else {
        $(cls).append('<a href="#" class="tooltip-test" data-toggle="tooltip" title="不通过"><img src="../img/not_pass.png" alt="审核中"/></a>');
    }
}

//展示提示信息
//单按钮
function showMsg(msg) {
    $(".warmPromptBg,.warmSure").show();
    $(".warmPrompt .xinXi").html(msg);

    $(".hideWarm").click(function () {
        $(".warmPromptBg,.warmSure").hide();
    });
}

//双按钮
function showDialog(msg) {
    $(".warmPromptBg,.warmPrompt").show();
    $(".warmPrompt .yuanyin").html(msg);

    $(".warmPrompt .btnSure").click(function () {
        leavaFn();
        $(".warmPromptBg,.warmPrompt").hide();
    });
    $(".warmPrompt .btnNo").click(function () {
        $(".warmPromptBg,.warmPrompt").hide();
    });
}


//    发布通知
$('.send_info').click(function () {
    var AdminId = localStorage.AdminId;
  
    var result = checkMsge();
    if (result == false) {
        return false;
    }
    var InfoTit = $(".notifo_txt").val(),  //标题
        golId = $(".goal").attr("datid"), //目标类型
        InfoCon = $('.notifi_txt').val();   //内容
    var departMent = $(".department").attr("datid");  //获取部门
    $.xiaodie.ajax({
        url: '/Msg/SendNotice',
        type: 'post',
        data: { adminId: AdminId, title: InfoTit, target: golId, departmentIds: departMent, content: InfoCon },
        success: function (data) {
            if (data.ResultType == '3') {
                showError("发送成功");
                $(".sure_btn").click(function () {
                    $(".showMgBg,.showMg").hide();
                    window.location.href = "bangongguanli.html";
                });
            }
            else {
                showError(data.Message);
            }
        }

    });

});
$(".select_bg").click(function () {
    $(".select_bg,#seleId").hide();
});

//序列化目标
$(".goal").click(function () {
    $(".select_bg,#seleId").show();
    $("#dptMent").hide();
    $.xiaodie.ajax({
        url: '/Msg/GetNoticeTarget',
        type: 'post',
        success: function (data) {
            $(".select_id").empty();
            if (data.ResultType == '3') {
                var datas = data.Data;
                for (var i = 0; i < datas.length; i++) {
                    var selLi = '<li onclick="getSelHtml(this)" data-id=' + datas[i].value + '>' + datas[i].name + '</li>';
                    $(".select_id").append(selLi);
                }
            }
        }
    });
});
$(".select_id li").click(function () {
    var thHtml = $(this).html();
    $(".goal").val($tthHtml);
    $(".select_bg,#seleId").hide();

});

//目标选定并序列化部门列表
function getSelHtml(op) {
    var $t = $(op).html(),
        dataId = $(op).attr("data-id");
    $(".goal").val($t).attr("datId", dataId);
    $(".select_bg,#seleId").hide();
    if (dataId == 2) {
        $(".department").css("display", "block");
        $.xiaodie.ajax({
            url: '/Msg/GetDepartmentInfo',
            type: 'post',
            success: function (data) {
                $(".dptMent").empty();
                if (data.ResultType == '3') {
                    var depList = data.Data;
                    for (var i = 0; i < depList.length; i++) {
                        var depLi = ' <li data-id=' + depList[i].value + '>' + depList[i].Name + '</li>';
                        $(".dptMent").append(depLi);
                    }
                }
            }
        });
    } else {
        $(".department").css("display", "none");
    }
}


//获取部门列表
$(".department").click(function () {
    $(".select_bg,#dptMent").show();
    $("#dptMent li").click(function () {
        var character = $(this).html(),
            datId = $(this).attr("data-id");
        $(".department").val(character).attr("datId", datId);
        $(".select_bg,#dptMent").hide();
    });
});
//校验通知form表单
function checkMsge() {
    var tit = $(".notifo_txt").val(),
        gol = $(".goal").val(),
        cot = $('.notifi_txt').val();
    if (tit == '' || tit == undefined || tit == null) {
        showError("标题不能为空");
        return false;
    } else if (gol == '' || gol == undefined || gol == null) {
        showError("目标不能为空");
        return false;
    } else if (cot == '' || cot == undefined || cot == null) {
        showError("内容不能为空");
        return false;
    }
}
//end 发布通知

//工作日志
//校验工作日志
function checkWork() {
    var logtit = $(".log_type").val(),
        logtil = $(".log_til").val(),
        logcot = $('.log_txt').val();
    if (logtit == '' || logtit == undefined || logtit == null) {
        showError("日志类型不能为空");
        return false;
    } else if (logtil == '' || logtil == undefined || logtil == null) {
        showError("日志名称不能为空");
        return false;
    } else if (logcot == '' || logcot == undefined || logcot == null) {
        showError("日志内容不能为空");
        return false;
    }
}
//序列化日志类型
$(".log_type").click(function () {
    $(".select_type,.typeDiv").show();
    $.xiaodie.ajax({
        url: '/Work/GetWorkLogTags',
        type: 'post',
        success: function (data) {
            if (data.ResultType == '3') {
                var dataType = data.Data;
                $(".type_id").empty();
                for (var i = 0; i < dataType.length; i++) {
                    var liHd = '<li value=' + dataType[i].Id + '>' + dataType[i].WorkLogAttributeName + '</li>';
                    $(".type_id").append(liHd);
                }
            }
        }
    });

    //发送工作日志
    $(".send_log").click(function () {
        var resultWk = checkWork();
        if (resultWk == false) {
            return false;
        }
        var adminId = localStorage.AdminId,
            workType = $(".log_type").val(),
            workTil = $(".log_til").val(),
            workCon = $('.log_txt').val();
        $.xiaodie.ajax({
            url: '',
            type: 'post',
            data: { AdminId: adminId },
            success: function (data) {
                if (data.ResultType == 3) {
                    showError("发送成功");
                }
            }
        });

        $(".sure_btn").click(function () {
            $(".showMgBg,.showMg").hide();
            window.location.href = "bangongguanli.html";
        });
    });

    $(".type_id li,.select_type").click(function () {
        var typ = $(this).html();
        $(".log_type").val(typ);
        $(".select_type,.typeDiv").hide();
    });
});
//end 工作日志

//信息记录
//选择店铺
$(".shop_id").focus(function () {
    $("#type_box").hide();
    $(".bg_shade,#store_box").show();
    $("#select_store li").click(function () {
        var htl = $(this).html();
        $(".shop_id").val(htl);
        $(".bg_shade,#store_box").hide();
    });
});
//选择类型
$(".type_txt").focus(function () {
    $("#store_box").hide();
    $(".bg_shade,#type_box").show();
    $("#type_cost li").click(function () {
        var htl = $(this).html();
        $(".type_txt").val(htl);
        $(".bg_shade,#type_box").hide();
    });
});
//点击遮罩层关闭选框
$(".bg_shade").click(function () {
    $(".bg_shade,#store_box,#type_box").hide();
});
//提交记录
$(".record_btn").click(function () {
    var result = checkForm();
    if (result == false) {
        return false;
    } else {
        var adminId = localStorage.AdminId;
        $("#admin_id").attr("value", adminId);
        var formData = $("#record_form").serializeObject();  //序列化表单数据
        console.log(formData);
        showError("提交成功");
        //$(".sure_btn").click(function () {
        //    window.location.href = "bangongguanli.html";
        //});
        /*$.xiaodie.ajax({
         url: "",
         type: "post",
         data: entity,
         success: function (data) {
         if (data.ResultType == 3) {
         showError("提交成功");
         $(".sure_btn").click(function () {
         window.location.href = "bangongguanli.html";
         });
         } else {
         showError(data.Message);
         }
         }
         });*/

    }
});

//校验提交表单
function checkForm() {
    var recordtTil = $(".record_til").val(),
        shopId = $(".shop_id").val(),
        typeTxt = $(".type_txt").val(),
        moneyIp = $(".money_ip").val(),
        starTime = $("#start_time").val(),
        endTime = $("#end_time").val();
    var starDate = new Date(starTime),
        endDate = new Date(endTime),
        num = endDate - starDate;
    if (recordtTil == '' || recordtTil == null || recordtTil == undefined) {
        showError("标题不能为空");
        return false;
    } else if (shopId == '' || shopId == null || shopId == undefined) {
        showError("店铺不能为空");
        return false;
    } else if (typeTxt == '' || typeTxt == null || typeTxt == undefined) {
        showError("类型不能为空");
        return false;
    } else if (moneyIp == '' || moneyIp == null || moneyIp == undefined) {
        showError("总金额不能为空");
        return false;
    } else if (isNaN(moneyIp)) {  //校验金额是否为数字
        showError("金额有误");
        return false;
    } else if (starTime == '' || starTime == null || starTime == undefined) {
        showError("开始日期有误");
        return false;
    } else if (endTime == '' || endTime == null || endTime == undefined) {
        showError("结束日期有误");
        return false;
    } else if (num < 0) {
        showError("结束日期有误");
        return false;
    }
}
//end信息记录

//将表单数据序列化成Json数据
$.fn.serializeObject = function () {
    var o = {};
    var a = this.serializeArray();
    $.each(a, function () {
        if (o[this.name] !== undefined) {
            if (!o[this.name].push) {
                o[this.name] = [o[this.name]];
            }
            o[this.name].push(this.value || '');
        } else {
            o[this.name] = this.value || '';
        }
    });
    return o;
};
//提示框
function showError(txt) {
    $(".showMg .msg_info").html(txt);
    $(".showMgBg,.showMg").show();
}
$(".sure_btn").click(function () {
    $(".showMgBg,.showMg").hide();
});


//  end  发布通知
$("#leave,.send_back,.record_info").click(function () {
    window.location.href = "bangongguanli.html";
});




















