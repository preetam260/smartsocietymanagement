using SmartSociety.Domain.Models;

namespace SmartSociety.Repository.Interfaces;

public interface IFacilityRepository: IRepository<Facility>
{
    Task<IEnumerable<Facility>> GetActiveFacilitiesAsync();
    Task<Facility?> GetByIdForUpdateAsync(Guid id);
}
