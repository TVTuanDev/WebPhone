using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Accounts.Models.Accounts;
using WebPhone.Areas.Products.Models.CateProducts;
using WebPhone.EF;

namespace WebPhone.Areas.Products.Controllers
{
    [Area("Products")]
    [Route("/product/category/")]
    public class CateProductsController : Controller
    {
        private ILogger<CateProductsController> _logger;
        private readonly AppDbContext _context;

        public CateProductsController
            (
                ILogger<CateProductsController> logger,
                AppDbContext context
            )
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var cateProduct = await _context.CategoryProducts.Where(cp => cp.IdParent == null).ToListAsync();
            await GetCateChildren(cateProduct);

            var items = new List<CategoryProduct>();
            CreateSelectItem(cateProduct, items, 0);

            return View(items);
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.CategoryProducts == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryProduct == null)
            {
                return NotFound();
            }

            return View(categoryProduct);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            await RenderCatePoduct();

            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryName, IdParent")] CateProductDTO cateProductDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(cateProductDTO);
                }

                var categoryProduct = new CategoryProduct
                {
                    CategoryName = cateProductDTO.CategoryName,
                    IdParent = cateProductDTO.IdParent
                };

                _context.Add(categoryProduct);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Tạo danh mục thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Create));
            }
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            await RenderCatePoduct();

            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if (categoryProduct == null)
            {
                TempData["Message"] = "Error: Không tìm thấy danh mục sản phẩm";
                return RedirectToAction(nameof(Index));
            }

            var cateProductDTO = new CateProductDTO
            {
                Id = categoryProduct.Id,
                CategoryName = categoryProduct.CategoryName,
                IdParent = categoryProduct.IdParent,
            };

            return View(cateProductDTO);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CategoryName, IdParent")] CateProductDTO cateProductDTO)
        {
            if (id != cateProductDTO.Id)
            {
                TempData["Message"] = "Error: Danh mục không hợp lệ";
                return View(cateProductDTO);
            }

            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                return View(cateProductDTO);
            }

            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if(categoryProduct == null)
            {
                TempData["Message"] = "Error: Không tìm thấy danh mục";
                return View(cateProductDTO);
            }

            categoryProduct.CategoryName = cateProductDTO.CategoryName;
            categoryProduct.IdParent = cateProductDTO.IdParent;
            categoryProduct.UpdateAt = DateTime.UtcNow;

            _context.Update(categoryProduct);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null || _context.CategoryProducts == null)
            {
                return NotFound();
            }

            var categoryProduct = await _context.CategoryProducts
                .FirstOrDefaultAsync(m => m.Id == id);
            if (categoryProduct == null)
            {
                return NotFound();
            }

            return View(categoryProduct);
        }

        [HttpPost("delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (_context.CategoryProducts == null)
            {
                return Problem("Entity set 'AppDbContext.CategoryProducts'  is null.");
            }
            var categoryProduct = await _context.CategoryProducts.FindAsync(id);
            if (categoryProduct == null)
            {
                TempData["Message"] = "Error: Không tìm thấy danh mục sản phẩm";
                return RedirectToAction(nameof(Index));
            }

            var cateProductChildren = await _context.CategoryProducts.Where(cp => cp.IdParent == categoryProduct.Id).ToListAsync();
            if(cateProductChildren.Count > 0)
            {
                TempData["Message"] = "Error: Không thể xóa danh mục có chứa danh mục con";
                return RedirectToAction(nameof(Index));
            }

            _context.CategoryProducts.Remove(categoryProduct);
            
            await _context.SaveChangesAsync();

            TempData["Message"] = "Success: Xóa danh mục thành công";
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryProductExists(Guid id)
        {
          return (_context.CategoryProducts?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private async Task RenderCatePoduct()
        {
            var cateProduct = await _context.CategoryProducts.Where(cp => cp.IdParent == null).ToListAsync();
            await GetCateChildren(cateProduct);

            var items = new List<CategoryProduct>();
            CreateSelectItem(cateProduct, items, 0);
            var selectList = new SelectList(items, "Id", "CategoryName");

            ViewBag.SelectList = selectList;
        }

        private async Task GetCateChildren(List<CategoryProduct> categories)
        {
            foreach (var category in categories)
            {
                // Lấy tất cả category con của category
                var categoryChildren = await _context.CategoryProducts
                                        .Include(cp => cp.CateProductChildren)
                                        .Where(cp => cp.IdParent == category.Id)
                                        .ToListAsync();

                await GetCateChildren(categoryChildren);

                category.CateProductChildren = categoryChildren;
            }
        }

        private void CreateSelectItem(List<CategoryProduct> sourse, List<CategoryProduct> des, int level)
        {
            string prefix = string.Concat(Enumerable.Repeat("--", level));
            foreach (var cateProduct in sourse)
            {
                des.Add(new CategoryProduct
                {
                    Id = cateProduct.Id,
                    CategoryName = prefix + " " + cateProduct.CategoryName,
                    CreateAt = cateProduct.CreateAt,
                    UpdateAt = cateProduct.UpdateAt,
                });
                if(cateProduct.CateProductChildren.Count > 0)
                {
                    CreateSelectItem(cateProduct.CateProductChildren.ToList(), des, ++level);
                }
            } 
        }
    }
}
