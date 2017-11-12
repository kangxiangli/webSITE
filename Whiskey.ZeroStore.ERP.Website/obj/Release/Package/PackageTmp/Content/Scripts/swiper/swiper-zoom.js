(function ($) {
    $.fn.extend({
        fZoom: function (selector, isUnique, selSrc) {
            var $wrapper = $(this);

            $selector = $(selector);

            function initialOptions(isUnique) {
                var selectorNew = $selector.filter(function () {
                    var src = $(this).attr("src");
                    return src != null && src != "null" && src != "";
                });

                var allimgs = $(selectorNew).map(function () {
                    return $(this).attr("src");
                }).get();

                function unique(arr) {
                    var ret = []
                    for (var i = 0; i < arr.length; i++) {
                        var item = arr[i]
                        if (ret.indexOf(item) === -1) {
                            ret.push(item)
                        }
                    }
                    return ret
                }

                if (isUnique) {
                    allimgs = unique(allimgs);
                }

                var allimgitems = [];
                var selIndex = 0;
                $.each(allimgs, function (index, item) {
                    if (item == selSrc) {
                        selIndex = index;
                    }
                    var curImg = new Image();
                    curImg.src = item;
                    allimgitems.push({
                        src: item,
                        w: curImg.width,
                        h: curImg.height
                    });
                });

                var options = {
                    index: selIndex,
                    history: false,
                    focus: false,
                    maxSpreadZoom :3,
                    showAnimationDuration: 0,
                    hideAnimationDuration: 0
                };

                return { items: allimgitems, options: options };
            }

            function openPhotoSwipe(options) {
                if ($wrapper.length > 0) {
                    var gallery = new PhotoSwipe($wrapper[0], PhotoSwipeUI_Default, options.items, options.options);
                    gallery.init();
                }
            };

            openPhotoSwipe(initialOptions(isUnique));
        }
    });
})(jQuery);