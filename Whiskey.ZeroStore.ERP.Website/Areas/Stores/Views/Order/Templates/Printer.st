




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
                <th>所属店铺</th>
                <th>购买会员</th>
                <th>快递公司</th>
                <th>支付公司</th>
                <th>订单标识</th>
                <th>订单号码</th>
                <th>订单状态</th>
                <th>备注信息</th>
                <th>运输方式</th>
                <th>快递运费</th>
                <th>快递公司</th>
                <th>快递单号</th>
                <th>收件人</th>
                <th>所在城市</th>
                <th>所在省份</th>
                <th>家庭地址</th>
                <th>固定电话</th>
                <th>手机号码</th>
                <th>邮政编码</th>
                <th>付款方式</th>
                <th>支付公司</th>
                <th>付款帐号</th>
                <th>付款流水</th>
                <th>现金消费</th>
                <th>刷卡消费</th>
                <th>积分消费</th>
                <th>储值消费</th>
                <th>抹去价格</th>
                <th>找回零钱</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
                <td>$it.StoreId$</td>
                <td>$it.MemberId$</td>
                <td>$it.ExpressId$</td>
                <td>$it.PaymentId$</td>
                <td>$it.OrderGuid$</td>
                <td>$it.OrderNumber$</td>
                <td>$it.OrderStatus$</td>
                <td>$it.Notes$</td>
                <td>$it.DeliverType$</td>
                <td>$it.ExpressFee$</td>
                <td>$it.ExpressCompany$</td>
                <td>$it.ExpressNumber$</td>
                <td>$it.Receiver$</td>
                <td>$it.CityName$</td>
                <td>$it.ProvinceName$</td>
                <td>$it.HomeAddress$</td>
                <td>$it.Telephone$</td>
                <td>$it.MobilePhone$</td>
                <td>$it.ZipCode$</td>
                <td>$it.PaymentType$</td>
                <td>$it.PaymentCompany$</td>
                <td>$it.PaymentAccount$</td>
                <td>$it.PaymentTradeNumber$</td>
                <td>$it.Cash$</td>
                <td>$it.Card$</td>
                <td>$it.Score$</td>
                <td>$it.Balance$</td>
                <td>$it.Coupon$</td>
                <td>$it.Change$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Operator.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

