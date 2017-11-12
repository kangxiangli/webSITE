




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
                <th>消费多少金额获得1积分</th>
                <th>推荐一个会员获得多少积分</th>
                <th>是否用余额消费可获得积分</th>
                <th>是否销售商品后自动打印</th>
                <th>是否会员充值后自动打印</th>
                <th>是否开启库存预警</th>
                <th>到达库存预警数量</th>
                <th>快递合作商名称</th>
                <th>快递合作商密钥</th>
                <th>快递API请求路径</th>
                <th>快递API公司参数名</th>
                <th>快递API订单号参数名</th>
                <th>快递API其他附加参数名</th>
                <th>短信合作商名称</th>
                <th>短信合作商帐号</th>
                <th>短信合作商密钥</th>
                <th>短信API请求路径</th>
                <th>短信API手机号名参数名</th>
                <th>短信API短信内容参数名</th>
                <th>短信内容字数限制</th>
                <th>是否开启会员生日提醒</th>
                <th>会员生日提醒提前天数</th>
                <th>是否开启会员生日自动短信祝贺</th>
                <th>会员生日自动发送祝贺短信内容</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
                <td>$it.SystemMemberConsumeGetScore$</td>
                <td>$it.SystemRecommandUserGetScore$</td>
                <td>$it.SystemIsBalanceConsumeGetScore$</td>
                <td>$it.SystemIsPrintWhenSaleProduct$</td>
                <td>$it.SystemIsPrintWhenMemberDeposit$</td>
                <td>$it.SystemIsInventoryWarning$</td>
                <td>$it.SystemInventoryWarningNumber$</td>
                <td>$it.DeliveryPartner$</td>
                <td>$it.DeliveryKey$</td>
                <td>$it.DeliveryAPIUrl$</td>
                <td>$it.DeliveryAPICompany$</td>
                <td>$it.DeliveryAPIOrderNumber$</td>
                <td>$it.DeliveryAPIOtherParams$</td>
                <td>$it.SMSPartner$</td>
                <td>$it.SMSPartnerAccount$</td>
                <td>$it.SMSKey$</td>
                <td>$it.SMSAPIUrl$</td>
                <td>$it.SMSAPIMobilePhone$</td>
                <td>$it.SMSAPIMessageContent$</td>
                <td>$it.SMSNumberOfWordLimit$</td>
                <td>$it.MemberIsBirthdayWarning$</td>
                <td>$it.MemberBirthdayWarningAdvancedDays$</td>
                <td>$it.MemberIsSendSMSOnBirthday$</td>
                <td>$it.MemberSendSMSContent$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Operator.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

