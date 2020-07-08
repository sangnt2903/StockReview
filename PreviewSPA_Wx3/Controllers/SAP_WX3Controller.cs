using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using OfficeOpenXml;
using PreviewSPA_Wx3.Models;

namespace PreviewSPA_Wx3.Controllers
{
    public class SAP_WX3Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public bool CheckGetExtentionsFileIsSupported(IFormFile f)
        {
            string[] extentionsFile = new string[2] {
            ".xlsx",
            ".csv"
            };

            string extentionFile = Path.GetExtension(f.FileName).ToLower();

            if (Array.IndexOf(extentionsFile, extentionFile) != -1)
            {
                return true;
            }
            return false;
        }

        public bool checkExsistingInListSAP(List<SAPModel> saps, SAPModel model)
        {
            return saps.SingleOrDefault(p=>p.Batch == model.Batch && p.LocalStorage == model.LocalStorage && p.MaterialCode == model.LocalStorage) != null;
        }
        public bool checkExsistingInListWx3(List<WX3Model> wx3s, WX3Model model)
        {
            return wx3s.SingleOrDefault(p => p.LotNo == model.LotNo && p.VendorCode == model.VendorCode && p.ProductCode == model.ProductCode) != null;
        }

        public SAPModel UpdateModelSAPExsits(SAPModel res, SAPModel model)
        {
            SAPModel update = res;
            update.Quantity = new SAPQtyModel
            {
                Unrestricted = res.Quantity.Unrestricted + model.Quantity.Unrestricted,
                QualityInspection = res.Quantity.QualityInspection + model.Quantity.QualityInspection,
                Blocked = res.Quantity.Blocked + model.Quantity.Blocked,
                TransitAndTransfer = res.Quantity.TransitAndTransfer + model.Quantity.TransitAndTransfer
            };
            return update;
        }
        public WX3Model UpdateModelWx3Exsits(WX3Model res, WX3Model model)
        {
            WX3Model update = res;
            update.AvailQty += model.AvailQty;
            update.ResvQty += model.ResvQty;
            return update;
        }


        public bool IsStorageLocationIlegal(string sloc)
        {
            List<String> slocs = new List<String>() { "FG03", "FG04", "PM04" };
            return slocs.SingleOrDefault(p=>p.Equals(sloc)) != null;
        }

        [HttpPost]
        public IActionResult UploadSAP(IFormFile fExcelSAP)
        {
            // import data
            if (fExcelSAP != null && CheckGetExtentionsFileIsSupported(fExcelSAP))
            {
                using (var stream = new MemoryStream())
                {
                    fExcelSAP.CopyTo(stream);
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
                        if (workSheet != null)
                        {
                            // List to ADD database
                            List<SAPModel> saps = new List<SAPModel>();
                            int totalRows = workSheet.Dimension.Rows;
                            for (int i = 2; i <= totalRows; i++)
                            {
                                
                                SAPModel model = new SAPModel
                                {
                                    MaterialCode = workSheet.Cells[i, 1].Value.ToString(),
                                    MaterialDesc = String.Empty,
                                    LocalStorage = workSheet.Cells[i, 3].Value.ToString() != String.Empty ? workSheet.Cells[i, 3].Value.ToString(): "_" ,
                                    Batch = workSheet.Cells[i, 5].Value.ToString() != String.Empty ? workSheet.Cells[i, 5].Value.ToString() : "_",
                                    Quantity = new SAPQtyModel
                                    {
                                        Unrestricted = Convert.ToInt32(workSheet.Cells[i, 7].Value),
                                        QualityInspection = Convert.ToInt32(workSheet.Cells[i, 13].Value),
                                        Blocked = Convert.ToInt32(workSheet.Cells[i, 17].Value),
                                        TransitAndTransfer = Convert.ToInt32(workSheet.Cells[i, 11].Value)
                                    }
                                };
                                if (IsStorageLocationIlegal(model.LocalStorage))
                                {
                                    if (checkExsistingInListSAP(saps, model))
                                    {

                                        SAPModel res = saps.SingleOrDefault(p => p.Batch == model.Batch && p.LocalStorage == model.LocalStorage && p.MaterialCode == model.LocalStorage);
                                        res = UpdateModelSAPExsits(res, model);
                                    }
                                    else
                                    {
                                        SAPModel res = new SAPModel()
                                        {
                                            MaterialCode = model.MaterialCode,
                                            MaterialDesc = model.MaterialDesc,
                                            Batch = model.Batch,
                                            LocalStorage = model.LocalStorage,
                                            Quantity = model.Quantity
                                        };
                                        saps.Add(res);
                                    }
                                } 
                            }

                            HttpContext.Session.SetComplexData("SAPList", saps.OrderBy(p=>p.LocalStorage).ToList());
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.Error = "Không tìm thấy sheet cần thiết của hệ thống để import dữ liệu! Vui lòng kiểm tra tên của Sheet theo yêu cầu của hệ thống !";
                            return View("Index");
                        }
                    }

                }
            }       
            else
            {
                ViewBag.Error = "Vui lòng chọn file excel hoặc định dạng file của bạn không được hỗ trợ. Lưu ý những file được hỗ trợ bao gồm : .xlsx, .csv ";
                return View("Index");
            }
        }

        [HttpPost]
        public IActionResult UploadWX3(IFormFile fExcelWX3)
        {
            if (fExcelWX3 != null && CheckGetExtentionsFileIsSupported(fExcelWX3))
            {
                using (var stream = new MemoryStream())
                {
                    fExcelWX3.CopyTo(stream);
                    using (ExcelPackage package = new ExcelPackage(stream))
                    {
                        ExcelWorksheet workSheet = package.Workbook.Worksheets.First();
                        if (workSheet != null)
                        {
                            // List to ADD database
                            List<WX3Model> wx3s = new List<WX3Model>();
                            int totalRows = workSheet.Dimension.Rows;
                            for (int i = 4; i <= totalRows; i++)
                            {
                                if(workSheet.Cells[i, 4].Value != null && workSheet.Cells[i, 4].Value.ToString() == "STORAGE")
                                {
                                    WX3Model model = new WX3Model
                                    {
                                        ProductCode = workSheet.Cells[i, 7].Value.ToString(),
                                        ProductDesc = workSheet.Cells[i, 8].Value.ToString(),
                                        VendorCode = workSheet.Cells[i, 6].Value.ToString() != String.Empty ? workSheet.Cells[i, 6].Value.ToString() : "_",
                                        LotNo = workSheet.Cells[i, 9].Value != null ? workSheet.Cells[i, 9].Value.ToString() : "_",
                                        AvailQty = Convert.ToInt32(workSheet.Cells[i, 32].Value.ToString()),
                                        ResvQty = Convert.ToInt32(workSheet.Cells[i, 33].Value.ToString())
                                    };

                                    if (IsStorageLocationIlegal(model.VendorCode))
                                    {
                                        if (checkExsistingInListWx3(wx3s, model))
                                        {

                                            WX3Model res = wx3s.SingleOrDefault(p => p.LotNo == model.LotNo && p.VendorCode == model.VendorCode && p.ProductCode == model.ProductCode);
                                            res = UpdateModelWx3Exsits(res, model);
                                        }
                                        else
                                        {
                                            WX3Model res = new WX3Model()
                                            {
                                                ProductCode = model.ProductCode,
                                                ProductDesc = model.ProductDesc,
                                                VendorCode = model.VendorCode,
                                                LotNo = model.LotNo,
                                                AvailQty = Convert.ToInt32(workSheet.Cells[i, 32].Value.ToString()),
                                                ResvQty = Convert.ToInt32(workSheet.Cells[i, 33].Value.ToString())
                                            };
                                            wx3s.Add(res);
                                        }
                                    }
                                }
                            }
                            HttpContext.Session.SetComplexData("WX3List", wx3s);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            ViewBag.Error = "Không tìm thấy sheet cần thiết của hệ thống để import dữ liệu! Vui lòng kiểm tra tên của Sheet theo yêu cầu của hệ thống !";
                            return View("Index");
                        }
                    }

                }
            }
            else
            {
                ViewBag.Error = "Vui lòng chọn file excel hoặc định dạng file của bạn không được hỗ trợ. Lưu ý những file được hỗ trợ bao gồm : .xlsx, .csv ";
                return RedirectToAction("Index");
            }
            return RedirectToAction("Index");
        }
    }
}