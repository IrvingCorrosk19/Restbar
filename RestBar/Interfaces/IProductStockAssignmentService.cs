using RestBar.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestBar.Interfaces
{
    public interface IProductStockAssignmentService
    {
        Task<IEnumerable<ProductStockAssignment>> GetAllAsync(Guid? branchId = null);
        Task<ProductStockAssignment?> GetByIdAsync(Guid id);
        Task<IEnumerable<ProductStockAssignment>> GetByProductIdAsync(Guid productId, Guid? branchId = null);
        Task<IEnumerable<ProductStockAssignment>> GetByStationIdAsync(Guid stationId, Guid? branchId = null);
        Task<ProductStockAssignment> CreateAsync(ProductStockAssignment assignment);
        Task<ProductStockAssignment> UpdateAsync(Guid id, ProductStockAssignment assignment);
        Task<bool> DeleteAsync(Guid id);
    }
}

