using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Mvc.Rendering;
using Microsoft.Data.Entity;
using SwiftCourier.Models;
using SwiftCourier.ViewModels;
using System;
using System.Security.Claims;

namespace SwiftCourier.Controllers
{
    public class BookingsController : BaseController
    {
        private ApplicationDbContext _context;

        public BookingsController(ApplicationDbContext context)
        {
            _context = context;    
        }
        
        public async Task<IActionResult> Index()
        {
            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Package)
                .Include(b => b.Invoice)
                .Include(b => b.Service).ToListAsync();

            return View(bookings.ToListViewModel());
        }
        
        public async Task<IActionResult> Details(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Package)
                .Include(b => b.Service)
                .Include(b => b.CreatedBy)
                .SingleAsync(m => m.Id == id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking.ToDetailsViewModel());
        }

        public async Task<IActionResult> Package(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Package)
                .Include(b => b.Package.PackageLogs)
                .Include(b => b.Service)
                .Include(b => b.CreatedBy)
                .SingleAsync(m => m.Id == id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking.ToDetailsViewModel());
        }

        public async Task<IActionResult> Invoice(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Package)
                .Include(b => b.Service)
                .Include(b => b.CreatedBy)
                .SingleAsync(m => m.Id == id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking.ToDetailsViewModel());
        }

        public async Task<IActionResult> BillOfLading(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Package)
                .Include(b => b.Service)
                .Include(b => b.CreatedBy)
                .SingleAsync(m => m.Id == id);
            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking.ToDetailsViewModel());
        }

        public IActionResult Create()
        {
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name");
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name");

            return View();
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var booking = model.ToEntity();

                booking.CreatedAt = DateTime.Now;

                int userId;

                if (!int.TryParse(HttpContext.User.GetUserId(), out userId))
                {
                    //XXX:TODO Gracefully handle this
                    throw new Exception("Unable to get logged in User Id.");
                }

                booking.CreatedByUserId = userId;

                if (booking.Invoice != null)
                {
                    booking.Invoice.Status = InvoiceStatus.Pending;
                    booking.Invoice.AmountDue = booking.Invoice.Total;
                    booking.Invoice.AmountPaid = 0;
                }
                
                if(booking.Package != null)
                {
                    booking.Package.TrackingNumber = Guid.NewGuid().ToString();

                    if (booking.PickupRequired)
                    {
                        booking.Package.Status = PackageStatus.PendingPickup;
                    }
                    else
                    {
                        booking.Package.Status = PackageStatus.ReceivedByLocation;
                    }
                }

                _context.Bookings.Add(booking);

                await _context.SaveChangesAsync();

                if(booking.Invoice.BillingMode == BillingMode.PayNow)
                {
                    return RedirectToAction("Create", "Payments", new { id = booking.Id });
                }

                return RedirectToAction("Index");
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", model.CustomerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", model.ServiceId);

            return View(model);
        }
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Booking booking = await _context.Bookings
                    .Include(b => b.Invoice)
                    .Include(b => b.Package)
                    .SingleAsync(m => m.Id == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            var model = booking.ToViewModel();

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", model.CustomerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", model.ServiceId);

            return View(model);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookingViewModel model)
        {
            if (ModelState.IsValid)
            {

                var booking = await _context.Bookings
                    .Include(b => b.Invoice)
                    .Include(b => b.Package)
                    .SingleAsync(m => m.Id == model.Id);

                booking = model.UpdateEntity(booking);

                if (booking.Invoice.AmountPaid >= booking.Invoice.Total)
                {
                    booking.Invoice.Status = InvoiceStatus.Paid;
                    booking.Invoice.AmountDue = 0;
                }

                _context.Update(booking);

                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Name", model.CustomerId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Name", model.ServiceId);

            return View(model);
        }

        public async Task<IActionResult> Dispatch(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Package)
                .Include(b => b.Service)
                .SingleAsync(m => m.Id == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Dispatch(int id, DispatchViewModel model)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Package)
                .Include(b => b.Package.PackageLogs)
                .Include(b => b.Service)
                .SingleAsync(m => m.Id == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            int userId;

            if (!int.TryParse(HttpContext.User.GetUserId(), out userId))
            {
                //XXX:TODO Gracefully handle this
                throw new Exception("Unable to get logged in User Id.");
            }

            if (ModelState.IsValid)
            {
                booking.Package.AssignedToUserId = model.UserId;

                //XXX:TODO Fix when couriers are able to receive packages to their personal inventory
                // Update status to OutForDelivery when the courier has confirmed receipt of the package
                // For now, set as out for delivery when package is dispatched to courier
                //booking.Package.Status = PackageStatus.DispatchedToCourier;
                booking.Package.Status = PackageStatus.OutForDelivery;

                var packageLog = new PackageLog() {
                    PackageId = booking.Package.BookingId,
                    //XXX:TODO See comment above
                    //LogMessage = "Dispatched To Courier",
                    LogMessage = string.Format("Received By Courier {0}.", model.UserId),
                    LoggedAt = DateTime.Now
                };

                booking.Package.PackageLogs.Add(packageLog);

                _context.Update(booking);

                await _context.SaveChangesAsync();

                return RedirectToAction("Package", "Booking", new { id = booking.Id });
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");

            return View(model);
        }

        public async Task<IActionResult> Deliver(int id)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Package)
                .Include(b => b.Service)
                .SingleAsync(m => m.Id == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deliver(int id, DeliverViewModel model)
        {
            var booking = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.Invoice)
                .Include(b => b.Package)
                .Include(b => b.Package.PackageLogs)
                .Include(b => b.Service)
                .SingleAsync(m => m.Id == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            int userId;

            if (!int.TryParse(HttpContext.User.GetUserId(), out userId))
            {
                //XXX:TODO Gracefully handle this
                throw new Exception("Unable to get logged in User Id.");
            }

            if (ModelState.IsValid)
            {
                booking.Package.DeliveredByUserId = model.UserId;
                booking.Package.Status = PackageStatus.Delivered;

                var packageLog = new PackageLog()
                {
                    PackageId = booking.Package.BookingId,
                    LogMessage = string.Format("Delivered To Consignee by Courier {0}.", model.UserId),
                    LoggedAt = DateTime.Now
                };

                booking.Package.PackageLogs.Add(packageLog);

                _context.Update(booking);

                await _context.SaveChangesAsync();

                return RedirectToAction("Package", "Booking", new { id = booking.Id });
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "UserName");

            return View(model);
        }

        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return HttpNotFound();
            }

            Booking booking = await _context.Bookings
                            .Include(b => b.Customer)
                            .Include(b => b.Invoice)
                            .Include(b => b.Package)
                            .Include(b => b.Service)
                            .SingleAsync(m => m.Id == id);

            if (booking == null)
            {
                return HttpNotFound();
            }

            return View(booking.ToDetailsViewModel());
        }
        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            Booking booking = await _context.Bookings.SingleAsync(m => m.Id == id);
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
