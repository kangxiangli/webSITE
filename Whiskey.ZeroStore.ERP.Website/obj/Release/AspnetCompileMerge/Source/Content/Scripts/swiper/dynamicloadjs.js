(function () {
    $.extend({
        /*
         * @params:"pager.js","../jquery/jquery-1.11.1.min.js","~/layer/layer.js"
         * @description:动态加载CSS,路径会自动转换成绝对路径
         */
        loadjs: function () {
            var head = document.getElementsByTagName('head')[0];
            var nowhref = function () {
                var a = document.scripts,
                b = a[a.length - 1],
                c = b.src;
                return c.substring(0, c.lastIndexOf("/"));
            }();
            $.each(arguments, function (index, value) {
                var curhref = nowhref;
                while (value.indexOf("../") == 0) {
                    value = value.substring(3);
                    curhref = curhref.substring(0, curhref.lastIndexOf("/"));
                }
                if (value.indexOf("~/") == 0) {
                    curhref = document.location.host;
                    value = value.substring(2);
                }
                var link = document.createElement('script');
                link.src = curhref + "/" + value;
                link.type = 'text/javascript';
                head.appendChild(link);
            });
        }
    });
})(jQuery);