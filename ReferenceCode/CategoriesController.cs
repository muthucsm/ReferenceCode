using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using POSProfessional.Interfaces;
using POSProfessional.Models;
using System.Linq.Dynamic.Core;
using POSProfessional.Extensions;

namespace POSProfessional.Controllers
{
    
    public class CategoriesController : Controller
    {
        private readonly POSProfessionalDBContext _context;
        IsettingsRepository settingsRepository;
        public CategoriesController(POSProfessionalDBContext context, IsettingsRepository _settingsRepository)
        {
            _context = context;
            settingsRepository = _settingsRepository;
        }

        [HttpPost]
        public async Task<IActionResult> LoadTable([FromBody] DtParameters dtParameters)
        {
            var searchBy = dtParameters.Search?.Value;
            var orderCriteria = string.Empty;
            var orderAscendingDirection = true;

            if (dtParameters.Order != null)
            {
                orderCriteria = dtParameters.Columns[dtParameters.Order[0].Column].Data;
                orderAscendingDirection = dtParameters.Order[0].Dir.ToString().ToLower() == "asc";
            }
            else
            {
                orderCriteria = "Id";
                orderAscendingDirection = true;
            }

            var result = await _context.Categories.ToListAsync();

            if (!string.IsNullOrEmpty(searchBy))
            {
                result = result.Where(r => r.Catname != null && r.Catname.ToUpper().Contains(searchBy.ToUpper()) ||
                                           r.Catdesc != null && r.Catdesc.ToUpper().Contains(searchBy.ToUpper()) 
                                           )
                    .ToList();
            }

            result = orderAscendingDirection ? result.AsQueryable().OrderByDynamic(orderCriteria, DtOrderDir.Asc).ToList() : result.AsQueryable().OrderByDynamic(orderCriteria, DtOrderDir.Desc).ToList();

            var filteredResultsCount = result.Count();
            var totalResultsCount = await _context.Categories.CountAsync();

            return Json(new
            {
                draw = dtParameters.Draw,
                recordsTotal = totalResultsCount,
                recordsFiltered = filteredResultsCount,
                data = result
                    .Skip(dtParameters.Start)
                    .Take(dtParameters.Length)
                    .ToList()
            });
        }        
    
        [HttpGet("api/products/categories")]
        [Produces("application/json")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();
                if (categories == null)
                {
                    return NotFound();
                }

                return Ok(categories);                
            }
            catch (Exception)
            {
                return BadRequest();
            }

            //try
            //{
            //    var draw = HttpContext.Request.Form["draw"].FirstOrDefault();
            //    // Skiping number of Rows count  
            //    var start = Request.Form["start"].FirstOrDefault();
            //    // Paging Length 10,20  
            //    var length = Request.Form["length"].FirstOrDefault();
            //    // Sort Column Name  
            //    var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
            //    // Sort Column Direction ( asc ,desc)  
            //    var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
            //    // Search Value from (Search box)  
            //    var searchValue = Request.Form["search[value]"].FirstOrDefault();

            //    //Paging Size (10,20,50,100)  
            //    int pageSize = length != null ? Convert.ToInt32(length) : 0;
            //    int skip = start != null ? Convert.ToInt32(start) : 0;
            //    int recordsTotal = 0;

            //    // Getting all Customer data  
            //    var Data = await _context.Categories.ToListAsync();
            //    //var customerData = (from tempcustomer in _context.Categories select tempcustomer);
            //    var customerData = Data.AsQueryable();
            //    //Sorting  
            //    if (!(string.IsNullOrEmpty(sortColumn) && string.IsNullOrEmpty(sortColumnDirection)))
            //    {
            //        customerData = Data.AsQueryable().OrderBy(sortColumn + " " + sortColumnDirection);
            //    }
            //    //Search  
            //    if (!string.IsNullOrEmpty(searchValue))
            //    {
            //        customerData = customerData.Where(m => m.Catname == searchValue || m.Catdesc == searchValue);
            //    }

            //    //total number of rows count   
            //    recordsTotal = customerData.Count();
            //    //Paging   
            //    var data = customerData.Skip(skip).Take(pageSize).ToList();
            //    //Returning Json Data  
            //    return Json(new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data });

            //}
            //catch (Exception ex)
            //{
            //    throw;
            //}

        }

        // GET: Categories
        [HttpGet("api/MasterTables/Categories")]
        [Produces("application/json")]
        public async Task<IActionResult> Categories()
        {
            try
            {
                var categories = await settingsRepository.GetCategories();
                if (categories == null)
                {
                    return NotFound();
                }

                return Json(new { data = categories });
            }
            catch (Exception)
            {
                return BadRequest();
            }

            //return Ok(await _context.Categories.ToListAsync());
        }

        [HttpGet("api/MasterTables/Categories1")]
        public async Task<IActionResult> Categories1()
        {
            try
            {
                var categories = await _context.Categories.ToListAsync();
                if (categories == null)
                {
                    return NotFound();
                }

                return Ok(categories);
            }
            catch (Exception)
            {
                return BadRequest();
            }

            //return Ok(await _context.Categories.ToListAsync());
        }

        // GET: Categories               
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories
                .FirstOrDefaultAsync(m => m.Catid == id);
            if (categories == null)
            {
                return NotFound();
            }

            return View(categories);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Catid,Catname,Catdesc")] Categories categories)
        {
            if (ModelState.IsValid)
            {
                _context.Add(categories);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categories);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.FindAsync(id);
            if (categories == null)
            {
                return NotFound();
            }
            return View(categories);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Catid,Catname,Catdesc")] Categories categories)
        {
            if (id != categories.Catid)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(categories);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoriesExists(categories.Catid))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(categories);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories
                .FirstOrDefaultAsync(m => m.Catid == id);
            if (categories == null)
            {
                return NotFound();
            }

            return View(categories);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var categories = await _context.Categories.FindAsync(id);
            _context.Categories.Remove(categories);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoriesExists(int id)
        {
            return _context.Categories.Any(e => e.Catid == id);
        }
    }
}
