var host = 'http://11.1.1.113:8888';
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

var memberId, Unum, dpId, listMemberId;
/********************************** 获取这条数据的memberId **************************************/

listMemberId = $(".memberId").val();
dpId = $(".palette").attr("data-viewId");

/*************************************** 获取登录信息 **********************************/

$.ajax({
    type: "post",
    url: "/products/memberCollocation/getMemberProfile",
    success: function (res) {
        if (res.ResultType == 3) {
            memberId = res.Data.MemberId;
            Unum = res.Data.U_Num;
            //debugger
            console.log('成功', memberId, Unum);
            if (memberId == listMemberId) {
                $(".monogamy").hide();
                $(".dialogShowMsg").hide();
                //请求成功后调用的回调函数 
                EDIT.initData();
            } else {
                showDialog("无权操作");
            }
        } else {
            showDialog("请求有误");
            console.log('失败' + res);
        }
        
    } 
});

function showDialog(msg) {
    $(".monogamy").hide();
    $(".dialogShowMsg").hide().html(msg);
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
    textareaList = [];
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
                MemberId: memberId,
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
            }
        })
    },
    setBackground: function (bgImgUrl, id) {

        fabric.Image.fromURL(bgImgUrl, function (img) {
            img.ProductId = id;
            img.ProductSource = 4; //素材类型
            img.Level = 0; //背景层级为0
            img.OperationType = 1
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
            MemberId: memberId,
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
            MemberId: memberId, 
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
//保存
$(".addSave").click(function () {
    canvas.discardActiveObject();
    var imageUrl = canvas.toDataURL({
        format: 'png'
    });
    localStorage.addDpImageBase64 = imageUrl;
    localStorage.showDpId = dpId;

    if (getImgPositionList(dpId)) {
        //openNewWin('adddapei.html', {
        //    "imgPositionList": imgPositionList,
        //    "textareaList": textareaList
        //});
        console.log("保存成功");

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


//添加商品

//商品详情页
var MemberId = localStorage.MemberId;
var CategoryId = localStorage.ParentCategoryName;
var generatedCount = 0,
    generated = 0,
    PageIndex = 1,
    pagdex = 1,
    PageSize = 10;

    //商品列表
getCommodity();
function getCommodity() {
    $.ajax({
        type: "post",
        url: host + "/api/Products/Product/GetListByStore",
        async: true,
        data: {
            MemberId: MemberId,
            StoreId: 14,
            CategoryId: CategoryId,
            PageIndex: PageIndex,
            PageSize: PageSize,
        },
        success: function (data) {
            var divFooter = '</div></div>',
                sedDivHed = '<div class="message_box"><div class="message">',
                sedDivFooter = '</div></div>';
            if (data.ResultType == '3') {
                var contacts = data.Data;
                if (generatedCount == 0 && contacts.length == 0) {
                    var div = '<div class="dialog">还没有此类商品</div>';
                    $(".appendBox").append(div);
                }
                $.each(data.Data, function (i, item) {
                    var divHed = '<div class="white_bg" data-product-id=' + item.ProductId + ' data-ProductId=' + item.ProductId + ' data-ProductNumber=' + item.ProductNumber + '><div class="commodity">';
                    var img = '<img  class="picture" src=' + item.ImagePath + ' />';
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
                $(".white_bg").bind('click', function () {
                    var dataId = $(this).attr('data-product-id');
                    var ProductId = $(this).attr("data-ProductId");
                    var ProductNumber = $(this).attr("data-ProductNumber");
                    localStorage.dataId = dataId;
                    localStorage.ProductId = ProductId;
                    localStorage.ProductNumber = ProductNumber;
                    //location.href = "xiangqing.html";
                })
            }
        }
    });
    PageIndex++;
}
var myscroll;

function loaded() {
    myScroll = new iScroll('wrapper', {
        hScrollbar: false,
        vScrollbar: false,
        snap: true,
        onBeforeScrollStart: function (e) {
            var target = e.target;
            while (target.nodeType != 1) target = target.parentNode;
            if (target.tagName != 'SELECT' && target.tagName != 'INPUT' && target.tagName != 'TEXTAREA') {
                e.preventDefault();
            }
        },
        onBeforeScrollMove: function (e) {
            e.preventDefault();
        },
        onScrollEnd: function (e) {
            setTimeout(function () {
                //if (datt == 1) {
                //    getId();
                //} else {
                //    getCommodity();
                //}
                getCommodity();
                myScroll.refresh();
                myScroll.options.snap = true;
            }, 100);
        }
    }, 120);
}
window.addEventListener("load", loaded, true);
document.addEventListener('touchmove', function (e) {
    e.preventDefault();
}, true);
document.addEventListener('DOMContentLoaded', allowFormsInIscroll, false);

function allowFormsInIscroll() {
    [].slice.call(document.querySelectorAll('input, select, button')).forEach(function (el) {
        el.addEventListener(('ontouchstart' in window) ? 'touchstart' : 'mousedown', function (e) {
            e.stopPropagation();
        })
    })
}
