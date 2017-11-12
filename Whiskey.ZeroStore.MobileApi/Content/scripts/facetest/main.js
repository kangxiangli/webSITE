(function ($) {
    $.tools = {
        alert: function (msg, seconds) {
            var $promp = $(".bg_dialog,.prompt_box");
            $promp.find(".inform_txt").html(msg || "未识别到人脸").end().fadeIn(400);
            setTimeout(function () { $promp.fadeOut() }, seconds || 1200);
        },

        debounce: function (func, wait) {
            var _timer;
            return function () {
                clearTimeout(_timer);
                _timer = setTimeout(func, wait);
            }
        }

    }
})(jQuery);

$(function () {

    var MemberId;
    var U_num;
    var data_Store;
    //				var phoneNum=18395601551;
    //				var pwd=123123;
    var num = 0;
    var phoneNum;
    var pwd;
    var host = "https://api.0-fashion.com/";//https://api.0-fashion.com/
    var cameraOpen = false;
    /*--动画--*/
    $(".returnIndex").click(function () {
        var isShow = $(".loginbox").css("display") == "block";
        if (isShow) {
            $("#close").click();
            if ($(".loginResult").css("display") == "block") {
                $(".loginResult,.canvas_box").hide();
            }
            $(".icbox").animate({ 'marginLeft': '0', 'opacity': '1' }, 450, function () {
                $(this).css("display", "flex");
                $(".loginbox,.returnIndex").hide()
            });
        } else {
            Camera.closeCamera();
            $(".load_centent").hide();
            $(".icbox").animate({ 'marginRight': '0', 'opacity': '1' }, 450, function () {
                $(this).css("display", "flex");
                $(".select,.canvas_box2,.camera_setting").css("display", "none");
                $(".returnIndex").hide();
            });
        }

    })

    $("#entryAndLogin").bind("click", function () {
        $(".icbox").animate({ 'marginLeft': '-100%', 'opacity': '0' }, 750, function () {
            $(this).css("display", "none");
            $(".loginbox").fadeIn()
            $(".returnIndex").fadeIn();
        });
        $(".userPhoto").remove();
        // $(".canvas_box,.loginResult").animate({"left":"100%","opacity":"0"},400);
        $("#login span").text("登录");
        $("#username,#pwd").fadeIn();
        // $("#login").unbind().bind("click.login",login);
    })
    $("#camera_img").click(function () {
        $(".icbox").animate({ 'marginRight': '-100%', 'opacity': '0' }, 750, function () {
            $(this).css("display", "none");
            Camera.openCamera(2, false);
            $(".select,.canvas_box2,.camera_setting").css("display", "block");
            $(".returnIndex").fadeIn();
        });
    })
    $('#upload').click(function () {
        Camera.addFace();
    })
    /***---------点击录入用户信息--------**/


    $("#username").bind("keyup", function (e) {
        if (e.keyCode == 13) {
            $("#pwd").focus();
        }
    })
    $("#pwd").bind("keyup", function (e) {
        if (e.keyCode == 13) {
            $("#login").click();
        }
    })
    $("#login").bind("click.login", login);
    function login() {


        if ($("#login>span").text() == "切换账号") {
            $(".userPhoto").remove();
            $(".canvas_box,.loginResult").animate({ "left": "100%", "opacity": "0" }, 400);
            $("#login span").text("登录");
            $("#username,#pwd").fadeIn();
        } else {
            phoneNum = $("#username").val();
            pwd = $("#pwd").val();

            $.ajax({
                type: "POST",
                url: host + 'api/members/member/login',
                data: {
                    PhoneNumber: phoneNum,
                    PassWord: pwd
                },
                success: function (da) {
                    if (da.ResultType == 3) {
                        var data = da.Data;
                        MemberId = data.MemberId;
                        U_num = data.U_Num;
                        /*再获取归属店铺信息*/
                        $.post(host + 'api/Members/MemberInfo/GetMemberStore',
						{
						    MemberId: MemberId,
						    U_Num: U_num
						}, function (dataStore) {
						    if (dataStore.ResultType == 3) {
						        var data_Store = dataStore.Data;
						        var minStore;
						        if (data_Store) {
						            minStore = data_Store.filter(function (item, ind) {
						                return !item.IsMainStore;
						            })[0];
						        }
						        if (minStore) {
						            $("#username,#pwd").val("");
						            $(".loginResult").html("欢迎尊贵的" + minStore.StoreName + "会员 " + data.MemberName);
						            $(".canvas_box,.loginResult").show();
						            $(".canvas_box,.loginResult").animate({ "left": "0%", "opacity": "1" }, 400);
						            $("#username,#pwd").hide();
						            $(".loginboxheader").after("<div class='userPhoto'><img src='" + data.UserPhoto + "' /></div>")
						            $("#login span").text("切换账号");
						            Camera.openCamera(1);
						            $("#close").click(function () {
						                Camera.closeCamera();
						            })

						        }
						    }
						})
                    } else {
                        $.tools.alert(da.Message)
                    }
                }
            })
        }
    }

    function poople_detail(data) {
        if (data && data.length > 0) {
            var MemberInfo = data[0];
            var MemberName = MemberInfo.MemberName;
            var MobilePhone = MemberInfo.MobilePhone;
            var StoreName = MemberInfo.StoreName;
            var MemberType = MemberInfo.MemberType;
            var CardNumber = MemberInfo.CardNumber;
            var DateofBirth = MemberInfo.DateofBirth;
            var UserPhoto = MemberInfo.UserPhoto;
            var MemberFigure = MemberInfo.MemberFigure;
            var ApparelSize = MemberFigure.ApparelSize;
            var PreferenceColor = MemberFigure.PreferenceColor;
            var Bust = MemberFigure.Bust;
            var FigureDes = MemberFigure.FigureDes;
            var FigureType = MemberFigure.FigureType;
            var Height = MemberFigure.Height;
            var Hips = MemberFigure.Hips;
            var Shoulder = MemberFigure.Shoulder;
            var Waistline = MemberFigure.Waistline;
            var Weight = MemberFigure.Weight;

            var topSize = "";
            var bottomSize = "";
            if (ApparelSize) {
                var _appsize = ApparelSize.split(",");
                if (_appsize.length > 0) {
                    topSize = _appsize[0];
                }
                if (_appsize.length > 1) {
                    bottomSize = _appsize[1];
                }
            }
            var Gender = MemberInfo.Gender == 0 ? "女" : "男";
            DateofBirth = function (str) {
                if (str)
                    return new Date(parseInt(str.substr(6, 13))).toLocaleDateString();
                return "";
            }(DateofBirth);

            $(".MemberName_2").html(MemberName);
            $(".MobilePhone_2").html(MobilePhone);
            $(".DateofBirth_2").html(DateofBirth);
            $(".StoreName_2").html(StoreName);
            $(".MemberType_2").html(MemberType);
            $(".CardNumber_2").html(CardNumber);
            $(".UserPhoto_2").attr("src", UserPhoto);
            $(".topSize_2").html(topSize);
            $(".bottomSize_2").html(bottomSize);
            $(".MemberName_2").html(MemberName);
            $(".PreferenceColor_2").html(PreferenceColor);
            $(".Bust_2").html(Bust+"cm");
            $(".FigureDes_2").html(FigureDes);
            $(".FigureType_2").html(FigureType);
            $(".Gender_2").html(Gender)
            $(".Hips_2").html(Hips+"cm");
            $(".Shoulder_2").html(Shoulder);
            $(".Waistline_2").html(Waistline+"cm");
            $(".Height_2").html(Height+"cm");
            $(".Weight_2").html(Weight + "kg");
            $(".load_text_container").css("display", "block")
            var htmlStr = $(".load_text_container").prop("outerHTML");
     
        
            Typed.new('.load_text', {
                strings: [htmlStr],
                showCursor: false,
            });
            $('#load_animate,.load_centent').fadeIn();
           
        }
    }


    window.Camera = {
        /*打开摄像头*/
        openCamera: function (type, autoclose) {

            if (type == 1) { openCamera(type); }
            else {
                var storeId = $(".select_li").attr("data-id");
                if (storeId) {
                    openCamera(type);
                    cameraOpen = true;
                    $(".camera_set").show();
                }
            }
            if (autoclose) {
                var _closeTime = typeof autoclose == "number" ? autoclose : 300000;
                $.tools.debounce(Camera.closeCamera, _closeTime);
                //					cameraOpen=false;
            }
        },
        closeCamera: function () {
            cameraOpen = false;
            mediaStreamTrack && mediaStreamTrack.stop();
            $(".camera_set").hide();
        },
        /*------------获得facetokens删除单个时用-------------------*/
        getfaceToken: function () {

            $("#getface").click(function () {
                var val = $("#imgValue").val();
                $(".result").html('');
                $.post("/Api/Intelligents/Face/GetFace", {
                    MemberId: MemberId,
                    U_Num: U_num,
                }, function (da) {
                    $(".result").text(JSON.stringify(da));

                })
            });
        },
        /*---------添加face-------------*/
        addFace: function () {
            var imgurl = canvas.toDataURL("image/jpeg", 0.5);
            imgurl = imgurl.replace("image/jpeg");
            var img = convertBase64UrlToBlob(imgurl);
            var formData = new FormData();
            formData.append("file", img);
            formData.append("MemberId", MemberId);
            formData.append("U_Num", U_num);

            $.ajax({
                url: host + "Api/Intelligents/Face/AddFace",
                type: "post",
                data: formData,
                processData: false, //默认为true  对data参数进行序列化处理
                contentType: false,
                success: function (data) {

                    if (data.ResultType == 3) {
                        $.tools.alert("已添加到人脸库");
                    } else if (data.Message == "CONCURRENCY_LIMIT_EXCEEDED") {

                        if (num == 15) {
                            $.tools.alert("添加失败，请矫正姿势，重新录入");
                            num = 0;
                        } else {
                            num += 1
                            Camera.addFace();
                        }
                    } else {
                        $.tools.alert(data.Message);
                    }
                },
                error: function (data) {
                    $.tools.alert(data);
                }
            });
        },
        /*-------------匹配face----------*/
        serach: function () {
            //			$.tools.debounce(Camera.closeCamera,5000);
            //cameraOpen=false;
            var imgurl = canvas2.toDataURL("image/jpeg", 0.5);
            imgurl = imgurl.replace("image/jpeg");
            var img = convertBase64UrlToBlob(imgurl);
            var storeId = $(".select_li").attr("data-id");

            var formData = new FormData();
            formData.append("file", img);
            formData.append("storeId", storeId);
            $.ajax({
                url: host + "/FaceTest/SearchMemberIds",
                type: "post",
                data: formData,
                processData: false, //默认为true  对data参数进行序列化处理
                contentType: false,
                success: function (data) {
                    if (data.ResultType == 3) {
                        $(".camera_set").hide();
                        poople_detail(data.Data);
                    }
                    else if (data.Message == "CONCURRENCY_LIMIT_EXCEEDED") {
                        if (num == 15) {
                            $.tools.alert("未匹配到会员");
                            num = 0;
                        } else {
                            num += 1
                            Camera.serach();
                        }
                    } else if (data.Message == "INVALID_OUTER_ID") {
                        $.tools.alert("未匹配到会员");
                    } else {
                        $.tools.alert(data.Message);
                    }
                },
                error: function (data) {
                    console.log(JSON.stringify(data));
                }
            });
        },

        /*---------删除单个face需要先拿到token 用getFace接口------*/
        remove: function (facetoken) {
            $.post("/Api/Intelligents/Face/RemoveFace", {
                MemberId: MemberId,
                U_Num: U_num,
                FaceToken: facetoken,
            }, function (da) {
                $(".result").text(JSON.stringify(da))
            })
        },
        /*---------删除整个faceset集合 ------*/
        removeAll: function () {
            $.post("/Api/Intelligents/Face/RemoveFaceAll", {
                MemberId: MemberId,
                U_Num: U_num,
            }, function (da) {
                $(".result").text(JSON.stringify(da))
            })
        }
    }
    $("#addFace").click(function () {
        Camera.addFace();
    })
    //Base64转换Blob
    function convertBase64UrlToBlob(urlData) {
        //	alert(urlData);
        var bytes = window.atob(urlData.split(',')[1]); //去掉url的头，并转换为byte
        //处理异常,将ascii码小于0的转换为大于0
        var ab = new ArrayBuffer(bytes.length);
        var ia = new Uint8Array(ab);
        for (var i = 0; i < bytes.length; i++) {
            ia[i] = bytes.charCodeAt(i);
        }
        return new Blob([ab], {
            type: 'image/png'
        });
    }

    //模拟下拉框
    $.ajax({
        type: "POST",
        url: host + 'api/stores/store/queryattachstore',
        success: function (da) {
            var newStore = {};

            if (da.ResultType == 3) {
                var data = da.Data;
                $.each(data, function (index, val) {
                    var StoreTypeName = val.StoreTypeName;
                    if (newStore[StoreTypeName]) {
                        newStore[StoreTypeName].push(val)
                    } else {
                        newStore[StoreTypeName] = [val]
                    }
                });
                var str = "";
                $.each(newStore, function (StoreTypeName, val) {
                    str += "<li class='disabled'>--- " + StoreTypeName + " ---</li>"
                    $(val).each(function (i, item) {
                        str += '<li data-Id=' + item.Id + '>' + item.StoreName + '</li>'
                    })
                });
                $(".select_ul").append(str);
            }
        }
    })
    $('.select input').on('click', function () {
        if ($('.select .city').is('.hide')) {
            $('.select .city').removeClass('hide');
        } else {
            $('.select .city').addClass('hide');
        }
    })
    $('.select_ul').on('click', "li", function () {
        $(this).addClass("select_li").siblings().removeClass("select_li")
        $('.select input').val($(this).html());
        $('.select .city').addClass('hide');
        if ($(".load_centent").css("display") == "block") {
            $(".load_centent").fadeOut();
        }
        if (cameraOpen == false) {
            Camera.openCamera(2, true);
        }
        if (context) {
            context.clearRect(0, 0, canvas.width, canvas.height);
        }


    })

})