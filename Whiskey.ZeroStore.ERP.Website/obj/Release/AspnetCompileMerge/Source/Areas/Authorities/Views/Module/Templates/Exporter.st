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
	<caption><h4>零时尚数据报表</h4><br></caption>
        <thead>
            <tr>
				<th>编号</th>
                <th>类别名称</th>
                <th>类别备注</th>
                <th>更新时间</th>
                <th>创建时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$it.Id$</td>
                <td>$it.WorkOrderCategoryName$</td>
                <td>$it.ModuleName$</td>
                <td>$it.Notes$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.CreatedTime$</td>
                <td>$it.Operator.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

