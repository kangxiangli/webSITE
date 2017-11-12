
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
                <th>IMEI</th>
                <th>上次经度</th>
                <th>上次纬度</th>
                <th>上次位置</th>
                <th>上次更新时间</th>
                <th>经度</th>
                <th>纬度</th>
                <th>地理位置</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i$</td>
                <td>$it.StoreName$</td>
                <td>$it.IMEI$</td>
                <td>$it.PrevLongitude$</td>
                <td>$it.PrevLatitude$</td>
                <td>$it.PrevAddress$</td>
                <td>$it.PrevUpdatedTime$</td>
                <td>$it.Longitude$</td>
                <td>$it.Latitude$</td>
                <td>$it.Address$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.OperatorName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>


