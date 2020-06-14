using System;
using System.Data.Entity.Validation;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public class ExceptionHelper
    {
        public static String GetDbEntityValidationExceptionDetail(DbEntityValidationException ex)
        {
            var errorMessages = (from eve in ex.EntityValidationErrors
                                 let entity = eve.Entry.Entity.GetType().Name
                                 from ev in eve.ValidationErrors
                                 select new
                                 {
                                     Entity = entity,
                                     PropertyName = ev.PropertyName,
                                     ErrorMessage = ev.ErrorMessage
                                 });

            var fullErrorMessage = string.Join("; ", errorMessages.Select(e => string.Format("[Entity: {0}, Property: {1}] {2}", e.Entity, e.PropertyName, e.ErrorMessage)));

            var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

            return exceptionMessage;
        }
    }
}