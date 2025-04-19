using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Bootcamp.Api;
using Bootcamp.Api.Models.Db;
using Bootcamp.Api.Models.Requests;
using Bootcamp.Api.Models.Responses;
using Bootcamp.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LoginRequest = Bootcamp.Api.Models.Requests.LoginRequest;
using RegisterRequest = Bootcamp.Api.Models.Requests.RegisterRequest;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<BootcampContext>(o => o.UseInMemoryDatabase("Bootcamp"));

builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddSingleton<PasswordHasher<string>>();
builder.Services.AddScoped<IMlService, MlService>();
builder.Services.AddScoped<IModerationService, ModerationService>();
builder.Services.AddScoped<IMatchEngineService, MatchEngineService>();
builder.Services.AddScoped<IJobStatusService, JobStatusService>();

builder.Services.AddExceptionHandler<ExceptionFilter>();

builder.Services.Configure<JsonOptions>(options => {
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/ping", () => "pong")
    .WithName("Ping");

#region Auth Endpoints
app.MapPost("/login", (LoginRequest request, BootcampContext db, IJwtService jwtService, PasswordHasher<string> hasher) =>
{
    var user = db.Users.FirstOrDefault(u => 
        u.Email == request.email);

    if (user == null)
        return Results.NotFound();
    if (hasher.VerifyHashedPassword(user.Email, user.Password, request.password) == PasswordVerificationResult.Failed)
        return Results.Unauthorized();
    
    return Results.Ok(new AuthResponse(jwtService.GenerateToken(user)));
});

app.MapPost("/register", async (RegisterRequest request, BootcampContext db, IJwtService jwtService, PasswordHasher<string> hasher) =>
{
    if (await db.Users.AnyAsync(u => u.Email == request.email))
        return Results.Conflict("Пользователь с таким email уже существует");

    var user = new User
    {
        FirstName = request.firstName,
        LastName = request.lastName,
        Email = request.email,
        Password = hasher.HashPassword(request.email, request.password)
    };

    db.Users.Add(user);
    await db.SaveChangesAsync();

    return Results.Ok(new AuthResponse(jwtService.GenerateToken(user)));
});
#endregion

#region User Profile
app.MapGet("/users/me", async (HttpContext context, BootcampContext db) =>
{
    var id = context.User.Id();
    var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);

    return user == null ? Results.NotFound() : Results.Ok(new GetUserResponse(user.FirstName, user.LastName, user.Email, user.Skills));
}).RequireAuthorization();

app.MapPut("/users/me/update-skills", async (List<string> skills, BootcampContext db, HttpContext context) =>
{
    var id = context.User.Id();
    var user = await db.Users.FindAsync(id);
    
    if (user == null)
        return Results.NotFound();
    
    user.Skills = skills;
    
    await db.SaveChangesAsync();
    return Results.Ok(new GetUserResponse(user.FirstName, user.LastName, user.Email, user.Skills));
}).RequireAuthorization();

app.MapGet("users/me/jobs", async (HttpContext context, BootcampContext db) =>
{
    var userId = context.User.Id();
    var user = await db.Users.FindAsync(userId);

    if (user == null)
        return Results.NotFound();
    
    var jobExecs = await db.JobExecs.Where(j => j.ExecutorId == userId).ToListAsync();
    return Results.Ok(jobExecs);
}).RequireAuthorization();

app.MapPost("users/me/jobs/{id:Guid}/cancel",
async (Guid jobId, BootcampContext db, JobStatusService jobService, HttpContext context) =>
{
    var userId = context.User.Id();
    var user = await db.Users.FindAsync(userId);

    if (user == null)
        return Results.NotFound();

    var jobExec = await db.JobExecs.FindAsync(jobId, userId);

    if (jobExec == null)
        return Results.Forbid();

    jobExec.Status = JobExecStatus.Canceled;
    jobExec.EndTime = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(new {jobExec});
}).RequireAuthorization();

#endregion

#region ML Block
app.MapPost("/generate-text", async (GenerateTextRequest request, IMlService mlService) 
        => await mlService.GenerateText(request))
    .WithName("GenerateText");

app.MapGet("/jobs/{id:Guid}/moderate", async (HttpContext context, IModerationService moderator, Guid id) =>
{
    var isProfanity = await moderator.Moderate(id);
    return Results.Ok(new { isProfanity });
});

app.MapGet("/users/me/feed", async (IMatchEngineService matchEngine, BootcampContext db, HttpContext context) =>
{
    var id = context.User.Id();
    var user = await db.Users.FindAsync(id);
    
    return user == null ? Results.NotFound() : Results.Ok(await matchEngine.MatchJobsForUser(user));
}).RequireAuthorization();

app.MapGet("/users/me/feed/all", async (BootcampContext db) =>
{
    var jobs = await db.Jobs.ToListAsync();
    return Results.Ok(jobs);
});
#endregion

#region Jobs

app.MapPost("jobs/create", async (CreateJobRequest createJob, BootcampContext db) =>
{
    var job = new Job()
    {
        JobName = createJob.jobName,
        Description = createJob.description,
        Company = createJob.company,
        IsActive = true,
        Requirements = createJob.requirements,
        Salary = createJob.salary,
        SalaryDescription = createJob.salaryDescription,
        Type = createJob.jobType
    };
    
    await db.Jobs.AddAsync(job);
    await db.SaveChangesAsync();
    
    return Results.Ok(job.Id);
});

app.MapDelete("jobs/{id:Guid}/delete", async (Guid jobId, BootcampContext db) =>
{
    var job = await db.Jobs.FindAsync(jobId);
    if (job == null)
        return Results.NotFound();
    
    var jobExecTask = db.JobExecs.AsTracking().FirstOrDefaultAsync(je => je.JobId == jobId);

    db.Jobs.Remove(job);
    var jobExec = await jobExecTask;

    if (jobExec != null)
    {
        jobExec.Status = JobExecStatus.Canceled;
        jobExec.EndTime = DateTime.UtcNow;     
    }
    
    await db.SaveChangesAsync();
    
    return Results.Ok();
});

app.MapGet("jobs/{id:Guid}", async (Guid id, BootcampContext db) =>
{
    var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == id);
    return job == null ? Results.NotFound() : Results.Ok(job);
});

app.MapPost("jobs/{id:Guid}/active", async (Guid id, BootcampContext db) =>
{
    var job = await db.Jobs.FindAsync(id);
    
    if (job == null)
        return Results.NotFound();
    
    job.IsActive = true;
    await db.SaveChangesAsync();
    return Results.Ok(job);
});

app.MapPost("jobs/{id:Guid}/disable", async (Guid id, BootcampContext db) =>
{
    var job = await db.Jobs.FindAsync(id);
    
    if (job == null)
        return Results.NotFound();
    
    job.IsActive = false;
    await db.SaveChangesAsync();
    return Results.Ok(job);
});

app.MapPut("jobs/{id:Guid}/confirm", async (Guid id, BootcampContext db) =>
{
    var job = await db.Jobs.FindAsync(id);
    
    if (job == null)
        return Results.NotFound();
    
    job.IsActive = false;
    await db.SaveChangesAsync();
    return Results.Ok(job);
});

app.MapPut("jobs/{id:Guid}/complete", async (CompleteJobRequest request, Guid id, BootcampContext db, IJobStatusService jobService) =>
{
    var jobExec = await jobService.EndJobExec(id, request);
    
    return jobExec == null ? Results.NotFound() : Results.Ok(jobExec);
});

app.MapPost("jobs/{id:Guid}/take", async (Guid jobId, BootcampContext db, IJobStatusService jobService, HttpContext context) =>
{
    var userId = context.User.Id();
    var user = await db.Users.FindAsync(userId);
    
    if (user == null)
        return Results.NotFound();
    
    var job = await db.Jobs.FindAsync(jobId);
    
    if (job == null)
        return Results.NotFound();
    
    var taken = await jobService.TakeJob(user, job);
    return Results.Ok(new {taken});
}).RequireAuthorization();

#endregion

app.Run();