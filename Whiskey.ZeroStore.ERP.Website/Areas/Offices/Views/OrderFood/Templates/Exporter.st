
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
                <th>网络部</th>
                <th>仓储部</th>
                <th>运营部</th>
                <th>合规部</th>
                <th>人事部</th>
                <th>财务部</th>
                <th>编辑部</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.WL$</td>
                <td>$it.CC$</td>
                <td>$it.YY$</td>
                <td>$it.HG$</td>
                <td>$it.RS$</td>
                <td>$it.CW$</td>
                <td>$it.BJ$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.OperatorName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>


