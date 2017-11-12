/**
 * Created by Administrator on 2016/11/30.
 */

$(".qiangzhi_tc .back_befo").click(function () {
    window.location.href = '../dk.html';
});


//点击确定调用函数

$(function () {
    var adminId, uuId;
    if (window.localStorage) {
       adminId = localStorage.AdminId;

         uuId = localStorage.uuid;
    } else {
        showMsg("未获取到uuid");
    }
//获取当前用户登录的所有设备
    $.xiaodie.ajax({
        url: '/Authorities/Login/GetAllLoginDevice',
        type: 'post',
        data: {AdminId:adminId},
        success: function (da) {
            if(da.ResultType ==3) {
                
                var data = da.Data;
              
                var str = '';
                //debugger;
                //if(da.Data.length == 0){
                //    showMsg1("没有设备");
                //    return;
                //}
                $.each(data,function(i,item){
                    //解密
                    var paintText = $.Aes.Decrypt(adminId.toString(), item);
                    if(paintText.length>0){
                        var curData = paintText.split(';');
                        str += '<ul class="qztc_list" ><li><span>IP</span>: <span>' + curData[0] + '</span></li>';
                        str += '<li><span>系统</span>: <span>' + curData[1] + '</span></li>';
                        str += '<li><span>浏览器</span>: <span>' + curData[2] + '</span></li>';
                        str += '<li><span>时间</span>: <span class="time">'+a+'</li>';
                        str += '<li id="qztc_right" class="qztc_right" data-id='+item+'>'+"强制退出"+'</li></ul>';
                    }

                });
                $(".boxlist").html(str)
            }
        }
    });

    //强制退出单个
    $(".boxlist").on('click','.qztc_right',function () {
        var $this=$(this).parent('ul');
        var adminId = localStorage.AdminId;

        var uuId = $(this).attr("data-id");
        //console.log({AdminId:  adminId, uuid: uuId,clearAll: false});
        $.xiaodie.ajax({
            url: "/Authorities/Login/AutoLogout",
            type: "post",
            data: {AdminId:  adminId, uuid: uuId,clearAll: false},

            success: function (data) {
                //console.log(data)
                if (data.ResultType == '3') {
                    //console.log(data);
                    $this.fadeOut("fast",function(){

                        $this.remove();
                    });

                    //showMsg("强制退出成功");

                }
                else {
                    //showMsg(data.Message);
                }
            }
        })
    })
    ;
    //强制退出所有
    $("#tcImg_right").click(function () {
        var adminId = localStorage.AdminId;
        $.xiaodie.ajax({
            url: "/Authorities/Login/AutoLogout",
            type: "post",
            data: {AdminId:  adminId,clearAll: true},

            success: function (data) {
                if (data.ResultType == '3') {

                    window.location.href = '../dk.html';

                }
                else {
                    showMsg(data.Message);
                }
            }
        });

        function showMsg(inf) {
            $(".txtMsg").show();
            $(".txtP").html(inf);
        }
    });
});