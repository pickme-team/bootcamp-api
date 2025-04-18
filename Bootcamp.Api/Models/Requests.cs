// ReSharper disable once CheckNamespace

using Bootcamp.Api.Models.Db;

namespace Bootcamp.Api.Models.Requests;

public record GenerateTextRequest(string prompt, float temperature);

public record LoginRequest(string email, string password);
public record RegisterRequest(string firstName, string lastName, string email, string password);
public record CreateJobRequest(
    string jobName,
    string description,
    string company,
    List<string> requirements,
    JobType jobType,
    float salary,
    string salaryDescription);