




<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd">
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
	<style type="text/css" media="all">
        table{
            font-size:14px;
			text-align:center;
			line-height:20px;
			word-break: break-all;
			border:solid 1px rgb(149,119,205);
			border-collapse:collapse;
        }
		.thumbnail-wrapper{
			display:block;
			width:60px;
			height:60px;
			overflow:hidden;
			border:1px solid #eaeaea;
			margin:0 auto 0 auto;
		}
		.thumbnail-wrapper img{
			margin:2px;
			max-width:40px;
		}
    </style>
</head>
<body>
    <table width="100%" border="1">
	<caption><h4>零时尚数据报表-$FileTitle$</h4><br></caption>
        <thead>
            <tr>
				<th>序号</th>
                <th>配货单号</th>
                <th>对应采购单号</th>
                <th>发货店铺</th>
                <th>发货仓库</th>
                <th>收货店铺</th>
                <th>收货仓库</th>
                <th>采购单类型</th>
                <th>配货数量</th>
                <th>提交时间</th>
                <th>状态</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.OrderBlankNumber$</td>
                <td>$it.PurchaseNumber$</td>
                <td>$it.OutStoreName$</td>
                <td>$it.OutStorageName$</td>
                <td>$it.ReceiverStoreName$</td>
                <td>$it.ReceiverStorageName$</td>
                <td>$it.OrderblankType$</td>
                <td>$it.Quantity$</td>
                <td>$it.CreatedTime$</td>
                <td>$it.Status$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

