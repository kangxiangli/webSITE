




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
                <th>店铺名称</th>
                <th>店铺类型</th>
                <th>官方总店</th>
                <th>店铺简介</th>
                <th>店铺信誉</th>
                <th>店铺余额</th>
                <th>联系人</th>
                <th>手机号码</th>
                <th>店铺电话</th>
                <th>所在省份</th>
                <th>所在城市</th>
                <th>邮政编码</th>
                <th>店铺地址</th>
                <th>备注信息</th>
                <th>更新时间</th>
                <th>操作人员</th>
            </tr>
        </thead>
        <tbody>
            $list:{
            <tr>
				<td>$i0$</td>
                <td>$it.StoreName$</td>
                <td>$it.StoreType$</td>
                <td>$it.IsMainStore$</td>
                <td>$it.Description$</td>
                <td>$it.StoreCredit$</td>
                <td>$it.Balance$</td>
                <td>$it.ContactPerson$</td>
                <td>$it.MobilePhone$</td>
                <td>$it.Telephone$</td>
                <td>$it.ProvinceId$</td>
                <td>$it.CityId$</td>
                <td>$it.ZipCode$</td>
                <td>$it.Address$</td>
                <td>$it.Notes$</td>
                <td>$it.UpdatedTime$</td>
                <td>$it.Operator.AdminName$</td>
            </tr>
            }$
        </tbody>
    </table>
</body>
</html>

