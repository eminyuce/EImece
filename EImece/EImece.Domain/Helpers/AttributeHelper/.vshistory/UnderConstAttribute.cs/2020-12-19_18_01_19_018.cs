﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EImece.Domain.Helpers.AttributeHelper
{
    public class UnderConstAttribute : ActionFilterAttribute
    {
        private readonly static AppSettingsReader _reader;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            _reader = new AppSettingsReader();
            if (_reader.GetValue("UnderConst", typeof(bool)))
                filterContext.HttpContext.Response.Redirect("/Underconst.html");
        }
    }
}
