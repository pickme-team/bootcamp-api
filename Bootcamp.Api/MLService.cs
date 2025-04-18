using Bootcamp.Api.Models.Requests;
using Bootcamp.Api.Models.Responses;
using YandexFoundationModels.ApiClient;
using YandexFoundationModels.ApiClient.Dto;

namespace Bootcamp.Api;

public interface IMlService
{
    Task<GenerateTextResponse> GenerateText(GenerateTextRequest request);
}

public class MlServiceException(string message) : Exception(message);

public class MlService(IConfiguration config) : IMlService
{
    private readonly string _folderId = config["YANDEX_FOLDER_ID"] ?? "";
    private readonly string _apiKey = config["YANDEX_API_KEY"] ?? "";

    public string ModelUrl => $"gpt://{_folderId}/{YandexGptModels.YandexGptLatest}";
    
    public async Task<GenerateTextResponse> GenerateText(GenerateTextRequest request)
    {
        CompletionRequest completionRequest = 
            new(ModelUrl,
                new(false, request.temperature, "2000"), 
                [new(Role.User, request.prompt)]);
        
        try
        {
            ILlmApiClient llmApiClient = ApiClientsFactory.CreateLlmApiClient(_apiKey);
            IOperationApiClient operationApiClient = ApiClientsFactory.CreateOperationApiClient(_apiKey);
            GetOperationResponse response = await llmApiClient.CompletionAsync(completionRequest);
            string operationId = response.Id;
            int counter = 100;
            while (!response.Done)
            {
                if (counter-- < 0) throw new TimeoutException("Operation timed out");
                response = await operationApiClient.GetOperation(operationId);
                await Task.Delay(500);
            }

            var text = response.Response?.Alternatives.Single().Message.Text;
            return new(text ?? "");
        }
        catch (Exception e)
        {
            throw new MlServiceException($"Failed to generate text: {e.Message}");
        }
    }
}