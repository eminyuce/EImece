using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EImece.Controllers
{
    public abstract class BasePaymentController : BaseController
    {
      
        private void InformCustomerToFillOutForm(Customer customer)
        {
            if (string.IsNullOrEmpty(customer.Name))
            {
                ModelState.AddModelError("customer.Name", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.Surname))
            {
                ModelState.AddModelError("customer.Surname", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.GsmNumber))
            {
                ModelState.AddModelError("customer.GsmNumber", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.Email))
            {
                ModelState.AddModelError("customer.Email", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.City))
            {
                ModelState.AddModelError("customer.City", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.Town))
            {
                ModelState.AddModelError("customer.Town", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.Country))
            {
                ModelState.AddModelError("customer.Country", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.District))
            {
                ModelState.AddModelError("customer.District", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.Street))
            {
                ModelState.AddModelError("customer.Street", Resource.MandatoryField);
            }
            if (string.IsNullOrEmpty(customer.IdentityNumber))
            {
                ModelState.AddModelError("customer.IdentityNumber", Resource.MandatoryField);
            }
            ModelState.AddModelError("", Resource.PleaseFillOutMandatoryBelowFields);
        }
    }
}