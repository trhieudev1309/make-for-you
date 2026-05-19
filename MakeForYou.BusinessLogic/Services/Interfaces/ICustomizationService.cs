using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;

namespace MakeForYou.BusinessLogic.Services.Interfaces
{
    public interface ICustomizationService
    {
        // Create customization groups with options for a product
        Task<bool> CreateCustomizationGroupsAsync(long productId, List<CustomizationGroupRequest> customizationGroups);

        // Get customizations for a product
        Task<List<CustomizationGroup>> GetCustomizationsByProductIdAsync(long productId);

        // Get customizations as ViewModels (for display without circular references)
        Task<List<CustomizationGroupViewModel>> GetCustomizationViewModelsByProductIdAsync(long productId);

        // Update a customization group
        Task<bool> UpdateCustomizationGroupAsync(long customizationGroupId, CustomizationGroupRequest request);

        // Delete a customization group
        Task<bool> DeleteCustomizationGroupAsync(long customizationGroupId);

        // Delete all customizations for a product
        Task<bool> DeleteAllCustomizationsByProductIdAsync(long productId);
    }
}