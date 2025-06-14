using Bootcamp.Api.Models.Db;
using Bootcamp.Api.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace Bootcamp.Api.Services;

public interface IJobStatusService
{
    Task<List<JobExec>> GetJobsForUser(User user, JobExecStatus status);
    Task<bool> TakeJob(User user, Job jobId);
    Task<JobExec?> EndJobExec(Guid jobId, CompleteJobRequest completeJobRequest);
}

public class JobStatusService(BootcampContext db) : IJobStatusService
{
    public async Task<List<JobExec>> GetJobsForUser(User user, JobExecStatus status)
    {
        var jobs = await db.JobExecs.Where(je => je.ExecutorId == user.Id && je.Status == status)
            .Include(je => je.Executor)
            .Include(je => je.Job)
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

    public async Task<JobExec?> EndJobExec(Guid jobId, CompleteJobRequest completeJobRequest)
    {
        var jobExecTask = db.JobExecs.AsTracking()
            .Include(je => je.Executor)
            .Include(je => je.Job)
            .FirstOrDefaultAsync(je => je.JobId == jobId);
        
        var jobExec = await jobExecTask;
        if (jobExec != null)
        {
            jobExec.Status = completeJobRequest.isSuccess ? JobExecStatus.Completed : JobExecStatus.Failed;
            jobExec.EndTime = DateTime.UtcNow;
        }
        
        await db.SaveChangesAsync();

        return jobExec;
    }
}