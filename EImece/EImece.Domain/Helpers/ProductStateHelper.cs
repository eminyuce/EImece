using EImece.Domain.Models.Enums;
using System;
using System.Collections.Generic;

namespace EImece.Domain.Helpers
{
    /// <summary>
    /// Helper for checking and resolving product sale suitability based on admin system settings.
    /// Replaces hardcoded product state checks so store administrators can dynamically configure
    /// which product states (e.g., In Stock, Pre-Order, Limited Stock, Backorder, etc.) are eligible
    /// for purchase ("In Sale" / "Satışta" badge and "Add to Cart" button).
    /// </summary>
    public static class ProductStateHelper
    {
        private static readonly ProductState[] AllNonNoneStates = new[]
        {
            ProductState.ProductInStock,
            ProductState.LimitedStock,
            ProductState.PreOrder,
            ProductState.Backorder,
            ProductState.ComingSoon,
            ProductState.AwaitingRestock,
            ProductState.Reserved,
            ProductState.ProductOutOfStock,
            ProductState.Discontinued,
            ProductState.NotForSale
        };

        /// <summary>
        /// Returns all valid ProductState values available for store configuration (excluding NONE).
        /// </summary>
        public static IReadOnlyList<ProductState> GetAllSelectableStates()
        {
            return AllNonNoneStates;
        }

        /// <summary>
        /// Gets the localized display name for a ProductState using Resource.
        /// </summary>
        public static string GetStateDisplayName(ProductState state)
        {
            switch (state)
            {
                case ProductState.ProductInStock:
                    return Resources.Resource.ProductInStock;
                case ProductState.ProductOutOfStock:
                    return Resources.Resource.ProductOutOfStock;
                case ProductState.PreOrder:
                    return Resources.Resource.PreOrderAvailable;
                case ProductState.Discontinued:
                    return Resources.Resource.Discontinued;
                case ProductState.Backorder:
                    return Resources.Resource.BackorderAvailable;
                case ProductState.ComingSoon:
                    return Resources.Resource.ComingSoon;
                case ProductState.LimitedStock:
                    return Resources.Resource.LimitedStockAvailable;
                case ProductState.Reserved:
                    return Resources.Resource.ReservedForCustomers;
                case ProductState.AwaitingRestock:
                    return Resources.Resource.AwaitingRestock;
                case ProductState.NotForSale:
                    return Resources.Resource.NotForSale;
                default:
                    return state.ToString();
            }
        }

        /// <summary>
        /// Parses a comma- or semicolon-delimited string of product state names/integers into a HashSet of ProductState enums.
        /// Falls back to Constants.DefaultSuitableForSaleProductStates when the input is null.
        /// </summary>
        public static HashSet<ProductState> ParseSuitableForSaleStates(string settingValue)
        {
            if (settingValue == null)
            {
                settingValue = Constants.DefaultSuitableForSaleProductStates;
            }

            var result = new HashSet<ProductState>();
            if (string.IsNullOrWhiteSpace(settingValue))
            {
                return result;
            }

            var tokens = settingValue.Split(new[] { ',', ';', '|', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var token in tokens)
            {
                var trimmed = token.Trim();
                if (Enum.TryParse<ProductState>(trimmed, true, out var parsedEnum))
                {
                    if (parsedEnum != ProductState.NONE)
                    {
                        result.Add(parsedEnum);
                    }
                }
                else if (int.TryParse(trimmed, out var intVal) && Enum.IsDefined(typeof(ProductState), intVal))
                {
                    var enumVal = (ProductState)intVal;
                    if (enumVal != ProductState.NONE)
                    {
                        result.Add(enumVal);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Resolves the configured suitable-for-sale product states set.
        /// If configuredStates string is provided, parses it; otherwise resolves from ISettingService / AppConfig / Setting key.
        /// </summary>
        public static HashSet<ProductState> GetConfiguredSuitableStates(string configuredStates = null)
        {
            if (!string.IsNullOrWhiteSpace(configuredStates))
            {
                return ParseSuitableForSaleStates(configuredStates);
            }

            try
            {
                var settingService = Domain.DependencyInjection.DomainServiceProvider.Instance?.GetService(typeof(EImece.Domain.Services.IServices.ISettingService)) as EImece.Domain.Services.IServices.ISettingService;
                var dbValue = settingService?.GetSettingByKey(Constants.SuitableForSaleProductStates);
                if (dbValue != null)
                {
                    return ParseSuitableForSaleStates(dbValue);
                }
            }
            catch
            {
                // Fallback when DomainServiceProvider / ISettingService is not available
            }

            var appValue = AppConfig.GetConfigString(Constants.SuitableForSaleProductStates, Constants.DefaultSuitableForSaleProductStates);
            return ParseSuitableForSaleStates(appValue);
        }

        /// <summary>
        /// Returns true if the product state and price make the product eligible for sale / cart addition.
        /// </summary>
        public static bool IsSuitableForSale(ProductState state, decimal price = 1, string configuredStates = null)
        {
            if (price <= 0)
            {
                return false;
            }

            if (state == ProductState.NONE)
            {
                return false;
            }

            var suitableSet = GetConfiguredSuitableStates(configuredStates);
            return suitableSet.Contains(state);
        }

        /// <summary>
        /// String overload for state checking.
        /// </summary>
        public static bool IsSuitableForSale(string state, decimal price = 1, string configuredStates = null)
        {
            if (price <= 0 || string.IsNullOrWhiteSpace(state))
            {
                return false;
            }

            if (!Enum.TryParse<ProductState>(state, true, out var stateEnum))
            {
                return false;
            }

            return IsSuitableForSale(stateEnum, price, configuredStates);
        }
    }
}
