

var $DataHelper = new Object();

$DataHelper.nowDate = new Date();
$DataHelper.year = $DataHelper.nowDate.getFullYear();
$DataHelper.month = $DataHelper.nowDate.getMonth() + 1;
$DataHelper.day = $DataHelper.nowDate.getDate();
$DataHelper.seperator1 = "-";
$DataHelper.seperator2 = ":";

$DataHelper.longMonth = function () {
    var monthStr = this.month;
    if (this.month >= 1 && this.month <= 9) {
        monthStr = "0" + this.month;
    }
    return monthStr;
};

$DataHelper.getNowFormatDate = function (seperator, IsDate) {
    //获取当前的日期时间 格式“yyyy-MM-dd HH:MM:SS”
    var monthStr = this.month;
    var dayStr = this.day;
    if (this.month >= 1 && this.month <= 9) {
        monthStr = "0" + this.month;
    }
    if (this.day >= 0 && this.day <= 9) {
        dayStr = "0" + this.day;
    }
    var currentdate = this.year + seperator + monthStr + seperator + dayStr
    if (!IsDate) {
        currentdate = currentdate + " " + this.nowDate.getHours() + this.seperator2 + this.nowDate.getMinutes()
        + this.seperator2 + this.nowDate.getSeconds();
    }
    return currentdate;
};

//比较时间大小
$DataHelper.CompareDate = function (strDate, EndDate) {
    return ((new Date(strDate.replace(/-/g, "\/"))) >= (new Date(EndDate.replace(/-/g, "\/"))));
}

//将 2016年7月 获取 年或月
$DataHelper.getYearOrMonth = function (formatStr, type) {
    formatStr = formatStr || "";
    var returnStr = "";
    if (formatStr != "") {
        formatStr = formatStr.replace("年", "|").replace("月", "");
        var arry = formatStr.split("|");
        if (type == 1) {
            returnStr = arry[0];
        } else {
            returnStr = arry[1];
        }
    }
    return returnStr;
}



