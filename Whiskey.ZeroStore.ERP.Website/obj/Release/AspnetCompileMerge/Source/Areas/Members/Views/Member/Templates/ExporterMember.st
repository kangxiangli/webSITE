




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
                <th>卡号</th>
                <th>归属店铺</th>
                <th>姓名</th>
                <th>性别</th>
                <th>手机号</th>
                <th>会员类型</th>
                <th>会员等级</th>
                <th>会员系数</th>
                <th>账户余额</th>
                <th>账户积分</th>
                <th>注册日期</th>
              
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
				<td>$it.UniquelyIdentifies$</td>
				<td>$it.StoreName$</td>
				<td>$it.RealName$</td>
				<td>$it.Gender$</td>
				<td>$it.MobilePhone$</td>
				<td>$it.MemberTypeName$</td>
				<td>$it.LevelName$</td>
				<td>$it.Quotiety$</td>
				<td>$it.Balance$</td>
				<td>$it.Score$</td>
				<td>$it.CreatedTime$</td>
               
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

