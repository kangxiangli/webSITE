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
                <th>姓名</th>
                <th>手机号</th>
                <th>充值金额</th>
                <th>现金消费</th>
                <th>刷卡消费</th>
                <th>优惠赠送</th>
                <th>积分</th>
                <th>充值类型</th>
                <th>订单信息</th>
                <th>系数</th>
                <th>更新时间</th>
                <th>操作人员</th>
                <th>部门</th>
                <th>店铺</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.RealName$</td>
                <td>$it.MobilePhone$</td>
                <td>$it.Price$</td>
                <td>$it.Cash$</td>
                <td>$it.Card$</td>
                <td>$it.Coupon$</td>
                <td>$it.Score$</td>
                <td>$it.DepositContext$</td>
                <td>$it.RelatedOrderNumber$</td>
                <td>$it.Quotiety$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.AdminName$</td>
                <td>$it.DepartmentName$</td>
                <td>$it.StoreName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

