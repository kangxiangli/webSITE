(function ($) {
    function queryStore(sender, url, option) {
        var $self = $(sender);
        $.getJSON(url, function (res) {
            $self.empty();
            $self.addClass('selectpicker').data('live-search', true);
            $self.addClass('show-tick');
            if (res.ResultType !== 3 /* Success */) {
                console.log(res.Message);
                return;
            }
            // 生成optgroup字典,根据option的店铺类型分组,并生成optgroup,最终将optgroup存在字典中
            var data = res.Data;
            var segment = '';
            var groupDict = {};
            if (option && option.filter) {
                data = data.filter(option.filter);
            }
            for (var i = 0; i < data.length; i++) {
                data[i].City = data[i].City || '';
                var groupName = data[i].StoreTypeName;
                var optGroup = groupDict[groupName];
                if (!optGroup) {
                    optGroup = document.createElement('optgroup');
                    optGroup.label = data[i].StoreTypeName;
                }
                var _a = computeIsDisable(data[i], option.context), isDisable = _a[0], desc = _a[1];
                $(optGroup).append("<option value=\"" + data[i].Id + "\" " + (isDisable ? "disabled" : "") + " data-subtext=\"" + data[i].City + "\" data-tokens='" + data[i].StoreName + "," + data[i].StoreTypeName + "," + data[i].Telephone + "," + data[i].City + "'>" + data[i].StoreName + (isDisable ? "(" + desc + ")" : '') + "</option>");
                groupDict[groupName] = optGroup;
            }
            // 拼接option字符串
            for (var group in groupDict) {
                segment += groupDict[group].outerHTML;
            }
            $self.append(segment);
            // selectpicker 初始化
            $self.selectpicker({
                size: 10,
                title: '请选择店铺',
                liveSearchPlaceholder: '店铺名,手机号,城市,类型'
            });
            if (option.selected && option.selected.length > 0) {
                // 选中指定项
                $self.selectpicker('val', option.selected);
            }
            else if (data.length > 0 && option.defaultSelect) {
                // 默认选中
                var first = data[0].Id;
                $self.selectpicker('val', first.toString());
            }
            else {
                $self.selectpicker('val', '');
            }
            // 执行callback
            if (option.callback) {
                option.callback();
            }
        });
    }
    function queryStorage(sender, option) {
        $.getJSON("/Common/QueryAllStorage", { storeId: option.storeId })
            .done(function (res) {
            var $self = $(sender);
            $self.empty();
            $self.addClass('selectpicker').addClass('show-tick');
            if (res.ResultType !== 3 /* Success */) {
                console.log(res.Message);
                return;
            }
            // 生成optgroup字典,根据option的店铺类型分组,并生成optgroup,最终将optgroup存在字典中
            var data = res.Data;
            data.forEach(function (item) { return $self.append("<option value='" + item.Id + "'>" + item.StorageName + "</option>"); });
            // selectpicker 初始化
            $self.selectpicker({
                size: 10,
                title: '请选择仓库'
            });
            if (option.selected && option.selected.length > 0) {
                // 选中指定项
                $self.selectpicker('val', option.selected);
            }
            else if (data.length > 0 && option.defaultSelect) {
                // 默认选中
                var first = data[0].Id;
                $self.selectpicker('val', first.toString());
            }
            else {
                $self.selectpicker('val', '');
            }
        });
    }
    function computeIsDisable(storeEntry, context) {
        var res = [false, ''];
        if (!context === null || context.length <= 0) {
            return res;
        }
        context = context.toLowerCase();
        switch (context) {
            case 'normal':
                break;
            case 'orderblank':
                {
                    if (storeEntry.IsClosed) {
                        res = [true, '已闭店'];
                    }
                    else if (storeEntry.IsChecking) {
                        res = [true, '盘点中'];
                    }
                    break;
                }
            case 'retail': // 零售
            case 'return': // 退货
            case 'addinventory':
                {
                    if (storeEntry.IsClosed) {
                        res = [true, '已闭店'];
                    }
                    else if (storeEntry.IsChecking) {
                        res = [true, '盘点中'];
                    }
                    else if (storeEntry.IsOrderBlanking) {
                        res = [true, '配货中'];
                    }
                    break;
                }
            case 'addpurchase':
                {
                    if (storeEntry.IsChecking) {
                        res = [true, '盘点中'];
                    }
                    break;
                }
            default:
                break;
        }
        return res;
    }
    /**
     * 查询所有店铺
     */
    $.fn.queryAllStore = function (option) {
        /**
         * 配置默认值
         */
        var _defaultOption = {
            context: 'normal',
            selected: [],
            defaultSelect: false,
            callback: null,
            defaultSelectAndHideIfSingleStore: false,
            onlyAttach: false
        };
        option = $.extend(_defaultOption, option);
        var url = '/Common/QueryAllStore?onlyAttach=' + option.onlyAttach;
        queryStore(this, url, option);
    };
    $.fn.queryAllStorage = function (option) {
        queryStorage(this, option);
    };
    /**
     * 查询权限内的店铺
     */
    $.fn.queryManageStore = function (option) {
        /**
         * 配置默认值
         */
        var _defaultOption = {
            context: 'normal',
            selected: [],
            defaultSelect: false,
            callback: null,
            defaultSelectAndHideIfSingleStore: false,
            onlyAttach: false
        };
        option = $.extend(_defaultOption, option);
        var url = "/Common/QueryManageStore?onlyAttach=" + option.onlyAttach;
        queryStore(this, url, option);
    };
})(jQuery);
