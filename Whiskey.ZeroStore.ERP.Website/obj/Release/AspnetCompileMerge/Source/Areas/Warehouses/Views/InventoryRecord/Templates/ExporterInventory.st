﻿




<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
	<style type="text/css" media="all">
        table{
            font-size:14px;
			text-align:center;
			line-height:25px;
			word-break: break-all;
			border:solid 1px rgb(149,119,205);
			border-collapse:collapse;
        }
		.thumbnail-wrapper{
			display:block;
			width:80px;
			height:80px;
			overflow:hidden;
			border:1px solid #eaeaea;
			margin:0 auto 0 auto;
		}
		.thumbnail-wrapper img{
			margin:2px;
			max-width:74px;
		}
    </style>
</head>
<body>
    <table width="100%" border="1">
	<caption><h4>零库房数据报表</h4><br></caption>
        <thead>
            <tr>
				<th>序号</th>
                <th>条码</th>
                <th>所属店铺</th>
                <th>所属仓库</th>
                <th>品牌</th>
                <th>款式</th>
                <th>季节</th>
                <th>尺码</th>
                <th>颜色</th>
                <th>入库时间</th>
                <th>操作人员</th>
               
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
                <td>$it.ProductBarcode$</td>
                <td>$it.StoreName$</td>
                <td>$it.StorageName$</td>
                <td>$it.BrandName$</td>
                <td>$it.CategoryName$</td>
                <td>$it.SeasonName$</td>
                <td>$it.SizeName$</td>
                <td>$it.ColorName$</td>
                <td>$it.CreatedTime$</td>
                <td>$it.MemberName$$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

