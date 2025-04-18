using System.ComponentModel.DataAnnotations;

namespace Bootcamp.Api.Models.Db;

public enum JobType
{
    PartTime,
    FullTime,
    Intern
}

public class Job
{
    [Key]
    public Guid Id { get; set; }
    public string JobName { get; set; }
    public string Description { get; set; }
    public string Company { get; set; }
    public List<string> Requirements { get; set; }
    public JobType Type { get; set; }
    public float Salary { get; set; }
    public string SalaryDescription { get; set; }
    public bool IsActive { get; set; }

    public override string ToString()
    {
        return $"""
               Название вакансии: {JobName};
               Описание вакансии: {Description};
               Требования: {Requirements};
               Тип вакансии: {Enum.Format(typeof(JobType), Type, "g")};
               Оплата: {string.Format(SalaryDescription, Salary)};
               """;
    }
}