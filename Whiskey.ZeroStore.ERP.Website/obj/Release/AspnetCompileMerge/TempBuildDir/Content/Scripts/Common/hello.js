
var words;
var day = new Date();
var hr = day.getHours();
var min = day.getMinutes();

switch (hr) {
    case 1:
        words = "你真是工作狂啊，别忘了休息哦！";
        break;
    case 2:
        words = "该休息了，身体是革命的本钱！";
        break;
    case 3:
        words = "午夜三点！你还不准备睡觉吗？";
        break;
    case 4:
        words = "一定要把您这种忘我的工作精神上报中央！";
        break;
    case 5:
        words = "您是刚起床还是还没睡啊？";
        break;
    case 6:
        words = "又是忙碌的一天！";
        break;
    case 7:
        words = "亲，吃过早饭了吗？";
        break;
    case 8:
        words = "新一天又开始啦！";
        break;
    case 9:
        words = "静谧的一刻，从阳光射进窗台开始！";
        break;
    case 10:
        words = "在新的一天里，遇见最好的自己！";
        break;
    case 11:
        words = "清晨，一个微笑，给自己加油打气！";
        break;
    case 12:
        words = "工作再忙，午餐必不可少哦！";
        break;
    case 13:
        words = "忙碌的下午即将开始！";
        break;
    case 14:
        words = "开始奋斗吧！";
        break;
    case 15:
        words = "加油哦！辛勤工作，劳有所得！";
        break;
    case 16:
        words = "忙碌了一下午，稍作休息继续战斗。";
        break;
    case 17:
        words = "忙碌的一天快结束了哦！";
        break;
    case 18:
        words = "下班了吗？吃一顿丰盛的晚餐吧！";
        break;
    case 19:
        words = "工作狂，吃晚饭了没？";
        break;
    case 20:
        words = "抽点时间看新闻联播哈。";
        break;
    case 21:
        words = "听一曲轻松的歌儿吧！";
        break;
    case 22:
        words = "别工作了，找个电影看看睡觉吧？";
        break;
    case 23:
        words = "不早了，快休息吧！";
        break;
    case 0:
        words = "午夜时分了！";
        break;

}


$("#AdminName").grumble(
    {
        angle: 45,
        text: words,
        type: 'alt-',
        distance: 200,
        hideOnClick: true,
        onShow: function () {
            var angle = 45, dir = 1;
            interval = setInterval(function () {
                (angle > 60 ? (dir = -0.1, angle--) : (angle < 45 ? (dir = 0.1, angle++) : angle += dir));
                $("#AdminName").grumble('adjust', { angle: angle });
            }, 25);
        },
        onHide: function () {
            clearInterval(interval);
        }
    });