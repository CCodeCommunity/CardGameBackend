using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Authorization;
using Api.Dtos;
using Api.Models;
using Ardalis.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Authorize]
[ValidateModel]
[ApiController, Route("[controller]")]
public class CollectionsController : ControllerBase
{
    private readonly DatabaseContext db;

    public CollectionsController(DatabaseContext db)
    {
        this.db = db;
    }
    
    [HttpGet]
    public async Task<ActionResult<List<CardCollection>>> GetAll()
    {
        var collections = await db.CardCollections.ToListAsync();
        return Ok(collections);
    }

    [HttpGet("{collectionId:int}")]
    public async Task<ActionResult<CardCollection>> Get(int collectionId)
    {
        var result = await db.CardCollections.FindAsync(collectionId);
        if (result == null) return UnprocessableEntity();

        return Ok(result);
    }
    
    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<CardCollection>> Create([FromBody] CreateCardCollection.Request request)
    {
        var userId = User.FindFirstValue(ClaimTypes.PrimarySid);
        
        var newCollection = new CardCollection 
        {
            Name = request.Name,
            ImageUrl = request.ImageUrl,
            CreatorId = userId, 
        };

        await db.AddAsync(newCollection);

        await db.SaveChangesAsync();

        return Ok(newCollection);
    }

    [HttpPatch("{collectionId:int}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> Patch(int collectionId, [FromBody] PatchCardCollection.Request request)
    {
        var collection = await db.CardCollections.FindAsync(collectionId);
        if (collection == null) return UnprocessableEntity();
        
        collection.Name = request.Name ?? collection.Name;
        collection.ImageUrl = request.Name ?? collection.ImageUrl;

        await db.SaveChangesAsync();

        return Ok();
    }
    
    [HttpPatch("{collectionId:int}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> Delete(int collectionId)
    {
        var recordsAffected = await db.CardCollections.DeleteByKeyAsync(collectionId);
        if (recordsAffected == 0) return UnprocessableEntity();
        
        return Ok();
    }
}