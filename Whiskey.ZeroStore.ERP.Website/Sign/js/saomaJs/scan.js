function openCommodity() {
    uexScanner.open();
    uexScanner.cbOpen = function(opCode, dataType, data) {
        var obj = eval('(' + data + ')');
        window.localStorage.datas = obj.code;
        window.location.href = 'xinxichax/xinxichax.html';
    }
}

function aLogin() {
    uexScanner.open();
    uexScanner.cbOpen = function(opCode, dataType, data) {
        data = eval('(' + data + ')');
          alert("6666:"+data);return
        if (data.type.toLowerCase() == "qr_code") {//登录
          
            return;
            qrlogin(data.code);
            
        } else {
            readysendbarcode(data.code);
            //扫描
            aLogin();
            //扫码成功后再次调用摄像头
        }
    }
}

function qrlogin(dCode) {
   
    uuId = dCode.split(';')[0].split('=')[1];
  
    alert(dCode)
    type = dCode.split(';')[1].split('=')[1];
    alert("2")
    var AdminId = localStorage.AdminId;
     alert("3")
    if (type == 'qr_login') {
        window.localStorage.uuid = uuId;

        $.xiaodie.post("/Login/ScanComplete", {
            uuid : uuId,
            adminId : AdminId
        }, function(data) {
            if (data.ResultType == 3) {
                alert("2")
                window.location.href = 'login/login.html';
            }
        }, function(data) {
            alert(JSON.stringify(data))
        });

    }
}

//扫描二维码后执行的函数
var arr = [];
function readysendbarcode(dCode) {
    var result = dCode;
    //扫描结果
    var AdminId = localStorage.AdminId;
    // var AdminId=2119;
    arr.push(result);
    window.localStorage.setItem('arr', arr);
    var str = "";
    str = "<p class='scan_code'>" + result + "<span class='del'>删除</span></p>";

    $('#code_content').append(str);
}

//点击发送执行的函数
function sendmassage() {
    var AdminId = localStorage.AdminId;
    // var AdminId=2119;
    $.xiaodie.ajax({
        url : "/Msg/SendBarCodeInfo",
        type : "post",
        data : {
            barcode : arr,
            AdminId : AdminId
        },
        success : function(data) {
            if (data.ResultType == "3") {
                if (window.localStorage) {
                    window.localStorage.clear();
                }
            } else {
                showMsg(data.Message)
            }
        }
    });
}

//点击清空执行的函数
function clearAll() {
    var aDiv = $("#clearAll");
    aDiv.click(function() {
        $(this).parents().siblings("#code_content").find("p").remove();
    });
}

function showMsg(msg) {
    $(".warmPromptBg,.warmSure").show();
    $(".warmPrompt .xinXi").html(msg);

    $(".hideWarm").click(function() {
        $(".warmPromptBg,.warmSure").hide();
    });
}

//删除每条扫数据
$('#code_content').on('click', '.del', function() {
    var index = $(this).parent('.scan_code').index();
    arr.splice(index, 1);
    $(this).parent('.scan_code').fadeOut(500)
});

