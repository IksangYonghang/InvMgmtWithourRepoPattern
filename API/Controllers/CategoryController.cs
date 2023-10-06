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
	public class CategoryController : ControllerBase
	{
		private readonly AppDbContext _context;
		private readonly IMapper _mapper;

		public CategoryController(AppDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		[HttpPost]
		public async Task<ActionResult<Category>> CreateCategory(CategoryCreateDto categoryCreateDto)
		{
			var existingCategory = await _context.Categories.FirstOrDefaultAsync(c => c.Name == categoryCreateDto.Name);
			if (existingCategory != null)
			{
				return Conflict("Category you want to create already exist");
			}

			var newCategory = _mapper.Map<Category>(categoryCreateDto);
			await _context.AddRangeAsync(newCategory);
			await _context.SaveChangesAsync();
			return Ok("Category created successfully");
		}

		[HttpGet("All")]
		public async Task<ActionResult<List<Category>>> GetCategories()
		{
			var categories = await _context.Categories.ToListAsync();
			if (categories != null)
			{
				return Ok(categories);
			}
			return NotFound("Categories list is empty");
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Category>> GetCategory(int id)
		{
			var category = await _context.Categories.FindAsync(id);
			if (category != null)
			{
				return Ok(category);
			}
			return NotFound("Category to be deleted not found");
		}

		[HttpPut]
		public async Task<ActionResult<Category>> UpdateCategory(CategoryUpdateDto categoryUpdateDto, int id)
		{
			var categoryToUpdate = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
			if (categoryToUpdate == null)
			{
				return NotFound("Categor to be updated not found");
			}
			_mapper.Map(categoryUpdateDto, categoryToUpdate);
			categoryToUpdate.UpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return Ok("Category updated successfylly");
		}

		[HttpDelete]
		public async Task<ActionResult<Category>> DeleteCategory(int id)
		{
			var categoryToBeDeleted = await _context.Categories.FindAsync(id);
			if (categoryToBeDeleted == null)
			{
				return NotFound("Category to be deleted not found");
			}

			/* Checking if the category is associated with any products or not */

			var productsWithCategory = await _context.Products.Where(p => p.CategoryId == id).ToListAsync();
			if (productsWithCategory.Count > 0)
			{
				return BadRequest("Category cannot be deleted because it is associated with products");
			}

			_context.Categories.Remove(categoryToBeDeleted);
			await _context.SaveChangesAsync();
			return Ok("Category deleted successfully");
		}


	}
}
