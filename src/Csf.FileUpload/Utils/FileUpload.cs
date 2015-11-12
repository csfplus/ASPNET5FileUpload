using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Hosting;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.IO;
using System.ComponentModel.DataAnnotations;
/*
author:csfplus
website:http://csfplus.com
verision:v20151112
*/
namespace Csf.FileUpload.Utils
{
    public class FileUpload
    {
        #region 公共属性
        /// <summary>
        /// 上传文件保存二级目录
        /// /upload/{SubDir}目录下
        /// </summary>
        [Display(Name = "二级目录")]
        public string SubDir { get; set; }

        /// <summary>
        /// 保存文件目录类型
        /// 0：为空 默认，
        /// 1：按年（yyyy）,
        /// 2：按月（yyyyMM）,
        /// 3：按日（yyyyMMdd),
        /// 4：按年月(yyyy/MM),
        /// 5：按年月日(yyyy/MM/dd),
        /// 6：按扩展名(ext)
        /// </summary>
        [Display(Name = "目录类型")]
        public int DNType { get; set; }

        /// <summary>
        /// 文件名命名规则
        /// 0：原文件名，默认
        /// 1：Guid文件名，
        /// 2：yyyyMMddHHmmss+4位随机数，
        /// 3：yyyyMMddHHmmss+原文件名
        /// 4：8位随机字符（字母+数字）
        /// 5：4位随机字符+原文件名
        /// </summary>
        [Display(Name = "命名规则")]
        public int FNType { get; set; }

        [Required]
        [Display(Name = "上传文件")]
        //[FileExtensions(Extensions = "jpg,jpeg,gif,png,bmp,rar,doc,docx,xls,xlsx,ppt,pptx")]
        public IFormFile FromFile { get; set; }
        #endregion

        #region 只读属性
        /// <summary>
        /// 出错信息 
        /// </summary>
        public string ErrorMessage { get; private set; }
        /// <summary>
        /// 上传原文件名
        /// </summary>
        public string OriginFileName { get; private set; }
        /// <summary>
        /// 最终文件名
        /// </summary>
        public string TargetFileName { get; private set; }
        /// <summary>
        /// 文件保存相对地址（包括文件名）
        /// </summary>
        public string TargetFilePath { get; private set; }
        #endregion

        #region 文件扩展名&大小
        /// <summary>
        /// 
        /// </summary>
        /// <param name="allowedExtensions">允许上传的文件扩展名类型，多个用英文|隔开，</param>
        /// <param name="allowedFileSizeInMB">允许上传的附件大小(MB)</param>
        public void Set(string allowedExtensions, int allowedFileSizeInMB = 1)
        {
            this.allowedExtensions = allowedExtensions.ToLower();
            allowedFileSize = allowedFileSizeInMB;
        }

        /// <summary>
        /// 允许上传的文件扩展名类型，多个用英文|隔开，
        /// 默认.jpg|.jpeg|.gif|.png|.bmp|.rar|.doc|.docx|.xls|.xlsx|.ppt|.pptx
        /// </summary>
        private string allowedExtensions = ".jpg|.jpeg|.gif|.png|.bmp|.rar|.doc|.docx|.xls|.xlsx|.ppt|.pptx";

        /// <summary>
        /// 允许上传的附件大小(MB)
        /// </summary>
        private int allowedFileSize = 1;
        #endregion


        private string newFileName = "";
        /// <summary>
        /// 自定义文件名,此时FNType无效
        /// </summary>
        /// <param name="newFileName">新文件名</param>
        public void SetFileName(string newFileName)
        {
            this.newFileName = newFileName;
            FNType = -1;
        }

        /// <summary>
        /// 保存文件，true：保存成功，false：出错，通过ErrorMessage查看消息
        /// </summary>
        /// <param name="context"></param>
        public async Task<bool> SaveFileAsAsync(HttpContext context)
        {
            try
            {
                OriginFileName = ContentDispositionHeaderValue.Parse(FromFile.ContentDisposition).FileName.Trim('"');

                var sExt = Path.GetExtension(OriginFileName);
                //验证扩展名
                if (!(allowedExtensions + "|").Contains(string.Format("{0}|", sExt)))
                {
                    ErrorMessage = $"文件{this.OriginFileName}扩展名不正确，只允许上传{allowedExtensions}格式的文件";
                    return false;
                }

                //验证文件大小
                Double fileSize = 0;
                using (var reader = FromFile.OpenReadStream())
                {
                    //get filesize in MB
                    fileSize = (reader.Length / 1024 / 1024);
                }
                if (fileSize > allowedFileSize)
                {
                    ErrorMessage = $"文件大小{fileSize}MB超过最大允许上传限制{allowedFileSize}MB";
                    return false;
                }

                //文件保存目录
                var path = "Upload";
                if (!string.IsNullOrEmpty(SubDir)) path = string.Format("{0}/{1}", path, SubDir);
                switch (DNType)
                {
                    case 1: path = string.Format("{0}/{1}/", path, DateTime.Now.Year); break;
                    case 2: path = string.Format("{0}/{1:yyyyMM}/", path, DateTime.Now); break;
                    case 3: path = string.Format("{0}/{1:yyyyMMdd}/", path, DateTime.Now); break;
                    case 4: path = string.Format("{0}/{1:yyyy/MM}/", path, DateTime.Now); break;
                    case 5: path = string.Format("{0}/{1:yyyy/MM/dd}/", path, DateTime.Now); break;
                    case 6: path = string.Format("{0}/{1}/", path, sExt.Trim('.').ToUpper()); break;
                }
                //保存文件名
                TargetFileName = OriginFileName;
                switch (FNType)
                {
                    case -1: TargetFileName = string.Format("{0}{1}", newFileName, sExt); break;
                    case 1: TargetFileName = string.Format("{0}{1}", Guid.NewGuid(), sExt); break;
                    case 2:
                        Random r = new Random(Guid.NewGuid().GetHashCode());
                        TargetFileName = string.Format("{0:yyyyMMddHHmmss}{1}{2}", DateTime.Now, r.Next(1000, 10000), sExt);
                        break;
                    case 3: TargetFileName = string.Format("{0}{1}", DateTime.Now, OriginFileName); break;
                    case 4: TargetFileName = string.Format("{0}{1}", getRandomChar(8), sExt); break;
                    case 5: TargetFileName = string.Format("{0}{1}", getRandomChar(4), OriginFileName); break;
                }
                TargetFilePath = Path.Combine("/", path, TargetFileName);

                var savepath = Path.Combine(context.RequestServices.GetService<IHostingEnvironment>().WebRootPath, path);
                if (!Directory.Exists(savepath)) Directory.CreateDirectory(savepath);

                savepath = Path.Combine(savepath, TargetFileName);
                ErrorMessage = savepath;
                await FromFile.SaveAsAsync(savepath);
                return true;
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// 获取指定长度{len}的字符
        /// </summary>
        /// <param name="len">长度</param>
        /// <returns></returns>
        private string getRandomChar(int len)
        {
            string str = "qwertyuiopasdfghjklzxcvbnm1234567890_-QWERTYUIOPASDFGHJKLZXCVBNM";
            string s = "";
            Random r = new Random(Guid.NewGuid().GetHashCode());
            for (int i = 0; i < len; i++)
                s += str.Substring(r.Next(64), 1);
            return s;
        }

    }
}
