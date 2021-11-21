using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Dtos;
using Api.Models;
using Api.Utilities;
using Ardalis.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Api.Authorization.AuthorizationPolicies;

namespace Api.Controllers;

[Authorize]
[ValidateModel]
[ApiController, Route("api/[controller]")]
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
        if (result == null) 
            return UnprocessableEntity();

        return Ok(result);
    }
    
    [HttpPost]
    [Authorize(Policy = RequireAdminOnly)]
    public async Task<ActionResult<CardCollection>> Create([FromBody] CreateCardCollection.Request request)
    {
        var userId = User.Id();
        
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
    [Authorize(Policy = RequireAdminOnly)]
    public async Task<ActionResult> Patch(int collectionId, [FromBody] PatchCardCollection.Request request)
    {
        var collection = await db.CardCollections.FindAsync(collectionId);
        if (collection == null) 
            return UnprocessableEntity();
        
        collection.Name = request.Name ?? collection.Name;
        collection.ImageUrl = request.ImageUrl ?? collection.ImageUrl;

        await db.SaveChangesAsync();

        return Ok();
    }
    
    [HttpDelete("{collectionId:int}")]
    [Authorize(Policy = RequireAdminOnly)]
    public async Task<ActionResult> Delete(int collectionId)
    {
        var recordsAffected = await db.CardCollections.DeleteByKeyAsync(collectionId);
        if (recordsAffected == 0) 
            return UnprocessableEntity();
        
        return Ok();
    }
}