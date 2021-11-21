using System.ComponentModel.DataAnnotations;

namespace Api.Dtos;

public record Profile(string Id, string Name, [Url] string? Picture);