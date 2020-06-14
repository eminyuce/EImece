﻿using System;
using System.IO;
using System.Text;
using System.Web.Mvc;
using System.Web.UI;

namespace EImece.Domain.Helpers
{
    public static class PartialViewToString
    {
        public static string RenderPartialToString(this Controller controller, string partialViewName, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            ControllerContext controllerContext = controller.ControllerContext;
            if (tempData == null)
            {
                tempData = new TempDataDictionary();
            }

            ViewEngineResult result = ViewEngines.Engines.FindPartialView(controllerContext, partialViewName);

            if (result.View != null)
            {
                StringBuilder sb = new StringBuilder();
                using (StringWriter sw = new StringWriter(sb))
                {
                    using (HtmlTextWriter output = new HtmlTextWriter(sw))
                    {
                        ViewContext viewContext = new ViewContext(controllerContext, result.View, viewData, tempData, output);
                        //  viewContext.ViewBag.location = location;
                        result.View.Render(viewContext, output);
                    }
                }

                return sb.ToString();
            }

            return String.Empty;
        }

        public static string RenderPartialToString(this Controller controller, string partialView)
        {
            return RenderPartialToString(controller, partialView, new ViewDataDictionary(), new TempDataDictionary());
        }

        public static string RenderPartialToString(this Controller controller, string partialView, object model)
        {
            return RenderPartialToString(controller, partialView, new ViewDataDictionary(model), new TempDataDictionary());
        }
    }
}