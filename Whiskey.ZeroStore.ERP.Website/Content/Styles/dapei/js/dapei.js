var host = 'http://11.1.1.113:8888';
var myScroll;
var ImageObjectArr = new Array();
ImageObjectArr = [0];
var selectObj = null; //被选中的canvas对象
var canvas = new fabric.Canvas('palette');
var context = canvas.getContext("2d");

//*DPR的目的就是放大canvas达到canvas渲染按1:1的比例
//var canvasWidth = $('.canvasBox').width();
var canvasHeight = canvasWidth =400;
//console.log(canvasWidth);
canvas.setWidth(canvasHeight);
canvas.setHeight(canvasHeight);

//最后在缩小DPR倍的区域，实现大小的控制
//背景
canvas.setBackgroundColor('rgba(0, 0, 0,.2)');

//添加搭配图btn
fabric.Image.fromURL('', function(img) {
	img.set({
		left: (canvasWidth / 2) - 30,
		top: (canvasHeight / 2) - 30,
		height: 60,
		width: 60,
		angle: 0,
		lockMovementX: true,
		lockMovementY: true,
		hasControls: false,
		hasBorders: false,
		selectable: false,
	})
	canvas.add(img);
	img.bringToFront();
	addBtn = img;
	img.moveTo(0);
	img.type = 'add_danpin';
	canvas.renderAll();
})

$(".sucai").click(function() {
	$(".img_box").show().animate({
	    left:0,
	});
	$(".backImg").animate({
	    left: '110%',
	});;
})
$(".addBackground").click(function() {
	$(".img_box").animate({
	    left: '110%',
	});;
	$(".backImg").show().animate({
	    left: 0,
	});;
})

var mebId, Unum, dpId, listMemberId;
/********************************** 获取这条数据的memberId **************************************/

listMemberId = $(".memberId").val();
dpId = $(".palette").attr("data-viewId");

/*************************************** 获取登录信息 **********************************/

$.ajax({
    type: "post",
    url: "/products/memberCollocation/getMemberProfile",
    success: function (res) {
        if (res.ResultType == 3) {
            mebId = res.Data.MemberId;
            Unum = res.Data.U_Num;
            //debugger
            getIcon(mebId, Unum);
            console.log('成功', mebId, Unum);

            if (mebId == listMemberId) {
                $(".monogamy").show();
                $(".shangpin,.dialogShowMsg,saveAll").hide();
                console.log(mebId, listMemberId);
                //请求成功后调用的回调函数 
                EDIT.initData();
                colorGet();
            } else {
                $(".monogamy,.loadGig,.shangpin,.saveAll").hide();
                showDialog("无权操作");
            }
        } else {
            showDialog("请求有误");
        }
        
    } 
});

function showDialog(msg) {
    $(".monogamy").hide();
    $(".dialogShowMsg").show().html(msg);
}

/*************************************** 保存 ************************************/

//获取位置 
function getPosition(img) {
    var role = Math.round(img.getAngle()); //角度
    var x_y = img.getCenterPoint(); //x+y坐标
    var x = img.getLeft() //x_y.toString().split(',')[0];//x      
    var y = img.getTop() //x_y.toString().split(',')[1];//y
    var width = Math.round(img.getWidth()); //拉伸宽度
    var height = Math.round(img.getHeight());
    img.Frame = '{{' + x + ', ' + y + '}, {' + width + ', ' + height + '}}';
    img.Spin = matrixTotransform(role);
}
//获取图片位置信息
var imgPositionList = [];
var textareaList = [];

function getImgPositionList(edit) {

    var toAddStatus = true;
    if (ImageObjectArr.length <= 1) {
        alert('请至少添加一张图片');
        toAddStatus = false;
        return false;
    }
    $(ImageObjectArr).each(function (i, item) {
        if (i == 0) {
            if (!backgroundObject.ProductId) {
                alert('请添加背景图');
                toAddStatus = false;
                return false;
            }
            ImageObjectArr[0] = {
                Level: backgroundObject.Level,
                ProductId: backgroundObject.ProductId,
                OperationType: 1,
                ProductSource: backgroundObject.ProductSource,
                Frame: 'null',
                Spin: '[]'
            }
        } else {
            item.Level = i;
            getPosition(item);
        };

    })
    $.each(editDelImageLIst, function (i, item) {
        item.OperationType = 2;
    })
    editAllImgList = ImageObjectArr.concat(editDelImageLIst);
    //循环保留所需参数
    imgPositionList = [];

    $(editAllImgList).each(function (i, item) {
        var data = {
            Level: item.Level,
            ProductId: parseInt(item.ProductId),
            ProductSource: item.ProductSource,
            Frame: item.Frame,
            Spin: item.Spin
        }
        if (edit) {
            data.OperationType = item.OperationType;
            data.Id = parseInt(item.ProductId);
        }
        imgPositionList.push(data)
    })
    //获取文字
    var textareaList = [];
    var editTextList = [];
    $(editTextList).each(function (i, item) {
        var json = {};
        json.Text = item.text;
        getPosition(item);
        json.Frame = item.Frame;
        json.Spin = item.Spin;
        json.FontSize = item.fontSize;
        json.Color = item.fill.RGBtocolorIOS();
        //json.Color = item.fill.colorRgb();

        if (edit) {
            json.OperationType = item.OperationType;
        }
        if (item.Id) {
            json.Id = parseInt(item.Id);
        }
        textareaList.push(json);
    })
    localStorage.dapeiToSaveTextList = textareaList;
    localStorage.dapeiToSaveImgList = imgPositionList;
    return toAddStatus;
}
//矩阵转角度
function getmatrix(matrix) {
    matrix = matrix.replace(/\[/, '').replace(/\]/, '');
    var arr = [];
    arr = matrix.split(', ');
    var aa = Math.round(180 * Math.asin(arr[0]) / Math.PI);
    var bb = Math.round(180 * Math.acos(arr[1]) / Math.PI);
    var cc = Math.round(180 * Math.asin(arr[2]) / Math.PI);
    var dd = Math.round(180 * Math.acos(arr[3]) / Math.PI);
    var deg = 0;
    if (aa == bb || -aa == bb) {
        deg = dd;
    } else if (-aa + bb == 180) {
        deg = 180 + cc;
    } else if (aa + bb == 180) {
        deg = 360 - cc || 360 - dd;
    }
    return deg >= 360 ? deg % 360 : deg;
}
//角度转矩阵
function matrixTotransform(deg) {
    var cosVal = Math.cos(deg * Math.PI / 180);
    var sinVal = Math.sin(deg * Math.PI / 180);
    var valTransform = '[' + cosVal.toFixed(6) + ', ' + sinVal.toFixed(6) + ', ' + (-1 * sinVal).toFixed(6) + ', ' + cosVal.toFixed(6) + ', 0, 0]'
    return valTransform;
}

/*IOS color转换RGBA */
String.prototype.colorRGBA = function () {
    var that = this;
    var aColor = that.replace(/^\[/, "").replace(/\]$/, "").split(' ');
    for (var i = 0; i < aColor.length - 1; i++) {
        aColor[i] = aColor[i] * 255;
    }
    var rgba = 'rgba(' + aColor.join(",") + ')'
    console.log(rgba)
    return rgba;
};
/*RGBA转换IOS color */
String.prototype.RGBtocolorIOS = function () {
    var that = this;
    var aColor = that.replace(/^rgba\(|RGBA\(/, "").replace(/\)/, '').split(",");
    console.log(aColor)
    for (var i = 0; i < aColor.length - 1; i++) {
        aColor[i] = aColor[i] / 255;
    }
    var IOScolor = '[' + aColor.join(" ") + ']'
    return IOScolor;
    // return rgba;  
};
var str = 'rgba(0,0,0,1)'
console.log(str.RGBtocolorIOS())
/*16进制颜色转为RGB格式*/
String.prototype.colorRgb = function () {
    var reg = /^#([0-9a-fA-f]{3}|[0-9a-fA-f]{6})$/;
    var sColor = this.toLowerCase();
    if (sColor && reg.test(sColor)) {
        if (sColor.length === 4) {
            var sColorNew = "#";
            for (var i = 1; i < 4; i += 1) {
                sColorNew += sColor.slice(i, i + 1).concat(sColor.slice(i, i + 1));
            }
            sColor = sColorNew;
        }
        //处理六位的颜色值  
        var sColorChange = [];
        for (var i = 1; i < 7; i += 2) {
            sColorChange.push(parseInt("0x" + sColor.slice(i, i + 2)) / 255);
        }
        return "[" + sColorChange.join(" ") + " 1]";
    } else {
        return sColor;
    }
}


/********************************** 编辑我的搭配 **************************************/
var EDIT = {
    getData: function () {
        var _this = this;
        $.ajax({
            type: "post",
            url: host + '/api/products/membercollocation/getedit',
            data: {
                MemberId: mebId,
                U_Num: Unum,
                ColloId: parseInt(dpId)
            },
            success: function (data) {
                //debugger
                console.log('我的搭配',JSON.stringify(data))
                var json = data.Data;
                if (data.ResultType == 3) {
                    _this.setBackground(json.BackGroundPath, json.BackGroundId);
                    _this.setImage(json.ImageList, json.TextList);
                }
                else {
                    $(".monogamy,.shangpin,.dialogShowMsg,.saveAll").hide();
                    $(".outBox").append('<div class="nosea">' + data.Message + '</div>');

                }
                $('.loadGig').hide();
            }
        })
    },
    setBackground: function (bgImgUrl, id) {
        fabric.Image.fromURL(bgImgUrl, function (img) {
            img.ProductId = id;
            img.ProductSource = 4; //素材类型
            img.Level = 0; //背景层级为0
            img.OperationType = 1;
            img.set({
                width:canvas.width,
                height:canvas.height,
            })
            img.set('width', canvas.width);
            img.set('height', canvas.height);
            backgroundObject = img;
            canvas.setBackgroundImage(img, canvas.renderAll.bind(canvas));
            ImageObjectArr[0] = img;
        });

    },
    setImage: function (imageList, textList) {
        var _this = this;
        var sortImageList = [];
        //console.log(imageList)
        //数组按层级排序
        for (var Level = 1; Level <= imageList.length; Level++) {
            var LevelIndex = Level;
            $.each(imageList, function (i, item) {
                if (item.Level == LevelIndex) {
                    sortImageList.push(item);
                    return false;
                };
            })
        }
        //console.log(sortImageList)
        $.each(sortImageList, function (i, item) {
            var index = i;
            var deg = getmatrix(item.Spin);
            fabric.Image.fromURL(item.ImagePath, function (img) {
                img.status = 'edit';
                img.OperationType = 1;
                img.ProductId = item.ProductId;
                //img.Level = item.Level;
                //img.ProductId = id;
                img.ProductSource = item.ProductSource; //素材类型
                var position = _this.getFrame(item.Frame);
                console.log('postiongxy', position.x, position.y, deg);
                img.set({
                    left: position.x,
                    top: position.y,
                    width: position.width,
                    height: position.height,
                    angle: deg,
                    cornerSize: 20,
                    cornerColor: "#AAA",
                    borderColor: "#AAA",
                    //cornerStyle :'circle'
                });
                img.setControlVisible("ml", false); //去除不需要的控制点
                img.setControlVisible("tl", false);
                img.setControlVisible("tr", false);
                img.setControlVisible("bl", false);
                img.setControlVisible("mt", false);
                img.setControlVisible("mr", false);
                img.setControlVisible("mb", false);
                canvas.add(img).setActiveObject(img);
                ImageObjectArr.push(img);
                if (index == sortImageList.length - 1) {
                    _this.setText(textList);
                }
            })
        })
    },
    setText: function (textList) {
        var _this = this;
        $.each(textList, function (i, item) {
            var deg = getmatrix(item.Spin);
            var position = _this.getFrame(item.Frame);
            console.log(item)
            var canText = new fabric.Text(item.Text, {
                left: position.x,
                top: position.y,
                angle: deg,
                fill: item.Color.colorRGBA(), //颜色转换RGBA
                fontSize: item.FontSize,
                fontFamily: "Microsoft YaHei"
            });
            canText.status = 'edit';
            canText.Id = item.Id;
            canText.OperationType = 1;
            canvas.add(canText);
            canvas.setActiveObject(canText);
            //canText.setCoords();
            //canText.bringToFront();
            editTextList.push(canText);
            //console.log(editTextList)
        })
    },
    getFrame: function (frame) {
        var frame = frame.split(', ');
        var x = parseFloat(frame[0].replace(/\{*/, ''));
        var y = parseFloat(frame[1].replace(/\{*/, ''));
        var width = parseFloat(frame[2].replace(/\{*/, ''));
        var height = parseFloat(frame[3].replace(/\{*/, ''));
        console.log('getFram',x, y, width, height);
        var position = {
            x: x,
            y: y,
            width: width,
            height: height
        };
        return position;
    },

    initData: function () {
        //canvas.remove(addBtn);
        //debugger
        this.getData();
    }

}

/********************************** 层级关系 **************************************/
canvas.on('object:selected', function(e) {
	var el = e.target;
	selectObj = el;
});

/*************************************** 渲染商品素材 **********************************/
var pruductIndex = 1;
var maxPruductIndex = 999;
var backgroundIndex = 1;
var maxBackgroundIndex = 999;

setProduct(1);
function setProduct(i) {
    $.ajax({
        type: "post",
        url: host + '/api/materials/material/getlist',
        data: {
            MemberId: mebId,
            U_Num: Unum,
            MaterialType: 1,
            PageIndex: i,
            PageSize: 10
        },
        success: function (data) {
            //console.log(data)
            var html = '';
            if(data.Data.length == 0) {
            	maxPruductIndex = pruductIndex;
            	return;
            }
            $(data.Data).each(function(i, item) {
            	html += '<img src=' + item.IconPath + ' data-id=' + item.MaterialId + ' alt="">'
            })
            $('.img_box').append(html);
            $(".img_box img").click(function (e) {
                canvas.renderAll();
                e.preventDefault();
                var selectImg = e.target;
                var newImage = new fabric.Image(selectImg);
                newImage.ProductId = $(this).attr('data-id');
                newImage.ProductSource = 4; //素材类型
                newImage.OperationType = 0; //操作类型（0添加；1修改；2删除；）
                newImage.set({
                    left: canvas.getWidth() / 2,
                    top: canvas.getHeight() / 2,
                    //angle:0,
                    cornerColor: "#AAA",
                    borderColor: "#AAA",
                    //transparentCorners: false      //控制点大小
                    //centeredRotation:true
                });
                newImage.setControlVisible("ml", false); //去除不需要的控制点
                newImage.setControlVisible("tl", false);
                newImage.setControlVisible("tr", false);
                newImage.setControlVisible("bl", false);
                newImage.setControlVisible("mt", false);
                newImage.setControlVisible("mr", false);
                newImage.setControlVisible("mb", false);
                canvas.add(newImage).setActiveObject(newImage);
                ImageObjectArr.push(newImage);
                canvas.renderAll();
                $('.img_box').hide();
            });
        }

    })
}

/*************************************** 添加背景 **************************************/
setBackground(1);
function setBackground(i) {
    $.ajax({
        url: host + '/api/materials/material/getlist',
        data: {
            MemberId: mebId,
            U_Num: Unum,
            MaterialType: 0,
            PageIndex: i,
            PageSize: 10
        },
        success: function (data) {
            //console.log(data)
            if (data.Data.length == 0) {
                maxBackgroundIndex = backgroundIndex;
                return;
            }
            var html = '';
            $(data.Data).each(function (i, item) {
                html += '<img src=' + item.IconPath + ' data-id=' + item.MaterialId + ' alt="">'
            })
            $('.backImg').append(html)
        }
    });
}


var backgroundObject = new Object();
$('.backImg').on('click', 'img', function(e) {
	e.preventDefault();
	var bgel = e.target;
	var newBg = new fabric.Image(bgel);
	newBg.ProductId = $(this).attr('data-id');
	newBg.ProductSource = 4; //素材类型
	newBg.Level = 0; //背景层级为0
	backgroundObject = newBg;
	newBg.set('width', canvas.width);
	newBg.set('height', canvas.height);
	canvas.setBackgroundImage(newBg);
	canvas.renderAll();
	canvas.selection = false;
	//	$('.wu').click(function() {
	//		canvas.setBackgroundImage(null);
	//		canvas.renderAll();
	//		$('.back-img').hide();
	//	})
	//	$('.back-img').hide();
});

/********************************* 添加 **********************************/
$(".addCloth").click(function () {
    $(".monogamy").hide();
    $(".shangpin").show();
});

/********************************* 上移下移 **********************************/
$('.shangyi').click(function () {
	for(var i = 0; i < ImageObjectArr.length; i++) {
		if(selectObj == ImageObjectArr[ImageObjectArr.length - 1]) {
			//					lessalog('已到最高层');
			return;
		}
		if(selectObj == ImageObjectArr[i]) {
			ImageObjectArr[i].moveTo(i + 1);
			var item = ImageObjectArr[i];
			ImageObjectArr[i] = ImageObjectArr[i + 1];
			ImageObjectArr[i + 1] = item;
			break;
		}
	}
})

$('.xiayi').click(function () {
    for (var i = 1; i < ImageObjectArr.length; i++) {
        if (selectObj == ImageObjectArr[1]) {
            //					lessalog(' 已经是最底层了');
            return;
        }
        if (selectObj == ImageObjectArr[i]) {
            ImageObjectArr[i].moveTo(i - 1);
            var item = ImageObjectArr[i - 1];
            ImageObjectArr[i - 1] = ImageObjectArr[i];
            ImageObjectArr[i] = item;
            break;
        }
    }
})

/************************************ 删除对像 ************************** **********/
var editDelImageLIst = [];
$('.remove').click(function() {
	canvas.remove(selectObj);
	for(var i = 0; i < ImageObjectArr.length; i++) {
		if(selectObj == ImageObjectArr[i]) {
			if(selectObj.status == 'edit') { //编辑时删除已保存零件
				editDelImageLIst.push(ImageObjectArr[i]);
			}
			ImageObjectArr.splice(i, 1);
			break;
		}
	}   
})

//相册上传预览    IE是用了滤镜。
function previewImage(file) {
    $('.img_box,.backImg').hide().animate({
        left: 0,
    });

	var MAXWIDTH = 0;
	var MAXHEIGHT = 0;
	var div = document.getElementById('preview');
	if(file.files && file.files[0]) {
		div.innerHTML = '<img id=imghead>';
		var img = document.getElementById('imghead');
		img.onload = function() {
			var rect = clacImgZoomParam(MAXWIDTH, MAXHEIGHT, img.offsetWidth, img.offsetHeight);
			img.width = rect.width;
			img.height = rect.height;
		}
        var reader = new FileReader();
		reader.onload = function(evt) {
			img.src = evt.target.result;
			//把图片渲染到canvas
			canvasImg(evt);
		}
		reader.readAsDataURL(file.files[0]);
	} else //兼容IE
	{
		var sFilter = 'filter:progid:DXImageTransform.Microsoft.AlphaImageLoader(sizingMethod=scale,src="';
		file.select();
		var src = document.selection.createRange().text;
		div.innerHTML = '<img id=imghead>';
		var img = document.getElementById('imghead');
		img.filters.item('DXImageTransform.Microsoft.AlphaImageLoader').src = src;
		var rect = clacImgZoomParam(MAXWIDTH, MAXHEIGHT, img.offsetWidth, img.offsetHeight);
		status = ('rect:' + rect.top + ',' + rect.left + ',' + rect.width + ',' + rect.height);
		div.innerHTML = "<div id=divhead style='width:" + rect.width + "px;height:" + rect.height + "px;margin-top:" + rect.top + "px;" + sFilter + src + "\"'></div>";
		canvasImg(evt);
	}
}

//图像翻转
$(".jingxiang").click(function () {
    selectObj.setScaleX(-selectObj.getScaleX()).setCoords();
    canvas.renderAll();
})
//重置
$(".chongzhi").click(function () {
    canvas.clear();
});

//保存
var imageUrl;
$(".addSave").click(function () {
    //canvas.discardActiveObject();  
    //imageUrl = canvas.toDataURL({
    //    format: 'png'
    //});
    console.log(imageUrl);
    localStorage.addDpImageBase64 = imageUrl;
    localStorage.showDpId = dpId;

    if (getImgPositionList(dpId)) {
        //openNewWin('adddapei.html', {
        //    "imgPositionList": imgPositionList,
        //    "textareaList": textareaList
        //});
        //console.log("保存成功");
        $(".monogamy").hide();
        $(".saveAll").show();
    }
})

function canvasImg(e) {
	canvas.renderAll();
	e.preventDefault();
	var selectImg = document.getElementById('imghead');
	var newImage = new fabric.Image(selectImg);
	newImage.ProductId = $(this).attr('data-id');
	newImage.ProductSource = 4; //素材类型
	newImage.OperationType = 0; //操作类型（0添加；1修改；2删除；）
	newImage.set({
		left: 170,
		top: 170,
		//angle:0,
		cornerColor: "#AAA",
		borderColor: "#AAA",
		//transparentCorners: false      //控制点大小
		//centeredRotation:true
	});
	newImage.setControlVisible("ml", false); //去除不需要的控制点
	newImage.setControlVisible("tl", false);
	newImage.setControlVisible("tr", false);
	newImage.setControlVisible("bl", false);
	newImage.setControlVisible("mt", false);
	newImage.setControlVisible("mr", false);
	newImage.setControlVisible("mb", false);
	canvas.add(newImage).setActiveObject(newImage);
	ImageObjectArr.push(newImage);
	canvas.renderAll();
}

function clacImgZoomParam(maxWidth, maxHeight, width, height) {
	var param = {
		top: 0,
		left: 0,
		width: width,
		height: height
	};
	if(width > maxWidth || height > maxHeight) {
		rateWidth = width / maxWidth;
		rateHeight = height / maxHeight;

		if(rateWidth > rateHeight) {
			param.width = maxWidth;
			param.height = Math.round(height / rateWidth);
		} else {
			param.width = Math.round(width / rateHeight);
			param.height = maxHeight;
		}
	}
	param.left = Math.round((maxWidth - param.width) / 2);
	param.top = Math.round((maxHeight - param.height) / 2);
	return param;
}

var swiperFooter = new Swiper('#swiperFooter', {
    width: 85.7,
    onSlideChangeEnd: function (swiper) {
    }
})

//商品列表
var da,dialogsd;
var generated = 0,
    pageIndex,
    pagdex = 1,
    PageSize = 10,
    generatedCount = 0;

var scrollTop;     //获取滚动条到顶部的距离
var contentH;  //获取文档区域高度 
var viewH;  //获取滚动条的高度
var flag = true;   //加载数据标志
//添加商品
function getIcon(meId,unum) {
    $.ajax({
        type: 'post',
        url: host + '/api/properties/category/getlist',
        data: {
            MemberId: meId,
            U_Num: unum,
        },
        success: function (data) {
            $(".footerWraper").empty();
            if (data.ResultType == 3) {
                    $(data.Data).each(function (i, item) {
                        if (item.ParentId == 0) {
                            var msg = '<div class="swiper-slide" data-categoryId=' + item.CategoryId + '><img class="top_cloth" src=' + item.IconPath + ' /><span>' + item.CategoryName + '</span></div>';
                            $(".footerWraper").append(msg);
                        }
                })
            }
            var mySwiper = new Swiper('.footerList', {
                width: 120,
                initialSlide: 2,
                onSlideChangeEnd: function (swiper) {
                    pageIndex = 1;
                    $("#wrapper").hide(function () {
                        $(".dialog").remove();
                        $('.loadGig').show();
                        getCommodity(0, 0);
                    });
                    da = $(".swiper-slide-active").attr('data-categoryid');                  
                }
            })
           
        }

    })
}

function getCommodity(empt,gencout) {
    $.ajax({
        type: "post",
        url: host + "/api/Products/Product/GetListByStore",
        data: {
            MemberId: mebId,
            StoreId: 14,
            CategoryId: da,
            PageIndex: pageIndex,
            PageSize: PageSize,
        },
        beforeSend: function () {
            flag = false;
        },
        success: function (data) {
            if (empt == 0) {
                $(".clo1,.clo2,.clo3").empty();
            }
            var divFooter = '</div></div>',
                sedDivHed = '<div class="message_box"><div class="message">',
                sedDivFooter = '</div></div>';
           
            if (data.ResultType == '3') {
                var contacts = data.Data;
                if (gencout == 0) {
                    generatedCount = 0;
                } else {
                    generatedCount = generatedCount;
                }
                if (generatedCount == 0 && contacts.length == 0) {
                    dialogsd = '<div class="dialog">还没有此类商品</div>';
                    $('.loadGig').hide();
                    $(".wrappers").append(dialogsd);
                }
                //console.log(generatedCount, contacts.length);
                $.each(data.Data, function (i, item) {
                    var divHed = '<div class="white_bg" data-product-id=' + item.ProductId + ' data-ProductId=' + item.ProductId + ' data-ProductNumber=' + item.ProductNumber + '><div class="commodity">';
                    var img = '<img  class="picture" src=' + item.ImagePath + ' data-noBg=' + item.ColloImgPath+ '>';
                    var cost = '<p>￥' + item.Price + '</p>';
                    var CategoryName = '<ul class="mold"><li>' + item.CategoryName + '</li><li>' + item.SeasonName + '</li></ul>	'
                    var html = divHed + img + sedDivHed + cost + CategoryName + sedDivFooter + divFooter;

                    var da = generatedCount % 3;
                    if (da == 0) {
                        $(html).appendTo($(".clo1"));

                    } else if (da == 1) {
                        $(html).appendTo($(".clo2"));

                    } else if (da == 2) {
                        $(html).appendTo($(".clo3"));

                    }
                    generatedCount++;
                });
                $("#wrapper").show();
                $('.picture').load(function () {
                    $('.loadGig').hide();

                })
            
            }
            $(".white_bg").unbind('click').bind('click', function (e) {
                $(".shangpin").hide();
                $(".monogamy").show();
                var noBg = $(this).find("img").attr("data-nobg");

                var imgSplit = noBg.split('.')[2].split('_');
                var imgWhOme = imgSplit[3];
                var imgHgTwo = imgSplit[4];
                console.log('1111', imgWhOme, imgHgTwo);
                var imgWh, imgHg, bili;
                if (Number(imgWhOme)< Number(imgHgTwo)) {
                    imgWh = imgWhOme * 300 / imgHgTwo;
                    imgHg = 300;
                    console.log(11)
                   
                } else {
                    imgWh = 200;
                    imgHg = imgHgTwo * 200 / imgWhOme;
                }
                //debugger
                canvas.renderAll();
                e.preventDefault();
                console.log(e);
                fabric.Image.fromURL(noBg, function (newImage) {
                    newImage.ProductId = $(this).attr('data-productnumber');
                    newImage.ProductSource = 4; //素材类型
                    newImage.OperationType = 0; //操作类型（0添加；1修改；2删除；）
                    newImage.set({
                        width: imgWh,
                        height: imgHg,
                        left: 100,
                        top: 50,
                        //angle:0,
                        cornerColor: "#AAA",
                        borderColor: "#AAA",
                        //transparentCorners: false      //控制点大小
                        //centeredRotation:true
                    });
                    newImage.setControlVisible("ml", false); //去除不需要的控制点
                    newImage.setControlVisible("tl", false);
                    newImage.setControlVisible("tr", false);
                    newImage.setControlVisible("bl", false);
                    newImage.setControlVisible("mt", false);
                    newImage.setControlVisible("mr", false);
                    newImage.setControlVisible("mb", false);
                    canvas.add(newImage).setActiveObject(newImage);
                    ImageObjectArr.push(newImage);
                    canvas.renderAll();
                })
            });
            
        },
        complete:function(){
            flag = true;
        }
    });
    $(".wrappers").scroll(function () {
        var $this = $(this);
        viewH = $(this).height(); //可见高度
        contentH = $(this).get(0).scrollHeight; //内容高度
        scrollTop = $(this).scrollTop(); //滚动高度
        //console.log(viewH, contentH, scrollTop);
        if (contentH - viewH - scrollTop <= 100 && flag == true) { //到达底部100px时,加载新内容  
            //if (scrollTop / (contentH - viewH) >= 2) { //到达底部100px时,加载新内容 
            getCommodity(pageIndex += 1, 1, 1);
        }
    });
}
//颜色
var colrSel;
function colorGet() {
    $.ajax({
        type: "post",
        url: host + "/api/properties/color/getlist",
        data: {
            MemberId: 7,
            U_Num: '4f5e4f38bef794ec',
        },
        success: function (data) {
            $('.colors').empty();
            if (data.ResultType == 3) {
                $.each(data.Data, function (i, item) {
                    colrSel = '<li class="swiper-slide seleColor" data-ColorId=' + item.ColorId + '><span> <img src='+ item.IconPath+'></span><p>' + item.ColorName + '</p></li>'
                    $('.colors').append(colrSel);
                })
            } else {
                console.log('颜色', data.Message);
            }
            var swiperColor = new Swiper('.colorsBox', {
                width: 90,
                onSlideChangeEnd: function (swiper) {
                }
            })
            var flag = true;
            $(".seleColor").click(function () {
                if ($(this).hasClass("duigou")) {
                    $(this).removeClass("duigou");
                    $(this).children("span").removeClass("face");
                } else {
                    $(this).addClass("duigou");
                    $(this).children("span").addClass("face");
                } 
            })
        }
    })
}

//季节
var liSel;
$(".selectSeason").click(function () {
    $(".saveAll").addClass("bgfilter");
    $(".selectSyle,.closeFile").show();
    $.ajax({
        type: "post",
        url: host + "/api/properties/season/getlist",
        data: {
            MemberId: mebId,
            U_Num: Unum,
        },
        success: function (data) {
            $('.swiperData').empty();
            if (data.ResultType == 3) {
                $.each(data.Data, function (i, item) {
                    liSel = '<li class="selectData" data-Id=' + item.SeasonId + '>' + item.SeasonName + '</li>'
                    $('.swiperData').append(liSel);

                })
            } else {
                console.log('季节', data.Message);
            }
            $(".selectData").click(function () {
                var dataId = $(this).attr("data-id");
                var dataHtml = $(this).html();
                $(".selectSeason input").val(dataHtml);
                clickSele();
            })
        }
    })
})
//风格
var liStyle;
$(".selectStyle").click(function () {
    $(".saveAll").addClass("bgfilter");
    $(".selectSyle,.closeFile").show();
    $.ajax({
        type: "post",
        url: host + "/api/products/productattribute/getlist",
        data: {
            MemberId: mebId,
            U_Num: Unum,
            ProductAttrType:0
        },
        success: function (data) {
            $('.swiperData').empty();
            if (data.ResultType == 3) {
                $.each(data.Data, function (i, item) {
                    liStyle = '<li class="selectData" data-Id=' + item.ProductAttrId + '>' + item.AttributeName + '</li>'
                    $('.swiperData').append(liStyle);

                })
            } else {
                console.log('风格',data.Message);
            }
            $(".selectData").click(function () {
                var dataId = $(this).attr("data-id");
                var dataHtml = $(this).html();
                $(".selectStyle input").val(dataHtml);
                clickSele()
            })
        }
    })
})

//场合
var situation;
$(".liStyle").click(function () {
    $(".saveAll").addClass("bgfilter");
    $(".selectSyle,.closeFile").show();
    $.ajax({
        type: "post",
        url: host + "/api/properties/situation/getlist",
        data: {
            MemberId: mebId,
            U_Num: Unum,
        },
        success: function (data) {
            $('.swiperData').empty();
            if (data.ResultType == 3) {
                $.each(data.Data, function (i, item) {
                    situation = '<li class="selectData" data-Id=' + item.SituationId + '>' + item.SituationName + '</li>'
                    $('.swiperData').append(situation);
                })
            } else {
                console.log('场合', data.Message);
            }
            $(".selectData").click(function () {
                var dataId = $(this).attr("data-id");
                var dataHtml = $(this).html();
                $(".liStyle input").val(dataHtml);
                clickSele();
            })
        }
    })
})
$(".closeFile").click(function () {
    $(this).hide();
    clickSele();
})
function clickSele() {
    $(".saveAll").removeClass("bgfilter");
    $(".selectSyle,.closeFile").hide();
}

$(".dapeiImg img").attr('src', imageUrl);

function getBase64() {
    var imgBase64 = $('.dapeiImg').attr('src');
    if (!imgBase64) { return };
    console.log(imgBase64.length)
    console.log(imgBase64)
    imgBase64 = imgBase64.replace("data:image/png;base64,", "");
    return imgBase64;
}

$("tijiao").click(function () {
    function tijiao() {
        var Image = getBase64();
        var namTxt = $(".clothName").val();
        var colOR = $(".duigou").attr("data-colorid");
        var jijie = $(".selectSeason input").val();
        var fengge = $(".selectStyle input").val();
        var changhe = $(".liStyle input").val();
        var beizhu = $(".txtArea").val();
        if (!Image) {
            showDialog('请添加搭配图');
        } else if (!namTxt) {
            showDialog('请添加名称');
        } else if (!colOR) {
            showDialog('请选择颜色');
            return false;
        } else if (!jijie) {
            showDialog('请选择季节');
            return false;
        } else if (!fengge) {
            showDialog('请选择风格');
            return false;
        } else if (!changhe) {
            showDialog('请选择场合');
            return false;
        } else {
            return true;
        }
    }
})
function showDialog(msg) {
    $(".showSendMsg").show();
    $(".showSendMsg span").html(msg);
}
$(".sureSend").click(function () {
    $(".showSendMsg").hide();
})