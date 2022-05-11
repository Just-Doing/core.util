using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Sharewinfo.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Text;
using Xunit;

namespace Unit
{
    class CusExport : ExcelUtils
    {
        public override int RenderBodyData(DataTable dt, ISheet sheet, List<ExcelColumn> columnList, XSSFWorkbook workbook, ICellStyle style, int startRowIndex)
        {
            var allCellStyle = new ICellStyle[columnList.Count];
            for (var i = 0; i < dt.Rows.Count; ++i)//写数据
            {
                var dataRow = dt.Rows[i];
                var row = sheet.CreateRow(i + startRowIndex + 1);
                var cellIndex = 0;
                for (var j = 0; j < columnList.Count; ++j)
                {
                    ICellStyle prevStyle = null;
                    if (i > 0 && allCellStyle[j] == null)
                    {
                        var prevRow = sheet.GetRow(i + startRowIndex);
                        if (null != prevRow)
                        {
                            var prevRowCell = prevRow.GetCell(cellIndex);
                            if (null != prevRowCell)
                            {
                                prevStyle = prevRowCell.CellStyle;
                            }
                        }
                        allCellStyle[j] = prevStyle;
                    }
                    var column = columnList[j];
                    if (null != column.Children && column.Children.Count > 0)
                    {
                        SetCellValue(sheet, dataRow, row, column.Children, workbook, style, ref cellIndex, allCellStyle[j]);
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
                            if (column.Type == "2")
                            {
                                double v = 0;
                                if (double.TryParse(cellValue, out v))
                                {
                                    cell.SetCellValue(v);
                                }
                                else
                                {
                                    cell.SetCellValue(cellValue);
                                }
                            }
                            else
                            {
                                cell.SetCellValue(cellValue);
                            }
                            SetBodyStyle(workbook, cell, column, style, allCellStyle[j]);
                            cellIndex += 1;
                        }
                        
                    }
                }
                row.Height = 100 * 20;
            }
            return startRowIndex + dt.Rows.Count + 1;
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
            var dr = dt.NewRow();
            dr["name"] = "nicol";
            dr["age"] = 18;
            var dr1 = dt.NewRow();
            dr1["name"] = "nicol11111111111";
            dr1["age"] = 20;
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
                new ExcelColumn("年龄", "age", 100)
            });
            dc.Add("table2", new List<ExcelColumn>() {
                new ExcelColumn("名称222", "name2", 300),
                new ExcelColumn("年龄222", "age2", 100)
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

    }
}
