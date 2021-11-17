using System.Threading.Tasks;
using Ardalis.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ValidateModel]
[ApiController, Route("[controller]")]
public class UsersController : Controller
{
    private readonly DatabaseContext _db;

    public UsersController(DatabaseContext db)
    {
        _db = db;
    }

    public async Task<ActionResult> ChangeUserDetails()
    {
        return Ok();
    }
}