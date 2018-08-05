using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebAppMongoDb.Models;

namespace WebAppMongoDb.Controllers
{
    public class HomeController : Controller
    {

        private readonly ComputerContext db = new ComputerContext();

        public async Task<ActionResult> Index(ComputerFilter filter)
        {
            var computers = await db.GetComputers(filter.Year, filter.ComputerName);
            var model = new ComputerList { Computers = computers, Filter = filter };
            return View(model);
        }

        public ActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Create(Computer c)
        {
            if (ModelState.IsValid)
            {
                await db.Create(c);
                return RedirectToAction("Index");
            }
            return View(c);
        }

        public async Task<ActionResult> Edit(string id)
        {
            Computer c = await db.GetComputer(id);
            if (c == null)
                return HttpNotFound();
            return View(c);
        }
        [HttpPost]
        public async Task<ActionResult> Edit(Computer c)
        {
            if (ModelState.IsValid)
            {
                await db.Update(c);
                return RedirectToAction("Index");
            }
            return View(c);
        }
        public async Task<ActionResult> Delete(string id)
        {
            await db.Remove(id);
            return RedirectToAction("Index");
        }
    }
}