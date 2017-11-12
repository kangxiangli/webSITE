/**
 * Created by xiuxia on 2016/9/28.
 */
var wh = $(window).height();
$("#jifenMokuai").height(wh - $(".til_bak").height() - $(".allIntegral").height());
$(".wodeJf .back_befo").click(function () {
    window.location.href = '../dk.html';
})