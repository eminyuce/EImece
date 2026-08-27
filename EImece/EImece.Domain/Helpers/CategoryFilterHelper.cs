using EImece.Domain.Entities;
using EImece.Domain.Helpers.Extensions;
using EImece.Domain.Models.FrontModels;
using Newtonsoft.Json;
using Resources;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EImece.Domain.Helpers
{
    public class CategoryFilterHelper
    {
        private readonly List<CategoryFilterType> categoryFilterTypes;
        private readonly List<string> selectedFilters;
        private readonly Setting priceFilterSetting;
        private readonly Models.DTOs.SettingDto priceFilterSettingDto;
        private readonly Models.DTOs.Storefront.SettingValueDto priceFilterSettingValueDto;

        public CategoryFilterHelper(Setting priceFilterSetting)
        {
            this.priceFilterSetting = priceFilterSetting;
        }

        public CategoryFilterHelper(Models.DTOs.SettingDto priceFilterSettingDto)
        {
            this.priceFilterSettingDto = priceFilterSettingDto;
        }

        public CategoryFilterHelper(Models.DTOs.Storefront.SettingValueDto priceFilterSettingValueDto)
        {
            this.priceFilterSettingValueDto = priceFilterSettingValueDto;
        }

        public CategoryFilterHelper(List<CategoryFilterType> categoryFilterTypes, List<string> selectedFilters)
        {
            this.categoryFilterTypes = categoryFilterTypes;
            this.selectedFilters = selectedFilters;
        }

        public ICollection<Product> FilterProductsByPrice(ICollection<Product> products)
        {
            bool hasPriceFilter = false;
            List<Product> filteredProducts = new List<Product>();
            foreach (var categoryFilter in categoryFilterTypes)
            {
                foreach (var filterId in selectedFilters)
                {
                    if (categoryFilter.CategoryFilters.Any(t => t.
                    CategoryFilterId.Equals(filterId, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.
                            FirstOrDefault(t => t.CategoryFilterId == filterId);
                        switch (categoryFilter.FilterTypeName.FilterType)
                        {
                            case FilterType.Price:
                                filteredProducts.AddRange(products.Where(r => r.PriceWithDiscount >= filterProperty.minPrice && r.PriceWithDiscount < filterProperty.maxPrice).ToList());
                                hasPriceFilter = true;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            return hasPriceFilter ? filteredProducts : products;
        }

        public ICollection<Product> FilterProductsByRating(ICollection<Product> products)
        {
            bool hasFilter = false;
            List<Product> filteredProducts = new List<Product>();
            foreach (var categoryFilter in categoryFilterTypes)
            {
                foreach (var filterId in selectedFilters)
                {
                    if (categoryFilter.CategoryFilters.Any(t =>
                    t.CategoryFilterId.Equals(filterId,
                    StringComparison.InvariantCultureIgnoreCase)))
                    {
                        var filterProperty = categoryFilter.CategoryFilters.
                            FirstOrDefault(t => t.CategoryFilterId == filterId);
                        switch (categoryFilter.FilterTypeName.FilterType)
                        {
                            case FilterType.Rating:
                                filteredProducts.AddRange(products.Where(r => r.Rating >= filterProperty.ItemId && r.Rating < filterProperty.ItemId + 1).ToList());
                                hasFilter = true;
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
            return hasFilter ? filteredProducts : products;
        }

        public ICollection<Product> FilterProductsByBrand(ICollection<Product> products)
        {
            // Brand filters use ids like "b12". Match exactly (not >=) so deep-links from
            // product detail brand names return only that brand's products.
            var brandIds = (selectedFilters ?? new List<string>())
                .Where(f => !string.IsNullOrWhiteSpace(f)
                            && f.Length > 1
                            && (f[0] == 'b' || f[0] == 'B')
                            && f.Substring(1).All(char.IsDigit))
                .Select(f => f.Substring(1).ToInt())
                .Where(id => id > 0)
                .Distinct()
                .ToList();

            if (brandIds.Count == 0)
            {
                return products;
            }

            return products
                .Where(r => r.BrandId.HasValue && brandIds.Contains(r.BrandId.Value))
                .ToList();
        }

        public void AddBrandFilter(List<CategoryFilterType> categoryFilterTypes, List<Brand> brands)
        {
            if (brands.IsEmpty()) return;

            var item = new CategoryFilterType
            {
                Position = 0,
                FilterTypeName = new FilterTypeName
                {
                    FilterType = FilterType.Brand,
                    Text = Resource.Brands
                }
            };

            item.CategoryFilters = brands
                .Select(brand => new CategoryFilter
                {
                    CategoryFilterId = $"b{brand.Id}",
                    name = brand.Name
                })
                .ToList();

            item.CategoryFilters.ForEach(f => f.Parent = item);
            categoryFilterTypes.Add(item);
        }

        public void AddBrandFilter(List<CategoryFilterType> categoryFilterTypes, IEnumerable<Models.DTOs.Storefront.StorefrontBrandDto> brands)
        {
            if (brands == null || !brands.Any()) return;

            var item = new CategoryFilterType
            {
                Position = 0,
                FilterTypeName = new FilterTypeName
                {
                    FilterType = FilterType.Brand,
                    Text = Resource.Brands
                }
            };

            item.CategoryFilters = brands
                .Select(brand => new CategoryFilter
                {
                    CategoryFilterId = $"b{brand.Id}",
                    name = brand.Name
                })
                .ToList();

            item.CategoryFilters.ForEach(f => f.Parent = item);
            categoryFilterTypes.Add(item);
        }

        public void AddPriceFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            PriceFilterConfig priceRanges = ReadPriceFilterFromSetting();

            var item = new CategoryFilterType
            {
                Position = 1,
                FilterTypeName = new FilterTypeName { FilterType = FilterType.Price, Text = Resource.Price }
            };

            for (int i = 0; i < priceRanges.PriceRanges.Count; i++)
            {
                PriceRange priceRange = priceRanges.PriceRanges[i];
                var filter = new CategoryFilter
                {
                    CategoryFilterId = $"p{100 + i}",
                    minPrice = priceRange.Min,
                    maxPrice = priceRange.Max
                };

                filter.name = priceRange.IsLast
                    ? $"{priceRange.Min.CurrencySign()} {Resource.AndOverPrice}"
                    : i == 0
                        ? $"{priceRange.Max.CurrencySign()} {Resource.AndUnderPrice}"
                        : $"{priceRange.Min.CurrencySign()} - {priceRange.Max.CurrencySign()}";

                item.CategoryFilters.Add(filter);
            }

            item.CategoryFilters.ForEach(f => f.Parent = item);
            categoryFilterTypes.Add(item);
        }

        //       "PriceFilterConfig": {
        // "PriceRanges": [
        //   { "Min": 0, "Max": 49, "IsLast": false },
        //   { "Min": 49, "Max": 99, "IsLast": false },
        //   { "Min": 99, "Max": 499, "IsLast": false },
        //   { "Min": 499, "Max": 999, "IsLast": false },
        //   { "Min": 999, "Max": 4999, "IsLast": false },
        //   { "Min": 4999, "Max": 9999999, "IsLast": true }
        // ]
        //}

        public static PriceFilterConfig GetPriceFilterConfig(Setting priceFilterSetting)
        {
            if (priceFilterSetting == null || priceFilterSetting.IsEmpty() || string.IsNullOrWhiteSpace(priceFilterSetting.SettingValue))
            {
                return new PriceFilterConfig
                {
                    PriceRanges = new List<PriceRange>
                    {
                        new PriceRange { Min = 0, Max = 49, IsLast = false },
                        new PriceRange { Min = 49, Max = 99, IsLast = false },
                        new PriceRange { Min = 99, Max = 499, IsLast = false },
                        new PriceRange { Min = 499, Max = 999, IsLast = false },
                        new PriceRange { Min = 999, Max = 4999, IsLast = false },
                        new PriceRange { Min = 4999, Max = 9999999, IsLast = true }
                    }
                };
            }
            else
            {
                try
                {
                    var json = priceFilterSetting.SettingValue.ToStr();
                    var result = JsonConvert.DeserializeObject<PriceFilterConfig>(json);
                    if (result != null && result.PriceRanges != null && result.PriceRanges.Count > 0)
                    {
                        return result;
                    }
                }
                catch
                {
                    // fallback to default
                }
                return new PriceFilterConfig
                {
                    PriceRanges = new List<PriceRange>
                    {
                        new PriceRange { Min = 0, Max = 49, IsLast = false },
                        new PriceRange { Min = 49, Max = 99, IsLast = false },
                        new PriceRange { Min = 99, Max = 499, IsLast = false },
                        new PriceRange { Min = 499, Max = 999, IsLast = false },
                        new PriceRange { Min = 999, Max = 4999, IsLast = false },
                        new PriceRange { Min = 4999, Max = 9999999, IsLast = true }
                    }
                };
            }
        }

        public static PriceFilterConfig GetPriceFilterConfig(Models.DTOs.SettingDto priceFilterSetting)
        {
            if (priceFilterSetting == null || string.IsNullOrWhiteSpace(priceFilterSetting.SettingValue))
            {
                return new PriceFilterConfig
                {
                    PriceRanges = new List<PriceRange>
                    {
                        new PriceRange { Min = 0, Max = 49, IsLast = false },
                        new PriceRange { Min = 49, Max = 99, IsLast = false },
                        new PriceRange { Min = 99, Max = 499, IsLast = false },
                        new PriceRange { Min = 499, Max = 999, IsLast = false },
                        new PriceRange { Min = 999, Max = 4999, IsLast = false },
                        new PriceRange { Min = 4999, Max = 9999999, IsLast = true }
                    }
                };
            }
            else
            {
                try
                {
                    var json = priceFilterSetting.SettingValue.ToStr();
                    var result = JsonConvert.DeserializeObject<PriceFilterConfig>(json);
                    if (result != null && result.PriceRanges != null && result.PriceRanges.Count > 0)
                    {
                        return result;
                    }
                }
                catch
                {
                    // fallback to default
                }
                return new PriceFilterConfig
                {
                    PriceRanges = new List<PriceRange>
                    {
                        new PriceRange { Min = 0, Max = 49, IsLast = false },
                        new PriceRange { Min = 49, Max = 99, IsLast = false },
                        new PriceRange { Min = 99, Max = 499, IsLast = false },
                        new PriceRange { Min = 499, Max = 999, IsLast = false },
                        new PriceRange { Min = 999, Max = 4999, IsLast = false },
                        new PriceRange { Min = 4999, Max = 9999999, IsLast = true }
                    }
                };
            }
        }

        public static PriceFilterConfig GetPriceFilterConfig(Models.DTOs.Storefront.SettingValueDto priceFilterSetting)
        {
            if (priceFilterSetting == null || string.IsNullOrWhiteSpace(priceFilterSetting.SettingValue))
            {
                return new PriceFilterConfig
                {
                    PriceRanges = new List<PriceRange>
                    {
                        new PriceRange { Min = 0, Max = 49, IsLast = false },
                        new PriceRange { Min = 49, Max = 99, IsLast = false },
                        new PriceRange { Min = 99, Max = 499, IsLast = false },
                        new PriceRange { Min = 499, Max = 999, IsLast = false },
                        new PriceRange { Min = 999, Max = 4999, IsLast = false },
                        new PriceRange { Min = 4999, Max = 9999999, IsLast = true }
                    }
                };
            }
            else
            {
                try
                {
                    var json = priceFilterSetting.SettingValue.ToStr();
                    var result = JsonConvert.DeserializeObject<PriceFilterConfig>(json);
                    if (result != null && result.PriceRanges != null && result.PriceRanges.Count > 0)
                    {
                        return result;
                    }
                }
                catch
                {
                }
                return new PriceFilterConfig
                {
                    PriceRanges = new List<PriceRange>
                    {
                        new PriceRange { Min = 0, Max = 49, IsLast = false },
                        new PriceRange { Min = 49, Max = 99, IsLast = false },
                        new PriceRange { Min = 99, Max = 499, IsLast = false },
                        new PriceRange { Min = 499, Max = 999, IsLast = false },
                        new PriceRange { Min = 999, Max = 4999, IsLast = false },
                        new PriceRange { Min = 4999, Max = 9999999, IsLast = true }
                    }
                };
            }
        }

        public static void ParseCategoryFilter(
            string filter,
            Models.DTOs.SettingDto priceFilterSetting,
            out List<int> brandIds,
            out List<int> ratings,
            out List<PriceRange> priceRanges)
        {
            brandIds = new List<int>();
            ratings = new List<int>();
            priceRanges = new List<PriceRange>();

            if (string.IsNullOrWhiteSpace(filter))
            {
                return;
            }

            var tokens = filter.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(t => t.Trim())
                               .Where(t => !string.IsNullOrEmpty(t))
                               .ToList();

            var priceFilterConfig = GetPriceFilterConfig(priceFilterSetting);

            foreach (var token in tokens)
            {
                if (token.Length < 2)
                {
                    continue;
                }

                char prefix = token[0];
                string rest = token.Substring(1);

                if (prefix == 'b' || prefix == 'B')
                {
                    int brandId;
                    if (int.TryParse(rest, out brandId) && brandId > 0)
                    {
                        brandIds.Add(brandId);
                    }
                }
                else if (prefix == 'r' || prefix == 'R')
                {
                    int rating;
                    if (int.TryParse(rest, out rating) && rating >= 1 && rating <= 5)
                    {
                        ratings.Add(rating);
                    }
                }
                else if (prefix == 'p' || prefix == 'P')
                {
                    int index;
                    if (int.TryParse(rest, out index))
                    {
                        int rangeIdx = index - 100;
                        if (rangeIdx >= 0 && rangeIdx < priceFilterConfig.PriceRanges.Count)
                        {
                            priceRanges.Add(priceFilterConfig.PriceRanges[rangeIdx]);
                        }
                    }
                }
            }
        }

        public static void ParseCategoryFilter(
            string filter,
            Models.DTOs.Storefront.SettingValueDto priceFilterSetting,
            out List<int> brandIds,
            out List<int> ratings,
            out List<PriceRange> priceRanges)
        {
            brandIds = new List<int>();
            ratings = new List<int>();
            priceRanges = new List<PriceRange>();

            if (string.IsNullOrWhiteSpace(filter))
            {
                return;
            }

            var tokens = filter.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(t => t.Trim())
                               .Where(t => !string.IsNullOrEmpty(t))
                               .ToList();

            var priceFilterConfig = GetPriceFilterConfig(priceFilterSetting);

            foreach (var token in tokens)
            {
                if (token.Length < 2)
                {
                    continue;
                }

                char prefix = char.ToLowerInvariant(token[0]);
                string valueStr = token.Substring(1);

                if (prefix == 'b')
                {
                    if (int.TryParse(valueStr, out int bId) && bId > 0 && !brandIds.Contains(bId))
                    {
                        brandIds.Add(bId);
                    }
                }
                else if (prefix == 'r')
                {
                    if (int.TryParse(valueStr, out int rVal) && rVal > 0 && !ratings.Contains(rVal))
                    {
                        ratings.Add(rVal);
                    }
                }
                else if (prefix == 'p')
                {
                    if (int.TryParse(valueStr, out int pVal))
                    {
                        int index = pVal - 100;
                        if (priceFilterConfig.PriceRanges != null && index >= 0 && index < priceFilterConfig.PriceRanges.Count)
                        {
                            var pr = priceFilterConfig.PriceRanges[index];
                            if (!priceRanges.Contains(pr))
                            {
                                priceRanges.Add(pr);
                            }
                        }
                    }
                }
            }
        }

        public static void ParseCategoryFilter(
            string filter,
            Setting priceFilterSetting,
            out List<int> brandIds,
            out List<int> ratings,
            out List<PriceRange> priceRanges)
        {
            brandIds = new List<int>();
            ratings = new List<int>();
            priceRanges = new List<PriceRange>();

            if (string.IsNullOrWhiteSpace(filter))
            {
                return;
            }

            var tokens = filter.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries)
                               .Select(t => t.Trim())
                               .Where(t => !string.IsNullOrEmpty(t))
                               .ToList();

            var priceFilterConfig = GetPriceFilterConfig(priceFilterSetting);

            foreach (var token in tokens)
            {
                if (token.Length < 2)
                {
                    continue;
                }

                char prefix = char.ToLowerInvariant(token[0]);
                string valueStr = token.Substring(1);

                if (prefix == 'b')
                {
                    if (int.TryParse(valueStr, out int bId) && bId > 0 && !brandIds.Contains(bId))
                    {
                        brandIds.Add(bId);
                    }
                }
                else if (prefix == 'r')
                {
                    if (int.TryParse(valueStr, out int rVal) && rVal > 0 && !ratings.Contains(rVal))
                    {
                        ratings.Add(rVal);
                    }
                }
                else if (prefix == 'p')
                {
                    if (int.TryParse(valueStr, out int pVal))
                    {
                        int index = pVal - 100;
                        if (priceFilterConfig.PriceRanges != null && index >= 0 && index < priceFilterConfig.PriceRanges.Count)
                        {
                            var pr = priceFilterConfig.PriceRanges[index];
                            if (!priceRanges.Contains(pr))
                            {
                                priceRanges.Add(pr);
                            }
                        }
                    }
                }
            }
        }

        private PriceFilterConfig ReadPriceFilterFromSetting()
        {
            if (this.priceFilterSettingValueDto != null)
                return GetPriceFilterConfig(this.priceFilterSettingValueDto);
            if (this.priceFilterSettingDto != null)
                return GetPriceFilterConfig(this.priceFilterSettingDto);
            return GetPriceFilterConfig(this.priceFilterSetting);
        }

        public void AddRatingFilter(List<CategoryFilterType> categoryFilterTypes)
        {
            var item = new CategoryFilterType
            {
                Position = 5,
                FilterTypeName = new FilterTypeName
                {
                    FilterType = FilterType.Rating,
                    Text = Resource.Rating
                }
            };

            for (int rating = 5; rating >= 1; rating--)
            {
                var filter = new CategoryFilter
                {
                    CategoryFilterId = $"r{rating}",
                    name = $"{rating} {Resource.Star}",
                    rating = rating
                };
                item.CategoryFilters.Add(filter);
            }

            item.CategoryFilters.ForEach(f => f.Parent = item);
            categoryFilterTypes.Add(item);
        }
    }

    public class PriceRange
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public bool IsLast { get; set; }
    }

    public class PriceFilterConfig
    {
        public List<PriceRange> PriceRanges { get; set; } = new List<PriceRange>();

        public bool IsValid(out string errorMessage)
        {
            errorMessage = null;
            if (PriceRanges == null || PriceRanges.Count == 0)
            {
                errorMessage = "En az bir fiyat aralığı tanımlanmalıdır.";
                return false;
            }

            int lastCount = 0;
            for (int i = 0; i < PriceRanges.Count; i++)
            {
                var r = PriceRanges[i];
                if (r.Min < 0)
                {
                    errorMessage = $"{i + 1}. satır: Min fiyat 0'dan küçük olamaz.";
                    return false;
                }

                if (r.IsLast)
                {
                    lastCount++;
                    if (lastCount > 1)
                    {
                        errorMessage = "Sadece tek bir aralık 'Son Aralık' (Is Last) olarak işaretlenebilir.";
                        return false;
                    }
                }
                else if (r.Max <= r.Min)
                {
                    errorMessage = $"{i + 1}. satır: Max fiyat ({r.Max}), Min fiyattan ({r.Min}) büyük olmalıdır.";
                    return false;
                }
            }

            return true;
        }
    }
}