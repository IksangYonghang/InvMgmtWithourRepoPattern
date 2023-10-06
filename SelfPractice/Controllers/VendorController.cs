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
	public class VendorController : ControllerBase
	{
		protected readonly AppDbContext _context;
		protected readonly IMapper _mapper;

		public VendorController(AppDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}

		[HttpPost]
		public async Task<ActionResult<Vendor>> CreateVendor(VendorCreateDto vendorCreateDto)
		{
			var existingVendor = _context.Vendors.FirstOrDefault(v => vendorCreateDto.Name == v.Name);
			if (existingVendor != null)
			{
				return Conflict("Vendor with the same name already exist");
			}

			var newVendor = _mapper.Map<Vendor>(vendorCreateDto);
			await _context.AddAsync(newVendor);
			await _context.SaveChangesAsync();
			return Ok("Vendor created successfully");
		}

		[HttpGet("All")]
		public async Task<ActionResult<List<Vendor>>> GetAllVendors()
		{
			var vendors = await _context.Vendors.ToListAsync();
			if (vendors == null)
			{
				return NotFound("Vendors not found");
			}
			return Ok(vendors);
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Vendor>> GetVendor(int id)
		{
			var vendor = await _context.Vendors.FindAsync(id);
			if (vendor == null)
			{
				return NotFound("Vendor you are looking for does not exist");
			}
			return Ok(vendor);
		}

		[HttpPut]
		public async Task<ActionResult<Vendor>> UpdateVendor(int id, VendorUpdateDto vendorUpdateDto)
		{
			var vendorToUpdate = await _context.Vendors.FirstOrDefaultAsync(v => v.Id == id);
			if (vendorToUpdate == null)
			{
				return NotFound("Vendor to be updated not found");
			}

			_mapper.Map(vendorUpdateDto, vendorToUpdate);
			vendorToUpdate.UpdatedAt = DateTime.UtcNow;
			await _context.SaveChangesAsync();
			return Ok("Vendor updated successfully");
		}

		[HttpDelete]
		public async Task<ActionResult<Vendor>> DeleteVendor(int id)
		{
			var vendorToDelete = await _context.Vendors.FindAsync(id);

			if (vendorToDelete == null)
			{
				return NotFound("Vendor you are looking to delete does not exist");
			}

			/* Checking if the category is existing in product table or not */

			var productsWithCategory = await _context.Products.Where(p => p.VendorId == id).ToListAsync();
			if (productsWithCategory.Count > 0)
			{
				return BadRequest("Vendor cannot be deleted because it is associated with products");
			}

			_context.Vendors.Remove(vendorToDelete);
			await _context.SaveChangesAsync();
			return Ok("Vendor has been deleted sccessfully");
		}

	}
}
