[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(EImece.MVCGridConfig), "RegisterGrids")]

namespace EImece
{
    using Domain.Entities;
    using Domain.Helpers;
    using Domain.Repositories.IRepositories;
    using MVCGrid.Models;
    using MVCGrid.Web;
    using System;
    using System.Web.Mvc;

    public static class MVCGridConfig
    {
        public static void RegisterGrids()
        {
            GridDefaults gridDefaults = new GridDefaults()
            {
                Paging = true,
                ItemsPerPage = 20,
                Sorting = true,
                NoResultsMessage = "Sorry, no results were found"
            };
            ColumnDefaults colDefaults = new ColumnDefaults()
            {
                EnableSorting = true
            };
            MVCGridDefinitionTable.Add("ProductGridExample", new MVCGridBuilder<Product>()
                .WithAuthorizationType(AuthorizationType.AllowAnonymous)
                .AddColumns(cols =>
                {
                    // Add your columns here
                    cols.Add().WithColumnName("Id")
                        .WithHeaderText("Id")
                        .WithValueExpression(i => i.Id.ToStr()); // use the Value Expression to return the cell text for this column
                    cols.Add().WithColumnName("Name")
                      .WithHeaderText("Name")
                      .WithSorting(true)
                      .WithValueExpression(i => i.Name); // use the Value Expression to return the cell text for this column
                    cols.Add().WithColumnName("ProductCategoryName")
                    .WithHeaderText("ProductCategoryName")
                    .WithValueExpression(i => i.ProductCategory.Name); // use the Value Expression to return the cell text for this column

                    cols.Add().WithColumnName("UrlExample")
                        .WithHeaderText("Edit")
                        .WithValueExpression((i, c) => c.UrlHelper.Action("detail", "demo", new { id = i.Id }));
                })
                .WithRetrieveDataMethod((context) =>
                {
                    // Query your data here. Obey Ordering, paging and filtering parameters given in the context.QueryOptions.
                    // Use Entity Framework, a module from your IoC Container, or any other method.
                    // Return QueryResult object containing IEnumerable<YouModelItem>
                    var options = context.QueryOptions;
                    int totalRecords;
                    var repo = DependencyResolver.Current.GetService<IProductRepository>();
                    string globalSearch = options.GetAdditionalQueryOptionString("search");
                    String name = "";
                    var items = repo.GetData(out totalRecords,
                            globalSearch,
                            name,
                            options.GetLimitOffset(),
                            options.GetLimitRowcount(),
                            options.SortColumnName,
                            options.SortDirection == SortDirection.Dsc);
                    return new QueryResult<Product>()
                    {
                        Items = items,
                        TotalRecords = 0 // if paging is enabled, return the total number of records of all pages
                    };
                })
            );
        }
    }
}