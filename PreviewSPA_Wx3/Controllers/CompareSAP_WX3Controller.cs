using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using PreviewSPA_Wx3.Models;

namespace PreviewSPA_Wx3.Controllers
{
    public class CompareSAP_WX3Controller : Controller
    {
        public bool IsExistingSessionListsForCompare()
        {
            if (HttpContext.Session.GetComplexData<List<SAPModel>>("SAPList") != null && HttpContext.Session.GetComplexData<List<WX3Model>>("WX3List") != null)
                return true;
            return false;
        }

        public List<SAP_WX3ModelView> GetListAfterCompare()
        {
            List<SAPModel> saps = HttpContext.Session.GetComplexData<List<SAPModel>>("SAPList");
            List<WX3Model> wx3s = HttpContext.Session.GetComplexData<List<WX3Model>>("WX3List");
            List<SAP_WX3ModelView> res = new List<SAP_WX3ModelView>();
            foreach (var wx3 in wx3s)
            {
                var r = saps.SingleOrDefault(p=> p.Batch == wx3.LotNo && p.MaterialCode == wx3.ProductCode && p.LocalStorage == wx3.VendorCode);
                if(r != null)
                {
                    res.Add(new SAP_WX3ModelView
                    {
                        MaterialCode = r.MaterialCode,
                        MaterialDesc = r.MaterialDesc,
                        LocalStorage = r.LocalStorage,
                        Batch = r.Batch,
                        AllStockSAP = r.Quantity.AllStock,
                        AllStockWX3 = wx3.AllStock
                    });
                }
            }
            return res.ToList();
        }

        public IActionResult Index()
        {
            List<SAP_WX3ModelView> res = new List<SAP_WX3ModelView>();
            if(IsExistingSessionListsForCompare())
            {
                res = GetListAfterCompare();
            }
            return View(res);
        }

        public IActionResult ExportToExcel()
        {
            if (IsExistingSessionListsForCompare())
            {
                List<SAP_WX3ModelView> dataReport = GetListAfterCompare();
                //xuất ra excel dùng eplus
                var stream = new MemoryStream();
                using (var package = new ExcelPackage(stream))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Compare SAP and Wx3 Stock");
                    worksheet.Cells[1, 4].Value = "Báo cáo so sánh tồn kho giữa SAP và WX3 ngày " + DateTime.Now.ToString("dd/MM/yyyy");

                    //custome size
                    worksheet.Row(2).Height = 20;
                    worksheet.Column(1).Width = 10;
                    worksheet.Column(2).Width = 20;
                    worksheet.Column(3).Width = 30;
                    for (int i = 4; i <= 8; i++)
                    {
                        worksheet.Column(i).Width = 30;
                        worksheet.Column(i).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    }

                    //custom color
                    for (int i = 1; i <= 8; i++)
                    {
                        string colorSet = null;
                        worksheet.Cells[2, i].Style.Fill.PatternType = ExcelFillStyle.Solid;

                        if (i != 8)
                            colorSet = "#e6cfe5";
                        else colorSet = "#e0dcb4";
                        
                        Color colFromHex = System.Drawing.ColorTranslator.FromHtml(colorSet);
                        worksheet.Cells[2, i].Style.Fill.BackgroundColor.SetColor(colFromHex);
                    }

                    //custom format
                    worksheet.Row(2).Style.Font.Bold = true;

                    worksheet.Cells[2, 1].Value = "STT";
                    worksheet.Cells[2, 2].Value = "Material Code";
                    worksheet.Cells[2, 3].Value = "Material Desc";
                    worksheet.Cells[2, 4].Value = "Local Storage";
                    worksheet.Cells[2, 5].Value = "Batch";
                    worksheet.Cells[2, 6].Value = "All Stock SAP";
                    worksheet.Cells[2, 7].Value = "All Stock WX3";
                    worksheet.Cells[2, 8].Value = "Diff";

                    //body of table  
                    //  
                    int recordindex = 3;
                    int idx = 1;
                    foreach (var data in dataReport)
                    {
                        worksheet.Cells[recordindex, 1].Value = idx;
                        worksheet.Cells[recordindex, 2].Value = data.MaterialCode;
                        worksheet.Cells[recordindex, 3].Value = data.MaterialDesc;
                        worksheet.Cells[recordindex, 4].Value = data.LocalStorage;
                        worksheet.Cells[recordindex, 5].Value = data.Batch;
                        worksheet.Cells[recordindex, 6].Value = data.AllStockSAP;
                        worksheet.Cells[recordindex, 7].Value = data.AllStockWX3;

                        if(data.Diff >= 0)
                        {
                            worksheet.Cells[recordindex, 8].Value = data.Diff;
                        } else
                        {
                            worksheet.Cells[recordindex, 8].Style.Fill.PatternType = ExcelFillStyle.Solid;
                            worksheet.Cells[recordindex, 8].Style.Fill.BackgroundColor.SetColor(System.Drawing.ColorTranslator.FromHtml("#de3c57"));
                            worksheet.Cells[recordindex, 8].Value = data.Diff;
                        }
                        
                        recordindex++;
                        idx++;
                    }

                    package.Save();
                }
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CompareSAP&WX3.xlsx");
            }
            return RedirectToAction("Index");
        }
    }
}