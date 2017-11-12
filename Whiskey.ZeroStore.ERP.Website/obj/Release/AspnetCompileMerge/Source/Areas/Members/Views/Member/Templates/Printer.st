




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
                <th>归属店铺</th>
                <th>会员等级</th>
                <th>注册类型</th>
                <th>会员帐号</th>
                <th>会员密码</th>
                <th>会员类型</th>
                <th>账户余额</th>
                <th>账户积分</th>
                <th>会员卡号</th>
                <th>电子邮箱</th>
                <th>手机号码</th>
                <th>真实姓名</th>
                <th>会员性别</th>
                <th>出生日期</th>
                <th>身份证号</th>
                <th>会员照片</th>
                <th>签到次数</th>
                <th>登录次数</th>
                <th>登录时间</th>
                <th>推荐人</th>
                <th>备注信息</th>
                <th>锁定店铺</th>
                <th>扩展属性</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
                <td>$it.StoreId$</td>
                <td>$it.LevelId$</td>
                <td>$it.RegisterType$</td>
                <td>$it.MemberName$</td>
                <td>$it.MemberPass$</td>
                <td>$it.MemberType$</td>
                <td>$it.Balance$</td>
                <td>$it.Score$</td>
                <td>$it.CardNumber$</td>
                <td>$it.Email$</td>
                <td>$it.MobilePhone$</td>
                <td>$it.RealName$</td>
                <td>$it.Gender$</td>
                <td>$it.DateofBirth$</td>
                <td>$it.IDCard$</td>
                <td>$it.UserPhoto$</td>
                <td>$it.SignCount$</td>
                <td>$it.LoginCount$</td>
                <td>$it.LoginTime$</td>
                <td>$it.RecommendId$</td>
                <td>$it.Notes$</td>
                <td>$it.IsLockedStore$</td>
                <td>$it.Attributes$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Operator.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

