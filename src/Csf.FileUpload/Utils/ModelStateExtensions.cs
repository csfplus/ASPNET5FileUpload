using Microsoft.AspNet.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Csf.FileUpload
{
    static public class ModelStateExtensions
    {
        public static string ExpendErrors(this ModelStateDictionary modelState)
        {
            System.Text.StringBuilder sbErrors = new System.Text.StringBuilder();

            foreach (var item in modelState)
            {
                if (item.Value.Errors.Count > 0)
                {
                    sbErrors.AppendLine(string.Format("{0}:{1}", item.Key,
                        string.Join(",", item.Value.Errors.Select(a => a.ErrorMessage))));
                }
            }
            return sbErrors.ToString();
        }
    }
}