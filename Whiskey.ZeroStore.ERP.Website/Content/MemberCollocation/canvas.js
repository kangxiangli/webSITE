var host = 'http://api.0-fashion.com';
var userId = 3;
var userNum = "6654dd7186ab58af";
var StoreId = 14;
var dpId; //商城单品ID
var canvas;
var ImageObjectArr = [];
//获取图片位置信息
var imgPositionList = [];
var textareaList = [];


$(function() {
	getProductList(2)
	setProduct(pruductIndex); //获取素材
	setBackground(backgroundIndex); //获取背景

	$('.addCollcontent .dropdown-menu.inner').on('click', 'li', function() {
		var id = $(this).attr('data-id');
		var name = $(this).find('span').text()
		$(this).parents('.dropdown-menu.open').prev().attr('data-id', id).find('span.filter-option').html(name)

	})
	getClass('/api/properties/color/getlist', $('#colorSelected').next().find('.dropdown-menu.inner'), 0)
	getClass('/api/properties/season/getlist', $('#sesonSelected').next().find('.dropdown-menu.inner'), 1)
	
	getClass('/api/products/productattribute/getlist', $('#fgSelected').next().find('.dropdown-menu.inner'), 2)
	getClass('/api/properties/situation/getlist', $('#chSelected').next().find('.dropdown-menu.inner'), 3)

	/*========================================== canvas ============================================*/


	var selectObj = null; //被选中的canvas对象
	canvas = new fabric.Canvas('c');
	var context = canvas.getContext("2d");

	canvas.selection = false; //防止选择多个图片
	//宽度 ：屏幕的宽度 *百分比
	canvas.renderAll();
	//*DPR的目的就是放大canvas达到canvas渲染按1:1的比例
	var canvasWidth = 320;
	var canvasHeight = canvasWidth;

	canvas.setWidth(canvasWidth);
	canvas.setHeight(canvasHeight);

	//背景
	canvas.setBackgroundColor('rgba(255, 255, 255,.1)');

	/********************************** 层级关系 **************************************/
	canvas.on('object:selected', function(e) {
		var el = e.target;
		selectObj = el;
	});
	/********************************* click添加图片  ********************************/

	$('#wddp').click(function() {
		LSShang.setlocStorage('toMatch', 'true');
		openNewWin('danpin.html');
		$('#pop-up_box').hide();
	});
	$('#close').click(function() {
		$('#pop-up_box').hide();
		$('.pageone').removeClass('change_bg');
	});

	/********************************* 上移下移 **********************************/
	$('#shangyi').click(function() {
		//console.log(ImageObjectArr)
		if(selectObj == ImageObjectArr[ImageObjectArr.length - 1]) {
			LSShang.toast('已到最高层');
			return;
		}
		for(var i = 0; i < ImageObjectArr.length; i++) {
			if(selectObj == ImageObjectArr[i]) {
				ImageObjectArr[i].moveTo(i);
				var item = ImageObjectArr[i + 1];
				ImageObjectArr[i + 1] = ImageObjectArr[i];
				ImageObjectArr[i] = item;
				break;
			}
		}
	})
	$('#xiayi').click(function() {
		//console.log(ImageObjectArr)
		for(var i = 0; i < ImageObjectArr.length; i++) {
			if(selectObj == ImageObjectArr[0]) {
				LSShang.toast(' 已经是最底层了');
				return;
			}
			if(selectObj == ImageObjectArr[i]) {
				ImageObjectArr[i].moveTo(i - 1);
				var item = ImageObjectArr[i];
				ImageObjectArr[i] = ImageObjectArr[i];
				ImageObjectArr[i - 1] = item;
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
		for(var j = 0; j < editTextList.length; j++) {
			if(selectObj == editTextList[j]) {
				if(selectObj.status == 'edit') { //编辑时删除已保存零件
					editTextList[j].OperationType = 2;
				}
				console.log(editTextList)
				break;
			}
		}
	})
	/********************************* 图像翻转 *************************************/
	$('.fanzhuan').click(function() {
		selectObj.setScaleX(-selectObj.getScaleX()).setCoords();
		canvas.renderAll();
	})
	/*限制图片不能超过边界*/
	// canvas.on('object:moving',function(e){
	//   var el=e.target;
	//   var elWidth=el.getBoundingRectWidth();
	//   var elHeight=el.getBoundingRectHeight();

	//   el.left=el.left<0?0:el.left;
	//   el.top=el.top<0?0:el.top;
	//   el.left=el.left>canvas.width-elWidth?canvas.width-elWidth:el.left;
	//   el.top=el.top>canvas.height-elHeight?canvas.height-elHeight:el.top;
	// });

	//----------------------------------------------------------------- 添加文字
	var editTextList = [];
	$('.txt').click(function() {
		//$('.swiper-container').slideUp()
		$('.sucaibox,.back-img').hide();
		if($('.addtxt').is(':hidden')) {
			$('.addtxt').show();
		} else {
			$('.addtxt').hide();
		}
		return false;
	});
	$('.addText').click(function() {
		var txt = $('.text').val();
		var canText = new fabric.Text(txt, {
			left: 100,
			top: 100,
			angle: 0,
			fill: 'rgba(0,0,0,1)',
			fontFamily: "Microsoft YaHei"
		});
		canvas.add(canText);
		canvas.setActiveObject(canText);
		canText.OperationType = 0;
		//canText.setCoords();
		editTextList.push(canText);
		$('.addtxt').hide();
	});
	$('.text').focus(function() {
		$('.addtxt').addClass('activeaddtxt')
		$('.swiper-container').slideUp()
	})
	$('.text').focusout(function() {
		$('.addtxt').removeClass('activeaddtxt')
		$('.swiper-container').slideDown()
	})
	//----------------------------------------------------------------------------- 添加背景 
	var backgroundObject = new Object();
	$('.addBackground').click(function() {
		$('.sucaibox').hide();
		$('.back-img').is(':hidden') ? $('.back-img').show() : $('.back-img').hide()
		return false;
	})
	$('.img-box').on('click', 'img', function(e) {
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
	});

	//------------------------------------------------------------------------------- 素材
	$(document).click(function(e) {
		$('.imgBox,.addtxt').hide()
	})
	$('.sucai').click(function() {
		$('.back-img').hide()
		$('.sucaibox').is(':hidden') ? $('.sucaibox').show() : $('.sucaibox').hide()
		return false;
	});

	$('.sucai_img').on('click', 'img', function(e) {
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
			cornerSize: 10,
			transparentCorners: false
		});
		canvas.add(newImage).setActiveObject(newImage);
		ImageObjectArr.push(newImage);
		canvas.renderAll();
		$('.sucaibox').hide();
	});

	/*************************************** 锁定 ***************************************/
	//	$('.lock,.lock img,.lock p').click(function() {
	//		selectObj.set({
	//			lockMovementX: true,
	//			lockMovementY: true,
	//			hasControls: false,
	//			hasBorders: true,
	//			centeredRotation: true
	//
	//		});
	//	})

	
	/*************************************** 渲染商品素材 **********************************/
	var pruductIndex = 1;
	var maxPruductIndex = 999;
	var backgroundIndex = 1;
	var maxBackgroundIndex = 999;

	
	$('.sucaibox').scroll(function() {
		if(pruductIndex == maxPruductIndex) {
			return;
		}
		pruductIndex++;
		if(judgeNextPage($('.sucai_img img:last')) < 100) {
			setProduct(pruductIndex);
		}
	})
	$('.back-img').scroll(function() {
		if(maxBackgroundIndex == backgroundIndex) {
			return;

		}
		backgroundIndex++;
		if(judgeNextPage($('.sucai_img img:last')) < 100) {
			setBackground(backgroundIndex);
		}
	})
	//----------------------------------------------------------------------------添加本地图片
	$('.addfromFile').click(function() {
		$('#collFile').click()
	})
	$('#collFile').change(function(ev) {
		var file = ev.target.files[0];
		var fileReader = new FileReader();
		fileReader.onload = function(e) {
			var imageSrc = e.target.result;
			setSelectImg(imageSrc, 3)
		}
		fileReader.readAsDataURL(file);
	})
	

	//	//解决部分机型触摸显示问题
	$('#c').click(function() {
		canvas.renderAll();
	})
	setTimeout(function() {
		$('canvas').click();
	}, 1000)

	//----------------------------------------------------------------------------截图保存
	$("#save").on({
		click: function() {
			console.log(editTextList)
			canvas.discardActiveObject();
			var imageUrl = canvas.toDataURL({
				format: 'png'
			});

			if(getImgPositionList()) {

			};
		}
	});

})






//---------------------------------------------------------------获取用户信息
function getMemberProfile() {
	var getMemberProfileRequest = '/products/memberCollocation/getMemberProfile';

	$.ajax({
		type: "post",
		url: getMemberProfileRequest,
		data: {},
		success: function(data) {

			console.log(data)
		}
	});
}

//---------------------------------------------------------获取店铺ID
function GetMemberStore() {
	$.ajax({
		url: host + '/API/Members/MemberInfo/GetMemberStore',
		type: 'post',
		data: {
			MemberId: userId,
			U_Num: userNum,

		},
		success: function(data) {
			console.log(data)
			if(data.ResultType == 3) {
				$(data.Data).each(function(i, item) {

				})
			}
		}
	})
}
//----------------------------------------------------------------------------获取颜色、风格、场合、季节	
function getClass(url, container, className) {
	$.ajax({
		url: host + url,
		type: 'post',
		data: {
			MemberId: userId,
			U_Num: userNum,
			ProductAttrType: 1
		},
		success: function(data) {
			if(data.ResultType == 3) {
				switch(className) {
					case 0:
						$.each(data.Data, function(i, item) {
							container.append('<li data-id=' + item.ColorId + '><a href="javascript:;" data-tokens="null" role="option" aria-disabled="false" aria-selected="false"><img style="width:20px;display:inline-block;margin-right:15px" src="' + item.IconPath + '"/><span>' + item.ColorName + '</span></a></li>')

						})
						break;
					case 1:
						$.each(data.Data, function(i, item) {
							container.append('<li data-id=' + item.SeasonId + '><a href="javascript:;" data-tokens="null" role="option" aria-disabled="false" aria-selected="false"><span>' + item.SeasonName + '</span></a></li>')

						})
						break;
					case 2:
						$.each(data.Data, function(i, item) {
							container.append('<li data-ProductAttrId=' + item.ProductAttrId + '><a href="javascript:;"><img style="width:20px;display:inline-block;margin-right:15px" src="' + item.IconPath + '"/><span>' + item.ProductAttrName + '</span></a></li>')

						})
						break;
					case 3:
						$.each(data.Data, function(i, item) {
							container.append('<li data-ProductAttrId=' + item.SituationId + '><a href="javascript:;"><span>' + item.SituationName + '</span></a></li>')

						})
						break;
				}

			}
		}
	})
}
//--------------------------------------------------- 获取背景素材
	function setBackground(i) {
		$.ajax({
			url: host + '/api/materials/material/getlist',
			type: 'post',
			data: {
				MemberId: userId,
				U_Num: userNum,
				MaterialType: 0,
				PageIndex: i,
				PageSize: 20
			},
			success: function(data) {
				//console.log(data)
				if(data.Data.length == 0) {
					maxBackgroundIndex = backgroundIndex;
					return;
				}
				var html = '';
				$(data.Data).each(function(i, item) {
					html += '<img src=' + item.IconPath + ' data-id=' + item.MaterialId + ' alt="">'
				})
				$('.img-box').append(html)
			}
		})
	}
//--------------------------------------------------------------------获取单品素材
	function setProduct(i) {

		$.ajax({
			url: host + '/api/materials/material/getlist',
			type: 'post',
			data: {
				MemberId: userId,
				U_Num: userNum,
				MaterialType: 1,
				PageIndex: i,
				PageSize: 20
			},
			success: function(data) {
				//console.log(data)
				var html = '';
				if(data.Data.length == 0) {
					maxPruductIndex = pruductIndex;
					return;
				}
				$(data.Data).each(function(i, item) {
					html += '<img src=' + item.IconPath + ' data-id=' + item.MaterialId + ' alt="">'
				})
				$('.sucai_img').append(html)
			}
		})
	}
	

	function judgeNextPage(obj) {
		var dis = obj.position().top;
		return dis;
	}
	
//---------------------------------------------------------------------获取商城单品

function getProductList(categoryid, pageIndex) {

	var requestProductUrl = '/API/Products/Product/GetListByStore';
	var StoreId = StoreId || 14;
	var pageIndex = pageIndex || 1;
	var json = {
		MemberId: userId,
		U_Num: userNum,
		StoreId: StoreId,
		CategoryId: categoryid,
		PageIndex: pageIndex,
		PageSize: 20,
	}

	$.ajax({
		url: host + requestProductUrl,
		type: 'post',
		data: json,
		success: function(data) {
			console.log(data)
			if(data.ResultType == 3) {

				$(data.Data).each(function(i, item) {
					//console.log(item)
					getProducts(item);
				})
				pageIndex++;
			}
		},
	})
}
//渲染单品
function getProducts(entities) {
	var tpl = $('#productTpl').html();
	var fn = _.template(tpl);
	var html = fn(entities);
	$('#product_list').append(html);
}

function selectShopList(_this) {

	dpId = $(_this).attr('data-product-id');
	var imageSrc = $(_this).find('.single img').attr('src');
	$.ajax({
		url: host + '/api/products/product/getproductdetail',
		type: 'post',
		data: {
			MemberId: userId,
			U_Num: userNum,
			ProductId: dpId
		},
		success: function(data) {
			if(data.ResultType == 3) {
				if(data.Data.ProductImages.length > 0) {
					imageSrc = data.Data.ProductImages[0].ThumbnailSmallPath;
					setSelectImg(imageSrc, 1, dpId)
				}
			}
		}
	})
}
//----------------------------------------------------------------------------渲染单品到画布 
	function setSelectImg(imgURL, parm, ProductId) {
		var selectedIMG = fabric.Image.fromURL(imgURL, function(img) {
			img.scale(0.5).set({
				left: 50,
				top: 50,
				cornerSize: 10,
				transparentCorners: false
				//cornerStyle :'circle'
			});
			canvas.add(img).setActiveObject(img);
			img.OperationType = 0; //操作类型（0添加；1修改；2删除；）
			if(ProductId) {
				img.ProductId = ProductId;
			}
			img.ProductSource = parm || 0; //素材类型
			ImageObjectArr.push(img)
		});

	}
//=======================================================================================保存 

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
	

	function getImgPositionList(edit) {

		var toAddStatus = true;
		if(ImageObjectArr.length <= 1) {
			LSShang.toast('请至少添加一张图片');
			toAddStatus = false;
			return false;
		}
		$(ImageObjectArr).each(function(i, item) {
			if(i == 0) {
				if(!backgroundObject.ProductId) {
					LSShang.toast('请添加背景图');
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
		$.each(editDelImageLIst, function(i, item) {
			item.OperationType = 2;
		})
		editAllImgList = ImageObjectArr.concat(editDelImageLIst);
		//循环保留所需参数
		imgPositionList = [];

		$(editAllImgList).each(function(i, item) {
			var data = {
				Level: item.Level,
				ProductId: parseInt(item.ProductId),
				ProductSource: item.ProductSource,
				Frame: item.Frame,
				Spin: item.Spin
			}
			if(edit) {
				data.OperationType = item.OperationType;
				data.Id = parseInt(item.ProductId);
			}
			imgPositionList.push(data)
		})
		//获取文字
		textareaList = [];
		$(editTextList).each(function(i, item) {
			var json = {};
			json.Text = item.text;
			getPosition(item);
			json.Frame = item.Frame;
			json.Spin = item.Spin;
			json.FontSize = item.fontSize;
			json.Color = item.fill.RGBtocolorIOS();
			//json.Color = item.fill.colorRgb();

			if(edit) {
				json.OperationType = item.OperationType;
			}
			if(item.Id) {
				json.Id = parseInt(item.Id);
			}
			textareaList.push(json);
		})

		LSShang.setlocStorage('dapeiToSaveTextList', textareaList);
		LSShang.setlocStorage('dapeiToSaveImgList', imgPositionList);
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
		if(aa == bb || -aa == bb) {
			deg = dd;
		} else if(-aa + bb == 180) {
			deg = 180 + cc;
		} else if(aa + bb == 180) {
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
	String.prototype.colorRGBA = function() {
		var that = this;
		var aColor = that.replace(/^\[/, "").replace(/\]$/, "").split(' ');
		for(var i = 0; i < aColor.length - 1; i++) {
			aColor[i] = aColor[i] * 255;
		}
		var rgba = 'rgba(' + aColor.join(",") + ')'
		console.log(rgba)
		return rgba;
	};
	/*RGBA转换IOS color */
	String.prototype.RGBtocolorIOS = function() {
		var that = this;
		var aColor = that.replace(/^rgba\(|RGBA\(/, "").replace(/\)/, '').split(",");
		console.log(aColor)
		for(var i = 0; i < aColor.length - 1; i++) {
			aColor[i] = aColor[i] / 255;
		}
		var IOScolor = '[' + aColor.join(" ") + ']'
		return IOScolor;
		// return rgba;  
	};
	var str = 'rgba(0,0,0,1)'
	console.log(str.RGBtocolorIOS())
	/*16进制颜色转为RGB格式*/
	String.prototype.colorRgb = function() {
		var reg = /^#([0-9a-fA-f]{3}|[0-9a-fA-f]{6})$/;
		var sColor = this.toLowerCase();
		if(sColor && reg.test(sColor)) {
			if(sColor.length === 4) {
				var sColorNew = "#";
				for(var i = 1; i < 4; i += 1) {
					sColorNew += sColor.slice(i, i + 1).concat(sColor.slice(i, i + 1));
				}
				sColor = sColorNew;
			}
			//处理六位的颜色值  
			var sColorChange = [];
			for(var i = 1; i < 7; i += 2) {
				sColorChange.push(parseInt("0x" + sColor.slice(i, i + 2)) / 255);
			}
			return "[" + sColorChange.join(" ") + " 1]";
		} else {
			return sColor;
		}
	}
	


