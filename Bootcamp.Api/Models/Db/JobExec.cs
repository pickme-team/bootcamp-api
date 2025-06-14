using System.ComponentModel.DataAnnotations;
using Bootcamp.Api.Services;

namespace Bootcamp.Api.Models.Db;

public enum JobExecStatus
{
    Scheduled,
    Running,
    Completed,
    Failed,
    Canceled
}

public class JobExec
{
    [Key]
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;
    public Guid ExecutorId { get; set; }
    public User Executor { get; set; } = null!;
    public JobExecStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public override string ToString()
    {
        return $"""
               Описание работы: {Job};
               Время выполнения: {EndTime - StartTime};
               """;
    }
}