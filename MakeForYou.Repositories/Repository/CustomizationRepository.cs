using MakeForYou.BusinessLogic;
using MakeForYou.BusinessLogic.Entities;
using MakeForYou.BusinessLogic.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MakeForYou.Repositories.Repository
{
    public class CustomizationRepository : ICustomizationRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomizationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomizationGroup>> GetCustomizationGroupsByProductIdAsync(long productId)
        {
            return await _context.CustomizationGroups
                .Where(cg => cg.ProductId == productId)
                .Include(cg => cg.Options)
                .OrderBy(cg => cg.DisplayOrder)
                .ToListAsync();
        }

        public async Task<CustomizationGroup?> GetCustomizationGroupByIdAsync(long customizationGroupId)
        {
            return await _context.CustomizationGroups
                .Include(cg => cg.Options.Where(o => o.Status == 1))
                .FirstOrDefaultAsync(cg => cg.CustomizationGroupId == customizationGroupId);
        }

        public async Task<CustomizationGroup> AddCustomizationGroupAsync(CustomizationGroup customizationGroup)
        {
            _context.CustomizationGroups.Add(customizationGroup);
            await _context.SaveChangesAsync();
            return customizationGroup;
        }

        public async Task<CustomizationGroup> UpdateCustomizationGroupAsync(CustomizationGroup customizationGroup)
        {
            _context.CustomizationGroups.Update(customizationGroup);
            await _context.SaveChangesAsync();
            return customizationGroup;
        }

        public async Task DeleteCustomizationGroupAsync(long customizationGroupId)
        {
            var group = await _context.CustomizationGroups.FindAsync(customizationGroupId);
            if (group != null)
            {
                _context.CustomizationGroups.Remove(group);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<CustomizationOption> AddCustomizationOptionAsync(CustomizationOption customizationOption)
        {
            _context.CustomizationOptions.Add(customizationOption);
            await _context.SaveChangesAsync();
            return customizationOption;
        }

        public async Task<CustomizationOption> UpdateCustomizationOptionAsync(CustomizationOption customizationOption)
        {
            _context.CustomizationOptions.Update(customizationOption);
            await _context.SaveChangesAsync();
            return customizationOption;
        }

        public async Task DeleteCustomizationOptionAsync(long customizationOptionId)
        {
            var option = await _context.CustomizationOptions.FindAsync(customizationOptionId);
            if (option != null)
            {
                _context.CustomizationOptions.Remove(option);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CustomizationOption>> GetCustomizationOptionsByGroupIdAsync(long customizationGroupId)
        {
            return await _context.CustomizationOptions
                .Where(co => co.CustomizationGroupId == customizationGroupId && co.Status == 1)
                .OrderBy(co => co.DisplayOrder)
                .ToListAsync();
        }

        public async Task DeleteCustomizationGroupsByProductIdAsync(long productId)
        {
            var groups = await _context.CustomizationGroups
                .Where(cg => cg.ProductId == productId)
                .ToListAsync();

            _context.CustomizationGroups.RemoveRange(groups);
            await _context.SaveChangesAsync();
        }
    }
}