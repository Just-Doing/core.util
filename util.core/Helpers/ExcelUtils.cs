
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;


namespace SmartCribms.Common
{
    public class ExcelColumn
    {
        private int _width;

        public string FieldName { get; set; }
        public string Type { get; set; } // 数字   时间
        public string Format { get; set; } // 格式化
        public List<ExcelColumn> Children { get; set; }
        public int Width
        {
            get
            {
                return _width;
            }
            set => _width = value;
        }
        public int GroupCount { get; set; }
        public int RenderIndex { get; set; }// 用于 同一个sheet 2个表头 区分层次
        public ExcelColumn(string columnName, string filedName, int Width)
        {
            ColumnName = columnName;
            FiledName = filedName;
            FieldName = filedName;
            _width = Width;
        }

        public string ColumnName { get; set; }
        public string FiledName { get; set; }
    }
    public class ExcelUtils
    {
        /// <summary>
        /// 根据datatable返回文件流
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="fs">可空</param>
        /// <returns></returns>
        public static MemoryStream GetStreamByData(DataSet ds)
        {
            try
            {
                IWorkbook workbook = workbook = new HSSFWorkbook();
                ISheet sheet = null;
                foreach (DataTable dt in ds.Tables)
                {
                    if (workbook != null)
                    {
                        sheet = workbook.CreateSheet(dt.TableName);
                        var row = sheet.CreateRow(0);
                        for (var j = 0; j < dt.Columns.Count; ++j)//写列名
                        {
                            row.CreateCell(j).SetCellValue(dt.Columns[j].ColumnName);
                        }
                        for (var i = 0; i < dt.Rows.Count; ++i)//写数据
                        {
                            row = sheet.CreateRow(i + 1);
                            for (var j = 0; j < dt.Columns.Count; ++j)
                            {
                                row.CreateCell(j).SetCellValue(dt.Rows[i][j].ToString());
                            }
                        }
                    }
                }
                var newFile = new MemoryStream();
                workbook.Write(newFile);
                return newFile;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 根据datatable返回文件流
        /// </summary>
        /// <param name="ds"></param>
        /// <returns></returns>
        public FileStream GetStreamByData(DataSet ds, Dictionary<string, List<ExcelColumn>> columns, string fileName)
        {
            var newFile = string.IsNullOrEmpty(fileName) ? Environment.CurrentDirectory + "/temp/" + Guid.NewGuid() + ".xlsx": fileName;
            if (System.IO.File.Exists(newFile))
                System.IO.File.Delete(newFile);
            var workbook = new XSSFWorkbook();
            ICellStyle style = workbook.CreateCellStyle();//创建样式对象
            style.VerticalAlignment = VerticalAlignment.Center;
            IFont font = workbook.CreateFont(); //创建一个字体样式对象
            font.FontHeight = 8;
            font.FontName = "Arial"; //和excel里面的字体对应
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            style.BorderBottom = BorderStyle.Thin;
            style.BottomBorderColor = HSSFColor.Grey50Percent.Index;
            style.TopBorderColor = HSSFColor.Grey50Percent.Index;
            style.LeftBorderColor = HSSFColor.Grey50Percent.Index;
            style.RightBorderColor = HSSFColor.Grey50Percent.Index;
            style.SetFont(font); //将字体样式赋给样式对象
            foreach (DataTable dt in ds.Tables)
            {
                List<ExcelColumn> columnList = columns[dt.TableName];
                if (workbook != null)
                {
                    ISheet sheet = workbook.GetSheet(dt.TableName) ?? workbook.CreateSheet(dt.TableName);
                    int bodyStartIndex = RenaderHeader(sheet, columnList, workbook, 0);
                    RenderBodyData(dt, sheet, columnList, workbook, style, bodyStartIndex);
                }
                using (FileStream outFs = new FileStream(newFile, FileMode.OpenOrCreate))
                {
                    workbook.Write(outFs);
                }
            }
            GC.Collect();
            if (string.IsNullOrEmpty(fileName))
            {
                var fs = new FileStream(newFile, FileMode.Open);
                return fs;
            }
            return null;
        }

        public void SetHeaderStyle(IWorkbook workbook, ICell cell)
        {
            ICellStyle style = workbook.CreateCellStyle();//创建样式对象
            style.Alignment = HorizontalAlignment.Center;
            style.VerticalAlignment = VerticalAlignment.Center;
            IFont font = workbook.CreateFont(); //创建一个字体样式对象
            font.FontName = "Arial"; //和excel里面的字体对应
            font.Boldweight = short.MaxValue;//字体加粗
            style.SetFont(font); //将字体样式赋给样式对象
            cell.CellStyle = style; //把样式赋给单元格
        }
        public void SetBodyStyle(IWorkbook workbook, ICell cell, ExcelColumn column, ICellStyle style, ICellStyle PrevRowStyle)
        {
            if (PrevRowStyle == null)
            {
                var cellStyle = workbook.CreateCellStyle();
                cellStyle.CloneStyleFrom(style); //把样式赋给单元格
                var dataformat = workbook.CreateDataFormat();
                if (column.Type != "1" && !string.IsNullOrEmpty(column.Format))
                {
                    cellStyle.DataFormat = dataformat.GetFormat(column.Format);
                }
                cell.CellStyle = cellStyle;
            }
            else
            {
                cell.CellStyle = PrevRowStyle;
            }
        }
        /// <summary>
        /// 该方法 需要返回 当前行号
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="columnList"></param>
        /// <param name="workbook"></param>
        /// <param name="notices"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public virtual int RenaderHeader(ISheet sheet, List<ExcelColumn> columnList, XSSFWorkbook workbook, int rowIndex)
        {
            var lastRowIndex = rowIndex;
            var headerStartRowIndex = rowIndex ;// 如果有说明文本 则文本下会多一行空白、如果没有得去掉这空白行
            var awaitMergedRegion = new List<int[]>(); // 记录待 合并的列（主要是合并列 左右的列 需要进行 行合并）
            var cellIndex = RenderHeaderRow(columnList, sheet, workbook, ref rowIndex, ref lastRowIndex, 0, ref awaitMergedRegion);

            foreach (var cellPoint in awaitMergedRegion)
            {
                sheet.AddMergedRegion(new CellRangeAddress(cellPoint[0], lastRowIndex, cellPoint[1], cellPoint[1]));
            }
           
            for (var i = headerStartRowIndex; i <= lastRowIndex; i++)
            {
                SetCellBack(sheet, workbook, i, cellIndex, true);// 把所有的表头设置 边框
            }
            return lastRowIndex;
        }
        /// <summary>
        /// 该方法需要返回 当前列号
        /// </summary>
        /// <param name="columnList">所有列</param>
        /// <param name="sheet"></param>
        /// <param name="workbook"></param>
        /// <param name="rowIndex">行号</param>
        /// <param name="startCellIndex">开始 列号</param>
        /// <returns></returns>
        public virtual int RenderHeaderRow(List<ExcelColumn> columnList, ISheet sheet, XSSFWorkbook workbook, ref int rowIndex, ref int lastRowIndex, int startCellIndex, ref List<int[]> awaitMergeCell)
        {
            var childRow = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            var cellIndex = startCellIndex; // 开始的列号
            for (var j = 0; j < columnList.Count; ++j)//写列名
            {
                var column = columnList[j];
                var children = column.Children;
                var cell = childRow.CreateCell(cellIndex);
                cell.SetCellValue(column.ColumnName);
                sheet.SetColumnWidth(cellIndex, column.Width * 40);

                if (null != children && children.Count > 0)
                {
                    rowIndex += 1;
                    lastRowIndex = rowIndex > lastRowIndex ? rowIndex : lastRowIndex;// 记录最深的层级
                    var orgCellIndex = cellIndex;
                    cellIndex = RenderHeaderRow(children, sheet, workbook, ref rowIndex, ref lastRowIndex, cellIndex, ref awaitMergeCell);
                    sheet.AddMergedRegion(new CellRangeAddress(rowIndex - 1, rowIndex - 1, orgCellIndex, cellIndex - 1));
                    rowIndex -= 1;
                }
                else
                {
                    awaitMergeCell.Add(new int[] { rowIndex, cellIndex });
                    cellIndex++;
                }
            }

            sheet.RowSumsRight = false;
            for (var j = 0; j < columnList.Count; ++j) //分组
            {
                int start = 0;
                int end = 0;
                if (columnList[j].GroupCount > 0)
                {
                    start = j + 1;
                    end = j + columnList[j].GroupCount;
                    sheet.GroupColumn(start, end);
                }
            }
            return cellIndex;
        }


        public void SetCellBack(ISheet sheet, XSSFWorkbook workbook, int rowIndex, int endCellIndex, bool border)
        {
            ICellStyle noticeStyle = workbook.CreateCellStyle();//创建样式对象
            noticeStyle.FillForegroundColor = HSSFColor.White.Index;
            noticeStyle.FillPattern = FillPattern.SolidForeground;
            IFont font = workbook.CreateFont(); //创建一个字体样式对象
            font.FontName = "Arial"; //和excel里面的字体对应
            font.FontHeight = 8;
            noticeStyle.SetFont(font); //将字体样式赋给样式对象
            if (border)
            {
                noticeStyle.Alignment = HorizontalAlignment.Center;
                noticeStyle.VerticalAlignment = VerticalAlignment.Center;
                noticeStyle.WrapText = true;
                noticeStyle.BottomBorderColor = HSSFColor.Grey50Percent.Index;
                noticeStyle.TopBorderColor = HSSFColor.Grey50Percent.Index;
                noticeStyle.LeftBorderColor = HSSFColor.Grey50Percent.Index;
                noticeStyle.RightBorderColor = HSSFColor.Grey50Percent.Index;
                noticeStyle.BorderLeft = BorderStyle.Thin;
                noticeStyle.BorderRight = BorderStyle.Thin;
                noticeStyle.BorderTop = BorderStyle.Thin;
                noticeStyle.BorderBottom = BorderStyle.Thin;
                noticeStyle.FillForegroundColor = HSSFColor.Grey25Percent.Index;
                noticeStyle.FillPattern = FillPattern.SolidForeground;
            }
            for (var j = 0; j < endCellIndex; j++)
            {
                var noticeRow = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
                var noticeCell = noticeRow.GetCell(j) ?? noticeRow.CreateCell(j);
                noticeCell.CellStyle = noticeStyle; //把样式赋给单元格
            }
        }
        /// <summary>
        /// 该方法 拿着给的参数直接写数据
        /// </summary>
        /// <param name="dt">数据</param>
        /// <param name="sheet"></param>
        /// <param name="columnList">所有 列</param>
        /// <param name="workbook"></param>
        /// <param name="style">单元格 格式、 也可以自定义</param>
        /// <param name="startRowIndex">从这个行号 开始写数据</param>
        public virtual int RenderBodyData(DataTable dt, ISheet sheet, List<ExcelColumn> columnList, XSSFWorkbook workbook, ICellStyle style, int startRowIndex)
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
                row.Height = 17 * 20;
            }
            return startRowIndex + dt.Rows.Count + 1;
        }

        public virtual void SetCellValue(ISheet sheet, DataRow dataRow, IRow row, List<ExcelColumn> columnList, XSSFWorkbook workbook, ICellStyle style, ref int cellIndex, ICellStyle PrevRowStyle)
        {
            for (var j = 0; j < columnList.Count; ++j)
            {
                var column = columnList[j];

                if (null != column.Children && column.Children.Count > 0)
                {
                    SetCellValue(sheet, dataRow, row, column.Children, workbook, style, ref cellIndex, PrevRowStyle);
                }
                else
                {
                    var cell = row.CreateCell(cellIndex);
                    var cellValue = dataRow[column.FieldName].ToString().Replace("\\r\\n", "\r\n");
                    if (columnList[j].Type == "2")
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
                        //cell.SetCellValue(Convert.ToDouble(cellValue));
                    }
                    else
                    {
                        cell.SetCellValue(cellValue);
                    }
                    SetBodyStyle(workbook, cell, column, style, PrevRowStyle);
                    cellIndex += 1;
                }
            }
        }
        /// <summary>
        /// 将excel中的数据导入到Dataset中
        /// </summary>
        /// <param name="filePath">输出文件路径</param>
        /// <param name="firstRowToColumn">是否将第一行写为列名</param>
        /// <returns>DataSet</returns>
        public static DataSet ExcelToDataTable(string filePath, bool firstRowToColumn)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = null;
                try
                {
                    var ds = new DataSet();
                    if (filePath.EndsWith(".xlsx")) // 2007+
                        workbook = new XSSFWorkbook(fs);
                    else if (filePath.EndsWith(".xls")) // 2003
                        workbook = new HSSFWorkbook(fs);
                    for (var i = 0; null != workbook && i < workbook.NumberOfSheets; i++)
                    {
                        var dt = new DataTable { TableName = workbook.GetSheetName(i) };
                        var sheet = workbook.GetSheetAt(i);

                        
                        var e = new XSSFFormulaEvaluator(workbook);
                        var firstRow = sheet.GetRow(0);
                        if (null!= firstRow)
                        {
                            var headerMaxIndex = firstRow.LastCellNum;
                            // 如果 首行为表头，以表头为准，否则以最大列数为准
                            var readMaxColumn = headerMaxIndex;
                            for (var j = 0; j < readMaxColumn; j++)//write column to Table
                            {
                                var r = sheet.GetRow(0);
                                dt.Columns.Add(firstRowToColumn ? r.GetCell(j).ToString() : j.ToString());
                            }
                            var startIndex = firstRowToColumn ? 1 : 0;
                            for (var j = startIndex; j <= sheet.LastRowNum; j++)//write row to Table
                            {
                                var r = sheet.GetRow(j);
                                if (null == r) continue;
                                var dr = dt.NewRow();
                                var haveCellIsNotNull = false;
                                for (var rn = 0; rn < readMaxColumn; rn++)
                                {
                                    var cell = r.GetCell(rn);
                                    if (cell == null)
                                    {
                                        dr[rn] = null;
                                    }
                                    else
                                    {
                                        if (cell.CellType == CellType.Formula)
                                        {
                                            var v = "";
                                            if (cell.CachedFormulaResultType.ToString() == "Error")
                                            {
                                                throw new Exception("文档中含有表达式计算错误的情况，请更正后上传！");
                                            }
                                            if (cell.CachedFormulaResultType.ToString() == "Numeric")
                                            {
                                                v = cell.NumericCellValue.ToString();
                                            }
                                            else
                                            {
                                                v = cell.RichStringCellValue.ToString();
                                            }
                                            haveCellIsNotNull = v.Length > 0;
                                            dr[rn] = v;
                                        }
                                        else
                                        {
                                            var v = cell.ToString();
                                            dr[rn] = v;
                                            haveCellIsNotNull = haveCellIsNotNull || v.Length > 0;
                                        }
                                    }
                                }
                                if(haveCellIsNotNull)
                                    dt.Rows.Add(dr);
                            }
                            ds.Tables.Add(dt);
                        }
                    }
                    return ds;
                }
                catch (Exception ex)
                {
                    throw new Exception(ex.Message);
                }
            }
        }

    }
}
