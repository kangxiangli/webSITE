/**
 * Created by xiuxia on 2016/10/19.
 */
$(function () {
    $(".shopAccount .til_dk .back_befo").click(function () {
        window.location.href = "shujutongji.html";
    });

    $(".dateTim>div").click(function () {
        $(this).addClass("whiteBg");
        $(this).siblings().removeClass("whiteBg");
    });


    $('.swiper-container').height($(window).height() - $("#til_dk").height() - $(".shopDate").height());

    var swiper = new Swiper('.swiper-container', {
        pagination: '.swiper-pagination',
        paginationClickable: true,
        direction: 'vertical',
        onSlideChangeEnd: function (swiper) {
            hasClass();
        }
    });

    shopProfit();  //店铺盈利
    //水费
    var wh = $(".jinErK").width();
    $(".jinErK").height(wh);
    $(".dateQiu li,.pingPang li").width(wh / 7);
    var qoiQh = $(".qiuQ").width();
    $(".qiuQ").height(qoiQh);
    //水费 end

    //电费
    var hy = $(".huiyuan").width();
    $(".huiyuan").height(hy);
    $(".huiy_month li,.energy_img>li").height(hy / 7).css("line-height", hy / 7 + "px");

    //房租
    var whCZ = $(".chongZhi").width();
    $(".chongZhi,.montUl,.chongZhi .peopleUl,.moneyUl").height(whCZ);
    $(".montUl li,.chongZhi .peopleUl li,.peopleUl li b,.moneyUl li").height(whCZ / 7).css("line-height", whCZ / 7 + "px");
    //房租
    //工资
    var th = $(".tH_box").width();
    $(".tH_box").height(th);
    $(".month,.pillar").width(th);
    $(".month li,.pillar li i").width(th / 7);
    $(".pillar li").width(th / 14).css("margin-left", th / 28).css("margin-right", th / 28);
    $(".pillar li i").css("margin-left", -th / 14);
    $(".pillar li span").css("border-width", th / 28);
    //工资 end

    //   店铺盈利
    function shopProfit(){
        var myChart = echarts.init(document.getElementById('main'));
        myChart.setOption({
            tooltip: {
                trigger: 'axis',
                showDelay: 0 // 显示延迟，添加显示延迟可以避免频繁切换，单位ms
            },

            xAxis: [
                {
                    type: 'category', /*
                 name: '月/2016',  //坐标轴名字
                 nameLocation: 'end', //名字位置*/
                    axisLine: {
                        lineStyle: {
                            color: "rgba(255,255,255,0.1)"
                        }
                    },
                    data: ["2", "3", "4", "5", "6", "7", "8"],
                    axisLabel: {
                        textStyle: {
                            fontSize: '16px'  //刻度大小
                        }
                    },
                    axisTick: {
                        show: false
                    },
                    splitLine: {//终于找到了，背景图的内置表格中“边框”的颜色线条  这个是x轴的竖线
                        show: true,
                        lineStyle: {
                            color: "rgba(255,255,255,0.1)"
                        }
                    }
                }

            ],
            grid: { // 控制图的大小，调整下面这些值就可以，
                x: 50,
                x2: 65
            },

            yAxis: [
                {
                    type: 'value',
                    //name: '件',  //坐标轴名字
                    //nameLocation: 'end', //位置
                    splitNumber: 5,
                    axisLabel: {
                        textStyle: {
                            fontSize: 15  //刻度大小
                        }
                    },
                    axisLine: {
                        lineStyle: {
                            color: "rgba(255,255,255,0.1)"
                        }

                    },
                    splitLine: {//终于找到了，背景图的内置表格中“边框”的颜色线条  这个是y轴的竖线
                        show: true,
                        lineStyle: {
                            color: "rgba(255,255,255,0.1)"
                        }
                    },
                    axisTick: {
                        show: false
                    }

                }

            ],
            series: [
                {
                    "name": "盈利",
                    "type": "bar",
                    "data": [82, 52, 62, 72, 100, 82, 113],
                    'barWidth': 20//柱图宽度
                }
            ],
            itemStyle: {
                normal: {
                    // 设置柱形的颜色
                    color: function (params) {
                        // build a color map as your need.
                        var colorList = [
                            'rgba(255,255,255,0.7)'
                        ];
                        return colorList[params.dataIndex]
                    }
                }
            },
            textStyle: {
                color: 'rgba(255, 255, 255, 1)'
            }
        });
    }

    var slidLg = $(".swiper-slide").length;

    function hasClass() {
        for (var i = 0; i < slidLg; i++) {
            if ($(".swiper-slide").eq(i).hasClass('swiper-slide-active')) {
                //   店铺盈利
                if (i == 0) {

                    shopProfit();
                } // 店铺盈利     
                //水费
                else if (i == 1) {
                    var waterRate = [110, 110, 110, 160, 155, 130, 155];//定义一个空数组进行存放排序后的数组;
                    var maxhuiy = Math.max.apply(null, waterRate);  //求得数组中最大的数字
                    $(".water_img").empty();
                    var pli, pimg, linum, pilend;

                    if (maxhuiy <= 10) {
                        addImg(1);
                    } else if (maxhuiy <= 20) {
                        addImg(2);
                    } else if (maxhuiy <= 30) {
                        addImg(3);
                    } else if (maxhuiy <= 40) {
                        addImg(4);
                    } else if (maxhuiy <= 50) {
                        addImg(5);
                    } else if (maxhuiy <= 60) {
                        addImg(6);
                    } else if (maxhuiy <= 70) {
                        addImg(7);
                    } else if (maxhuiy <= 80) {
                        addImg(8);
                    } else if (maxhuiy <= 90) {
                        addImg(9);
                    } else if (maxhuiy <= 100) {
                        addImg(10);
                    } else if (maxhuiy <= 200) {
                        addImg(20);
                    } else if (maxhuiy <= 300) {
                        addImg(30);
                    } else {
                        addImg(50);
                    }

                    function addImg(ratio) {
                        for (var k = 0; k < waterRate.length; k++) {
                            var peo = waterRate[k];
                            pli = '<li><ul class="liImg"><li>';
                            pilend = '</li></ul></li>';
                            linum = '<li class="numNum">' + peo + '</li>';
                            for (var t = 0; t < peo / ratio; t++) {
                                pimg = '<img src="shujimg/s@2x.png" alt="png"/>';
                                pli += pimg;
                            }
                            $(".water_img").append(pli + linum + pilend);
                        }
                    }

                    var sliTim = 50;
                    for (var i = 0; i < $(".water_img li img").length; i++) {
                        $(".water_img .liImg li").eq(i).delay(sliTim).animate({"opacity": 1});
                        sliTim += 100;
                    }
                    var waterRt = waterRate.sort(function (a, b) {
                        return b - a;
                    });
                    for (var o = 0; o < waterRate.length; o++) {
                        var liNum = $(".water_img .numNum").eq(o).html();
                        //console.log(liNum, waterRate[o]);
                        if (waterRate[0] == liNum) {
                            $(".water_img .numNum").eq(o).parent().parent().css("opacity", 1);
                        } else if (waterRate[1] == liNum || waterRate[2] == liNum) {
                            $(".water_img .numNum").eq(o).parent().parent().css("opacity", 0.75);
                        } else if (waterRate[3] == liNum || waterRate[4] == liNum) {
                            $(".water_img .numNum").eq(o).parent().parent().css("opacity", 0.5);
                        } else if (waterRate[5] == liNum || waterRate[6] == liNum) {
                            $(".water_img .numNum").eq(o).parent().parent().css("opacity", 0.3);
                        }
                    }
                    $(".huiyuan").height(hy);
                    $(".huiy_month li,.water_img>li").height(hy / 7).css("line-height", hy / 7 + "px");
                }  // 水费
                //电费
                else if (i == 2) {
                    var energyCharge = [1000, 400, 3000,2000,500,300,600];
                    var electy = Math.max.apply(null, energyCharge);  //求得数组中最大的数字
                    $(".energy_img").empty();
                    if (electy <= 1000) {
                        electricity(0.15);
                    } else if (electy <= 2000) {
                        electricity(0.08);
                    } else if (electy <= 3000) {
                        electricity(0.05);
                    } else if (electy <= 4000) {
                        electricity(0.03);
                    } else if (electy <= 5000) {
                        electricity(0.03);
                    } else {
                        electricity(0.01);
                    }

                    function electricity(ratio) {
                        for (var k = 0; k < energyCharge.length; k++) {
                            var enry = energyCharge[k],
                                //ty = parseFloat(enry/1000).toFixed(1),
                                ty = enry/1000,
                                pUl = '<li><ul class="energyLi">',
                                pulend = '</ul></li>',
                                ulLi  ='<li class="elect"><span></span><img src="shujimg/inrg.png" alt="png"/></li>',
                                liNum = '<li class="enryCha">' + ty+' k' + '</li>';
                            $(".energy_img").append(pUl + ulLi + liNum + pulend);
                        }
                        for(var t = 0;t < energyCharge.length; t++){
                            $(".elect span").eq(t).animate({'width':energyCharge[t]*ratio},500);
                        }
                    }

                    var sldTime = 50;
                    for (var i = 0; i < $(".energy_img li img").length; i++) {
                        $(".energy_img .eley li").eq(i).delay(sldTime).animate({"opacity": 1});
                        sldTime += 100;
                    }
                    var eneChe = energyCharge.sort(function (a, b) {
                        return a - b;
                    });
                    for (var q = 0; q < energyCharge.length; q++) {
                        var enK = $(".energy_img .enryCha").eq(q).html().replace(' k','');
                        //console.log(energe, energyCharge[q]);
                        var energe= enK * 1000;
                        if (energyCharge[0] == energe) {
                            $(".energy_img .enryCha").eq(q).parent().parent().css("opacity", 1);
                        } else if (energyCharge[1] == energe || energyCharge[2] == energe) {
                            $(".energy_img .enryCha").eq(q).parent().parent().css("opacity", 0.75);
                        } else if (energyCharge[3] == energe || energyCharge[4] == energe) {
                            $(".energy_img .enryCha").eq(q).parent().parent().css("opacity", 0.5);
                        } else if (energyCharge[5] == energe || energyCharge[6] == energe) {
                            $(".energy_img .enryCha").eq(q).parent().parent().css("opacity", 0.3);
                        }
                    }
                    $(".energy_img").height(hy);
                    $(".energy_img>li").height(hy / 7).css({"line-height": hy / 7 + "px"});
                    $(".energyLi>li").height(hy / 14).css({"line-height":hy / 14 + "px","margin-top":$(".energyLi>li").height() / 2+ "px"});
                    $(".energyLi li span,.energyLi li img").height(hy / 14-4+'px');

                   /* var energyCharge = [110, 120, 130,110, 170, 160, 130];
                    var electy = Math.max.apply(null, energyCharge);  //求得数组中最大的数字
                    $(".energy_img").empty();
                    var pli, pimg, linum, pilend;

                    if (electy <= 10) {
                        electricity(1);
                    } else if (electy <= 20) {
                        electricity(2);
                    } else if (electy <= 30) {
                        electricity(3);
                    } else if (electy <= 40) {
                        electricity(4);
                    } else if (electy <= 50) {
                        electricity(5);
                    } else if (electy <= 60) {
                        electricity(6);
                    } else if (electy <= 70) {
                        electricity(7);
                    } else if (electy <= 80) {
                        electricity(8);
                    } else if (electy <= 90) {
                        electricity(9);
                    } else if (electy <= 100) {
                        electricity(10);
                    } else if (electy <= 200) {
                        electricity(20);
                    } else if (electy <= 300) {
                        electricity(30);
                    } else {
                        electricity(50);
                    }

                    function electricity(ratio) {
                        for (var k = 0; k < energyCharge.length; k++) {
                            var enry = energyCharge[k];
                            pli = '<li><ul class="eley"><li>';
                            pilend = '</li></ul></li>';
                            linum = '<li class="enryCha">' + enry + '</li>';
                            for (var t = 0; t < enry / ratio; t++) {
                                pimg = '<img src="shujimg/d@2x.png" alt="png"/>';
                                pli += pimg;
                            }
                            $(".energy_img").append(pli + linum + pilend);
                        }
                    }

                    var sldTime = 50;
                    for (var i = 0; i < $(".energy_img li img").length; i++) {
                        $(".energy_img .eley li").eq(i).delay(sldTime).animate({"opacity": 1});
                        sldTime += 100;
                    }
                    var eneChe = energyCharge.sort(function (a, b) {
                        return b - a;
                    });
                    for (var q = 0; q < energyCharge.length; q++) {
                        var energe = $(".energy_img .enryCha").eq(q).html();
                        //console.log(energe, energyCharge[q]);
                        if (energyCharge[0] == energe) {
                            $(".energy_img .enryCha").eq(q).parent().parent().css("opacity", 1);
                        } else if (energyCharge[1] == energe || energyCharge[2] == energe) {
                            $(".energy_img .enryCha").eq(q).parent().parent().css("opacity", 0.75);
                        } else if (energyCharge[3] == energe || energyCharge[4] == energe) {
                            $(".energy_img .enryCha").eq(q).parent().parent().css("opacity", 0.5);
                        } else if (energyCharge[5] == energe || energyCharge[6] == energe) {
                            $(".energy_img .enryCha").eq(q).parent().parent().css("opacity", 0.3);
                        }
                    }
                    $(".huiyuan").height(hy);
                    $(".huiy_month li,.energy_img>li").height(hy / 7).css("line-height", hy / 7 + "px");*/
                }  // 电费
                //房租
                else if (i == 3) {
                    var myChart2 = echarts.init(document.getElementById('main2'));
                    myChart2.setOption({
                        tooltip: {
                            trigger: 'axis',
                            showDelay: 0 // 显示延迟，添加显示延迟可以避免频繁切换，单位ms
                        },
                        grid: {
                            x: 50,
                            y: 50,
                            x2: 70,
                            y2: 100
                        },
                        xAxis: [
                            {
                                type: 'category',
                                data: ["2", "3", "4", "5", "6", "7", "8"],
                                axisLine: {//x轴的横坐标边框线
                                    lineStyle: {
                                        color: "rgba(255,255,255,0.6)"
                                    }
                                },
                                axisTick: {
                                    show: false
                                },
                                boundaryGap: false,  //从0开始  true不从0开始
                                splitLine: {//终于找到了，背景图的内置表格中“边框”的颜色线条  这个是x轴的竖线
                                    show: true,
                                    lineStyle: {
                                        color: "rgba(255,255,255,0.1)"
                                    }
                                }
                            }
                        ],
                        yAxis: [
                            {
                                type: 'value',
                                splitNumber: 10,
                                axisLine: {//x轴的横坐标边框线
                                    lineStyle: {
                                        color: "rgba(255,255,255,0.6)"
                                    }
                                },
                                axisTick: {
                                    show: false
                                },
                                splitLine: {//终于找到了，背景图的内置表格中“边框”的颜色线条   这个是y轴的横线
                                    show: true,
                                    lineStyle: {
                                        color: "rgba(255,255,255,0.1)"
                                    }
                                }
                            }
                        ],
                        series: [
                            {
                                name: '金额',
                                type: 'line',
                                /*  smooth:true,//表示折线图的气泡smooth:true,不显示，false显示 symbol: 'none'
                                 symbol: 'none',*/
                                show: true,
                                lineStyle: {//折线的颜色
                                    normal: {
                                        color: "#fff",
                                        width: 0  //不显示折线
                                    }
                                },
                                areaStyle: {
                                    normal: {
                                        color: new echarts.graphic.LinearGradient(0, 0, 0, 1, [{
                                            offset: 0,
                                            color: 'rgba(255, 255, 255, 0.9)'  //渐变色填充 开始值
                                        }, {
                                            offset: 1,
                                            color: 'rgba(255, 255, 255, 0.1)' //渐变色填充  结束值
                                        }])
                                    }
                                },
                                itemStyle: {
                                    normal: {
                                        borderWidth: '12',
                                        color: "rgba(255,255,255,0.5)"
                                    }
                                },
                                data: ["20", "23", "43", "25", "16", "27", "81"]
                            }
                        ],
                        textStyle: {
                            color: 'rgba(255, 255, 255, 1)'
                        }
                    });
                }  // 房租
                //工资
                else if (i == 4) {

                    $(".pillar li i,.pillar li b,.pillar li span,.pillar li").hide();
                    $(".pillar li span,.pillar li b").animate({"margin-top": 0});
                    $(".pillar li i").css("margin-left", -th / 14).css("margin-top", 0);
                    $(".pillar li").height(0);

                    var arry = [10000, 9000, 4000, 1800, 3900, 1100, 2500];

                    var maxArr = Math.max.apply(null, arry);
                    for (var i = 0; i < arry.length; i++) {
                        if (maxArr > 0 && maxArr <= 10000) {
                            hgt(i, 0.01);
                        } else if (maxArr > 10000 && maxArr <= 20000) {
                            hgt(i, 0.008);
                        } else if (maxArr > 20000 && maxArr <= 30000) {
                            hgt(i, 0.006);
                        } else if (maxArr > 30000 && maxArr <= 40000) {
                            hgt(i, 0.004);
                        } else if (maxArr > 40000 && maxArr <= 50000) {
                            hgt(i, 0.002);
                        }
                    }
                    function hgt(id, hg) {
                        $(".pillar li").eq(id).animate({"height": arry[i] * hg},400).show();
                        $(".pillar li span").eq(id).animate({"margin-top": arry[i] * hg + 2},500).show();
                        $(".pillar li b").eq(id).animate({"margin-top": arry[i] * hg + th / 28 + 6},500).show();
                        var gz = arry[id]/1000;
                        $(".pillar li i").eq(id).html(gz+"k").animate({"margin-top":arry[i] * hg + th / 28 + 10},1100).show();
                    }

                    var sortMax = arry.sort(function (a, b) {
                        return b - a;
                    });
                    for (var q = 0; q < arry.length; q++) {
                        var gzNum = $(".pillar li i").eq(q).html().replace('k',''),
                            liNum = gzNum*1000;
                        if (arry[0] == liNum) {
                            $(".pillar li i").eq(q).parent().css("opacity", 1);
                        } else if (arry[1] == liNum || arry[2] == liNum) {
                            $(".pillar li i").eq(q).parent().css("opacity", 0.75);
                        } else if (arry[3] == liNum || arry[4] == liNum) {
                            $(".pillar li i").eq(q).parent().css("opacity", 0.5);
                        } else if (arry[5] == liNum || arry[6] == liNum) {
                            $(".pillar li i").eq(q).parent().css("opacity", 0.3);
                        }
                    }
                }  // 杂费
                //杂费
                else if (i == 5) {
                    var qiu = [6, 4, 2, 6, 3, 5, 3];
                    $(".point").empty();
                    for (var i = 0; i < qiu.length; i++) {
                        $(".vertical .inner").eq(i).html(qiu[i] + 'K');
                        addPoint(i, qiu[i]);
                    }
                    function addPoint(indx, num) {
                        var h = 0;
                        while (h < num * 2) {
                            $(".point").eq(indx).append("<b>.</b>");
                            h++;
                        }
                    }

                    var qiuNum = qiu.sort(function (a, b) {
                        return a - b;
                    });
                    for (var t = 0; t < qiu.length; t++) {
                        var nun = $(".vertical .inner").eq(t).html(),
                            nn = nun[0];
                        if (qiu[0] == nn) {
                            $('.qiuQ').eq(t).css("opacity", 1);
                            aa(t, 74);
                        } else if (qiu[1] == nn) {
                            $('.qiuQ').eq(t).css("opacity", 0.8);
                            aa(t, 80);
                        } else if (qiu[2] == nn) {
                            $('.qiuQ').eq(t).css("opacity", 0.8);
                            aa(t, 84);
                        } else if (qiu[3] == nn) {
                            $('.qiuQ').eq(t).css("opacity", 0.55);
                            aa(t, 88);
                        } else if (qiu[4] == nn) {
                            $('.qiuQ').eq(t).css("opacity", 0.55);
                            aa(t, 92);
                        } else if (qiu[5] == nn) {
                            $('.qiuQ').eq(t).css("opacity", 0.35);
                            aa(t, 96);
                        } else if (qiu[6] == nn) {
                            $('.qiuQ').eq(t).css("opacity", 0.35);
                            aa(t, 100);
                        }
                    }
                    function aa(ind, num) {
                        $(".pingPang li").show();
                        $('.qiuQ .out_iner').eq(ind).width(num + '%').height($('.qiuQ .out_iner').eq(ind).width());
                        $('.outer').eq(ind).height($(".outer").eq(ind).width());
                        $('.inner').eq(ind).height($(".inner").eq(ind).width()).css("line-height", $(".inner").eq(ind).width() + 'px');

                    }

                    pointOpact();
                    function pointOpact() {
                        var pont = $(".pingPang b").length, del = 10, lay = 15;
                        for (var g = 0; g < pont; g++) {
                            $(".point b").eq(g).delay(del).animate({"opacity": 0.2});
                            $(".point b").eq(g).delay(lay).animate({"opacity": 1});
                            del += 30;
                            lay += 10;
                        }
                        var out_in = $(".qiuQ .out_iner").length, oi = 320;
                        for (var w = 0; w < out_in; w++) {
                            $(".qiuQ .out_iner").eq(w).delay(oi).animate({"opacity": 0.3});
                            $(".qiuQ .out_iner").eq(w).delay(oi).animate({"opacity": 1});
                            oi += 100;
                        }
                    }
                }  // 工资
                //税金
                else if (i == 6) {
                    var wgbl, peoLiwh, maxMoney, monyWh, hhh;
                    //    var peoArry = [320000, 220000, 620000, 999000, 205000, 562000, 250000];
                    var peoArry = [32000, 22000, 62000, 99900, 20000, 56200, 25000];
                    //    var peoArry = [3200, 2200, 6200, 9900, 2250, 5620, 2500];
                    maxMoney = Math.max.apply(null, peoArry);
                    wgbl = whCZ / 14; //每个小网格所占比例  16
                    peoLiwh = 11 * wgbl; //UL的宽度
                    monyWh = 3 * wgbl; //金额ul 宽度
                    $(".chongZhi .peopleUl,.chongZhi .peopleUl li").width(peoLiwh);
                    $('.moneyUl').width(monyWh);
                    var dela = 100, spade = 600;
                    for (var i = 0; i < peoArry.length; i++) {
                        var liHg = peoArry[i];
                        if (maxMoney > 100000 && maxMoney < 1000000) {  //当天的充值金额
                            heg(7000);
                        }
                        if (maxMoney > 10000 && maxMoney < 100000) {  //当天的充值金额
                            heg(700);
                        }
                        if (maxMoney > 1000 && maxMoney < 10000) {  //当天的充值金额
                            heg(70);
                        }
                        var nmu = (Number(liHg / 1000)).toFixed(1);
                        $('.moneyUl li').eq(i).html(nmu + 'K');
                        dela += 100;
                        continue;
                    }
                    function heg(ht) {
                        $(".peopleUl li span").empty();
                        $(".peopleUl li b").width(0);
                        $(".chongZhi .moneyUl li").css("opacity", 0);
                        $(".chongZhi .peopleUl").find('li b').eq(i).delay(dela).animate({"width": liHg / ht});
                        $(".chongZhi .peopleUl").find('li span').eq(i).delay(dela + 50).animate({"width": peoLiwh - liHg / ht}, 200);
                        $(".chongZhi .moneyUl li").eq(i).delay(dela + 1000).animate({"opacity": 0.9});
                        hhh = (peoLiwh - liHg / ht) / wgbl * 2;
                    }

                    var delStart = 500;
                    for (var j = 0; j < hhh; j++) {
                        $(".chongZhi .peopleUl").find('li span').append('<i>.</i>');
                        $(".chongZhi .peopleUl").find('li span i').delay(delStart).animate({"opacity": 1});
                        delStart += 50;
                    }

                    $(".chongZhi .peopleUl li span,.chongZhi .peopleUl li span i").height(whCZ / 7).css("line-height", whCZ / 12 + "px");
                    $(".chongZhi .peopleUl li span i").width(wgbl / 2);
                }  // 税金
                //盈利
                else if (i == 7) {
                    var profit = echarts.init(document.getElementById('profit'));
                    profit.setOption({
                        tooltip: {
                            trigger: 'axis'
                        },
                        xAxis: [
                            {
                                type: 'value',
                                splitNumber: 6,
                                boundaryGap: [0, 0.01],
                                axisLabel: {
                                    textStyle: {
                                        fontSize: '16px'  //刻度大小
                                    }
                                },
                                axisLine:{
                                    lineStyle: {
                                        color: "rgba(255,255,255,0.1)"
                                    }
                                },
                                axisTick: {
                                    show: false
                                },
                                splitLine: {//终于找到了，背景图的内置表格中“边框”的颜色线条  这个是x轴的竖线
                                    show: true,
                                    lineStyle: {
                                        color: "rgba(255,255,255,0.1)"
                                    }
                                }
                            }
                        ],
                        yAxis: [
                            {
                                type: 'category',
                                data: ['2', '3', '4', '5', '6', '7', '8'],
                                axisLine: {
                                    lineStyle: {
                                        color: "rgba(255,255,255,0.1)"
                                    }
                                },
                                axisTick: {
                                    show: false
                                },
                                splitLine: {//终于找到了，背景图的内置表格中“边框”的颜色线条  这个是y轴的竖线
                                    show: true,
                                    lineStyle: {
                                        color: "rgba(255,255,255,0.1)"
                                    }
                                }
                            }
                        ],

                        series: [
                            {
                                name: '盈利',
                                type: 'bar',
                                data: [1002, 1222, 2034, 1049, 1314, 630, 431],
                                barWidth: 15//柱图宽度
                            }

                        ],
                        itemStyle: {
                            normal: {
                                // 设置柱形的颜色
                                color: function (params) {
                                    // build a color map as your need.
                                    var colorList = [
                                        'rgba(255,255,255,0.6)'
                                    ];
                                    return colorList[params.dataIndex]
                                }
                            }
                        },
                        textStyle: {
                            color: 'rgba(255, 255, 255, 1)'
                        }
                    })
                }  // 盈利
            }
        }
    }
});
