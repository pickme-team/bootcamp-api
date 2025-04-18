// ReSharper disable once CheckNamespace
namespace Bootcamp.Api.Models.Requests;

public record GenerateTextRequest(string prompt, float temperature);

public record LoginRequest(string email, string password);
public record RegisterRequest(string firstName, string lastName, string email, string password);