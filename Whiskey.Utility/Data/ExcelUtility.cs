using System.Collections;
using Aspose.Cells;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;


namespace YxkSabri
{
    //需要引入 Asposes.Cells.dll
    /// <summary>
    /// excel 数据操作 2015-2-5 yxk
    /// </summary>
    public class ExcelUtility
    {
        /// <summary>
        /// 将表数据导出到excel
        /// </summary>
        /// <param name="dt">要导出的数据源</param>
        /// <param name="isUserDefaultTitle">是否使用表中的列名作excel的标题头</param>
        /// <param name="titles">如果isUserDefaultTitle为false，需要提供新的列名，如果没有提供则导出的数据不显示标题头</param>
        /// <param name="path">excel的保存路径</param>
        public void ExportExcel(DataTable dt, bool isUserDefaultTitle, string[] titles, string path)
        {

            if (dt == null || dt.Rows.Count == 0)
                return;
            Workbook workbook = new Workbook();
            Worksheet sheet = workbook.Worksheets[0];
            Cells cells = sheet.Cells;

            for (int i = 1; i <= dt.Rows.Count; i++)
            {
                //不使用默认列名
                if (!isUserDefaultTitle)
                {
                    if (titles != null && titles.Length > 0)
                    {
                        for (int t = 1; t <= titles.Length; t++)
                        {
                            cells[1, t].PutValue(titles[t - 1]); // 设置列名
                            cells.SetColumnWidth(i, titles[t - 1].Length);
                        }
                    }
                }
                else //使用默认列名 
                {
                    for (int t = 1; t <= dt.Columns.Count; t++)
                    {
                        string titl = dt.Columns[t - 1].ColumnName;
                        cells[1, t].PutValue(titl);
                        cells.SetColumnWidth(t, titl.Length);
                    }

                }

                for (int j = 1; j <= dt.Columns.Count; j++)
                {
                    //将表数据转到excel
                    cells[i + 1, j].PutValue(dt.Rows[i - 1][j - 1]);
                }

            }
            workbook.Save(path);


        }
        /// <summary>
        /// 将excel导出成json格式
        /// </summary>
        /// <param name="excelPath">excel文件的路径</param>
        /// <param name="starRow">起始行</param>
        /// <param name="endRow">终止行</param>
        /// <param name="starColumn">起始列</param>
        /// <param name="endColumn">终止列</param>
        /// <returns></returns>
        public List<List<string>> ExcelToArray(string excelPath, int starRow = 0, int starColumn = 0, string endRow = null, string endColumn = null)
        {
            if (!(File.Exists(excelPath) && IsExcelFile(Path.GetExtension(excelPath))))
            {
                return null;
            }
            else
            {
                Workbook wb = new Workbook(excelPath);
                Worksheet ws = wb.Worksheets[0];
                var rows = ws.Cells.Rows;
                if (starColumn < 0)
                    starColumn = 0;
                if (starRow < 0)
                    starRow = 0;
                int endrow = ws.Cells.MaxRow,
                endcolumn = ws.Cells.MaxColumn;

                if (endRow != null && endRow == "")
                    endrow = int.Parse(endRow);
                if (!string.IsNullOrEmpty(endColumn))
                    endcolumn = int.Parse(endColumn);
                List<List<string>> li = new List<List<string>>();
                for (; starRow <= endrow; starRow++)
                {
                    List<string> rowtem = new List<string>();
                    var row = rows[starRow];
                    int starColu = starColumn;
                    for (; starColu <= endcolumn; starColu++)
                    {
                        var te = row[starColu].Value;
                        rowtem.Add(te == null ? "" : te.ToString().Trim().ToUpper());
                        rowtem.Add((starRow + 1).ToString());
                        rowtem.Add((starColu + 1).ToString());
                    }
                    li.Add(rowtem);
                }
                return li;
            }

        }


        /// <summary>
        /// 将excel导出成json格式
        /// </summary>
        /// <param name="excelPath">excel文件的路径</param>
        /// <param name="starRow">起始行</param>
        /// <param name="endRow">终止行</param>
        /// <param name="starColumn">起始列</param>
        /// <param name="endColumn">终止列</param>
        /// <returns></returns>
        public List<List<string>> ExcelToArray(string excelPath)
        {
            if (!(File.Exists(excelPath) && IsExcelFile(Path.GetExtension(excelPath))))
            {
                return null;
            }
            else
            {
                Workbook wb = new Workbook(excelPath);
                Worksheet ws = wb.Worksheets[0];
                var rows = ws.Cells.Rows;
                int endrow = ws.Cells.MaxRow,
                endcolumn = ws.Cells.MaxColumn;


                List<List<string>> rowList = new List<List<string>>();
                for (var rowIndex = 0; rowIndex <= endrow; rowIndex++)
                {
                    List<string> rowItem = new List<string>();

                    for (var columnIndex = 0; columnIndex <= endcolumn; columnIndex++)
                    {
                        var value = rows[rowIndex][columnIndex].Value;
                        rowItem.Add(value == null ? "" : value.ToString());

                    }
                    rowList.Add(rowItem);
                }
                return rowList;
            }

        }



        public bool ExportMulitExcelSheet(List<List<List<string>>> lists, string filepath, params string[] sheetNames)
        {
            Aspose.Cells.Workbook workbook = new Aspose.Cells.Workbook();
            string err = "";
            try
            {
                for (int i = 0; i < lists.Count; i++)
                {
                    Worksheet sheet = workbook.Worksheets[0];
                    if (i >= 1)
                        sheet = workbook.Worksheets.Add("sheet" + i + 1);
                    if (sheetNames.Count() >= i + 1)
                        sheet.Name = sheetNames[i];

                    Aspose.Cells.Cells cells = sheet.Cells;
                    ListsToExcel(lists[i], sheet, cells, out err);

                }
                workbook.Save(filepath);
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public void ListsToExcel(List<List<string>> lists, Worksheet sheet, Cells cells, out string error)
        {
            error = "";
            int nRow = 0;
            sheet.Pictures.Clear();
            cells.Clear();
            foreach (IList list in lists)
            {
                for (int i = 0; i <= list.Count - 1; i++)
                {
                    try
                    {
                        if (list[i].GetType().ToString() == " System.Drawing.Bitmap ")
                        {
                            // 插入图片数据 
                            System.Drawing.Image image = (System.Drawing.Image)list[i];
                            MemoryStream mstream = new MemoryStream();
                            image.Save(mstream, System.Drawing.Imaging.ImageFormat.Jpeg);
                            sheet.Pictures.Add(nRow, i, mstream);
                        }
                        else
                        {
                            cells[nRow, i].PutValue(list[i]);
                        }
                    }
                    catch (System.Exception e)
                    {
                        error = error + e.Message;
                    }

                }
                nRow++;
            }

        }


        private bool IsExcelFile(string excelExtention)
        {
            return excelExtention == ".xls" || excelExtention == ".xlsx";
        }
    }
}
