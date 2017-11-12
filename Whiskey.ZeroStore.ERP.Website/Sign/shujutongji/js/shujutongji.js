/**
 * Created by xiuxia on 2016/9/27.
 */
$("#shujuTJ").height($(window).height() - $(".til_bak").height());
$("#shujuTJ>div").height($("#shujuTJ").height() / 4).css("line-height", $("#shujuTJ>div").height() + 'px');
$(".shujutjBox .til_bak .back_befo").click(function () {
    window.location.href="../dk.html";
});