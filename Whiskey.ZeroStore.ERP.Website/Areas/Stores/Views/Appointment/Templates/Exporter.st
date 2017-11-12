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
                <th>预约号</th>
                <th>店铺</th>
                <th>会员</th>
                <th>手机号</th>
                <th>喜欢</th>
                <th>不喜欢</th>
                <th>状态</th>
                <th>方案</th>
                <th>预约时间</th>
                <th>更新时间</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.Number$</td>
                <td>$it.StoreName$</td>
                <td>$it.RealName$</td>
                <td>$it.MobilePhone$</td>
                <td>$it.LikeCount$</td>
                <td>$it.DislikeCount$</td>
                <td>$it.State$</td>
                <td>$it.PlanCount$</td>
                <td>$it.StartTime$ ~ $it.EndTime$</td>
                <td>$it.UpdatedTime$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

