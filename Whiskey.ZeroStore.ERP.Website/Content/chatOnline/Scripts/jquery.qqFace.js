// QQ表情插件
var faceJSON = {};

(function($){  
	$.getJSON('/Content/chatOnline/face.json',function(data){
		faceJSON = data;
	})
	$.fn.qqFace = function(options){
		var defaults = {
			id : 'facebox',
			path : 'face/',
			assign : 'content',
			tip : 'em_'
		};
		var option = $.extend(defaults, options);
		var assign = $('#'+option.assign);
		var id = option.id;
		var path = option.path;
		var tip = option.tip;
		
		if(assign.length<=0){
			alert('缺少表情赋值对象。');
			return false;
		}
		
		$(this).click(function(e){
			var strFace, labFace;
			var _this = this;
			if($('#'+id).length<=0){
				strFace = '<div id="'+id+'" class="qqFace">' +
							  '<table border="0" cellspacing="0" cellpadding="0"><tr>';
			  	$.getJSON('/Content/chatOnline/face.json',function(data){
			  		faceJSON = data;
			  		var i = 0;
					for(var keyword in data){
						i++;
						labFace = keyword +']'
						strFace += '<td><img title="'+ keyword +'" src="'+data[keyword]+'" ondragstart="return false" onclick="$(\'#'+option.assign+'\').setCaret();$(\'#'+option.assign+'\').insertAtCaret(\'[' + labFace + '\')" /></td>';
						if( i % 15 == 0 ) strFace += '</tr><tr>';
					}
					strFace += '</tr></table></div>';
					$(_this).parent().append(strFace);
					var offset = $(_this).position();
					var top = offset.top + $(_this).outerHeight();
					$('#'+id).show();
				})
			}else{
				$('#'+id).show();
			}
			
			e.stopPropagation();
		});

		$(document).click(function(){
			$('#'+id).hide();
			//$('#'+id).remove();
		});
	};

})(jQuery);

jQuery.extend({ 
unselectContents: function(){ 
	if(window.getSelection) 
		window.getSelection().removeAllRanges(); 
	else if(document.selection) 
		document.selection.empty(); 
	} 
}); 
jQuery.fn.extend({ 
	selectContents: function(){ 
		$(this).each(function(i){ 
			var node = this; 
			var selection, range, doc, win; 
			if ((doc = node.ownerDocument) && (win = doc.defaultView) && typeof win.getSelection != 'undefined' && typeof doc.createRange != 'undefined' && (selection = window.getSelection()) && typeof selection.removeAllRanges != 'undefined'){ 
				range = doc.createRange(); 
				range.selectNode(node); 
				if(i == 0){ 
					selection.removeAllRanges(); 
				} 
				selection.addRange(range); 
			} else if (document.body && typeof document.body.createTextRange != 'undefined' && (range = document.body.createTextRange())){ 
				range.moveToElementText(node); 
				range.select(); 
			} 
		}); 
	}, 

	setCaret: function(){ 
		//if(!$.browser.msie) return; 
		var initSetCaret = function(){ 
			var textObj = $(this).get(0); 
			//textObj.caretPos = document.selection.createRange(); 
		}; 
		
		$(this).click(initSetCaret).select(initSetCaret).keyup(initSetCaret); 
	}, 

	insertAtCaret: function(textFeildValue){ 
		var textObj = $(this).get(0); 
		if(document.all && textObj.createTextRange && textObj.caretPos){ 
			var caretPos=textObj.caretPos; 
			caretPos.text = caretPos.text.charAt(caretPos.text.length-1) == '' ? 
			textFeildValue+'' : textFeildValue; 
		} else if(textObj.setSelectionRange){ 
			var rangeStart=textObj.selectionStart; 
			var rangeEnd=textObj.selectionEnd; 
			var tempStr1=textObj.value.substring(0,rangeStart); 
			var tempStr2=textObj.value.substring(rangeEnd); 
			textObj.value=tempStr1+textFeildValue+tempStr2; 
			textObj.focus(); 
			var len=textFeildValue.length; 
			textObj.setSelectionRange(rangeStart+len,rangeStart+len); 
			textObj.blur(); 
		}else{ 
			textObj.value+=textFeildValue; 
		} 
		$(this).focus()
	} 
});

function replace_em(str){
//	str = str.replace(/\</g,'&lt;');
//	str = str.replace(/\>/g,'&gt;');
//	str = str.replace(/\n/g,'<br/>');

	for(var keyword in faceJSON){
		var reg = '\\['+keyword+'\\]';
		str = str.replace(new RegExp(reg,"gi"),'<img src="'+faceJSON[keyword]+'" ondragstart="return false" border="0" />')
	}
	return str;
	
}

