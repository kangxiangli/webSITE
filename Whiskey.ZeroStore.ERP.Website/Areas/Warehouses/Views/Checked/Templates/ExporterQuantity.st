




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
                <th>商品名称</th>
                <th>商品条码</th>
                <th>吊牌价</th>
                <th>尺码</th>
                <th>颜色</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.Cherker.Store.StoreName$</td>
                <td>$it.Cherker.Storage.StorageName$</td>
				<td>$it.Product.ProductOriginNumber.ProductName$</td>
                <td>$it.ProductBarcode$</td>
                <td>$it.Product.ProductOriginNumber.TagPrice$</td>
                <td>$it.Product.Size.SizeName$</td>
                <td>$it.Product.Color.ColorName$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Cherker.Operator.Member.MemberName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

