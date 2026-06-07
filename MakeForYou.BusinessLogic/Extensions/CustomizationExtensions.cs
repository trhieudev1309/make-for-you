using System.Text.Json;
using MakeForYou.BusinessLogic.Entities.DTOs.Respond;

namespace MakeForYou.BusinessLogic.Extensions
{
    public static class CustomizationExtensions
    {
        public static Dictionary<long, CartCustomizationDisplayViewModel> ParseCustomizationsJson(this string? customizationsJson)
        {
            if (string.IsNullOrEmpty(customizationsJson))
                return new Dictionary<long, CartCustomizationDisplayViewModel>();

            try
            {
                var result = new Dictionary<long, CartCustomizationDisplayViewModel>();
                var jsonDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(customizationsJson);

                if (jsonDict != null)
                {
                    foreach (var kvp in jsonDict)
                    {
                        if (long.TryParse(kvp.Key, out var groupId))
                        {
                            var element = kvp.Value;
                            if (element.TryGetProperty("groupId", out var gId) &&
                                element.TryGetProperty("optionId", out var oId) &&
                                element.TryGetProperty("optionName", out var oName))
                            {
                                result[groupId] = new CartCustomizationDisplayViewModel
                                {
                                    GroupId = groupId,
                                    OptionId = long.Parse(oId.GetString() ?? "0"),
                                    OptionName = oName.GetString() ?? ""
                                };
                            }
                        }
                    }
                }

                return result;
            }
            catch
            {
                return new Dictionary<long, CartCustomizationDisplayViewModel>();
            }
        }
    }
}