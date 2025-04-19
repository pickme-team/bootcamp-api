using Bootcamp.Api.Models.Db;
using Microsoft.EntityFrameworkCore;

namespace Bootcamp.Api.Services;

public interface IJobStatusService
{
    Task<List<JobExec>> GetJobsForUser(User user, JobExecStatus status);
    Task<bool> TakeJob(User user, Job jobId);
}

public class JobStatusService(BootcampContext db) : IJobStatusService
{
    public async Task<List<JobExec>> GetJobsForUser(User user, JobExecStatus status)
    {
        var jobs = await db.JobExecs.Where(je => je.ExecutorId == user.Id && je.Status == status)
            .ToListAsync();

        return jobs;
    }

    public async Task<bool> TakeJob(User user, Job jobId)
    {
        var jobExec = new JobExec()
        {
            ExecutorId = user.Id,
            JobId = jobId.Id,
            StartTime = DateTime.UtcNow,
            Status = JobExecStatus.Scheduled
        };

        try
        {
            await db.JobExecs.AddAsync(jobExec);
            await db.SaveChangesAsync();
            
            return true;
        }
        catch
        {
            return false;
        }
    }
}