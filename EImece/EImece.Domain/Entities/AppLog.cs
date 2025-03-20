using System;

namespace EImece.Domain.Entities
{
    [Serializable]
    public class AppLog
    {
        // Entity annotions
        //[DataType(DataType.Text)]
        //[StringLength(100, ErrorMessage = "TestColumnName cannot be longer than 100 characters.")]
        //[Display(Name ="TestColumnName")]
        //[Required(ErrorMessage ="TestColumnName")]
        //[AllowHtml]
        public int Id { get; set; }

        public string EventDateTime { get; set; }
        public string EventLevel { get; set; }
        public string UserName { get; set; }
        public string MachineName { get; set; }
        public string EventMessage { get; set; }
        public string ErrorSource { get; set; }
        public string ErrorClass { get; set; }
        public string ErrorMethod { get; set; }
        public string ErrorMessage { get; set; }
        public string InnerErrorMessage { get; set; }
        public DateTime CreatedDate { get; set; }

        public string ToLogStr()
        {
            string result = EventDateTime;

            if (!string.IsNullOrEmpty(EventLevel))
                result += " - " + EventLevel;

            if (!string.IsNullOrEmpty(ErrorSource))
                result += " - " + ErrorSource;

            if (!string.IsNullOrEmpty(ErrorClass))
                result += " - " + ErrorClass;

            if (!string.IsNullOrEmpty(ErrorMethod))
                result += " - " + ErrorMethod;

            if (!string.IsNullOrEmpty(UserName))
                result += " - " + UserName;

            if (!string.IsNullOrEmpty(MachineName))
                result += " - " + MachineName;

            if (!string.IsNullOrEmpty(EventMessage))
                result += " - " + EventMessage;

            if (!string.IsNullOrEmpty(InnerErrorMessage))
                result += " - " + InnerErrorMessage;

            return result.Trim() + "\r\n";
        }
    }
}