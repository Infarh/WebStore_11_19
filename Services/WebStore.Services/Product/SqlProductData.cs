using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using WebStore.DAL.Context;
using WebStore.Domain.DTO.Products;
using WebStore.Domain.Entities;
using WebStore.Interfaces.Services;
using WebStore.Services.Map;

namespace WebStore.Services.Product
{
    public class SqlProductData : IProductData
    {
        private readonly WebStoreContext _db;

        public SqlProductData(WebStoreContext DB) => _db = DB;

        public IEnumerable<Section> GetSections() => _db.Sections
            //.Include(s => s.Products)                             // Грабли
            .AsEnumerable();

        public Section GetSectionById(int id) => _db.Sections.FirstOrDefault(s => s.Id == id);

        public IEnumerable<Brand> GetBrands() => _db.Brands
            //.Include(brand => brand.Products)                     // Грабли
            .AsEnumerable();

        public Brand GetBrandById(int id) => _db.Brands.FirstOrDefault(b => b.Id == id);

        public PagedProductDTO GetProducts(ProductFilter Filter)
        {
            IQueryable<Domain.Entities.Product> products = _db.Products
               .Include(p => p.Brand)
               .Include(p => p.Section);

            if (Filter?.SectionId != null)
                products = products.Where(product => product.SectionId == Filter.SectionId);

            if (Filter?.BrandId != null)
                products = products.Where(product => product.BrandId == Filter.BrandId);

            var total_count = products.Count();

            if (Filter?.PageSize != null)
                products = products
                   .Skip((Filter.Page - 1) * (int) Filter.PageSize)
                   .Take((int) Filter.PageSize);

            return new PagedProductDTO
            {
                Products = products
                   .ToArray()
                   .Select(ProductMapper.ToDTO),
                TotalCount = total_count
            };
        }

        public ProductDTO GetProductById(int id) =>
            _db.Products
               .Include(product => product.Brand)
               .Include(product => product.Section)
               .FirstOrDefault(product => product.Id == id)
               .ToDTO();
    }
}
