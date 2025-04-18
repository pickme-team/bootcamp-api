// ReSharper disable once CheckNamespace
namespace Bootcamp.Api.Models.Responses;

public record GenerateTextResponse(string text);

public record AuthResponse(string token);

public record GetUserResponse(string firtsName, string lastName, string email);