Vue.component('switcher-search', {
    template: '<input id="custswitcher" type="checkbox" data-class="switcher-default checked" checked="checked">',
    mounted() {
        this.$nextTick(function () {

            $("#custswitcher").off('click')
                .switcher({
                    on_state_content: "展开搜索",
                    off_state_content: "隐藏搜索"
                })
                .on('click', function () {
                    var panel_body = $(this).parents('.panel-heading').siblings(".panel-body");
                    if (panel_body.is(":hidden")) {
                        panel_body.slideDown('fast');
                    } else {
                        panel_body.slideUp('fast');
                    }
                })
                
        })
    }
})