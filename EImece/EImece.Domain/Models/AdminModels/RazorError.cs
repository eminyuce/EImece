namespace EImece.Domain.Models.AdminModels
{
    public class RazorError
    {
        //
        // Summary:
        //     The column number of the error location.
        public int Column { get; set; }

        //
        // Summary:
        //     The number of the error.
        public string ErrorNumber { get; set; }

        //
        // Summary:
        //     The error text of the error.
        public string ErrorText { get; set; }

        //
        // Summary:
        //     The file name of the error source.
        public string FileName { get; set; }

        //
        // Summary:
        //     Indicates whether the error is a warning.
        public bool IsWarning { get; set; }

        //
        // Summary:
        //     The line number of the error location
        public int Line { get; set; }

        public int LineAdjusted { get; set; }

        public string ErrorLine { get; set; }
    }
}