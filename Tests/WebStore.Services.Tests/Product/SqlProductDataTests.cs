using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WebStore.DAL.Context;
using WebStore.Domain.Entities;
using WebStore.Interfaces.Services;
using WebStore.Services.Database;
using WebStore.Services.Product;
using Assert = Xunit.Assert;

namespace WebStore.Services.Tests.Product
{
    [TestClass]
    public class SqlProductDataTests
    {
        private static IServiceProvider __Services;

        private const int __BrandsCount = 10;
        private const int __SectionsCount = 10;
        private const int __ProductsCount = 100;

        [ClassInitialize]
        public static void InitializeServices(TestContext Context) =>
            __Services = new ServiceCollection()
               .AddScoped<IProductData, SqlProductData>()
               .AddDbContext<WebStoreContext>(opt => opt.UseInMemoryDatabase("TestDatabase"))
               .AddIdentity<User, IdentityRole>(opt =>
                {
                    opt.Password.RequiredLength = 3;
                    opt.Password.RequireDigit = false;
                    opt.Password.RequireLowercase = false;
                    opt.Password.RequireUppercase = false;
                    opt.Password.RequireNonAlphanumeric = false;
                    opt.Password.RequiredUniqueChars = 3;
                })
               .AddEntityFrameworkStores<WebStoreContext>()
               .AddDefaultTokenProviders()
               .Services
               .BuildServiceProvider();

        [TestInitialize]
        public async Task Configure()
        {
            using var scope = __Services.CreateScope();
            var services = scope.ServiceProvider;
            await services
               .GetRequiredService<UserManager<User>>()
               .CreateAsync(new User { UserName = "Admin" }, "Password");

            var db = services.GetRequiredService<WebStoreContext>();
            var brands = db.Brands;

            if (!brands.Any())
                for (var i = 1; i <= __BrandsCount; i++)
                    await brands.AddAsync(new Brand { Name = $"Brand {i}", Order = i });

            var sections = db.Sections;
            if (!sections.Any())
                for (var i = 1; i <= __SectionsCount; i++)
                    await sections.AddAsync(new Section { Name = $"Section {i}", Order = i });

            var products = db.Products;
            if (!products.Any())
            {
                var rnd = new Random();
                for (var i = 1; i <= __ProductsCount; i++)
                    products.Add(new Domain.Entities.Product
                    {
                        Name = $"Product {i}",
                        Order = i,
                        SectionId = rnd.Next(__SectionsCount),
                        BrandId = rnd.Next(__BrandsCount)
                    });
            }

            await db.SaveChangesAsync();
        }

        [TestMethod]
        public async Task CheckAdminUserExistsInDB()
        {
            using var scope = __Services.CreateScope();
            var user_manager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var user = await user_manager.FindByNameAsync("Admin");
            Assert.NotNull(user);
            Assert.Equal("Admin", user.UserName);
        }

        [TestMethod]
        public void GetBrandsReturnAllBrands()
        {
            using var scope = __Services.CreateScope();
            var services = scope.ServiceProvider;
            var product_data = services.GetRequiredService<IProductData>();
            var brands = product_data.GetBrands();

            Assert.Equal(__BrandsCount, brands.Count());
        }

        [TestMethod]
        public void GetSectionsReturnAllSections()
        {
            using var scope = __Services.CreateScope();
            var services = scope.ServiceProvider;
            var product_data = services.GetRequiredService<IProductData>();
            var sections = product_data.GetSections();

            Assert.Equal(__SectionsCount, sections.Count());
        }

        [TestMethod]
        public void GetProductsWithDefaultFilterReturnAllProducts()
        {
            using var scope = __Services.CreateScope();
            var services = scope.ServiceProvider;
            var product_data = services.GetRequiredService<IProductData>();

            var all_products = product_data.GetProducts(new ProductFilter());

            Assert.Equal(__ProductsCount, all_products.Products.Count());
        }
    }
}
