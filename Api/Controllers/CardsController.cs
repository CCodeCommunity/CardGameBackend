using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
[ApiController, Route("[controller]")]
public class CardsController : ControllerBase
{
    private readonly DatabaseContext db;

    public CardsController(DatabaseContext db)
    {
        this.db = db;
    }
    
    [HttpGet("{cardId:int}")]
    [Authorize]
    public async Task<ActionResult<Card>> GetCard(int cardId)
    {
        var card = await db.Cards.FindAsync(cardId);
        return Ok(card);
    }
    
    [HttpGet]
    [Authorize]
    public async Task<ActionResult<List<Card>>> GetCards([FromQuery] int page, [FromQuery] int pageSize)
    {
        var cards = await db.Cards.GetPagedAsync(page, pageSize);
        return Ok(cards);
    }

    [HttpDelete("{cardId:Int}")]
    [Authorize]
    public async Task<ActionResult> Delete(int cardId)
    {
        var userId = User.FindFirstValue(ClaimTypes.PrimarySid);
        var userRole = Enum.Parse<AccountRole>(User.FindFirstValue(ClaimTypes.Role));
        if (userRole != AccountRole.Admin) return Forbid();

        var card = await db.Cards.Where(it => it.Id == cardId).Select(it => new { it.CreatorId }).FirstOrDefaultAsync();
        if (card == null) return UnprocessableEntity();
        if (card.CreatorId != userId) return Forbid();

        await db.Cards.SingleDeleteAsync(new Card { Id = cardId });
        await db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Card>> Create([FromBody] CreateCard.Request request)
    {
        var card = new Card
        {
            Name = request.Name,
            Type = request.Type,
            InvokeableBy = request.InvokeableBy,
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
            Behaviour = request.Behaviour,
            CreatorId = request.CreatorId,
        };

        await db.Cards.AddAsync(card);
        await db.SaveChangesAsync();

        return Ok(card);
    }
    
    [HttpPatch("{cardId:int}")]
    [Authorize]
    public async Task<ActionResult<Card>> Patch(int cardId, [FromBody] PatchCard.Request request)
    {
        var card = await db.Cards.FindAsync(cardId);
        if (card == null) return UnprocessableEntity();
        
        var userId = User.FindFirstValue(ClaimTypes.PrimarySid);
        var userRole = Enum.Parse<AccountRole>(User.FindFirstValue(ClaimTypes.Role));
        if (userRole != AccountRole.Admin && card.CreatorId != userId) return Forbid();

        card.Name = request.Name ?? card.Name;
        card.Type = request.Type ?? card.Type;
        card.InvokeableBy = request.InvokeableBy ?? card.InvokeableBy;
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
        card.Behaviour = request.Behaviour ?? card.Behaviour;

        await db.SaveChangesAsync();

        return Ok(card);
    }
}