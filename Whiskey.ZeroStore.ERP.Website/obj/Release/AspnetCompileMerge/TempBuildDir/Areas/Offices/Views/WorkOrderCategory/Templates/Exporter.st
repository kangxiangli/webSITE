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
                <th>父级模块</th>
                <th>模块名称</th>
                <th>模块图标</th>
                <th>模块类型</th>
                <th>模块简介</th>
                <th>页面路径</th>
                <th>页面区域</th>
                <th>页面控制器</th>
                <th>页面方法</th>
                <th>点击脚本</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
                <td>$it.ParentId$</td>
                <td>$it.ModuleName$</td>
                <td>$it.Icon$</td>
                <td>$it.ModuleType$</td>
                <td>$it.Description$</td>
                <td>$it.PageUrl$</td>
                <td>$it.PageArea$</td>
                <td>$it.PageController$</td>
                <td>$it.PageAction$</td>
                <td>$it.onClickScripts$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Operator.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

