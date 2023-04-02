using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;
using Document = Telegram.Bot.Types.Document;

namespace DiaryBotServerless;

public class DynamoDBService
{
   
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private const string BucketName = "diary-bot-bucket";
    private const string TableName = "UsersTable";


    public DynamoDBService()
    {
        var awsAccessKeyId = Environment.GetEnvironmentVariable("ACCESS_KEY");
        var awsSecretAccessKey = Environment.GetEnvironmentVariable("SECRET_KEY");
        _dynamoDbClient = new AmazonDynamoDBClient(awsAccessKeyId, awsSecretAccessKey);
    }
    
    public async Task AddUserAsync(User user)
    {
        Dictionary<string, AttributeValue> userAttributes = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new() { N = user.Id.ToString() },
            ["ChannelId"] = new() { N = user.ChannelId.ToString() },
            ["PostCount"] = new() { N = user.PostCount.ToString() },
            ["CurrentPostText"] = new() { S = user.CurrentPostText },
            ["State"] = new() {S = user.State.ToString()}
        };
      
        PutItemRequest request = new PutItemRequest
        {
            TableName = TableName,
            Item = userAttributes
        };
        PutItemResponse response = await _dynamoDbClient.PutItemAsync(request);
    }

    public async Task<User?> GetUserByIdAsync(long id)
    {
        QueryRequest request = new QueryRequest
        {
            TableName = TableName,
            KeyConditionExpression = "Id = :id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":id", new AttributeValue { N = id.ToString() } }
            }
        };

        QueryResponse response = await _dynamoDbClient.QueryAsync(request);
        if (response.Items.Count != 0)
        {
            User user = new User
            {
                Id = long.Parse(response.Items[0]["Id"].N),
                ChannelId = long.Parse(response.Items[0]["ChannelId"].N),
                PostCount = int.Parse(response.Items[0]["PostCount"].N),
                CurrentPostText = response.Items[0]["CurrentPostText"].S,
                State =  Enum.Parse<States>(response.Items[0]["State"].S)
            };
            return user;
        }

        return new User();
    }

    public async Task<bool> IsUserExists(long id)
    {
        QueryRequest request = new QueryRequest
        {
            TableName = TableName,
            KeyConditionExpression = "Id = :id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":id", new AttributeValue { N = id.ToString() } }
            }
        };

        QueryResponse response = await _dynamoDbClient.QueryAsync(request);
        if (response.Items.Count == 0)
        {
            return false;
        }

        return true;
    }

    public async Task<States> GetUserState(long id)
    {
        
        QueryRequest request = new QueryRequest
        {
            TableName = TableName,
            KeyConditionExpression = "Id = :id",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":id", new AttributeValue { N = id.ToString() } }
            }
        };

        QueryResponse response = await _dynamoDbClient.QueryAsync(request);
        if (response.Items.Count != 0)
        {
            return Enum.Parse<States>(response.Items[0]["State"].S);
        }

        return States.Idle;
    }
}