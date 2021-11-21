using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Api.Authorization;
using Api.Dtos;
using Api.Enums;
using Api.Models;
using Api.Utilities;
using Ardalis.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[Authorize]
[ValidateModel]
[ApiController, Route("api/[controller]")]
public class CardsController : ControllerBase
{
    private readonly DatabaseContext db;

    public CardsController(DatabaseContext db)
    {
        this.db = db;
    }

    [HttpGet("count")]
    public async Task<ActionResult<CardsCount.Response>> Count()
    {
        var result = await db.Cards.CountAsync();
        return Ok(new CardsCount.Response(result));
    }
    
    [HttpGet("{cardId:int}")]
    public async Task<ActionResult<Card>> GetCard(int cardId)
    {
        var card = await db.Cards.FindAsync(cardId);
        if (card == null) 
            return UnprocessableEntity();
        
        return Ok(card);
    }
    
    [HttpGet]
    public async Task<ActionResult<List<Card>>> GetCards(
        [FromQuery, Range(1, int.MaxValue)] int page, 
        [FromQuery, Range(1, int.MaxValue)] int pageSize)
    {
        var cards = await db.Cards.GetPagedAsync(page, pageSize);
        return Ok(cards);
    }

    [HttpDelete("{cardId:int}")]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> Delete(int cardId)
    {
        var affected = await db.Cards.DeleteByKeyAsync(cardId);
        if (affected == 0) 
            return UnprocessableEntity();

        await db.SaveChangesAsync();
        
        return Ok();
    }

    [HttpPost]
    public async Task<ActionResult<Card>> Create([FromBody] CreateCard.Request request)
    {
        var card = new Card
        {
            Name = request.Name,
            Type = request.Type,
            CardCollectionId = request.CardCollectionId,
            Rarity = request.Rarity,
            Attack = request.Attack,
            Health = request.Health,
            Cost = request.Cost,
            Description = request.Description,
            Quote = request.Quote,
            QuoteUrl = request.QuoteUrl,
            ImageUrl = request.ImageUrl,
            HearthstoneEquivalentUrl = request.HearthstoneEquivalentUrl,
            CreatorId = request.CreatorId,
        };

        await db.Cards.AddAsync(card);
        await db.SaveChangesAsync();

        return Ok(card);
    }
    
    [HttpPatch("{cardId:int}")]
    public async Task<ActionResult<Card>> Patch(int cardId, [FromBody] PatchCard.Request request)
    {
        var card = await db.Cards.FindAsync(cardId);
        if (card == null)
            return UnprocessableEntity();
        
        if (User.Role() != AccountRole.Admin && card.CreatorId != User.Id())
            return Forbid();

        card.Name = request.Name ?? card.Name;
        card.Type = request.Type ?? card.Type;
        card.CardCollectionId = request.CardCollectionId ?? card.CardCollectionId;
        card.Rarity = request.Rarity ?? card.Rarity;
        card.Attack = request.Attack ?? card.Attack;
        card.Health = request.Health ?? card.Health;
        card.Cost = request.Cost ?? card.Cost;
        card.Description = request.Description ?? card.Description;
        card.Quote = request.Quote ?? card.Quote;
        card.QuoteUrl = request.QuoteUrl ?? card.QuoteUrl;
        card.ImageUrl = request.ImageUrl ?? card.ImageUrl;
        card.HearthstoneEquivalentUrl = request.HearthstoneEquivalentUrl ?? card.HearthstoneEquivalentUrl;

        await db.SaveChangesAsync();

        return Ok(card);
    }
}