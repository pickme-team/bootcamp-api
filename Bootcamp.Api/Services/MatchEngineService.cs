using Bootcamp.Api.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace Bootcamp.Api.Services;

public interface IMatchEngineService
{
    Task<List<Job>> MatchJobsForUser(User user);
}

public class MatchEngineService(IMlService service, BootcampContext db) : IMatchEngineService
{
    private const string SystemPrompt = "Твоя задача понять подходит ли данная работа под дпнного студента, в ответе укажи только true или false";
    
    public async Task<List<Job>> MatchJobsForUser(User user)
    {
        var allJobs = await db.Jobs.ToListAsync();
        
        var jobs = allJobs.AsParallel()
            .Where(j =>
        {
            var prompt = SystemPrompt + "Работа: " + j + user;
            return bool.Parse(service.GenerateText(new (prompt, 0.7f)).Result.text);
        }).ToList();
        
        return jobs;
    }
}