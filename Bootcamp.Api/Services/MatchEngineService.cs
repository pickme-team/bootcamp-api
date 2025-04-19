using Bootcamp.Api.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace Bootcamp.Api.Services;

public interface IMatchEngineService
{
    Task<List<Job>> MatchJobsForUser(User user);
}

public class MatchEngineService(IMlService service, BootcampContext db) : IMatchEngineService
{
    private const string SystemPrompt = "Твоя задача понять подходит ли данная работа под дпнного студента," +
                                        "Тебе бедет предоставлен список последних выполненых работ (успешных и нет), учитывай сильные и слабые стороны студента" +
                                        " в ответе укажи только true или false";
    
    public async Task<List<Job>> MatchJobsForUser(User user)
    {
        var allJobs = await db.Jobs.Where(j => j.IsActive).ToListAsync();
        var latestSuccessJobs = await db.JobExecs
            .Where(je => je.ExecutorId == user.Id && je.Status == JobExecStatus.Completed)
            .Take(2)
            .ToListAsync();
        
        var latestFailedJobs = await db.JobExecs
            .Where(je => je.ExecutorId == user.Id && je.Status == JobExecStatus.Failed)
            .Take(2)
            .ToListAsync();

        var successJobs = "Успешно выполнены: " + string.Join(";\n", latestSuccessJobs);
        var failedJobs = "Провалены: " + string.Join(";\n", latestFailedJobs);

        var jobs = allJobs.AsParallel()
            .Where(j =>
        {
            var prompt = SystemPrompt + "Работа: " + j + user + "Примеры работ\n" + successJobs + "\n" + failedJobs;
            return bool.Parse(service.GenerateText(new (prompt, 0.7f)).Result.text);
        }).ToList();
        
        return jobs;
    }
}