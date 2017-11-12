//全局变量
var numCount;       //数据总数量
var columnsCounts;  //数据列数量
var pageCount;      //每页显示的数量
var pageNum;        //总页数
var currPageNum;   //当前页数

//页面标签变量
var blockTable;
var preSpan;
var firstSpan;
var nextSpan;
var lastSpan;
var pageNumSpan;
var currPageSpan;

$(function () {
    //页面标签变量
    const tableId = "tblCampCreate";
    blockTable = document.getElementById(tableId);
    preSpan = document.getElementById("spanPre");
    firstSpan = document.getElementById("spanFirst");
    nextSpan = document.getElementById("spanNext");
    lastSpan = document.getElementById("spanLast");
    pageNumSpan = document.getElementById("spanTotalPage");
    currPageSpan = document.getElementById("spanPageNum");
    numCount = (document.getElementById(tableId) as HTMLTableElement).rows.length - 1;       //取table的行数作为数据总数量（减去标题行1）
    columnsCounts = blockTable.rows[0].cells.length;
    pageCount = 10;
    pageNum = parseInt((numCount / pageCount).toString());
    if (0 != numCount % pageCount) {
        pageNum += 1;
    }

    firstPage();
});


function firstPage() {
    hide();
    currPageNum = 1;                //默认刚加载显示第一页，所以=1;
    reloadTable();
}

function prePage() {
    hide();
    currPageNum--;
    reloadTable();
}

function nextPage() {
    hide();
    currPageNum++;
    reloadTable();
}

function lastPage() {
    hide();
    currPageNum = pageNum;
    reloadTable();
}

// 计算将要显示的页面的首行和尾行
function firstRow(currPageNum) {
    return pageCount * (currPageNum - 1) + 1;
}

function lastRow(firstRow) {
    var lastRow = firstRow + pageCount;
    if (lastRow > numCount + 1) {
        lastRow = numCount + 1;
    }
    return lastRow;
}

function showCurrPage(cpn) {
    currPageSpan.innerHTML = cpn;
}

function showTotalPage() {
    pageNum = parseInt((numCount / pageCount).toString());
    if (0 != numCount % pageCount) {
        pageNum += 1;
    }
    pageNumSpan.innerHTML = pageNum;
}

//隐藏所有行
function hide() {
    for (var i = 1; i < numCount + 1; i++) {
        blockTable.rows[i].style.display = "none";
    }
}

//控制首页等功能的显示与不显示
function firstLink() { firstSpan.innerHTML = "<a href='javascript:firstPage();'>首页</a>"; }
function firstText() { firstSpan.innerHTML = "首页"; }

function preLink() { preSpan.innerHTML = "<a href='javascript:prePage();'>上一页</a>"; }
function preText() { preSpan.innerHTML = "上一页"; }

function nextLink() { nextSpan.innerHTML = "<a href='javascript:nextPage();'>下一页</a>"; }
function nextText() { nextSpan.innerHTML = "下一页"; }

function lastLink() { lastSpan.innerHTML = "<a href='javascript:lastPage();'>末页</a>"; }
function lastText() { lastSpan.innerHTML = "末页"; }

function reloadTable() {
    showCurrPage(currPageNum);      //当前显示第几页方法
    showTotalPage();                    //重新计算总页数
    var firstR = firstRow(currPageNum);
    var lastR = lastRow(firstR);
    hide();
    for (var i = firstR; i < lastR; i++) {
        blockTable.rows[i].style.display = "";
    }
    if (lastR - firstR == 0 && firstR != 1) {
        prePage();
    } else if (lastR - firstR == 0 && firstR == 1) {
        return;
    }

    if (1 == currPageNum && numCount <= pageCount) {//在第一页并且条目数<每页的规定的数量时
        preText();
        firstText();
        nextText();
        lastText();
    } else if (1 == currPageNum) { //在第一页
        firstText();
        preText();
        nextLink();
        lastLink();
    }
    else if (pageNum == currPageNum) {    //最后一页
        preLink();
        firstLink();
        nextText();
        lastText();
    } else {
       
            firstLink();
            preLink();
            nextLink();
            lastLink();
        
    }
}
