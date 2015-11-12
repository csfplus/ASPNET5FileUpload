using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.DependencyInjection;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Csf.FileUpload.Controllers
{
    public class FileUploadController : Controller
    {
        // GET: /<controller>/
        public IActionResult Index()
        {
            BindControls();
            return View(new Utils.FileUpload());
        }

        private void BindControls()
        {
            /// 0：为空 默认，
            /// 1：按年（yyyy）,
            /// 2：按月（yyyyMM）,
            /// 3：按日（yyyyMMdd),
            /// 4：按年月(yyyy/MM),
            /// 5：按年月日(yyyy/MM/dd),
            /// 6：按扩展名(ext)
            ViewBag.DNTypes = new List<SelectListItem>()
            {
                new SelectListItem() { Value="0", Text="默认" },
                new SelectListItem() { Value="1", Text="按年（yyyy）" },
                new SelectListItem() { Value="2", Text="月（yyyyMM）" },
                new SelectListItem() { Value="3", Text="按日（yyyyMMdd)" },
                new SelectListItem() { Value="4", Text="按年月(yyyy/MM)" },
                new SelectListItem() { Value="5", Text="按年月日(yyyy/MM/dd)" },
                new SelectListItem() { Value="6", Text="按扩展名(ext)" }
            };

            /// 0：原文件名，默认
            /// 1：Guid文件名，
            /// 2：yyyyMMddHHmmss+4位随机数，
            /// 3：yyyyMMddHHmmss+原文件名
            /// 4：8位随机字符（字母+数字）
            /// 5：4位随机字符+原文件名
            ViewBag.FNTypes = new List<SelectListItem>()
            {
                new SelectListItem() { Value="0", Text="默认(原文件名)" },
                new SelectListItem() { Value="1", Text="Guid文件名" },
                new SelectListItem() { Value="2", Text="yyyyMMddHHmmss+4位随机数" },
                new SelectListItem() { Value="3", Text="yyyyMMddHHmmss+原文件名" },
                new SelectListItem() { Value="4", Text="8位随机字符（字母+数字）" },
                new SelectListItem() { Value="5", Text="4位随机字符+原文件名" },
            };
        }

        [HttpPost]
        public async Task<IActionResult> Index(Utils.FileUpload m)
        {
            BindControls();
            if (ModelState.IsValid)
            {
                m.Set(".rar|.txt|.pdf|.doc|.jpeg|.jpg|.gif", 2);
                var isok = await m.SaveFileAsAsync(HttpContext);
                if (isok)
                {
                    //todo your code

                }
                else { ModelState.AddModelError("", m.ErrorMessage); }
            }
            return View(m);
        }


        public IActionResult CreateUser()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(Models.User m)
        {
            if (ModelState.IsValid)
            {
                Utils.FileUpload upload = new Utils.FileUpload()
                {
                    SubDir = "avatar",
                    DNType = 5,
                    FromFile = Request.Form.Files.GetFile("img")    //前台文件域的name
                };
                upload.Set(".jpg|.bmp|.jpeg|.gif|.bmp");            //只能上传图片，并且大小为1M
                upload.SetFileName("csfplus");                      //自定义文件名,此时FNType无效

                var isok = await upload.SaveFileAsAsync(HttpContext);
                if (isok)
                {
                    //设置头像图片地址
                    m.Avatar = upload.TargetFilePath;

                    //todo your code
                    // ........
                    m.UserId = 123;

                    return View("Detail", m);
                }
                else
                    ModelState.AddModelError("", upload.ErrorMessage);
            }
            return View(m);
        }


        public IActionResult Js()
        {
            return View();
        }

        public IActionResult JsUpload(Utils.FileUpload upload)
        {
            if (ModelState.IsValid)
            {
                //upload.Set("")
                if (upload.SaveFileAsAsync(HttpContext).Result)
                    return Json(new { ret = 1, src = upload.TargetFilePath });
                else
                    return Json(new { ret = 0, msg = upload.ErrorMessage });
            }
            else
                return Json(new { ret = 0, msg = ModelState.ExpendErrors() });
        }

    }
}
