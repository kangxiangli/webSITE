/*
 * 倒计时动画
 */
; (function ($) {
    $.fn.extend({
        djs: function (fn, seconds, content, content2) {
            djs(this, fn, seconds, content, content2);
        }
    });

    function djs(wrapper, fn, seconds, content, content2) {
        var rnid = "_canvas_djs_" + ($("canvas").length + 1);
        var Time = new Date();
        var endtime = Time.getTime() + 1000 * ((seconds || 60) + 1);
        Time.setTime(endtime);
        var end = 9999;
        $canvas_djs = $('<canvas id="' + rnid + '"></canvas>');
        $(wrapper).append($canvas_djs);
        var WINDOW_WIDTH = 920;
        var WINDOW_HEIGHT = 400;
        var RADIUS = 7; //球半径
        var NUMBER_GAP = 10; //数字之间的间隙
        var u = 0.65; //碰撞能量损耗系数
        var context; //Canvas绘制上下文
        var balls = []; //存储彩色的小球
        const colors = ["#33B5E5", "#0099CC", "#AA66CC", "#9933CC", "#99CC00", "#669900", "#FFBB33", "#FF8800", "#FF4444", "#CC0000"]; //彩色小球的颜色
        var currentNums = []; //屏幕显示的8个字符
        var digit = [
            [
                [0, 0, 1, 1, 1, 0, 0],
                [0, 1, 1, 0, 1, 1, 0],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 1, 1, 0, 1, 1, 0],
                [0, 0, 1, 1, 1, 0, 0]
            ], //0
            [
                [0, 0, 0, 1, 1, 0, 0],
                [0, 1, 1, 1, 1, 0, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [1, 1, 1, 1, 1, 1, 1]
            ], //1
            [
                [0, 1, 1, 1, 1, 1, 0],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 1, 1, 0, 0, 0],
                [0, 1, 1, 0, 0, 0, 0],
                [1, 1, 0, 0, 0, 0, 0],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 1, 1, 1, 1, 1]
            ], //2
            [
                [1, 1, 1, 1, 1, 1, 1],
                [0, 0, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 1, 1, 1, 0, 0],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 1, 1, 1, 1, 1, 0]
            ], //3
            [
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 1, 1, 1, 0],
                [0, 0, 1, 1, 1, 1, 0],
                [0, 1, 1, 0, 1, 1, 0],
                [1, 1, 0, 0, 1, 1, 0],
                [1, 1, 1, 1, 1, 1, 1],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 1, 1, 1, 1]
            ], //4
            [
                [1, 1, 1, 1, 1, 1, 1],
                [1, 1, 0, 0, 0, 0, 0],
                [1, 1, 0, 0, 0, 0, 0],
                [1, 1, 1, 1, 1, 1, 0],
                [0, 0, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 1, 1, 1, 1, 1, 0]
            ], //5
            [
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 1, 1, 0, 0, 0],
                [0, 1, 1, 0, 0, 0, 0],
                [1, 1, 0, 0, 0, 0, 0],
                [1, 1, 0, 1, 1, 1, 0],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 1, 1, 1, 1, 1, 0]
            ], //6
            [
                [1, 1, 1, 1, 1, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 0, 1, 1, 0, 0, 0],
                [0, 0, 1, 1, 0, 0, 0],
                [0, 0, 1, 1, 0, 0, 0],
                [0, 0, 1, 1, 0, 0, 0]
            ], //7
            [
                [0, 1, 1, 1, 1, 1, 0],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 1, 1, 1, 1, 1, 0],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 1, 1, 1, 1, 1, 0]
            ], //8
            [
                [0, 1, 1, 1, 1, 1, 0],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [1, 1, 0, 0, 0, 1, 1],
                [0, 1, 1, 1, 0, 1, 1],
                [0, 0, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 0, 1, 1],
                [0, 0, 0, 0, 1, 1, 0],
                [0, 0, 0, 1, 1, 0, 0],
                [0, 1, 1, 0, 0, 0, 0]
            ], //9
            [
                [0, 0, 0, 0],
                [0, 0, 0, 0],
                [0, 1, 1, 0],
                [0, 1, 1, 0],
                [0, 0, 0, 0],
                [0, 0, 0, 0],
                [0, 1, 1, 0],
                [0, 1, 1, 0],
                [0, 0, 0, 0],
                [0, 0, 0, 0]
            ] //:
        ];

        function drawDatetime(cxt) {
            var nums = [];

            context.fillStyle = "#005eac"
            var date = new Date();
            var offsetX = WINDOW_WIDTH / 10,
                offsetY = 30;
            var NowTime = new Date();
            var t = Time.getTime() - NowTime.getTime();
            end = t;
            var d = 0,
                h = 0,
                m = 0,
                s = 0;
            if (t >= 0) {
                d = Math.floor(t / 1000 / 60 / 60 / 24);
                var num1 = Math.floor(d / 10);
                var num2 = d % 10;
                h = Math.floor(t / 1000 / 60 / 60 % 24);
                var num1 = Math.floor(h / 10);
                var num2 = h % 10;
                nums.push({
                    num: num1
                });
                nums.push({
                    num: num2
                });
                nums.push({
                    num: 10
                }); //冒号
                m = Math.floor(t / 1000 / 60 % 60);
                var num1 = Math.floor(m / 10);
                var num2 = m % 10;
                nums.push({
                    num: num1
                });
                nums.push({
                    num: num2
                });
                nums.push({
                    num: 10
                }); //冒号
                s = Math.floor(t / 1000 % 60);
                var num1 = Math.floor(s / 10);
                var num2 = s % 10;
                nums.push({
                    num: num1
                });
                nums.push({
                    num: num2
                });
            }

            for (var x = 0; x < nums.length; x++) {
                nums[x].offsetX = offsetX;
                offsetX = drawSingleNumber(offsetX, offsetY, nums[x].num, cxt);
                //两个数字连一块，应该间隔一些距离
                if (x < nums.length - 1) {
                    if ((nums[x].num != 10) && (nums[x + 1].num != 10)) {
                        offsetX += NUMBER_GAP;
                    }
                }
            }

            //说明这是初始化
            if (currentNums.length == 0) {
                currentNums = nums;
            } else {
                //进行比较
                for (var index = 0; index < currentNums.length; index++) {
                    if (nums[index] && (currentNums[index].num != nums[index].num)) {
                        //不一样时，添加彩色小球
                        addBalls(nums[index]);
                        currentNums[index].num = nums[index].num;
                    }
                }
            }
            renderBalls(cxt);
            updateBalls();
            return date;
        }

        function addBalls(item) {
            var num = item.num;
            var numMatrix = digit[num];
            for (var y = 0; y < numMatrix.length; y++) {
                for (var x = 0; x < numMatrix[y].length; x++) {
                    if (numMatrix[y][x] == 1) {
                        var ball = {
                            offsetX: item.offsetX + RADIUS + RADIUS * 2 * x,
                            offsetY: 30 + RADIUS + RADIUS * 2 * y,
                            color: colors[Math.floor(Math.random() * colors.length)],
                            g: 1.5 + Math.random(),
                            vx: Math.pow(-1, Math.ceil(Math.random() * 10)) * 4 + Math.random(),
                            vy: -5
                        }
                        balls.push(ball);
                    }
                }
            }
        }

        function renderBalls(cxt) {

            for (var index = 0; index < balls.length; index++) {
                cxt.beginPath();
                cxt.fillStyle = balls[index].color;
                cxt.arc(balls[index].offsetX, balls[index].offsetY, RADIUS, 0, 2 * Math.PI);
                cxt.fill();
            }
        }

        function updateBalls() {
            var i = 0;
            for (var index = 0; index < balls.length; index++) {
                var ball = balls[index];
                ball.offsetX += ball.vx;
                ball.offsetY += ball.vy;
                ball.vy += ball.g;
                if (ball.offsetY > (WINDOW_HEIGHT - RADIUS)) {
                    ball.offsetY = WINDOW_HEIGHT - RADIUS;
                    ball.vy = -ball.vy * u;
                }
                if (ball.offsetX > RADIUS && ball.offsetX < (WINDOW_WIDTH - RADIUS)) {

                    balls[i] = balls[index];
                    i++;
                }
            }
            //去除出边界的球
            for (; i < balls.length; i++) {
                balls.pop();
            }
        }

        function drawSingleNumber(offsetX, offsetY, num, cxt) {
            var numMatrix = digit[num];
            for (var y = 0; y < numMatrix.length; y++) {
                for (var x = 0; x < numMatrix[y].length; x++) {
                    if (numMatrix[y][x] == 1) {
                        cxt.beginPath();
                        cxt.arc(offsetX + RADIUS + RADIUS * 2 * x, offsetY + RADIUS + RADIUS * 2 * y, RADIUS, 0, 2 * Math.PI);
                        cxt.fill();
                    }
                }
            }
            cxt.beginPath();
            offsetX += numMatrix[0].length * RADIUS * 2;
            return offsetX;
        }

        var canvas = $canvas_djs[0];
        canvas.width = WINDOW_WIDTH;
        canvas.height = WINDOW_HEIGHT;
        context = canvas.getContext("2d");

        //记录当前绘制的时刻
        var currentDate = new Date();

        var preBeginTime = 0;

        var timer1 = setInterval(function () {
            var thisBeginTime = new Date().getTime();
            if (thisBeginTime - preBeginTime > 900) {
                for (var x = 0; x < 20; x++) {
                    render()
                }
            } else {
                render()
            }
            if (content != content2 && content2 != null && content2.length > 0 && end < 5000) {
                content = content2;
            }
            //倒计时结束
            if (end < 1500) {
                if (fn) fn();
                clearInterval(timer1);
            }
            preBeginTime = new Date().getTime();
        }, 45)

        function render() {
            //清空整个Canvas，重新绘制内容
            context.clearRect(0, 0, context.canvas.width, context.canvas.height);
            context.font = "bold 40px '字体','字体','微软雅黑','宋体'"; //设置字体
            context.textBaseline = 'hanging'; //在绘制文本时使用的当前文本基线
            context.fillStyle = '#99CC00';
            context.textAlign = 'center';//文本水平对齐方式
            context.fillText(content, WINDOW_WIDTH / 2, 220); //设置文本内容
            drawDatetime(context);
        }
    }

})(jQuery);