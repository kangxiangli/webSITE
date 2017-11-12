




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
                <th>父级色彩</th>
                <th>色彩名称</th>
                <th>色彩编码</th>
                <th>色彩层级</th>
                <th>色彩描述</th>
                <th>最小色相值</th>
                <th>最大色相值</th>
                <th>最小饱和度</th>
                <th>最大饱和度</th>
                <th>最小明度值</th>
                <th>最大明度值</th>
                <th>RGB</th>
                <th>HSL</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
                <td>$it.ParentId$</td>
                <td>$it.ColorName$</td>
                <td>$it.ColorCode$</td>
                <td>$it.ColorLevel$</td>
                <td>$it.Description$</td>
                <td>$it.MinHue$</td>
                <td>$it.MaxHue$</td>
                <td>$it.MinSaturation$</td>
                <td>$it.MaxSaturation$</td>
                <td>$it.MinLightness$</td>
                <td>$it.MaxLightness$</td>
                <td>$it.RGB$</td>
                <td>$it.HSL$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Operator.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

