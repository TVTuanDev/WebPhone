﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Products.Models.CateProducts;
using WebPhone.Areas.Products.Models.Products;
using WebPhone.Controllers;
using WebPhone.EF;

namespace WebPhone.Areas.Products.Controllers
{
    [Area("Products")]
    [Route("/product/")]
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly AppDbContext _context;
        private readonly MediaHandle _mediaHandle;

        private readonly int ITEM_PER_PAGE = 10;

        public ProductsController
            (
                ILogger<ProductsController> logger,
                AppDbContext context,
                MediaHandle mediaHandle
            )
        {
            _logger = logger;
            _context = context;
            _mediaHandle = mediaHandle;
        }

        [HttpGet()]
        public async Task<IActionResult> Index(int page = 1)
        {
            try
            {
                // Lấy tổng số lượng sản phẩm
                var total = await _context.Products.CountAsync();
                // Chia ra số trang theo số lượng hiện thị sản phẩm trên mỗi trang
                var countPage = (int)Math.Ceiling((double)total / ITEM_PER_PAGE);
                countPage = countPage < 1 ? 1 : countPage;
                ViewBag.CountPage = countPage;
                // Nếu page truyền vào > số trang thì lấy số trang
                page = page < 1 ? 1 : page;
                page = page > countPage ? countPage : page;

                var listProduct = await (from product in _context.Products
                                         orderby product.Price ascending
                                         select (new Product
                                         {
                                             Id = product.Id,
                                             Avatar = product.Avatar,
                                             ProductName = product.ProductName,
                                             Price = product.Price,
                                             Discount = product.Discount
                                         }))
                                         .Skip((page - 1) * ITEM_PER_PAGE)
                                         .Take(ITEM_PER_PAGE)
                                         .ToListAsync();

                return View(listProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null || _context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.CategoryProduct)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpGet("create")]
        public async Task<IActionResult> Create()
        {
            await RenderCatePoduct();

            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDTO productDTO)
        {
            await RenderCatePoduct();
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(productDTO);
                }

                var product = new Product
                {
                    ProductName = productDTO.ProductName,
                    Avatar = await _mediaHandle.UploadImageAsync(productDTO.Avatar),
                    Description = productDTO.Description,
                    Price = (int)Math.Round((double)productDTO.Price / 1000) * 1000,
                    Discount = (int)Math.Round((double)(productDTO.Discount ?? 0) / 1000) * 1000,
                    CategoryId = productDTO.CategoryId
                };

                _context.Add(product);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Thêm mới sản phẩm thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                    {
                        TempData["Message"] = "Error: Tên sản phẩm đã được sử dụng";
                        return View(productDTO);
                    }
                }

                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Create));
            }
        }

        [HttpGet("Edit")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            await RenderCatePoduct();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Message"] = "Error: Không tìm thấy sản phẩm";
                return RedirectToAction(nameof(Index));
            }

            var productDTO = new ProductDTO
            {
                Id = product.Id,
                ProductName = product.ProductName,
                Description = product.Description,
                Price = product.Price,
                Discount = product.Discount,
                CategoryId = product.CategoryId
            };

            return View(productDTO);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductDTO productDTO)
        {
            await RenderCatePoduct();

            if (id != productDTO.Id)
            {
                TempData["Message"] = "Error: Danh mục không hợp lệ";
                return View(productDTO);
            }

            if (!ModelState.IsValid)
            {
                TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                return View(productDTO);
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Message"] = "Error: Không tìm thấy sản phẩm";
                return View(productDTO);
            }

            product.ProductName = productDTO.ProductName;
            product.Avatar = await _mediaHandle.UploadImageAsync(productDTO.Avatar);
            product.Description = productDTO.Description;
            product.Price = productDTO.Price;
            product.Discount = productDTO.Discount;
            product.CategoryId = productDTO.CategoryId;
            product.UpdateAt = DateTime.UtcNow;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Success: Sửa sản phẩm thành công";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(m => m.Id == id);

            if (product == null)
            {
                TempData["Message"] = "Error: Không tìm thấy sản phẩm";
                return RedirectToAction(nameof(Index));
            }

            return View(product);
        }

        [HttpPost("delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["Message"] = "Error: Không tìm thấy sản phẩm";
                return RedirectToAction(nameof(Delete));
            }

            _mediaHandle.DeleteImage(product.Avatar);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Success: Xóa sản phẩm thành công";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("random/{count}")]
        public async Task<IActionResult> RamdomProduct(int count)
        {
            try
            {
                var cateProIdList = await _context.CategoryProducts.Select(cp => cp.Id).ToListAsync();
                Random random = new Random();

                var countName = await _context.Products.CountAsync();

                for (int i = 1; i <= count; i++)
                {
                    var index = random.Next(cateProIdList.Count);
                    var cateProId = cateProIdList[index];
                    var price = random.Next(5000000, 50000000);
                    var discount = random.Next((price / 2), price);

                    var product = new Product
                    {
                        ProductName = $"Sản phẩm {++countName}",
                        Avatar = $"https://picsum.photos/100/50?random={countName}",
                        Description = $"Thông tin sản phẩm {i}",
                        Price = (int)Math.Round((double)price / 1000) * 1000,
                        Discount = (int)Math.Round((double)discount / 1000) * 1000,
                        CategoryId = cateProId,
                    };

                    _context.Products.Add(product);
                }
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Thêm sản phẩm thành công";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (ex.InnerException is SqlException sqlEx)
                {
                    // 2601: Cannot insert duplicate key row
                    // 2627: Violation of UNIQUE KEY constraint
                    if (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                    {
                        TempData["Message"] = "Error: Có sản phẩm bị trùng tên";
                        return RedirectToAction(nameof(Index));
                    }
                }

                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("random/delete/{count}")]
        public async Task<IActionResult> DeleteProduct(int count)
        {
            try
            {
                var products = await _context.Products.Take(count).ToListAsync();

                _context.Products.RemoveRange(products);

                //products.ForEach(product =>
                //{
                //    _context.Products.Remove(product);
                //});
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Xóa sản phẩm thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Index));
            }
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
                if (cateProduct.CateProductChildren.Count > 0)
                {
                    CreateSelectItem(cateProduct.CateProductChildren.ToList(), des, ++level);
                }
            }
        }
    }
}
