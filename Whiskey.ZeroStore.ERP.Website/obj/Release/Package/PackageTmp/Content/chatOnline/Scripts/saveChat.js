$(function(){
	

	var loc, allChatList, myAdminId, myPhoto, friendId, friendPhoto,$peoples = {};
	
	if(!window.localStorage) {
		alert('您的浏览器不支持保存聊天记录功能，建议您使用chrome浏览器');
	}
	//桌面通知设置
	Notifier.RequestPermission();
	Notifier.ModelUpdate();
	
	//*************************************************************************chat对象
	var chat = {
		//总未读消息数量
		allNotReadNum : function() {
			var num = 0;
			for(var key in allChatList) {
				if($('.people[data-id='+ key +']').length > 0 ){
					//统计在线用户的未读消息
					num += parseInt(allChatList[key].notReadNum)
				}
			};
			if(num > 0) {
				$('.notRead').html('您有<span>'+ num +'</span>未读消息')
			} else {
				$('.notRead').html('发起会话')
			}
			return num;
		},
		//未读消息数溢出
		moreNumCut : function(num, max) {
			if(num > max) {
				num = max + '+'
			}
			return num;
		},
		//change 消息列表本地缓存
		updateSetLoc : function() {
			if(!window.localStorage) return;
			localStorage[loc] = JSON.stringify(allChatList);
			chat.allNotReadNum();
		},
		//获取change本地缓存
		updateGetLoc : function() {
			if(!window.localStorage) return;
			allChatList = JSON.parse(localStorage.getItem(loc));
		},
		//自定义复制菜单
		showContextmenu : function(event, _this) {
			$('.copyBtn').remove();
			var txt = window.getSelection().toString();
			if(txt.length >= 1) {
				var html = '<i class="copyBtn chatbtn" data-clipboard-action="copy" data-clipboard-target="#chatbar" style="position: absolute; top:0px; left:45%;">复制<i>';
				$(_this).after(html)
			} else {
				return;
			}
			return txt;
		},
		//部门未读消息数
		bnNotReadNum : function(bmobj) {
			var allBmPeople = $(bmobj).find('.notReadNum');
			var bmnum = 0;
			$(allBmPeople).each(function(i, item) {
				var n = $(item).text()
				if(n == '') {
					n = 0;
				} else {
					n = parseInt($(item).text());
				}
				bmnum += n;
			})
			return bmnum;
		},
		//判断已使用localsrorage大小，超过2m清除全部本地聊天记录
		getLocalStroageSize : function() {
			if(!window.localStorage) return;
			var size = 0;
			var chat = [];
			for (item in window.localStorage) {
			    if (typeof item == "string") {
			        if (item.indexOf('chatList_') == 0) {
			            chat.push(item)
			        }
			        if (window.localStorage.hasOwnProperty(item)) {
			            size += window.localStorage.getItem(item).length;
			        }
			    }
			}
			size = (size / 1024).toFixed(2);
			if(size >= 2000) {
				$(chat).each(function(i, item) {
					localStorage.removeItem(item)
				})
				console.log('当前本地会话记录使用容量超限制，清除所有聊天记录');
			}
			//console.log('当前localStorage使用容量为' + size + 'KB');
		},
		//离线好友通讯中
		peopleOutLine : function(id){
	//		var pl = $('.peopleChating').parents('.peoples').find('.people').length;
			if($('.chatingPeopleName').attr('data-id') == id ) {
	//			if(pl <= 1 ){//部门无其他人删除部门
	//				$('.peopleChating').parents('.bm').remove()
	//			}
				prohibitReply(true)
			}
		},
	}
	//************************************************************************聊天记录对象
	var ChatList = {
		addMine : function(item,sendLoading){
			//转义html标签后转义表情
			var content = replace_em(ToHtmlString(item.content));
			if(IsURL(content)){
				content = '<a target="_blank" href="'+ item.content +'">'+ item.content +'</a>'
			}
			var html = '<div class="clearfix mineSpeak">' +
						'<img ondragstart="return false" src="' + myPhoto + '" ondragstart="return false" />' +
						'<span class="speakText">' + content + '</span>' 
			
			if(sendLoading){
				html += '<img class="sendLoading" src="/Content/chatOnline/Images/loading.png" ondragstart="return false" ></div>'
			}else{
				html += '</div>'
			}
			return html;
		},
		addFriend : function(item){
		//转义html标签后转义表情
			var content = replace_em(ToHtmlString(item.content));
			if(IsURL(content)){
				content = '<a target="_blank" href="'+ item.content +'">'+ item.content +'</a>'
			}
			html =
				'<div class="clearfix friendSpeak">' +
				'<img src="' + item.photo + '" ondragstart="return false" />' +
				'<span class="speakText">' + content + '</span></div>' 
			return html;
		}
	}
	//*************************************************************************部门对象
	var ChatBn = function(data) {
		var $this, admins, bnTitle;
		this.init = function() {
			$this = $('<li data-id="' + data.DepId + '" class="bm"><h2><span class="bnNotRead" style="display:none"></span>' + data.DepName + '<img class="bmOpen" ondragstart="return false" src="/Content/chatOnline/Images/close.png"></h2><div class="peoples"></div></li>')
			admins = data.Admins;
			//logionpeople上线创建部门
			if(!admins)return;
			
			$(admins).each(function(i, item) {
				$this.find('.peoples').append(new People(item).$this);
			})
			$('.peopleList ul').append($this); //.bn
			var bmnum = chat.bnNotReadNum($this);
			if(bmnum > 0){
				$this.find('.bnNotRead').show();
			}
		};
		this.init();
		bnTitle = $this.find('h2');
		bnTitle.click(function() {
	//		$('.textEnterBox').hide();
	//		$('.peopleChating').removeClass('peopleChating');
	//		$('.chating').addClass('notChating');
			var btn = $this.find('.bmOpen');
			if(btn.hasClass('bmOpenActive')) {
				btn.removeClass('bmOpenActive');
				$this.find('.peoples').slideUp('fast')
			} else {
				btn.addClass('bmOpenActive');
				$this.find('.peoples').slideDown('fast')
			}
		})
	return $this;
	}
//=================================================================================
//*************************************************************************登录用户对象
//=================================================================================
	var People = function (data) {
		var _this = this;
		this.id = data.AdminId;
		this.data = data;
		$peoples['people_'+this.id] = this;
		var $this = $(this.opeopleTmp(data));//.people
		this.$this = $this;
		var this_delete = $this.find('.delete');
		var this_oclear = $this.find('.oclear');
		//发起会话
		$this.click(function(){
			_this.clickOpenChat()
		});
		//右键菜单
		$this.on('contextmenu', function() {
			$('.delBox').not($(this).find('.delBox')).slideUp();
			$(this).find('.delBox').slideDown('fast');
			return false;
		});
		//消息免打扰好友
		this_delete.click(function(){
			_this.disturb();
			return false;
		})
		//清除聊天记录
		this_oclear.click(function(){
			$this.find('.lately').empty();
			$('.delBox').slideUp('fast');
			_this.delChatList();
			return false;
		})
		return this;
	}
	//本地存储会话
	People.prototype.setlocChatList = function(msgData, isMe) {
		var id = msgData.adminId;
		allChatList[id] = allChatList[id] || {};
		allChatList[id].UserPhoto = $('.people[data-id='+ id +']').find('.photo').attr('src') || '';
		allChatList[id].msg = allChatList[id].msg || [];
		allChatList[id].notReadNum = allChatList[id].notReadNum || 0;
		var msg = allChatList[id].msg;
		var time = msgData.time;
		allChatList[id].lately = msgData.content;
		//存储消息时更新显示最新消息
		var friend = $('.people[data-id=' + id + ']');
		var c = ToHtmlString(msgData.content).replace(/<br\/>/ig, ";");
		friend.find('.lately').html(replace_em(c))
		var oneChat = {
			"time": time,
			"content": msgData.content,
			"isme": isMe
		};
		//存储最大消息数限制
		if(msg.length >= 30) {
			var l = msg.length - 30;
			msg.splice(0, l)
		}
		msg.push(oneChat);
		chat.updateSetLoc();
	}
	//收到消息
	People.prototype.peopleGetMessage = function (data){
		var friendId = data.adminId;
		var friendPhoto = $('.people[data-id=' + friendId + ']').find('.photo').attr('src');
		var friend = $('.people[data-id=' + friendId + ']');
		var isturbPeople =  judgeGetMessageIsDisturbPeople(friendId);
		//好友未屏蔽提示进行消息提醒
		showNotifier(friendId);
		data.photo = friendPhoto;
		var toSendPeopleKey = 'people_'+ friendId;
		$peoples[toSendPeopleKey].setlocChatList(data, 0);
		//聊天好友置部门顶
		$(friend).prependTo($(friend).parent());
		//判断是否打开好友窗口聊天中
		if($('.chatingPeopleName').attr('data-id') != friendId) {
			if(!allChatList[friendId].notReadNum) {
				allChatList[friendId].notReadNum = 1;
			} else {
				var num = allChatList[friendId].notReadNum + 1;
				allChatList[friendId].notReadNum = num;
			}
	
			chat.updateSetLoc();
			var num = allChatList[friendId].notReadNum;
			num = chat.moreNumCut(num, 99);
			friend.find('.notReadNum').show().text(num);
			friend.parents('.bm').find('.bnNotRead').show();
		} else {
			$('.chatList .list').append(ChatList.addFriend(data));
			chalistScroll();
		}
		chat.allNotReadNum()
	}
	//发起会话
	People.prototype.clickOpenChat = function(){
		
		var friendId = this.id,friendPhoto = this.data.UserPhoto,friendName = this.data.RealName,$this = this.$this;
		
		//展示聊天面板
		$('.textEnterBox').show();
		$('.chating').removeClass('notChating');
		$('.chatingPeopleName').attr('data-id', friendId);
		$('.peopleChating').removeClass('peopleChating');
		$this.addClass('peopleChating');
		$('.chatList .list').empty();
		$('.chatingPeopleName').text(friendName);
		//清除好友未读气泡
		$this.find('.notReadNum').empty().hide();
		if(allChatList[friendId]) {
			allChatList[friendId].notReadNum = 0;
			chat.updateSetLoc();
		}
		//清除部门未读气泡
		var bmnum = chat.bnNotReadNum($this.parents('.bm'));
		if(bmnum == 0){
			$this.parents('.bm').find('.bnNotRead').hide();
		}else{
			$this.parents('.bm').find('.bnNotRead').show();
		}
		//取历史纪录,滚动到底部
		this.getAllLocChatList(friendId);
		chalistScroll();
		//离线消息禁止回复
		if($this.hasClass('outLine')){
			prohibitReply(true);
			return;
		}
		prohibitReply();
	}
	//获取对应用户本地会话列表渲染数据
	People.prototype.getAllLocChatList = function() {
		chat.updateGetLoc();
		var friendid = this.id;
		var friendPhoto = this.data.UserPhoto;
		if(!allChatList[friendid]) return;
		var msgArrList = allChatList[friendid].msg;
		var html = '';
	
		$(msgArrList).each(function(i, item) {
//			console.log(item)
			item.photo = friendPhoto;
			if(item.isme) {
				html += ChatList.addMine(item)//调用我的聊天模板
			} else {
				html += ChatList.addFriend(item)//调用好友聊天模板
			}
			
			if(i == msgArrList.length - 1 ){
				html += '<div class="time">'+ new Date(item.time*1000).toLocaleString() +'</div>'
			}else if((i % 10) == 1){
				html += '<div class="time">'+ new Date(item.time*1000).toLocaleString() +'</div>'
			}
		})
		$('.chatList .list').append(html)
	}
	//删除本地聊天记录
	People.prototype.delChatList = function() {
		var id = this.id;
		chat.updateGetLoc();
		var currentId = $('.chatingPeopleName').attr('data-id');
		if(allChatList.hasOwnProperty(id)) {
			delete allChatList[id];
			if(id == currentId) {
				$('.chatList .list').empty();
			}
			chat.updateSetLoc();
		}
	}
	//在线好友列表模板 .people
	People.prototype.opeopleTmp = function(data) {
		chat.updateGetLoc();
		//未读消息
		var notReadNum = null,
			lately = '';
		if(allChatList.hasOwnProperty(data.AdminId)) {
			if(allChatList[data.AdminId].notReadNum) {
				notReadNum = allChatList[data.AdminId].notReadNum;
				var num = notReadNum;
				num = chat.moreNumCut(num, 99);
			}
			if(allChatList[data.AdminId].lately) {
				var c = ToHtmlString(allChatList[data.AdminId].lately).replace(/<br\/>/ig, ";");
				lately = replace_em(c);
			}
		}
		var html = '<div data-id=' + data.AdminId + ' class="people">'
		if(notReadNum != null) {
			html += '<i class="notReadNum">' + num + '</i>';
		} else {
			html += '<i class="notReadNum" style="display:none"></i>';
		}
		html += '<div class="delBox"><span class="delete"><img title="消息免打扰" ondragstart="return false" src="/Content/chatOnline/Images/disturb.png"/></span><span class="oclear"><img title="清空聊天记录" ondragstart="return false" src="/Content/chatOnline/Images/delete.png"/></span></div><img class="photo" ondragstart="return false" src="' + data.UserPhoto + '" />' +
			'<div class="peopleName"><span class="jobname">' + data.JobName + '</span>&nbsp<span>' + data.RealName + '</span></div>' +
			'<div class="lately">' + lately + '</div></div>'
		return html;
	}
	//屏蔽好友
	People.prototype.disturb = function(){
		var id = this.id;
		var $this = this.$this;
		var dr = [
				{"src":'/Content/chatOnline/Images/disturb.png',"title":'消息免打扰'},
				{"src":'/Content/chatOnline/Images/closedr.png',"title":'关闭消息免打扰'}
			]
		//免打扰
		if($this.hasClass('disturbPeople')){
			$this.removeClass('disturbPeople');
			$this.find('.disturbpeople').remove();
			$('.delBox').slideUp();
			$this.find('.delete img').attr({'src':dr[0].src,'title':dr[0].title})
		}else{
			$this.find('.delete img').attr({'src':dr[1].src,'title':dr[1].title})
			$this.addClass('disturbPeople');
			$('.delBox').slideUp();
			$this.find('.disturbpeople').remove();
			$this.append('<img class="disturbpeople" ondragstart="return false" title="'+ dr[0].title +'" src="'+ dr[0].src +'"/>')
		
		}
	}
	//登陆用户模板
    People.prototype.Login = function() {
    	var data = this.data;
		$('.people[data-id=' + data.AdminId + ']').remove();
		var html = this.$this;
		var bmis = $('.bm[data-id=' + data.DepId + ']').length;
		if(bmis == 0) {
			var bnhtml = new ChatBn(this.data);
			bnhtml.find('.peoples').append(html)
			$('.peopleList ul').append(bnhtml)
		} else {
			$('.bm[data-id=' + data.DepId + ']').find('.peoples').append(html)
		}
	}
	

	//*************************************************************************离线消息禁止回复
	function prohibitReply(no){
			if(no){
				$('.textEnter .textarea').attr('disabled','disabled');
				$('.textEnter .textarea').css('color','#AAA');
				$('.textEnter .textarea').val('该员工已离线，不能发送消息');
				$('.emotion').hide();
			}else{
				$('.textEnter .textarea').removeAttr('disabled');
				$('.textEnter .textarea').val('');
				$('.textEnter .textarea').css('color','#000');
				$('.emotion').show();
			}
		}
	//-------------------------------------------------------------------------光标位置插入文本
	function insertText(obj, str) {
		if(document.selection) {
			var sel = document.selection.createRange();
			sel.text = str;
		} else if(typeof obj.selectionStart === 'number' && typeof obj.selectionEnd === 'number') {
			var startPos = obj.selectionStart,
				endPos = obj.selectionEnd,
				cursorPos = startPos,
				tmpStr = obj.value;
			obj.value = tmpStr.substring(0, startPos) + str + tmpStr.substring(endPos, tmpStr.length);
			cursorPos += str.length;
			obj.selectionStart = obj.selectionEnd = cursorPos;
		} else {
			obj.value += str;
		}
		moveEnd(obj);
	}
	//-------------------------------------------------------------------------光标移动到最后
	function moveEnd(obj) {
		obj.focus();
		var len = obj.value.length;
		if(document.selection) {
			var sel = obj.createTextRange();
			sel.moveStart('character', len);
			sel.collapse();
			sel.select();
		} else if(typeof obj.selectionStart == 'number' && typeof obj.selectionEnd == 'number') {
			obj.selectionStart = obj.selectionEnd = len;
		}
	}
	//-------------------------------------------------------------Html结构转字符串形式显示 支持<br>换行
	function ToHtmlString(htmlStr) {
		return toTXT(htmlStr).replace(/\&lt\;br[\&ensp\;|\&emsp\;]*[\/]?\&gt\;|\r\n|\n/g, "<br/>");
	}
	//-------------------------------------------------------------------------Html结构转字符串形式显示
	function toTXT(str) {
		var RexStr = /\<|\>|\"|\'|\&|　| /g
		str = str.replace(RexStr,
			function(MatchStr) {
				switch(MatchStr) {
					case "<":
						return "&lt;";
						break;
					case ">":
						return "&gt;";
						break;
					case "\"":
						return "&quot;";
						break;
					case "'":
						return "&#39;";
						break;
					case "&":
						return "&amp;";
						break;
					case " ":
						return "&ensp;";
						break;
					case "　":
						return "&emsp;";
						break;
					default:
						break;
				}
			}
		)
		return str;
	}
	//-------------------------------------------------------------------------正则网址
	function IsURL(urlString) {
		regExp = /^(http|ftp|https):\/\/[\w\-_]+(\.[\w\-_]+)+([\w\-\.,@?^=%&amp;:/~\+#]*[\w\-\@?^=%&amp;/~\+#])?$/
		if(urlString.match(regExp))
			return true;
		else
			return false;
	}
	//-------------------------------------------------------------------------限制输入字节数
	function cutText (obj,count) {
		count = count || 100;
		if($(obj).val().length > count){
			$(obj).val($(obj).val().substring(0,count))
		}
	}
	//--------------------------------------------------------------------------新消息title change提示
	var timer,startTitle = document.title||'零时尚ERP管理平台 V2 — 后台管理 — 我们的任务是让你的工作变得更简单！',titleIndex = 0;
	function setTimer(txt){
		titleIndex ++;
		clearTimeout(timer);
		timer = setTimeout(function(){
			if(titleIndex%2){
				$('.Conversation .letter').attr('src','/Content/chatOnline/Images/chat.png')
				document.title = txt;
			}else{
				$('.Conversation .letter').attr('src','/Content/chatOnline/Images/chatNewmessage.png')
				document.title = startTitle;
			}
			setTimer(txt);
		}, 500)
	}
	function clearTimer(){
		clearTimeout(timer);
		document.title = startTitle;
		$('.Conversation .letter').attr('src','/Content/chatOnline/Images/chat.png')
	}
	
	//--------------------------------------------------------------------监听当前窗口活动状态
	var pageOnActivate = true;//当前窗口活动状态
	if(typeof document.addEventListener !== "undefined" || typeof document[hidden] !== "undefined") {
		document.addEventListener("visibilitychange", function() {
			if(document.hidden) {
				pageOnActivate = false;
			} else{
				pageOnActivate = true;
				//获取页面焦点时关闭所有桌面通知
				Notifier.CloseAll(); 
			}
		}, false);
	}
	////判断是否是免打扰好友
	function judgeGetMessageIsDisturbPeople(id){
		if($('.disturbPeople[data-id='+ id +']').length > 0 ){
			return true;
		}else{
			return false;
		}
	}
	//判断是否为免打扰 推送新消息
	function showNotifier(friendId){
		var isturbPeople =  judgeGetMessageIsDisturbPeople(friendId);
		//好友未屏蔽提示新消息
		if( !isturbPeople ){
			setTimer('你有未读新消息');
			document.getElementById("getMsgAudio").play();
			//页面没有焦点推送桌面通知
			if(!pageOnActivate){
				Notifier.Notify('/Content/chatOnline/Images/logo.png', '零时尚ERP管理平台', '你有新的未读消息');
			}
		}
	}
	
	
	
	
	/*=====================================================================================
	 * *************************************************************************************
	 =====================================================================================*/
	//检查缓存容量
	chat.getLocalStroageSize();
	//------------------------------------8----------------登陆用户信息
	$.fashion.message.me = function(data) {
		myAdminId = data.AdminId;
		myPhoto = data.UserPhoto;
		if(!window.localStorage) return;
		loc = 'chatList_' + myAdminId;
		if(localStorage.getItem(loc) == null) {
			allChatList = {};
			localStorage[loc] = JSON.stringify(allChatList);
	
		} else {
			allChatList = JSON.parse(localStorage.getItem(loc));
		}
	}
	//----------------------------------------------------获取登录用户
	$.fashion.message.getAllAdmin = function(data) {
		var bm = data;
		if(data.length < 1) return;
		$('.peopleList ul').empty();
		$(data).each(function(i, item) {
			peoples = new ChatBn(item);
		})
		//判断部门未读消息展示
		chat.allNotReadNum();
	};
	//-----------------------------------------------------接收消息
	$.fashion.message.get = function(data) {
		var peopleKey = 'people_'+ data.adminId;
		$peoples[peopleKey].peopleGetMessage(data);
	}
	//----------------------------------------------------用户上线
	$.fashion.message.login = function(data) {
		$('.noLogin').remove();
		$(data).each(function(i, item) {
			//chat.loginPeople(item);
			var LoginPeople = new People(item);
			LoginPeople.Login(item);
			//聊天中好友离线后上线
			if($('.chatingPeopleName').attr('data-id') == item.AdminId ){
				$('.people[data-id='+ item.AdminId +']').click();
			};
		});
		chat.allNotReadNum();
	};
	//---------------------------------------------------用户离线
	$.fashion.message.exit = function(data) {
		chat.updateGetLoc();
		$(data).each(function(i, item) {
			chat.peopleOutLine(item);
			$('.people[data-id=' + item + ']').addClass('outLine');
		})
	};


//******************************************************************DOM
	var kc = 0,sleTxt = 0,copyTxt,chatDisY = 0,disY = 0;
	//禁止右键
	$('.chatBox').on('contextmenu', function() {
		return false;
	})
	

	//--------------------------------------------------发起/关闭会话
	$('.Conversation').click(function() {
		$('.chatBox').slideDown();
		
	})
	$('.chatBox,.Conversation').click(function(){
		clearTimer();
	})
	$('.chating .close').click(function() {
		$('.chatBox').slideUp();
		closeChating();
	})
	function closeChating(){
		$('.textEnterBox').hide();
		$('.chatingPeopleName').attr('data-id',"")
		$('.bmOpenActive').removeClass('bmOpenActive');
		$('.peoples').slideUp(0);
		$('.peopleChating').removeClass('peopleChating');
		$('.chating').addClass('notChating');
	}
	//-------------------------------------------------------- 发送会话消息
	$(".chatBox .textarea").bind("keydown", function(e) {
		var friendId = $('.chatingPeopleName').attr('data-id')
		cutText($(this),200);//限制字节数
		// 兼容FF和IE和Opera    
		var theEvent = e || window.event;
		var code = theEvent.keyCode || theEvent.which || theEvent.charCode;
		if(e.ctrlKey && code == 13){
			//换行
			$(".chatBox .textarea").val($(".chatBox .textarea").val()+'\r\n');
			return;
		}else  if( code == 13 ){
			var speak = $(".chatBox textarea").val();
			var originSpeak = speak;
			$(".chatBox .textarea").val('');
			if(speak.trim() == '') return false;
			var speaka = speak;
			//speak = ToHtmlString(speak);
			
			var html = $(ChatList.addMine({"content":speaka},true));
			
			$('.chatList .list').append(html);
			chalistScroll();
			//发送消息
			$.fashion.message.send(originSpeak, [friendId],function(type,msg){
				if(type == true ){
					$(html).find('.sendLoading').remove();
				}
			});
			var ochat = {
				"adminId": friendId,
				"content":  originSpeak,
				"time": new Date().getTime()/1000
			};
			
			var toSendPeopleKey = 'people_'+ friendId;
			$peoples[toSendPeopleKey].setlocChatList(ochat, 1);
			//chat.setlocChatList(ochat, 1);
			return false;
		}
	});
	//----------------------------------------------------------复原右键菜单
	$(document).click(function() {
		$('.delBox').slideUp('fast');
		$('#textEdit').hide();
	})
	//---------------------------------------------------------------复制粘贴
//	$('.chatList').on('mouseup', '.speakText,.time', function(ev) {
//		sleTxt = chat.showContextmenu(ev,$(this));
//		$('#chatbar').val(sleTxt);
//		return false;
//	})
	//初始化剪贴板
	//var clipboard = new Clipboard('.chatbtn');
	
	$('.chatList').on('mouseup', '.copyBtn', function() {
		copyTxt = $('#chatbar').val();
		return false;
	})
	
	$('.chatList,.textEnter').click(function(){
		$('.copyBtn').remove();
	})
	var textareaCopyBtn = false;
//	$('.textEnter').on('contextmenu',function() {
//		if(!textareaCopyBtn)return;
//		textareaCopyBtn = false;
//		$('#textEdit').show();
//	})
	
	$('.textEnter .textarea').on('mouseup',function(){
		sleTxt =  window.getSelection().toString();
		if(sleTxt.length > 0 )textareaCopyBtn = true;
		$('#chatbar').val(sleTxt)
	})
	
	$('.textEditprase').click(function(){
		if(!copyTxt)return;
		insertText($('.textEnter .textarea')[0],copyTxt)
	})
	
	//-----------------------------------------------------------------聊天信息滚动
	var _this =  $('.chatBox');
	if(navigator.userAgent.indexOf("Firefox")>0){  
        _this.addEventListener('DOMMouseScroll',function(e){  
            _this.scrollTop += e.detail > 0 ? 60 : -60;     
            e.preventDefault();  
        },false);   
    }else{  
        _this.onmousewheel = function(e){     
            e = e || window.event;     
            _this.scrollTop += e.wheelDelta > 0 ? -60 : 60;     
            return false;  
        };  
    }  
	$('.chatList').on("mousewheel DOMMouseScroll", function(e) {
          e.preventDefault();  
		chatDisY = parseInt($('.listBox .list').css('marginTop'));
		var bottom = $('.listBox .list').height() - Math.abs(chatDisY) - $('.listBox').height();
		var delta = (e.originalEvent.wheelDelta && (e.originalEvent.wheelDelta > 0 ? 1 : -1)) || // chrome & ie
			(e.originalEvent.detail && (e.originalEvent.detail > 0 ? -1 : 1)); // firefox
		if(delta > 0) {
			// 向上滚
			chatDisY += 20;
			if(chatDisY > 0){
				chatDisY = 0;
			} 
			
			
		} else if(delta < 0) {
			// 向下滚
			chatDisY -= 20;
			if(bottom <= 0) return;
			
		}
		$('.listBox .list').css('marginTop', chatDisY);
		return false;
		
	});
	
	//---------------------------------------------------------好友列表滚动
	$('.peopleList').on("mousewheel DOMMouseScroll", function(e) {
          e.preventDefault();  
		
		var top = $('.peopleList ul').position().top;
		var bottom = $('.peopleList ul').height() - Math.abs(top) - $('.peopleBox').height();
		var delta = (e.originalEvent.wheelDelta && (e.originalEvent.wheelDelta > 0 ? 1 : -1)) || // chrome & ie
			(e.originalEvent.detail && (e.originalEvent.detail > 0 ? -1 : 1)); // firefox
		if(delta > 0) {
			// 向上滚
			if(top >= 0) return;
			disY += 20;
		} else if(delta < 0) {
			// 向下滚
			if(bottom <= 0) return;
			disY -= 20;
		}
		$('.peopleList ul').css('top', disY);
	});
	//-----------------------------------------------------会话列表滚动到最底部
	function chalistScroll() {
		if($('.list').height() <= $('.listBox').height()){
			$('.listBox .list').css('marginTop', 0)
			return;
		}
		var scollY = $('.listBox .list').height() - $('.listBox').height();
		$('.listBox .list').css('marginTop', -scollY)
	}
	//-----------------------------------------------------搜索用户
	function search(){
		var key = $('.search input').val();
		if ( key == '' )return;
		$('.bm').hide();
		$('.people').hide();
		var peoples = $('.peopleName');
		$(peoples).each(function(i,item){
			if($(item).text().indexOf(key) >= 0 ){
				$(item).parents('.bm').show();
				$(item).parents('.bm').find('.bmOpen').addClass('bmOpenActive');
				$(item).parents('.people').show();
				$(item).parents('.peoples').slideDown();
			}
		})
		//好友列表滚动到顶部
		$('.peopleBox ul').css('top',0);
	}
	$('.searchBtn').click(function(){
		search();
	})
	$('.peopleList .search input').on('keyup',function(e){
		if($('.search input').val().length > 0 ){
			$('.peopleList .search .close').show();
		}else{
			$('.peopleList .search .close').hide();
			closePeopleSearch();
		}
		var theEvent = e || window.event;
		var code = theEvent.keyCode || theEvent.which || theEvent.charCode;
		if(code == 13){
			search()
		}
	})

	$('.peopleList .search .close').click(function(){
		closePeopleSearch();
	})
	function closePeopleSearch(){
		$('.peopleList .search .close').hide();
		$('.search input').val('');
		$('.people').show();
		$('.bm').show();
		var chatingPeople = $('.peopleChating').parents('.peoples');
		$('.peoples').not(chatingPeople).slideUp(0);
		chatingPeople.slideDown();
		$('.bmOpen').removeClass('bmOpenActive');
		$('.peopleChating').parents('.bm').find('.bmOpen').addClass('bmOpenActive')
	}
	//------------------------------------------------------------ 点击会话窗关联联系人
	$('.chatingPeopleName').click(function(){
		closePeopleSearch();
	})


//表情
	$('.emotion').qqFace({
		id : 'facebox', 
		assign:'saytext', 
		path:'/Content/chatOnline/arclist/',	//表情存放的路径
		
	});

})



