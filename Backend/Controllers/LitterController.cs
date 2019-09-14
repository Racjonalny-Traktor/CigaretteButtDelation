using microserv.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace microserv.Controllers
{
    [Route("api/[controller]")]
    public class LitterController : ControllerBase
    {
        private readonly DataContext _context;
        public LitterController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("")]
        public IActionResult GetAllLitter()
        {
            return Ok(_context.Litters.ToArray());
        }
    }
}
