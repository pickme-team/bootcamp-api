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
                                        "Анализируй текст на:" +
                                        "\n1. TOXICITY, SEVERE_TOXICITY, IDENTITY_ATTACK, " +
                                        "INSULT, PROFANITY, THREAT, SEXUALLY_EXPLICIT, " +
                                        "ATTACK_ON_AUTHOR, ATTACK_ON_COMMENTER, INCOHERENT, " +
                                        "INFLAMMATORY, LIKELY_TO_REJECT, OBSCENE, SPAM, UNSUBSTANTIAL.\n" +
                                        "2. Фильтруй казино, букмекеров и тд" +
                                        "3. Контекст важен: \n   - \"\"черный список\"\" \u2260 дискриминация\n   - \"\"только для мужчин\"\" \u2192 дискриминация" +
                                        "4. Особые случаи:\n   - Сарказм/шутки проверять особо\n   - Скрытые угрозы (\"\"у нас строгие последствия\"\") \u2192 анализ\n6. Не пропускать:\n   - Любые формы дискриминации\n   - Унизительные требования\n   - Нецензурные формулировки\"" +
                                        "5. Формат ответа: ТОЛЬКО И ТОЛЬКО дробное число от 0 до 1 (например 0.2) чем выше, тем болле опаснее, больше ничего только число";

    public async Task<bool> Moderate(Guid jobId)
    {
        var job = await db.Jobs.FirstOrDefaultAsync(j => j.Id == jobId);
        
        var prompt = SystemPrompt + job;
        var request = new GenerateTextRequest(prompt, 0.3f);
        
        var response = await mlService.GenerateText(request);

        var score = float.Parse(response.text);
        return score > 0.5f;
    }
}