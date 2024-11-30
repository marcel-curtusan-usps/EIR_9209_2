using EIR_9209_2.Models;
namespace EIR_9209_2.DataStore
{
    public interface IInMemoryInventoryRepository
    {
        Task<List<Inventory>> GetInventoryList();
        Task<Inventory> Add(Inventory inventory);
        Task<Inventory> Update(Inventory inventory);
        Task<Inventory> Delete(string id);
        Task<List<InventoryCategory>> GetInventoryCategoryList();
        Task<InventoryCategory> AddCategory(InventoryCategory inventory);
        Task<InventoryCategory> UpdateCategory(InventoryCategory inventory);
        Task<InventoryCategory> DeleteCategory(string id);
        Task<List<InventoryTracking>> GetInventoryTrackingList();
        Task<InventoryTracking> AddTracking(InventoryTracking inventory);
        Task<InventoryTracking> UpdateTracking(InventoryTracking inventory);
        Task<InventoryTracking> DeleteTracking(string id);
    }
}