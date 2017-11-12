




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
				<th>编号</th>
                <th>盘点店铺</th>
                <th>所属仓库</th>
                <th>品牌</th>
                <th>品类</th>
                <th>库存</th>
                <th>总盘</th>
                <th>有效</th>
                <th>缺货</th>
                <th>余货</th>
                <th>时间</th>
                <th>盘点状态（0失败1盘点中2中断3盘点完成4完成校对-1其它）</th>
                <th>操作人</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.StoreName$</td>
                <td>$it.StorageName$</td>
                <td>$it.BrandName$</td>
                <td>$it.CategoryName$</td>
                <td>$it.BeforeCheckQuantity$</td>
                <td>$it.CheckedQuantity$</td>
                <td>$it.ValidCount$</td>
                <td>$it.MissingCount$</td>
                <td>$it.ResidueCount$</td>
                <td>$it.CreatedTime$</td>
                <td>$it.CheckerState$</td>
                <td>$it.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

