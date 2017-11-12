(function ($) {
    $.fn.loadTree = function (data) {
        loadTree(data);
    }

    function findjasonthree(obj, $ul, level) {
        level = level || 0;
        var li = $("<li></li>");
        var i = $('<i class="glyphicon"></i>').text(obj.text);
        var span = $("<span></span>").prepend(i);
        var cb = $("<input type='checkbox' value='" + obj.id + "'/>").prop("checked", obj._checked);
        span.append(cb);
        if (level == 3) {
            cb.attr("name", "right");
            var cb1 = $("<input type='checkbox' class='isShow' value='" + obj.id + "' name='rightShow'/> ").prop("checked", obj._isShow).hide();
            if (obj._gtype)
                cb.attr("data-gtype", obj._gtype);
            span.append(cb1);

        }
        li.append(span);
        if (obj.children) {
            var ul = $("<ul></ul>");
            li.append(ul);
            $.each(obj.children, function (index, value) {
                findjasonthree(value, ul, level + 1);
            });
        }
        $ul.append(li);
    }

    function initialTree() {

        var group = { chakan: 1, jinyong: 5, shanchu: 3 };//逻辑分组

        /*初始化--收起所有栏目和按钮以及添加class*/
        $('.tree li:has(ul)').addClass('parent_li');
        var yiji = $('.tree').children().children().children('ul').children('li').addClass('yiji');
        var erji = yiji.children('ul').children('li').addClass('erji');
        var sanji = erji.children('ul').children('li').addClass('sanji');

        $('.sanji').parent('ul').addClass('sanji_border');
        $('.parent_li').children().find('ul').hide();
        var quanxuan_xianshi = $('<div class="quanxuan_xianshi"></div>').hide();
        //var quanxuan = '<input type="checkbox" class="quanxuan" name="quanxuan" value="全选" />全选';
        var xianshi = '<input type="checkbox" class="xianshi" value="显示" />显示';
        $('.erji:has(>ul:not(:empty))>span').after(quanxuan_xianshi);
        //$('.quanxuan_xianshi').append(quanxuan, xianshi);
        $('.quanxuan_xianshi').append(xianshi);

        $('.tree li.parent_li > span > i').on('click', function (e) {
            var me = $(this).parent();
            var li_par = me.parent('li.parent_li');
            var children = li_par.find('>ul');
            if (children.is(":visible")) {
                children.hide('fast');
                me.find('>i').addClass('glyphicon-plus').removeClass('glyphicon-minus');
                li_par.find(">span").nextAll('.quanxuan_xianshi').hide();
            } else {
                li_par.siblings().find('>ul').hide();
                children.show('fast');
                me.find(' >i').addClass('glyphicon-minus').removeClass('glyphicon-plus');
                me.parent('.parent_li').siblings().find('>span>i').addClass('glyphicon-plus').removeClass('glyphicon-minus');
                $('.erji>ul').siblings('.quanxuan_xianshi').hide();
                li_par.siblings().find('ul').hide().find('li.parent_li>span>i').addClass('glyphicon-plus').removeClass('glyphicon-minus');

                li_par.find(">span").nextAll('.quanxuan_xianshi').show();
            }
        });
        /*权限逻辑分组*/

        var cb_li = $('<li class="sanji"><span><i class="icon-plus-sign"></i></span></li>');
        var cbli_1 = cb_li.clone();
        var cbli_2 = cb_li.clone();
        var cbli_3 = cb_li.clone();

        cbli_1.children().append('查看<input type="checkbox" class="group_type" value=' + group.chakan + '>');
        cbli_2.children().append('禁用<input type="checkbox" class="group_type" value=' + group.jinyong + '>');
        cbli_3.children().append("删除<input type='checkbox'  class='group_type' value=" + group.shanchu + ">");
        $('.tree .sanji_border').prepend(cbli_1, cbli_2, cbli_3);

        $('.tree li.sanji').has(':checkbox[data-gtype="' + group.chakan + '"],:checkbox[data-gtype="' + group.shanchu + '"],:checkbox[data-gtype="' + group.jinyong + '"]').hide();

        $('.group_type').bind('click', function () {
            $(this).parents(".sanji_border:first").find(":checkbox[data-gtype=" + this.value + "]").prop('checked', this.checked);
        });

        //初始化gtype勾选状态
        for (var gtype in group) {
            var gtype_value = group[gtype];
            $('.sanji_border').find('.group_type[value=' + gtype_value + ']').each(function (index, obj) {
                var gtypeCb = $(obj).parents('.sanji_border').find(':checkbox[data-gtype=' + gtype_value + ']');
                var gtype_cheack_length = gtypeCb.length;
                var gtype_checked_length = gtypeCb.filter(":checked").length;
                if (gtype_cheack_length == gtype_checked_length && gtype_cheack_length != 0)
                    obj.checked = true;
            });
        }

        $(".erji").each(function () {
            var obj = $(this);
            //var allCb = obj.find(">ul li span>:checkbox:not(:checkbox[name=rightShow])");
            //var checkedCb = allCb.filter(":checked");
            //if (allCb.length == checkedCb.length && allCb.length != 0)
            //    obj.find(".quanxuan").prop("checked", true);
            var isshowCbCount = obj.find(":checkbox.isShow:checked").length;
            if (isshowCbCount > 0)
                obj.find(".xianshi").prop("checked", true);
        });

        $(".tree .parent_li>span>i").addClass("glyphicon-plus");
        $(".parent_li:first>span>i").addClass("glyphicon-minus");
    }

    //初始化checkbox的级联勾选状态
    function initialCheckBoxCheckedStatus() {
        $('.tree li:not(.parent_li) :checkbox:not(.quanxuan,.xianshi)').bind("click", function () { changecheckedFunc(this); });
        //$(".quanxuan:checkbox").bind("click", function () {
        //    var childCb = $(this).parent().nextAll("ul").find("li span>:checkbox:not(:checkbox[name=rightShow])");
        //    childCb.prop("checked", this.checked);//全选和取消全选
        //    $(this).parents("li:eq(0)").find(" > span >:checkbox").prop("checked", this.checked);
        //    if (!this.checked)
        //        $(this).siblings(".xianshi:checkbox").prop("checked", this.checked).parent().nextAll("ul").find("li span>:checkbox[name=rightShow]").prop("checked", this.checked);
        //    changecheckedFunc(this);
        //});
        $(".xianshi:checkbox").bind("click", function () {
            var childCb = $(this).parent().nextAll("ul").find("li span>:checkbox[name=rightShow]");
            childCb.prop("checked", this.checked);//全选和取消全选
        });
        //以下为父级选中关联子级也选中的代码
        $(".tree li.parent_li>span>:checkbox").bind("click", function () {
            $(this).parents("li:eq(0):has(>ul)").find(":checkbox").prop("checked", this.checked);
            changecheckedFunc(this);
        });

        function changecheckedFunc(obj) {
            var parentCb = $(obj).parents("li:eq(1):has(>span :checkbox)");
            if (parentCb.length) {
                var Cb = parentCb.find(">ul li span :checkbox");
                var CbAll = Cb.filter(":not([name='rightShow'])");//当前级别下所有checkbox
                var CbCheckedCount = CbAll.filter(":checked").length;//当前级别下所有选中的checkbox数量

                //------自动关联全选显示按扭----
                //parentCb.find(">div:has(.quanxuan)>.quanxuan").prop("checked", CbCheckedCount == CbAll.length);
                parentCb.find(">div:has(.xianshi)>.xianshi").prop("checked", Cb.filter("[name='rightShow']:checked").length > 0);
                //-----------END---------

                var nextCb = parentCb.find(">span>:checkbox").prop("checked", CbCheckedCount);//只要当前级别下有选中的checkbox，父级就需要被选中
                changecheckedFunc(nextCb);
            }
        }
    }

    function initialConstruction() {
        $(".tree").append('<ul id="headerUl"></ul>');
    }

    function loadTree(data) {
        initialConstruction();
        findjasonthree(data.obj, $("#headerUl"));
        initialTree();
        initialCheckBoxCheckedStatus();
    }
})(jQuery);