using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Entities.DTOs.Request;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;
using MakeForYou.BusinessLogic.Interfaces;
using MakeForYou.BusinessLogic.Services.Interfaces;

namespace MakeForYou.BusinessLogic.Services.Implement
{
    public class CustomizationService : ICustomizationService
    {
        private readonly ICustomizationRepository _customizationRepository;

        public CustomizationService(ICustomizationRepository customizationRepository)
        {
            _customizationRepository = customizationRepository;
        }

        public async Task<bool> CreateCustomizationGroupsAsync(long productId, List<CustomizationGroupRequest> customizationGroups)
        {
            try
            {
                foreach (var groupRequest in customizationGroups)
                {
                    var group = new CustomizationGroup
                    {
                        ProductId = productId,
                        Title = groupRequest.Title,
                        DisplayOrder = groupRequest.DisplayOrder,
                        Status = 1,
                        CreatedAt = DateTime.UtcNow
                    };

                    var createdGroup = await _customizationRepository.AddCustomizationGroupAsync(group);

                    // Add options for this group
                    foreach (var optionRequest in groupRequest.Options)
                    {
                        var option = new CustomizationOption
                        {
                            CustomizationGroupId = createdGroup.CustomizationGroupId,
                            OptionValue = optionRequest.OptionValue,
                            DisplayOrder = optionRequest.DisplayOrder,
                            Status = 1
                        };

                        await _customizationRepository.AddCustomizationOptionAsync(option);
                    }
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<CustomizationGroup>> GetCustomizationsByProductIdAsync(long productId)
        {
            return await _customizationRepository.GetCustomizationGroupsByProductIdAsync(productId);
        }

        public async Task<List<CustomizationGroupViewModel>> GetCustomizationViewModelsByProductIdAsync(long productId)
        {
            var customizations = await _customizationRepository.GetCustomizationGroupsByProductIdAsync(productId);

            return customizations.Select(cg => new CustomizationGroupViewModel
            {
                CustomizationGroupId = cg.CustomizationGroupId,
                Title = cg.Title,
                DisplayOrder = cg.DisplayOrder,
                Status = cg.Status,
                Options = cg.Options.Select(o => new CustomizationOptionViewModel
                {
                    CustomizationOptionId = o.CustomizationOptionId,
                    OptionValue = o.OptionValue,
                    DisplayOrder = o.DisplayOrder,
                    Status = o.Status
                }).ToList()
            }).ToList();
        }

        public async Task<bool> UpdateCustomizationGroupAsync(long customizationGroupId, CustomizationGroupRequest request)
        {
            try
            {
                var group = await _customizationRepository.GetCustomizationGroupByIdAsync(customizationGroupId);
                if (group == null) return false;

                group.Title = request.Title;
                group.DisplayOrder = request.DisplayOrder;

                await _customizationRepository.UpdateCustomizationGroupAsync(group);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteCustomizationGroupAsync(long customizationGroupId)
        {
            try
            {
                await _customizationRepository.DeleteCustomizationGroupAsync(customizationGroupId);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAllCustomizationsByProductIdAsync(long productId)
        {
            try
            {
                await _customizationRepository.DeleteCustomizationGroupsByProductIdAsync(productId);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}