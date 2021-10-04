using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Openpay.EpiCommerce.AddOns.PaymentGateway.PageTypes;

namespace Openpay.EpiCommerce.AddOns.PaymentGateway.Controllers
{
    public class OpenpayLandingPageController : PageController<OpenpayLandingPage>
    {
        public ActionResult Index(OpenpayLandingPage currentPage)
        {
            return View("OpenpayLandingPage", currentPage);
        }
    }
}