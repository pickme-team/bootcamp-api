using Bootcamp.Api.Models.Requests;
using Microsoft.EntityFrameworkCore;

namespace Bootcamp.Api.Services;

public interface IModerationService
{
    Task<bool> Moderate(Guid jobId);
}

public class ModerationService(IMlService mlService, BootcampContext db) : IModerationService
{
    private const string SystemPrompt = "Ты модератор вакансий для студентов, необходимо: " +
                                        "проверка на спам, мошенничество и некорректные формулировки." +
                                        "В своём ответе укажи только число от 0 до 1";

    public async Task<bool> Moderate(Guid jobId)
    {
        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        
        var prompt = SystemPrompt + job;
        var request = new GenerateTextRequest(prompt, 0.8f);
        
        var response = await mlService.GenerateText(request);

        var score = float.Parse(response.text);
        return score > 0.5f;
    }
}