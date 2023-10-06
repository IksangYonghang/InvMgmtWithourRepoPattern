using AutoMapper;
using Data.DataContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Dtos;
using Models.Entities;

namespace API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ProductController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IMapper _mapper;

		public ProductController(AppDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		[HttpPost]
		public async Task<ActionResult<Product>> CreatePost(ProductCreateDto productCreateDto)
		{
			var categoryExists = await _context.Categories.AnyAsync(c => c.Id == productCreateDto.CategoryId);
			if (!categoryExists)
			{
				return NotFound("Category not found");
			}

			var vendorExists = await _context.Vendors.AnyAsync(v => v.Id == productCreateDto.VendorId);
			if (!vendorExists)
			{
				return NotFound("Vendor not found");
			}

			var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductName == productCreateDto.ProductName || p.Size == productCreateDto.Size);
			if (existingProduct != null)
			{
				return Conflict("Product you are trying to create already exist");
			}

			var newProduct = _mapper.Map<Product>(productCreateDto);
			await _context.Products.AddAsync(newProduct);
			await _context.SaveChangesAsync();
			return Ok("Product created successfully");
		}

		[HttpGet("All")]
		public async Task<ActionResult<List<Product>>> GetProducts()
		{
			var products = await _context.Products.ToListAsync();
			if (products.Count > 0)
			{
				return Ok(products);
			}
			return NotFound("Product list is empty");
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Product>>GetProduct(int id)
		{
			var product= await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound("Product you are looking for not found");
			}
			return Ok(product);
		}

		[HttpPut]
		public async Task<ActionResult<Product>>UpdateProduct(ProductUpdateDto productUpdateDto, int id)
		{
			var categoryExists = await _context.Categories.AnyAsync(c => c.Id == productUpdateDto.CategoryId);
			if (!categoryExists)
			{
				return NotFound("Category not found");
			}

			var vendorExists = await _context.Vendors.AnyAsync(v => v.Id == productUpdateDto.VendorId);
			if (!vendorExists)
			{
				return NotFound("Vendor not found");
			}

			var productToBeUpdated = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
			if (productToBeUpdated == null)
			{
				return NotFound("Product to be updated not found");
			}
			_mapper.Map(productUpdateDto, productToBeUpdated);
			productToBeUpdated.UpdatedAt=DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return Ok("Product updated successfully");
		}

		[HttpDelete]
		public async Task<ActionResult<Product>>DeleteProduct(int id)
		{
			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound("Product to be deleted not found");
			}
			_context.Products.Remove(product);
			await _context.SaveChangesAsync();
			return Ok("Product Deleted successfully");
		}
	}
}
