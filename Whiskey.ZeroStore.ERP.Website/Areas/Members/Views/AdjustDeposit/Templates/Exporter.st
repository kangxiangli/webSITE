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
                <th>申请人</th>
                <th>会员名称</th>
                <th>手机号码</th>
                <th>调整金额</th>
                <th>调整积分</th>
                <th>审核状态（0审核中1通过2不通过3待确认）</th>
                <th>更新时间</th>
                <th>审核人</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.Applicant$</td>
                <td>$it.RealName$</td>
                <td>$it.MobilePhone$</td>
                <td>$it.Balance$</td>
                <td>$it.Score$</td>
                <td>$it.VerifyType$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Reviewers$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

