using CrossSellingApi.Common;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Util.Core;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Unit
{
    class CusExport : ExcelUtils
    {
        public override int RenderBodyData(DataTable dt, ISheet sheet, List<ExcelColumn> columnList, XSSFWorkbook workbook, Dictionary<string, ICellStyle> style, int startRowIndex)
        {
            for (var i = 0; i < dt.Rows.Count; ++i)//写数据
            {
                var dataRow = dt.Rows[i];
                var row = sheet.CreateRow(i + startRowIndex + 1);
                var cellIndex = 0;
                for (var j = 0; j < columnList.Count; ++j)
                {
                    var column = columnList[j];
                    if (null != column.Children && column.Children.Count > 0)
                    {
                        var columnStyleList = new Dictionary<string, ICellStyle>();
                        for (var ci = 0; ci < column.Children.Count; ci++)
                        {
                            ICellStyle childColumnCellType = workbook.CreateCellStyle();
                            childColumnCellType.CloneStyleFrom(columnStyleList["default"]);
                            columnStyleList[ci.ToString()] = childColumnCellType;
                        }
                        RenderColumnData(sheet, dataRow, row, column.Children, workbook, columnStyleList, ref cellIndex);
                    }
                    else
                    {
                        if (column.FieldName.StartsWith("imgs"))
                        {
                            // 读取图片
                            var imgInx = Convert.ToInt32(column.FieldName.Replace("imgs", ""));
                            var imgs = dataRow["imgs"].ToString().Split(";");
                            var img = imgInx >= imgs.Length? "" : imgs[imgInx];
                            if (!string.IsNullOrEmpty(img)) {
                                HttpWebRequest req = WebRequest.Create($"http://taikang.sharewinfo.com/service/api/Common/GetFileSignUrl?compress=imageView2/1/w/80/h/120/q/85&filePath=/Appresource/HDImage/{dataRow["ProjectID"]}/{dataRow["DrawID"]}/{img}.jpg") as HttpWebRequest;
                                HttpWebResponse response = req.GetResponse() as HttpWebResponse;
                                using (Stream stream = response.GetResponseStream())
                                {
                                    int pictureIdx = workbook.AddPicture(stream, (int)NPOI.SS.UserModel.PictureType.JPEG);
                                    IDrawing patriarch = sheet.CreateDrawingPatriarch();
                                    XSSFClientAnchor anchor = new XSSFClientAnchor(400, 400, 40, 40, j, i + startRowIndex + 1, j, i + startRowIndex + 1);
                                    IPicture pict = patriarch.CreatePicture(anchor, pictureIdx);
                                    // 宽度   高度  重置
                                    pict.Resize(1.0, 1.10);
                                }
                            }
                            cellIndex += 1;
                        }
                        else {
                            var cell = row.CreateCell(cellIndex);
                            var cellValue = dataRow[column.FieldName].ToString().Replace("\\r\\n", "\r\n");
                            SetCellValue(cell, cellValue, column);
                            cell.CellStyle = style["default"];
                            cellIndex += 1;
                        }
                        
                    }
                }
                row.Height = 100 * 20;
            }
            return startRowIndex + dt.Rows.Count + 1;
        }

        public override int RenderHeaderRow(DataTable dt, List<ExcelColumn> columnList, ISheet sheet, XSSFWorkbook workbook, ref int rowIndex, ref int lastRowIndex, int startCellIndex)
        {
            var index = dataSet.Tables.IndexOf(dt);
            return base.RenderHeaderRow(dt, columnList, sheet, workbook, ref rowIndex, ref lastRowIndex, startCellIndex);
        }
    }

    public class Export
    {

        [Fact]
        public void TestExportExcel()
        {

            var ds = new DataSet();
            var dt = new DataTable("table1");
            dt.Columns.Add(new DataColumn("name"));
            dt.Columns.Add(new DataColumn("age"));
            dt.Columns.Add(new DataColumn("date"));
            dt.Columns.Add(new DataColumn("datetime"));
            var dr = dt.NewRow();
            dr["name"] = "nicol";
            dr["age"] = 18;
            var dr1 = dt.NewRow();
            dr1["name"] = "nicol11111111111";
            dr1["age"] = 20;
            dr1["date"] = "2022-01-01";
            dr1["datetime"] = "2022-01-01 12:12:12";
            dt.Rows.Add(dr);
            dt.Rows.Add(dr1);
            ds.Tables.Add(dt);

            var dt2 = new DataTable("table2");
            dt2.Columns.Add(new DataColumn("name2"));
            dt2.Columns.Add(new DataColumn("age2"));
            var dr3 = dt2.NewRow();
            dr3["name2"] = "nicol333333333";
            dr3["age2"] = 18;
            var dr4 = dt2.NewRow();
            dr4["name2"] = "nico222222";
            dr4["age2"] = 20;
            dt2.Rows.Add(dr3);
            dt2.Rows.Add(dr4);
            ds.Tables.Add(dt2);
            Dictionary<string, List<ExcelColumn>> dc = new Dictionary<string, List<ExcelColumn>>();
            dc.Add("table1", new List<ExcelColumn>() {
                new ExcelColumn("名称", "name", 300),
                new ExcelColumn("年龄", "age", 100),
                new ExcelColumn("日期", "date", 100,ColumnType.Date, "yyyy-MM-dd"),
                new ExcelColumn("日期时间", "datetime", 100,ColumnType.DateTime, "yyyy-MM-dd HH:mm:ss")
            });
            dc.Add("table2", new List<ExcelColumn>() {
                new ExcelColumn("名称222", "name2", 300),
                new ExcelColumn("年龄222", "age2", 100, ColumnType.Number)
            });
            var st = new ExcelUtils().GetStreamByData(ds, dc);
            using (FileStream fs = new FileStream("d:/aaa.xlsx", FileMode.Create))
            {
                byte[] buffer = st.ToArray();//转化为byte格式存储
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
                buffer = null;
            }
        }


        [Fact]
        public void CusExportTest()
        {

            using (SqlConnection conn = new SqlConnection("data source=10.0.0.90;database=taikang_uat;user id=sa; pwd=1qaz!QAZ"))
            {
                conn.Open();
                var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT distinct top 1 p.ProjectName,
	build.BuildingCode+'-'+storey.StoreyName Area, 
	CASE [HiddendangerNature] 
		WHEN 0 THEN '未知' 
		WHEN 60001 THEN '观感品质' 
		WHEN 60002 THEN '使用功能' 
		WHEN 60003 THEN '运营安全' 
	END HiddendangerNature, 
	hd.[HiddenDangerDesc],
	employee.EmployeeName,
	hd.[DueTime], 
	comp.CompanyName,
	hdt.Code,
	hdc.CategoryName,
	hd.HiddenDangerDraw as imgs,
	hd.CreateTime,
	(case hd.HiddendangerStatus
		when 10001 then '待分派'
		when 10002 then '待解决'
		when 10003 then '待复核'
		when 10004 then '退回'
		when 10005 then '待确认'
		when 10006 then '申诉申请'
		when 10007 then '延期'
		when 10008 then '待发起'
		when 10009 then '关闭'
		when 10010 then '废弃'
		when 10011 then '暂存'
	end) as HiddendangerStatus,
	hd.ProjectID,
	hd.DrawID,
	hd.RectifyClaim
FROM [dbo].[HiddenDanger] hd 
	left JOIN [dbo].[Project] p ON hd.ProjectID = p.ProjectID
	left JOIN [dbo].[Draw] draw ON hd.DrawID = draw.DrawID
	left JOIN [dbo].[Building] build ON draw.BuildingID = build.BuildingID
	left JOIN [dbo].[Storey] storey ON draw.StoreyID = storey.StoreyID
	left JOIN [dbo].[Employee] employee ON hd.EmployeeID = employee.EmployeeID
	left JOIN [dbo].[Company] comp ON comp.CompanyID = hd.CompanyID
	left JOIN [dbo].[HiddenDangerType] hdt ON hdt.HiddenDangerTypeID = hd.HiddenDangerTypeID
	left JOIN [dbo].[HiddenDangerCategory] hdc ON hdc.HiddenDangerCategoryId = hd.HiddenDangerCategoryID
WHERE p.ProjectName IN ('琴园') 
                            ";
                var dataset = new DataSet();
                var adapter = new SqlDataAdapter(cmd);
                adapter.Fill(dataset);
                cmd.ExecuteNonQuery();
                dataset.Tables[0].TableName = "燕园3期";
                var st = new CusExport().GetStreamByData(dataset, new Dictionary<string, List<ExcelColumn>>() { { "燕园3期", new List<ExcelColumn>() {
                    new ExcelColumn("楼栋楼层", "Area", 200),
                    new ExcelColumn("隐患性质", "HiddendangerNature", 200),
                    new ExcelColumn("专业", "Code", 200),
                    new ExcelColumn("专业类型", "CategoryName", 200),
                    new ExcelColumn("描述", "HiddenDangerDesc", 200),
                    new ExcelColumn("整改要求", "RectifyClaim", 200),
                    new ExcelColumn("提交人", "EmployeeName", 200),
                    new ExcelColumn("创建时间", "CreateTime", 200),
                    new ExcelColumn("逾期时间", "DueTime", 200),
                    new ExcelColumn("处理公司", "CompanyName", 200),
                    new ExcelColumn("当前状态", "HiddendangerStatus", 200),
                    new ExcelColumn("图片1", "imgs0", 200),
                    new ExcelColumn("图片2", "imgs1", 200),
                    new ExcelColumn("图片3", "imgs2", 200),
                    new ExcelColumn("图片4", "imgs3", 200),
                    new ExcelColumn("图片5", "imgs4", 200),
                    new ExcelColumn("图片6", "imgs5", 200),
                    new ExcelColumn("图片7", "imgs6", 200),
                    new ExcelColumn("图片8", "imgs7", 200),
                    new ExcelColumn("图片9", "imgs8", 200),
                    new ExcelColumn("图片10", "imgs9", 200)
                } } });
                using (FileStream fs = new FileStream("d:/aaa.xlsx", FileMode.Create))
                {
                    byte[] buffer = st.ToArray();//转化为byte格式存储
                    fs.Write(buffer, 0, buffer.Length);
                    fs.Flush();
                    buffer = null;
                }
            }

            Assert.Equal<int>(7, 7);
        }



        [Fact]
        public void CrsExport()
        {
            var dt1 = new DataTable();
            var dt2 = new DataTable();
            var dt3 = new DataTable();
            var dt4 = new DataTable();
            var dt5 = new DataTable();
            var dt6 = new DataTable();
            var dt7 = new DataTable();
            var opptIds = "6494,6493,6492,6491,6490,6489,6488,6487,6486,6484,6482,6481,6480,6478,6477,6476,6474,6479,6456,6475,6470,6473,6472,6471,6469,6468,6467,6460,6466,6465,6461,6464,6463,6462,6446,6459,6457,6458,6455,6454,6453,6369,6451,6450,6449,6448,6440,6439,6447,6445,6443,6444,6435,6442,6441,6438,6428,6437,6434,6432,6433,6431,6427,6421,6418,6417,6416,6415,6414,6413,6412,6411,6400,6410,6406,6385,6409,6408,6407,6405,6404,6403,6402,6401,6383,6393,6384,6382,6381,6380,655,158,163,168,984,1888,318,322,332,334,342,345,1853,1856,1858,1865,1715,1447,1454,1457,1464,1467,1471,1539,1546,1547,1477,1484,1487,1491,1508,1535,2487,2488,1834,58,2201,1995,2000,2007,2009,668,676,678,1956,2572,26,27,31,32,43,45,49,402,412,420,424,1301,1915,1920,1925,2093,2098,375,377,379,394,1979,1986,1988,1993,1974,839,842,847,856,859,863,1071,2111,2114,2116,2118,2119,2120,2123,2124,2129,2131,2134,883,1572,1577,1578,1579,2071,2080,83,90,97,99,104,105,107,110,118,122,125,2333,2357,2364,2372,2396,2397,2414,2145,2148,2151,2157,2160,2162,2167,2168,269,271,283,285,291,292,301,302,308,2298,2310,2311,11,13,15,373,374,471,476,480,484,2519,2530,2531,434,436,440,455,1362,1364,1387,1902,1554,1558,1560,1562,2214,2217,2222,2235,2246,2250,2282,2286,2289,2291,2293,2256,2257,2263,2268,2275,2281,2635,2638,2639,2642,2643,2647,2670,2676,2678,2652,2654,2658,2663,2667,2669,593,906,907,909,912,913,917,948,919,920,932,934,937,941,720,722,724,728,730,739,749,214,2330,2331,2444,2451,2456,2458,2461,2468,2469,2473,2107,1428,1434,1439,132,1227,1229,1231,1233,1235,1239,1241,1242,1246,1254,1257,1258,1082,2321,1632,1639,761,763,774,775,792,797,799,817,1025,1037,1041,1045,540,542,178,184,185,1800,1808,1820,1830,2190,1411,1413,1418,1421,1422,564,567,568,571,575,576,2614,2617,2552,2016,2017,2029,2033,2034,2035,826,827,1647,1607,1609,1611,1614,1616,1618,1620,1622,698,700,704,706,708,711,712,715,2041,2042,2045,2047,2051,1767,1769,1789,868,875,876,880,590,1184,1185,1192,1194,1196,1206,1216,2420,2427,2432,2482,493,495,503,509,513,534,536,608,617,624,625,632,633,634,650,899";
            Task.WaitAll(new[] {
                Task.Run(() => { dt1 = GetOppBaseData( opptIds); }),
                Task.Run(() => { dt2 = GetBuApproveData( opptIds); }),
                Task.Run(() => { dt3 = GetGckByOpp( opptIds); }),
                Task.Run(() => { dt4=GetPckByOpp( opptIds); }),
                Task.Run(() => { dt5=GetPgByOpp( opptIds); }),
                Task.Run(() => { dt6=GetTransforByOpp( opptIds); }),
                Task.Run(() => { dt7=GetOtherCompByOpp( opptIds); })
            });

            var ds = new DataSet(); ds.Tables.Add(dt1); ds.Tables.Add(dt2); ds.Tables.Add(dt3); ds.Tables.Add(dt4); ds.Tables.Add(dt5); ds.Tables.Add(dt6); ds.Tables.Add(dt7);
            var dicColumns = new Dictionary<string, List<ExcelColumn>>() {
                { "机会点列表", new List<ExcelColumn>(){
                    new ExcelColumn("机会中文名称\r\nOpportunity Name CHN","NameCN",150),
                    new ExcelColumn("机会英文名称\r\nOpportunity Name ENG","NameEN",150),
                    new ExcelColumn("机会国别\r\nOpp Country","CountryName",150),
                    new ExcelColumn("省份/特殊区域\r\nOpp Province / Special Territory","Province",150),
                    new ExcelColumn("城市/机会地址\r\nCity / Opp Address","city",150),
                    new ExcelColumn("机会编号\r\nOpp ID","ShowSerialNumber",150),
                    new ExcelColumn("创建时间\r\nCreation Time","CreateTime",180, ColumnType.Date, "yyyy-MM-dd"),
                    new ExcelColumn("更新时间\r\nUpdate Time","UpdateTime",180, ColumnType.Date, "yyyy-MM-dd"),
                    new ExcelColumn("申请人ID","GID",150),
                    new ExcelColumn("申请人","UserName",150),
                    new ExcelColumn("机会审批状态\r\nApproval Status","IsLeaderCheck",150),
                    new ExcelColumn("申请人Region/Office/Team","OrgRank",150),
                    new ExcelColumn("Team Leader","TL",150),
                    new ExcelColumn("行业\r\nVertical","VerticalNameCn",150),
                    new ExcelColumn("父机会编号\r\nParent Opportunity ID","ParentShowSerialNumber",150),
                    new ExcelColumn("关联父机会\r\nParent Opportunity","ParentNameCN",150),
                    new ExcelColumn("关联父机会En\r\nParent Opportunity","ParentNameEN",150),
                    new ExcelColumn("机会标签\r\nOpportunity Attribute","attrs",150),
                    new ExcelColumn("是否为政府军工类机会\r\nGovernment & Military Opp","IsGovernmentMilitaryOpp",150),
                    new ExcelColumn("是否为ESG基建机会\r\nESG Infrastructure Opportunity","IsESGInfrastructureOpp",150),
                    new ExcelColumn("是否跨区销售\r\nCross Region","IsCrossRegion",150),
                    new ExcelColumn("机会描述\r\nOpportunity Description","OpportunityDescription",150),
                    new ExcelColumn("机会阶段\r\nOpp Stage","StageName",150),
                    new ExcelColumn("机会状态\r\nStatus","StatusName",150),
                    new ExcelColumn("机会中标率\r\nChance of Success","ChanceOfSuccessName",150),
                    new ExcelColumn("落地概率\r\nChance of Execution","ChanceOfExecution",150),
                    new ExcelColumn("竞争对手\r\nCompetitor","CompetitorName",150),
                    new ExcelColumn("预计投标时间\r\nEstimated Bidding Date","EstimatedBiddingDate",150, ColumnType.Date, "yyyy-MM-dd"),
                    new ExcelColumn("预计订单时间\r\nEstimated Order Date","EstimatedOrderDate",150, ColumnType.Date, "yyyy-MM-dd"),
                    new ExcelColumn("战略优先级\r\nStrategic Priority","StrategicPriorityName",150),
                    new ExcelColumn("是否参与预测\r\nRelevant for Forecast","IsRelevantForForecast",150),
                    new ExcelColumn("备选机会\r\nAlternative Opportunity","IsAlternativeOpportunity",150),
                    new ExcelColumn("机会潜力总金额\r\nOpportunity Potential","OpportunityPotential",150,ColumnType.Number),
                    new ExcelColumn("赢率后机会金额\r\nOpportunity Potential (Weighted)","OpportunityPotentialWeighted",150,ColumnType.Number),
                    new ExcelColumn("初始机会金额\r\nInitial Opportunity Amount","InitialOpportunityAmount",150,ColumnType.Number),
                    new ExcelColumn("机会数量\r\nQuantity","Quantity",150,ColumnType.Number),
                    new ExcelColumn("客户中文名称\r\nAccount Name CHN","AccountNameCHN",150),
                    new ExcelColumn("客户英文名称\r\nAccount Name ENG","AccountNameENG",150),
                    new ExcelColumn("客户主数据编号\r\nAccount SI ID","AccountSIID",150),
                    new ExcelColumn("客户=最终客户\r\nCustomer = End user","AccIsEndAcc",150),
                    new ExcelColumn("最终客户中文名称\r\nEnd Customer Name CHN","EndAccountNameCHN",150),
                    new ExcelColumn("最终客户英文名称\r\nEnd Customer Name ENG","EndAccountNameENG",150),
                    new ExcelColumn("最终客户主数据编号\r\nEnd Customer SI ID","EndAccountSIID",150),
                    new ExcelColumn("大客户中文名称\r\nAccount Group CHN","AccountGroupCHN",150),
                    new ExcelColumn("大客户英文名称\r\nAccount Group ENG","AccountGroupENG",150),
                    new ExcelColumn("大客户编号\r\nAccount Group ID","AccountGroupID",150),
                    new ExcelColumn("变压器容量\r\nTransformer Total Capacity","TransformerCapacitySum",150),
                    new ExcelColumn("中压柜台数\r\nMedium Voltage Cabinet Qty","MediumVoltageCabinets",150,ColumnType.Number),
                    new ExcelColumn("低压柜台数\r\nLow Voltage Cabinet Qty","LowVoltageCabinets",150,ColumnType.Number),
                    new ExcelColumn("配电房台数\r\nPower Distribution Room Qty","DistributionRooms",150,ColumnType.Number),
                    new ExcelColumn("一次性投标或多次议标\r\nMultiple Bidding","MultipleBidingName",150),
                    new ExcelColumn("MVC类型\r\nMVC Type","MVCType",150),
                    new ExcelColumn("集采/非集采\r\nCentralized Purchase","IsCentralizedPurchase",150),
                    new ExcelColumn("是否TIP提供机会信息\r\nTIP Disclosed","IsTIPDisclosed",150),
                    new ExcelColumn("TIP销售姓名\r\nTIP Sales","TipSalesName",150),
                    new ExcelColumn("是否TIP销售成功上图\r\nListed by TIP Sales","IsListedByTIPSales",150),
                    new ExcelColumn("上图时间\r\nDrawing Fix Date","ListedDate",150, ColumnType.Date, "yyyy-MM-dd")
                } },
                { "BU审批", new List<ExcelColumn>(){
                    new ExcelColumn("机会编号\r\nOpp ID","ShowSerialNumber",150),
                    new ExcelColumn("机会中文名称\r\nOpportunity Name CHN","NameCN",150),
                    new ExcelColumn("机会英文名称\r\nOpportunity Name ENG","NameEN",150),
                    new ExcelColumn("下游系统","DownstreamBUName", 100),
                    new ExcelColumn("状态","BPMResult", 100)
                } },
                { "GCK", new List<ExcelColumn>(){
                    new ExcelColumn("机会编号\r\nOpp ID","ShowSerialNumber",150),
                    new ExcelColumn("机会中文名称\r\nOpportunity Name CHN","NameCN",150),
                    new ExcelColumn("机会英文名称\r\nOpportunity Name ENG","NameEN",150),
                    new ExcelColumn("BU/.../GCK","GckName", 100),
                    new ExcelColumn("数量","GCKNum", 100,ColumnType.Number),
                    new ExcelColumn("GCK预估金额","GCKSum", 100,ColumnType.Number)
                } },
                { "PCK", new List<ExcelColumn>(){
                    new ExcelColumn("机会编号\r\nOpp ID","ShowSerialNumber",150),
                    new ExcelColumn("机会中文名称\r\nOpportunity Name CHN","NameCN",150),
                    new ExcelColumn("机会英文名称\r\nOpportunity Name ENG","NameEN",150),
                    new ExcelColumn("PCK","PckName", 100),
                    new ExcelColumn("PCK数量","PCKQuantity", 100,ColumnType.Number),
                    new ExcelColumn("单价｜未税（人民币元）","Unitprice", 100,ColumnType.Number)
                } },
                { "PG", new List<ExcelColumn>(){
                    new ExcelColumn("机会编号\r\nOpp ID","ShowSerialNumber",150),
                    new ExcelColumn("机会中文名称\r\nOpportunity Name CHN","NameCN",150),
                    new ExcelColumn("机会英文名称\r\nOpportunity Name ENG","NameEN",150),
                    new ExcelColumn("BU/.../GCK","GckName", 100),
                    new ExcelColumn("PG1","PGOne", 100),
                    new ExcelColumn("PG2","PGTwo", 100),
                    new ExcelColumn("PG3","PGThree", 100),
                    new ExcelColumn("Quantity","PG3Quantity", 100,ColumnType.Number),
                    new ExcelColumn("Unit Price","UnitPrice", 100,ColumnType.Number)
                } },
                { "变压器", new List<ExcelColumn>(){
                    new ExcelColumn("机会编号\r\nOpp ID","ShowSerialNumber",150),
                    new ExcelColumn("机会中文名称\r\nOpportunity Name CHN","NameCN",150),
                    new ExcelColumn("机会英文名称\r\nOpportunity Name ENG","NameEN",150),
                    new ExcelColumn("变压器类型","Name", 100,ColumnType.Number),
                    new ExcelColumn("变压器数量","TransformerNum", 100,ColumnType.Number)
                } },
                { "其他相关方", new List<ExcelColumn>(){
                    new ExcelColumn("机会编号\r\nOpp ID","ShowSerialNumber",150),
                    new ExcelColumn("机会中文名称\r\nOpportunity Name CHN","NameCN",150),
                    new ExcelColumn("机会英文名称\r\nOpportunity Name ENG","NameEN",150),
                    new ExcelColumn("客户角色","CustomerTypeName", 100),
                    new ExcelColumn("其他相关方中文名称","CustomerOtherRPartName", 100),
                    new ExcelColumn("其他相关方英文名称","CustomerOtherRPartEnName", 100),
                    new ExcelColumn("其他相关方主数据编号","CustomerOtherRPartID", 100),
                    new ExcelColumn("备注","Remark", 100)
                } }
            };
            var dt = new ExcelUtils().GetStreamByData(ds, dicColumns);
            using (FileStream fs = new FileStream("d:/aaa.xlsx", FileMode.Create))
            {
                byte[] buffer = dt.ToArray();//转化为byte格式存储
                fs.Write(buffer, 0, buffer.Length);
                fs.Flush();
                buffer = null;
            }
        }
        private static DataTable GetOppBaseData( string opptIds)
        {
            #region 机会点主表
            var oppBaseSql = @"select o.NameCN,
					o.NameEN,
					ctry.Name CountryName,
					o.Province + '/' + isnull(spt.SpecialTerritory, '') Province, 
					o.city + '/'+ isnull(o.Address,'') city,
					o.ShowSerialNumber,
					CONVERT(varchar(10),DATEADD(HOUR, 8, o.CreateTime), 120) CreateTime,
					CONVERT(varchar(10),DATEADD(HOUR, 8, o.UpdateTime), 120) UpdateTime,
					u.GID,
					u.Name UserName,
					stg.Name StageName,
					(case o.IsLeaderCheck when 0 then N'没审核' when 1 then N'审核通过' when 2 then N'正在审核' when 3 then N'审核拒绝' end) IsLeaderCheck,
					org.OrgRank,tl.Name TL,
					vert.NameCn VerticalNameCn,
					vert.NameEn VerticalNameEn,
					parent.ShowSerialNumber ParentShowSerialNumber,
					parent.NameCN ParentNameCN,
					parent.NameEN ParentNameEN,
					(stuff(
							(select ',' + [name] 
								from Oppt_Attribute_R attrR 
									inner join Attribute_DIC attr on attr.Id = attrR.AttrubiteId 
								where attrR.OpptId=o.Id for xml path('')
							)
							,1,1,'')
					) attrs,
					o.IsGovernmentMilitaryOpp,
					o.IsESGInfrastructureOpp,
					o.IsCrossRegion,
					o.OpportunityDescription,
					stus.Name StatusName,
					chance.Name ChanceOfSuccessName,
					chanceEx.ChanceOfExecution,
					(stuff(
							(select ',' + [name] 
								from CustomerCompetitor_DIC c
									inner join string_split(CONVERT(varchar(max),o.CompetitorId),',') v on c.Id = v.value
									 for xml path('')
							)
							,1,1,'')
					) CompetitorName,
					CONVERT(varchar(10),o.EstimatedBiddingDate , 120) EstimatedBiddingDate,
					CONVERT(varchar(10),o.EstimatedOrderDate , 120) EstimatedOrderDate,
					strg.StrategicPriorityName,
					o.IsRelevantForForecast,
					o.IsAlternativeOpportunity,
					o.OpportunityPotential,
					o.OpportunityPotentialWeighted,
					o.InitialOpportunityAmount,
					o.Quantity,
					o.AccountNameCHN,
					o.AccountNameENG,
					o.AccountSIID,
					(case o.AccountSIID when o.EndAccountSIID then 1 else 0 end) AccIsEndAcc,
					o.EndAccountNameCHN,o.EndAccountNameENG,o.EndAccountSIID,o.AccountGroupCHN,o.AccountGroupENG,o.AccountGroupID,
					o.TransformerCapacitySum,o.MediumVoltageCabinets,o.LowVoltageCabinets,o.DistributionRooms,
					mub.Name MultipleBidingName,
					mvct.MVCType,
					o.IsCentralizedPurchase,o.IsTIPDisclosed,tipSales.Name TipSalesName,
					o.IsListedByTIPSales,
					CONVERT(varchar(10),DATEADD(HOUR, 8, o.ListedDate) , 120) ListedDate
				from [dbo].[Opportunity] o
				inner join string_split(@ids,',') ids on ids.value = o.id
				inner join Country_DIC ctry on ctry.Id = o.CountryId
				left join SpecialTerritory_DIC spt on spt.Id = o.SpecialTerritoryId
				inner join [User] u on u.Id = o.UserId
				inner join Stage_DIC stg on stg.Id = o.StageId
				inner join Vertical_DIC vert on vert.Id = o.VerticalId
				outer apply (select top 1 * from UserOrg uo where uo.UserId = o.UserId) t
				inner join Organization org on t.OrgId = org.Id
				left join [User] tl on tl.Id=org.ManagerUserId
				left join Opportunity parent on parent.Id = o.ParentOpptId
				inner join Status_DIC stus on stus.Id = o.StatusId
				inner join ChanceOfSuccess_DIC chance on chance.Id = o.ChanceOfSuccessId
				inner join ChanceOfExecution_DIC chanceEx on chanceEx.id=o.ChanceOfExecutionId
				inner join StrategicPriority_DIC strg on strg.id= o.StrategicPriorityId
				left join  MultipleBiding_DIC mub on mub.Id = o.MultipleBidingId
				left join MVCType_DIC mvct on mvct.Id=o.MVCTypeId
				left join [User] tipSales on tipSales.Id = o.TIPSalesId
			order by o.CreateTime desc";
            var oppBaseDt = new EfExcuteQueryBySql().Query("", oppBaseSql, new SqlParameter[] {
                new SqlParameter("ids", opptIds)
            });
            if (oppBaseDt != null)
            {
                oppBaseDt.TableName = "机会点列表";
                return oppBaseDt;
            }
            return new DataTable();
            #endregion
        }

        private static DataTable GetBuApproveData( string opptIds)
        {
            #region BU审批
            var buSql = @"
					select o.ShowSerialNumber,o.NameEN,o.NameCN,down.* from DownstreamSysOpptID down
						inner join string_split(@ids,',') ids on ids.value = down.OpptId
						inner join [dbo].[Opportunity] o on o.Id = ids.value
					order by o.CreateTime desc
			";
            var buDt = new EfExcuteQueryBySql().Query("", buSql, new SqlParameter[] {
                new SqlParameter("ids", opptIds)
            });
            if (buDt != null)
            {
                buDt.TableName = "BU审批";
                return buDt;
            }
            return new DataTable();
            #endregion
        }

        private static DataTable GetGckByOpp( string opptIds)
        {
            #region GCK
            var gckSql = @"
						select o.ShowSerialNumber,o.NameEN,o.NameCN,
							REPLACE(ogn.OrgRank,'SI/BU/','')+'/'+gck.SubSegment+'/'+gck.GCKCode GckName,
							ogr.GCKNum,ogr.GCKSum
						from Oppt_SegmentGCK_R ogr
						inner join string_split(@ids,',') ids on ids.value = ogr.OpportunityId
						inner join [dbo].[Opportunity] o on o.Id = ids.value
						inner join SegmentGCK_DIC gck on gck.Id = ogr.SegmentGCKId
						inner join Organization ogn on ogn.Id = gck.OrgId
					order by o.CreateTime desc
			";
            var gckDt = new EfExcuteQueryBySql().Query("", gckSql, new SqlParameter[] {
                new SqlParameter("ids", opptIds)
            });
            if (gckDt != null)
            {
                gckDt.TableName = "GCK";
                return gckDt;
            }
            return new DataTable();
            #endregion
        }

        private static DataTable GetPckByOpp( string opptIds)
        {
            #region PCK
            var pckSql = @"
						select o.ShowSerialNumber,o.NameEN,o.NameCN,
							pck.PCK+'/'+pck.Description PckName,
							opr.PCKQuantity,opr.Unitprice
						from Oppt_PCK_R opr
						inner join string_split(@ids,',') ids on ids.value = opr.OpptId
						inner join [dbo].[Opportunity] o on o.Id = ids.value
						inner join PCK_DIC pck on pck.Id = opr.PCKId
					order by o.CreateTime desc
			";
            var pckDt = new EfExcuteQueryBySql().Query("", pckSql, new SqlParameter[] {
                new SqlParameter("ids", opptIds)
            });
            if (pckDt != null)
            {
                pckDt.TableName = "PCK";
                return pckDt;
            }
            return new DataTable();
            #endregion
        }
        private static DataTable GetPgByOpp(string opptIds)
        {
            #region PG
            var pgSql = @"
						select o.ShowSerialNumber,o.NameEN,o.NameCN,ogpr.PG3Quantity,ogpr.UnitPrice,
							pg.PGOne,pg.PGTwo,pg.PGThree,
							REPLACE(ogn.OrgRank,'SI/BU/','')+'/'+gck.SubSegment+'/'+gck.GCKCode GckName
						from Oppt_SegmentGCK_R ogr
						inner join string_split(@ids,',') ids on ids.value = ogr.OpportunityId
						inner join [dbo].[Opportunity] o on o.Id = ids.value
						inner join OpptSegmentGCK_R_PG_R ogpr on ogpr.OpptSegmentGCKId = ogr.Id
						inner join SegmentGCK_DIC gck on gck.Id = ogr.SegmentGCKId
						inner join Organization ogn on ogn.Id = gck.OrgId
						inner join PGPGPG_DIC pg on pg.Id = ogpr.PGPGPGId
					order by o.CreateTime desc
			";
            var pgDt = new EfExcuteQueryBySql().Query("", pgSql, new SqlParameter[] {
                new SqlParameter("ids", opptIds)
            });
            if (pgDt != null)
            {
                pgDt.TableName = "PG";
                return pgDt;
            }
            return new DataTable();
            #endregion
        }
        private static DataTable GetTransforByOpp( string opptIds)
        {
            #region 变压器
            var transforSql = @"
						select o.ShowSerialNumber,o.NameEN,o.NameCN, tranf.Name, otr.TransformerNum
							from Oppt_Transformer_R otr
							inner join string_split(@ids,',') ids on ids.value = otr.OpportunityId
							inner join [dbo].[Opportunity] o on o.Id = ids.value
							inner join Transformer_DIC tranf on tranf.Id = otr.TransformerId
						order by o.CreateTime desc
			";
            var transforDt = new EfExcuteQueryBySql().Query("", transforSql, new SqlParameter[] {
                new SqlParameter("ids", opptIds)
            });
            if (transforDt != null)
            {
                transforDt.TableName = "变压器";
                return transforDt;
            }
            return new DataTable();

            #endregion
        }
        private static DataTable GetOtherCompByOpp( string opptIds)
        {
            #region 其他相关方
            var otherCompSql = @"
							select o.ShowSerialNumber,o.NameEN,o.NameCN, ocp.CustomerOtherRPartName, 
								ocp.CustomerOtherRPartEnName,ocp.CustomerOtherRPartID,ocp.CustomerTypeName,ocp.Remark
								from CustomerOtherRPart ocp
								inner join string_split(@ids,',') ids on ids.value = ocp.OpptId
								inner join [dbo].[Opportunity] o on o.Id = ids.value
							order by o.CreateTime desc
			";
            var otherCompDt = new EfExcuteQueryBySql().Query("", otherCompSql, new SqlParameter[] {
                new SqlParameter("ids", opptIds)
            });
            if (otherCompDt != null)
            {
                otherCompDt.TableName = "其他相关方";
                return otherCompDt;
            }
            return new DataTable();

            #endregion
        }

    }


}
