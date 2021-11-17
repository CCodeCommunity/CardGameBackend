using Api.Enums;

namespace Api.Dtos;

public static class ChangeAccountState
{
    public record Request(AccountState NewState);
}