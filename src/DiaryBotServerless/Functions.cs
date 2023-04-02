using System.Net;
using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace DiaryBotServerless;

public class Functions
{
   
   
    private static readonly UpdateService UpdateService = new UpdateService();
    
    
    public async Task<APIGatewayProxyResponse> Get(JObject request, ILambdaContext context)
    {
        context.Logger.LogInformation("Get Request\n");
        
            Update? update = JObject.Parse(request.GetValue("body").ToString()).ToObject<Update>();
            await UpdateService.EchoAsync(update);
        
       
       


        var response = new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.OK,
            Body = "OK",
            Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
        };

        return response;
    }
}