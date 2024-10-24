namespace WebPhone.Areas.Warehouses.Models.Warehouses
{
    public class InventoryDTO
    {
        public Guid WarehouseId { get; set; }
        public List<ProductImport> ProductImports { get; set; } = null!;
    }

    public class ProductImport
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public int ImportPrice { get; set; }
    }
}
