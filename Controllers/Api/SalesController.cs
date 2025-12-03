using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmacyChain.Data;

namespace PharmacyChain.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SalesController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCount()
        {
            var count = await _db.Sales.CountAsync();
            return Ok(new { count });
        }
    }
}
