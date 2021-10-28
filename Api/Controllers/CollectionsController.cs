using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Dtos;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CollectionsController : ControllerBase
    {
        private readonly DatabaseContext _db;

        public CollectionsController(DatabaseContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Collection>>> Get()
        {
            return Ok(await _db.Collections.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult<Collection>> Post([FromBody] PostCollection.Request request)
        {
            var newCollection = new Collection 
            {
                Name = request.Name,
                ImageUrl = request.ImageUrl
            };

            await _db.AddAsync(newCollection);

            await _db.SaveChangesAsync();

            return Ok(newCollection);
        }
    }
}