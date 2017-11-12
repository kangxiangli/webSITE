(function () {
	var flag=true;
    var intc;
    var setOptions = {
        countdown: 120,//短信验证有效时间
        bodyBlur: function () {
            $("#main-wrapper").addClass("blur-effect");
            window.onmousewheel = document.onmousewheel = function () {//禁止滚轮
                return false;
            };
        },
        bodyRemoveBlur: function () {
            $("#main-wrapper").removeClass("blur-effect");
            window.onmousewheel = document.onmousewheel = function () {
                return true;
            };
        },
        maskShow: function () {
            $(".fadeInMask").css("visibility", "visible");
        },
        maskHide: function () {
            $(".fadeInMask").css("visibility", "hidden");
        },

    };

           $(".triangle").click(function () {
                if (flag) {
                    $(".bg_saoma").show();
                     $(".bg_zhanghao").hide();
                  	$(".loginView").show();
                     $(".qr_code").animate({marginTop:'-150px',opacity:"0",},500);
                     $(".scan_code").hide();

                    flag = false;
                } else {
                	  $(".bg_saoma").hide();
                	  $(".bg_zhanghao").show();
                     $(".qr_code").animate({marginTop:'100px',opacity:"1",});
                      	$(".loginView").hide();
                      	$(".scan_code").show();
                    flag = true;
                }
            });

    /*----------------登录和注册动画------------*/

    $("#login_content").bind("click", function () {
        setOptions.maskShow();
        $(".login_content").show().animate({ "marginTop": "120px", "opacity": "1" }, 400);
        setOptions.bodyBlur();
    });

    $(".loginClose").bind("click", function () {
        setOptions.maskHide();
        $(".login_content").fadeOut(100).animate({ "marginTop": "0px", "opacity": "0" }, 200);
        setOptions.bodyRemoveBlur();
    });
    $(".rigisterClose").bind("click", function () {
        $(".register_content").fadeOut(100);
        setOptions.maskHide();
        setOptions.bodyRemoveBlur();
        $(".login_content").fadeOut(100).animate({ "marginTop": "0px", "opacity": "0" }, 200);
    })
    $(".loginbt img").hover(function () {
        $(this).parent().addClass("box-shadow-effect");
    }, function () {
        $(this).parent().removeClass("box-shadow-effect");
    })

    $("#register").click(function () {
        $(".login_content").fadeOut(200);
        setOptions.maskHide();
        $(".register_content").fadeIn(200);
    });
    /*获得验证码*/
    $("#getverification").click(function () {
        settime();
        localStorage.setItem("Countdown", setOptions.countdown);
        intc = setInterval(settime, 1000);
        var MobilePhone = $("#registerPhone").val();
        /*短信接口*/
        $.post("/verifycode/get", { "VerifyCodeType": 0, "MobilePhone": MobilePhone }, function (da) {
            if (da.ResultType == 3) {

            } else {
            layer.msg("手机号不能为空!"); 
                cleardaojishi();
            }

            layer.msg(da.Message);
        });
    });

    if (localStorage.Countdown) {
        setOptions.countdown = localStorage.Countdown;
        intc = setInterval(settime, 1000);
    }


	    /*验证码倒计时*/
    function settime() {
        var $this = $("#getverification");
        if (setOptions.countdown == 0) {
            cleardaojishi();
        } else {
            $this.css("background", "#777");
            $this.attr("disabled", "disabled");
            $this.text("重新发送(" + setOptions.countdown + ")");
            setOptions.countdown--;
            localStorage.setItem("Countdown", setOptions.countdown);
        }
    };

    function cleardaojishi(){
    	var $this = $("#getverification");
        $this.text("获取验证码");
        setOptions.countdown = 120;
        clearInterval(intc);
        $this.removeAttr("disabled");
        $this.css("background", "rgb(64,57,48)");
        localStorage.removeItem("Countdown");
    }
    /*注册接口*/
    $("#register_confirm").click(function () {
        var userName = $("#registerName").val();
        if (userName == "") { layer.msg("用户名不能为空!", {"icon":2}); return; };
        var PhoneNumber = $("#registerPhone").val();
        if (PhoneNumber == "") { layer.msg("手机号不能为空!"); return; };
        var VerifyCode = $("#verification").val();
        if (VerifyCode == "") { layer.msg("请填写短信验证码!"); return; };
        var PassWord = $("#setPwd").val();
        if (PassWord == "") { layer.msg("密码不能为空!"); return; };
        var confirmPwd = $("#confirmPwd").val();
        if (confirmPwd == "") { layer.msg("请确认密码!"); return; };
        if (confirmPwd != PassWord) layer.msg("两次密码输入不一致!");

        var userNameReg = /(\d*[a-zA-Z\u4e00-\u9fa5]+\d*)/;

        if (userNameReg.test(userName) == false) {
            layer.msg("不可为纯数字！"); return;
        } else {
            if (userName.length >= 2 && userName.length <= 10) {
                $.post("/Authorities/Login/register",

                {
                    "MemberName": userName,
                    "VerifyCode": VerifyCode,
                    "MemberPass": confirmPwd,// $.Aes.Encrypt("@ViewBag._tc_key", confirmPwd),
                    "MobilePhone":PhoneNumber,
                }
                , function (da) {
                    if (da.ResultType == 3) {
                        layer.msg("注册成功!",{icon:6});
                        $(".register_content").fadeOut(100);
                        setOptions.maskHide();
                        setOptions.bodyRemoveBlur();
                        $(".login_content").fadeOut(100).animate({ "marginTop": "0px", "opacity": "0" }, 200);
                    } 
                })
            } else {
                layer.msg("用户名长度应大于2位小于10位");
                return;
            }
        }
    });

    $("#pwd").keyup(function (e) {
        if (e.keyCode == 13) {
            $("#loginbtn").click();
        }
    });
       /*登录*/
    $("#loginbtn").click(function () {
        var MemberName = $("#usernames").val();
		var MemberPass = $("#pwd").val();
		if (MemberName == "") {$.fashion.web.msg({msg:"用户名不能为空!",icon:2 }); return; };
    	if (MemberPass == "") { $.fashion.web.msg({ msg: "密码不能为空!", icon: 2 }); return; };

    	$.fashion.web.ajax({
    		url: "/Authorities/Login/check",
            type: "post",
            data: {
                "MemberName": MemberName,
                "MemberPass": $.Aes.Encrypt($("#_tc_key").val(), MemberPass)
            },
            lockButton: $("#loginbtn"),
    	    errorIsMsg:true,
            closeDialogTimer: 1200,
            closeDialogEvent: function (da) {
                location.reload();
            },
            success: function (da) {
                if (da.ResultType == 3) {
                    setOptions.maskHide();
                    $(".login_content").fadeOut(100).animate({ "marginTop": "0px", "opacity": "0" }, 200);
                    setOptions.bodyRemoveBlur();
                }
            }
    	})
      
   

    });

    window.setOptions = setOptions;
})();

