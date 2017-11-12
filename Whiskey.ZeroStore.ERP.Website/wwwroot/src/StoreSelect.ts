

(function ($: JQueryStatic) {
    type queryContext = 'normal' | 'retail' | 'return' | 'orderblank' | 'addinventory' | 'addpurchase'; //查询上下文,用于区分不同场景下的查询

    /**
     * 查询配置项
     * 
     * @interface queryOptions
     */
    interface queryOptions {
        /**
         * 查询上下文,默认值:normal
         * 
         * @type {string}
         * @memberOf queryOptions
         */
        context: string;

        /**
         * 默认选中值
         * 
         * @type {Array<any>}
         * @memberOf queryOptions
         */
        selected: Array<any>;

        /**
         * 是否默认选择第一条option
         * 
         * @type {boolean}
         * @memberOf queryOptions
         */
        defaultSelect: boolean;

        /**
         * 生成之后的回调
         * 
         * @type {Function}
         * @memberOf queryOptions
         */
        callback: Function;

        /**
        * 是否仅查询可归属的店铺
        * 
        * @type {boolean}
        * @memberOf queryOptions
        */
        onlyAttach: boolean;


        filter: (item: QueryAllStoreRes) => boolean;

    }

    interface queryStorageOption {
        storeId: Number,
        /**
         * 默认选中值
         * 
         * @type {Array<any>}
         * @memberOf queryOptions
         */
        selected: Array<any>;

        /**
         * 是否默认选择第一条option
         * 
         * @type {boolean}
         * @memberOf queryOptions
         */
        defaultSelect: boolean;

        /**
         * 生成之后的回调
         * 
         * @type {Function}
         * @memberOf queryOptions
         */
        callback: Function;
    }

    function queryStore(sender: any, url: string, option?: queryOptions) {
        var $self = $(sender);
        $.getJSON(url, function (res: OperationResult<QueryAllStoreRes[]>) {
            $self.empty();
            $self.addClass('selectpicker').data('live-search', true);
            $self.addClass('show-tick');
            if (res.ResultType !== OperationResultType.Success) {
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
                let groupName = data[i].StoreTypeName;
                let optGroup = groupDict[groupName] as HTMLOptGroupElement;
                if (!optGroup) {
                    optGroup = document.createElement('optgroup') as HTMLOptGroupElement;
                    optGroup.label = data[i].StoreTypeName;
                }
                let [isDisable, desc] = computeIsDisable(data[i], option.context);
                $(optGroup).append(`<option value="${data[i].Id}" ${isDisable ? "disabled" : ""} data-subtext="${data[i].City}" data-tokens='${data[i].StoreName},${data[i].StoreTypeName},${data[i].Telephone},${data[i].City}'>${data[i].StoreName}${isDisable ? `(${desc})` : ''}</option>`);
                groupDict[groupName] = optGroup;

            }

            // 拼接option字符串
            for (let group in groupDict) {
                segment += (groupDict[group] as HTMLOptGroupElement).outerHTML;
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

        })
    }
    function queryStorage(sender: any, option: queryStorageOption) {
        $.getJSON("/Common/QueryAllStorage", { storeId: option.storeId })
            .done(function (res) {
                var $self = $(sender)
                $self.empty();
                $self.addClass('selectpicker').addClass('show-tick');
                if (res.ResultType !== OperationResultType.Success) {
                    console.log(res.Message);
                    return;
                }

                // 生成optgroup字典,根据option的店铺类型分组,并生成optgroup,最终将optgroup存在字典中
                var data = res.Data as Array<any>;
                data.forEach(item => $self.append(`<option value='${item.Id}'>${item.StorageName}</option>`))

              

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
            })
    }

    function computeIsDisable(storeEntry: QueryAllStoreRes, context: string): [boolean, string] {
        let res: [boolean, string] = [false, ''];
        if (!context === null || context.length <= 0) {
            return res;
        }
        context = context.toLowerCase() as queryContext;


        switch (context) {
            case 'normal':     //
                break;
            case 'orderblank': // 配货
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
            case 'addinventory': // 入库

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
            case 'addpurchase': //采购
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
    $.fn.queryAllStore = function (option?: queryOptions) {
        /**
         * 配置默认值
         */
        let _defaultOption = {
            context: 'normal',
            selected: [],
            defaultSelect: false,
            callback: null,
            defaultSelectAndHideIfSingleStore: false,
            onlyAttach: false
        }
        option = $.extend(_defaultOption, option);
        const url = '/Common/QueryAllStore?onlyAttach=' + option.onlyAttach;
        queryStore(this, url, option);

    }

    $.fn.queryAllStorage = function (option: queryStorageOption) {
        queryStorage(this, option)
    }

    /**
     * 查询权限内的店铺
     */
    $.fn.queryManageStore = function (option?: queryOptions) {
        /**
         * 配置默认值
         */
        let _defaultOption = {
            context: 'normal',
            selected: [],
            defaultSelect: false,
            callback: null,
            defaultSelectAndHideIfSingleStore: false,
            onlyAttach: false
        }
        option = $.extend(_defaultOption, option);
        const url = `/Common/QueryManageStore?onlyAttach=${option.onlyAttach}`;
        queryStore(this, url, option);
    }
})(jQuery)

/**
 * 店铺查询返回结构
 * 
 * @interface QueryAllStoreRes
 */
interface QueryAllStoreRes {
    /**
     * 店铺id
     * 
     * @type {Number}
     * @memberOf QueryAllStoreRes
     */
    Id: Number;

    /**
     * 店铺名称
     * 
     * @type {string}
     * @memberOf QueryAllStoreRes
     */
    StoreName: string;

    /**
     * 店铺类型,直营|授权|加盟
     * 
     * @type {string}
     * @memberOf QueryAllStoreRes
     */
    StoreTypeName: string;

    /**
     * 所在城市
     * 
     * @type {string}
     * @memberOf QueryAllStoreRes
     */
    City: string;

    /**
     * 联系电话
     * 
     * @type {string}
     * @memberOf QueryAllStoreRes
     */
    Telephone: string;

    /**
     * 是否已闭店
     * 
     * @type {boolean}
     * @memberOf QueryAllStoreRes
     */
    IsClosed: boolean;

    /**
     * 是否配货中
     * 
     * @type {boolean}
     * @memberOf QueryAllStoreRes
     */
    IsOrderBlanking: boolean;

    /**
     * 是否盘点中
     * 
     * @type {boolean}
     * @memberOf QueryAllStoreRes
     */
    IsChecking: boolean;
}