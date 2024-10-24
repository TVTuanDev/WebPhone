using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebPhone.Areas.Admins.Models.Users;
using WebPhone.Areas.Warehouses.Models.Warehouses;
using WebPhone.EF;
using WebPhone.Models;

namespace WebPhone.Areas.Warehouses.Controllers
{
    [Area("Warehouses")]
    [Route("/warehouse/")]
    public class WarehousesController : Controller
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _context;

        public WarehousesController
            (
                AppDbContext context, 
                ILogger<WarehousesController> logger
            )
        {
            _context = context;
            _logger = logger;
        }

        #region CURD Warehouse
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var warehouses = await _context.Warehouses.ToListAsync();
            return View(warehouses);
        }

        [HttpGet("details")]
        public async Task<IActionResult> Details(Guid? id)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (warehouse == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin kho";
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        [HttpGet("create")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost("create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(WarehouseDTO warehouseDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(warehouseDTO);
                }

                var warehouse = new Warehouse
                {
                    WarehouseName = warehouseDTO.WarehouseName,
                    Address = warehouseDTO.Address,
                    Capacity = warehouseDTO.Capacity
                };

                _context.Warehouses.Add(warehouse);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Thêm mới thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("edit")]
        public async Task<IActionResult> Edit(Guid? id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin kho";
                return RedirectToAction(nameof(Index));
            }

            var warehouseDTO = new WarehouseDTO
            {
                Id = warehouse.Id,
                WarehouseName = warehouse.WarehouseName,
                Address = warehouse.Address,
                Capacity = warehouse.Capacity
            };

            return View(warehouseDTO);
        }

        [HttpPost("edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, WarehouseDTO warehouseDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    TempData["Message"] = "Error: Vui lòng nhập đầy đủ thông tin";
                    return View(warehouseDTO);
                }

                if(id != warehouseDTO.Id)
                {
                    TempData["Message"] = "Error: Mã định danh không trùng khớp";
                    return View(warehouseDTO);
                }

                var warehouse = await _context.Warehouses.FindAsync(warehouseDTO.Id);
                if (warehouse == null)
                {
                    TempData["Message"] = "Error: Không tìm thấy thông tin kho";
                    return View(warehouseDTO);
                }

                warehouse.WarehouseName = warehouseDTO.WarehouseName;
                warehouse.Address = warehouseDTO.Address;
                warehouse.Capacity = warehouseDTO.Capacity;
                warehouse.UpdateAt = DateTime.UtcNow;

                _context.Warehouses.Update(warehouse);
                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                TempData["Message"] = "Error: Lỗi hệ thống";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet("delete")]
        public async Task<IActionResult> Delete(Guid? id)
        {
            var warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (warehouse == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin kho";
                return RedirectToAction(nameof(Index));
            }

            return View(warehouse);
        }

        [HttpPost("delete"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var warehouse = await _context.Warehouses.FindAsync(id);
            if (warehouse == null)
            {
                TempData["Message"] = "Error: Không tìm thấy thông tin kho";
                return RedirectToAction(nameof(Index));
            }

            _context.Warehouses.Remove(warehouse);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Success: Xóa kho thành công";
            return RedirectToAction(nameof(Index));
        }
        #endregion

        [HttpGet("import")]
        public async Task<IActionResult> ImportWarehouse()
        {
            ViewData["Warehouses"] = await _context.Warehouses.Take(100).ToListAsync();
            ViewData["Products"] = await _context.Products.Take(100).ToListAsync();
            return View();
        }

        [HttpPost("import")]
        public async Task<JsonResult> ImportWarehouse(InventoryDTO inventoryDTO)
        {
            try
            {
                if(!ModelState.IsValid)
                    return Json(new
                    {
                        Success = false,
                        Message = "Vui lòng nhập đầy đủ thông tin"
                    });

                var warehouse = await _context.Warehouses.FindAsync(inventoryDTO.WarehouseId);
                if(warehouse == null)
                    return Json(new
                    {
                        Success = false,
                        Message = "Không tìm thấy thông tin kho"
                    });

                foreach (var productImport in inventoryDTO.ProductImports)
                {
                    var product = await _context.Products.FindAsync(productImport.ProductId);
                    if(product == null)
                        return Json(new
                        {
                            Success = false,
                            Message = "Không tìm thấy thông tin sản phẩm"
                        });

                    var inventory = new Inventory
                    {
                        ProductId = productImport.ProductId,
                        WarehouseId = warehouse.Id,
                        Quantity = productImport.Quantity,
                        ImportPrice = productImport.ImportPrice,
                    };

                    _context.Inventories.Add(inventory);
                }

                await _context.SaveChangesAsync();

                TempData["Message"] = "Success: Nhập hàng thành công";

                return Json(new
                {
                    Success = true,
                    Message = "Success",
                    Data = Url.Action(nameof(Index))
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return Json(new
                {
                    Success = false,
                    Message = "Lỗi hệ thống"
                });
            }
        }

        [HttpPost("filter")]
        public async Task<JsonResult> FilterWarehouseByName(string name)
        {
            name = string.IsNullOrEmpty(name) ? "" : name;

            var warehouses = await _context.Warehouses
                            .Where(w => w.WarehouseName.Contains(name))
                            .Take(100)
                            .Select(w => new Warehouse
                            {
                                Id = w.Id,
                                WarehouseName = w.WarehouseName,
                                Address = w.Address,
                                Capacity = w.Capacity,
                            })
                            .ToListAsync();

            return Json(new
            {
                Success = true,
                Message = "Success",
                Data = warehouses
            });
        }

        [HttpPost("create-warehouse")]
        public async Task<JsonResult> CreateWarehouse(WarehouseDTO warehouseDTO)
        {
            if (!ModelState.IsValid)
                return Json(new
                {
                    Success = false,
                    Message = "Vui lòng nhập đầy đủ thông tin"
                });

            var warehouse = new Warehouse
            {
                Id = Guid.NewGuid(),
                WarehouseName = warehouseDTO.WarehouseName,
                Address = warehouseDTO.Address,
                Capacity = warehouseDTO.Capacity,
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();

            warehouseDTO.Id = warehouse.Id;

            return Json(new
            {
                Success = true,
                Message = "Success",
                Data = warehouseDTO
            });
        }
    }
}
