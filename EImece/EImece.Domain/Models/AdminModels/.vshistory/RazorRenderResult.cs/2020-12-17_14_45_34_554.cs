using EImece.Domain.Helpers;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.IO;

namespace EImece.Domain.Models.AdminModels
{
    public class RazorRenderResult
    {
        public String Source { get; set; }

        public String Result { get; set; }
        public Exception GeneralError { get; set; }

        public TemplateCompilationException templateCompilationException { get; set; }

        public List<RazorError> RazorErrors
        {
            get
            {
                var sourceLines = Source.ToStr().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
                var resultErrors = new List<RazorError>();
                if (templateCompilationException != null)
                {
                    foreach (var item in templateCompilationException.CompilerErrors)
                    {
                        resultErrors.Add(NewMethod(item));
                    }
                }

                return resultErrors;
            }
        }

        private   RazorError NewMethod( RazorEngineCompilerError item)
        {
            var fileLines = File.ReadAllLines(item.FileName);
            int lineOff = 0;
            foreach (var line in fileLines)
            {
                lineOff++;
                if (line.IndexOf("public override void Execute()", StringComparison.InvariantCultureIgnoreCase) > 0)
                {
                    break;
                }
            }
            var i = new RazorError();

            i.Column = item.Column;
            i.ErrorNumber = item.ErrorNumber;
            i.Line = item.Line;
            i.LineAdjusted = item.Line - lineOff;
            i.ErrorLine = fileLines[item.Line - 1];
            i.IsWarning = item.IsWarning;
            i.ErrorText = item.ErrorText;
     
        }
    }
}