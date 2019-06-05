using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bangazon.Data;
using Bangazon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Bangazon.Controllers
{
    public class ProfileController : Controller
    {

        private readonly UserManager<ApplicationUser> _userManager;
        public ProfileController(ApplicationDbContext ctx,
                          UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
            _context = ctx;
        }
        private Task<ApplicationUser> GetCurrentUserAsync() => _userManager.GetUserAsync(HttpContext.User);

        private readonly ApplicationDbContext _context;

        [Authorize]
        // Only Users logged in can view a profile
        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var user = await GetCurrentUserAsync();
            var userProducts = _context.Product.Include(p => p.ProductRatings).Where(p => p.UserId == user.Id).ToList();

            int? productRatingTotal = 0;
            double productTotal = 0;

            foreach (Product product in userProducts)
            {
                productTotal += product.ProductRatings.Count;
                productRatingTotal += product.ProductRatings.Sum(pr => pr.Rating);
            }

            ViewBag.ReviewAverage = productRatingTotal / productTotal;
            return View();
        }

        public ActionResult SettingsIndex()
        {
            return View();
        }

       



        public async Task<IActionResult> ViewPaymentTypes()
        {
            var applicationDbContext = _context.PaymentType.Include(p => p.User);
            return View(await applicationDbContext.ToListAsync());
        }

        //Get orders
        public async Task<IActionResult> OrderHistoryIndex()
        {
            var user = await GetCurrentUserAsync();
            //gets the current value of message and puts it in viewbag
            ViewBag.Message = HttpContext.Session.GetString("Message");
            //resets message to default value
            HttpContext.Session.SetString("Message", "");

            ViewBag.Orders = _context.Order
                .Include(o => o.PaymentType)
                .Where(o => o.UserId == user.Id && o.DateCompleted != null);
            
            return View();
        }
        public ActionResult MultipleOrders()
        {

            var users = _context.ApplicationUsers.Include(au => au.Orders).Where(au => au.Orders.Count() > 1);

            if (users == null)
            {
                return NotFound();
            }

            ViewBag.Users = users;

            return View(users);
        }


        // GET: Profile/CreatePaymentType
        public IActionResult CreatePaymentType()
        {

            return View();
        }

        // POST: Profile/CreatePaymentType
        // Allows users to add a new payment type

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePaymentType([Bind("PaymentTypeId,DateCreated,Description,UserId,AccountNumber")] PaymentType paymenttype)
        {
            //adds current date to payment type
            paymenttype.DateCreated = DateTime.Now;
            //removes userId from  ModelState so that it will be valid
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                var user = await GetCurrentUserAsync();
                paymenttype.UserId = user.Id;

                _context.Add(paymenttype);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.ApplicationUsers, "Id", "Id", paymenttype.UserId);
            return View(paymenttype);
        }

        //Goes to confirmation page to delete payment type
        public async Task<IActionResult> DeletePaymentType(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var paymenttype = await _context.PaymentType
                .Include(p => p.User)
                .FirstOrDefaultAsync(m => m.PaymentTypeId == id);
            if (paymenttype == null)
            {
                return NotFound();
            }

            return View(paymenttype);
        }

        // POST: Products/DeletePaymentType/5
        //Deletes a PaymentType
        [HttpPost, ActionName("DeletePaymentType")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePaymentTypeConfirmed(int id)
        {
            var paymenttype = await _context.PaymentType.FindAsync(id);
            _context.PaymentType.Remove(paymenttype);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PaymentTypeExists(int id)
        {
            return _context.PaymentType.Any(e => e.PaymentTypeId == id);
        }
    }

}


