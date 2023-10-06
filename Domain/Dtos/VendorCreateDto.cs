using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dtos
{
    public class VendorCreateDto
    {
		public string Name { get; set; }
		public string PhoneNumber { get; set; } = string.Empty;
		public string Address { get; set; } = string.Empty;

	}
}
