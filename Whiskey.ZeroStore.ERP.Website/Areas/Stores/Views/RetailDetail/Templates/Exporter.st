




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
                <th>订单编号</th>
                <th>店铺名称</th>
                <th>会员</th>
                <th>数量</th>
                <th>总额</th>
                <th>出库时间</th>
                <th>是否参与店铺活动</th>
                <th>是否使用优惠券</th>
                <th>状态</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.RetailNumber$</td>
                <td>$it.StoreName$</td>
                <td>$it.ConsumerName$</td>
                <td>$it.RetailCount$</td>
                <td>$it.ConsumeCount$</td>
                <td>$it.OutStorageDatetime$</td>
                <td>$it.IsStoreActivityDiscount$</td>
                <td>$it.IsCouponConsume$</td>
                <td>$it.RetailState$</td>
                <td>$it.OperatorName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

