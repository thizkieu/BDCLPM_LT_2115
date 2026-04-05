using System;
using System.IO;
using NPOI.XSSF.UserModel;
using NPOI.SS.UserModel;

namespace FootballSeleniumTest.Utilities
{
    public class ExcelHelper
    {
        private static XSSFWorkbook? _workbook;
        private static ISheet? _sheet;
        private static string? _filePath;

        private static ICellStyle? _failStyle;
        private static ICellStyle? _passStyle;

        public void OpenExcel(string path, string sheetName)
        {
            _filePath = path;
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                _workbook = new XSSFWorkbook(fs);
            }
            _sheet = _workbook.GetSheet(sheetName) ?? _workbook.CreateSheet(sheetName);

            CreateStyles();
        }

        private void CreateStyles()
        {
            if (_workbook == null) return;

            // 1. Style cho FAIL: Nền đỏ, chữ trắng, in đậm
            _failStyle = _workbook.CreateCellStyle();
            _failStyle.FillForegroundColor = IndexedColors.Red.Index;
            _failStyle.FillPattern = FillPattern.SolidForeground;
            _failStyle.Alignment = HorizontalAlignment.Center;
            _failStyle.VerticalAlignment = VerticalAlignment.Center;

            IFont fontFail = _workbook.CreateFont();
            fontFail.Color = IndexedColors.White.Index;
            // SỬA TẠI ĐÂY: Dùng IsBold thay cho Boldweight
            fontFail.IsBold = true;
            _failStyle.SetFont(fontFail);

            // 2. Style cho PASS: Nền xanh lá, chữ trắng, in đậm
            _passStyle = _workbook.CreateCellStyle();
            _passStyle.FillForegroundColor = IndexedColors.Green.Index;
            _passStyle.FillPattern = FillPattern.SolidForeground;
            _passStyle.Alignment = HorizontalAlignment.Center;
            _passStyle.VerticalAlignment = VerticalAlignment.Center;

            IFont fontPass = _workbook.CreateFont();
            fontPass.Color = IndexedColors.White.Index;
            // SỬA TẠI ĐÂY: Dùng IsBold thay cho Boldweight
            fontPass.IsBold = true;
            _passStyle.SetFont(fontPass);
        }

        public static void WriteResult(int row, string actual, string status, string screenshotPath)
        {
            if (_sheet == null || _workbook == null) return;

            try
            {
                var r = _sheet.GetRow(row) ?? _sheet.CreateRow(row);

                // Ghi Actual Result vào cột K (10)
                r.CreateCell(10).SetCellValue(actual);

                // Ghi Status vào cột L (11) và tô màu
                ICell cellL = r.CreateCell(11);
                cellL.SetCellValue(status);

                if (status == "FAIL" && _failStyle != null)
                    cellL.CellStyle = _failStyle;
                else if (status == "PASS" && _passStyle != null)
                    cellL.CellStyle = _passStyle;

                // Ghi đường dẫn ảnh vào cột M (12)
                ICell cellM = r.CreateCell(12);
                cellM.SetCellValue(screenshotPath);

                if (!string.IsNullOrEmpty(screenshotPath) && File.Exists(screenshotPath))
                {
                    var link = _workbook.GetCreationHelper().CreateHyperlink(HyperlinkType.File);
                    link.Address = screenshotPath;
                    cellM.Hyperlink = link;

                    // Định dạng link cho đẹp (Xanh + Gạch chân)
                    ICellStyle linkStyle = _workbook.CreateCellStyle();
                    IFont fontLink = _workbook.CreateFont();
                    fontLink.Underline = FontUnderlineType.Single;
                    fontLink.Color = IndexedColors.Blue.Index;
                    linkStyle.SetFont(fontLink);
                    cellM.CellStyle = linkStyle;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi ghi kết quả Excel: " + ex.Message);
            }
        }

        public void CloseExcel()
        {
            if (_workbook == null || _filePath == null) return;

            try
            {
                using (FileStream outFile = new FileStream(_filePath, FileMode.Create, FileAccess.Write))
                {
                    _workbook.Write(outFile);
                }
                _workbook.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi lưu file Excel: " + ex.Message);
            }
        }
    }
}