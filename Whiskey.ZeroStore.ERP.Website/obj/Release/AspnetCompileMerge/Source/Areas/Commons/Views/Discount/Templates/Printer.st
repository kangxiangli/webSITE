




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
                <th>名称</th>
                <th>打折类型</th>
                <th>折扣简介</th>
                <th>开始时间</th>
                <th>结束时间</th>
                <th>按订单金额打折</th>
                <th>订单金额满</th>
                <th>按货品数量打折</th>
                <th>货品数量满</th>
                <th>折扣类型</th>
                <th>折扣比例</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
                <td>$it.DiscountName$</td>
                <td>$it.DiscountMethod$</td>
                <td>$it.Description$</td>
                <td>$it.StartTime$</td>
                <td>$it.EndTime$</td>
                <td>$it.DiscountByPrice$</td>
                <td>$it.DiscountPrice$</td>
                <td>$it.DiscountByAmount$</td>
                <td>$it.DiscountAmount$</td>
                <td>$it.DiscountType$</td>
                <td>$it.DiscountRate$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Operator.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

