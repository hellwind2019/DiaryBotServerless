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
    private readonly AmazonS3Client _s3Client;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private const string BucketName = "diary-bot-bucket";
    private const string TableName = "UsersTable";


    public DynamoDBService()
    {
        var awsAccessKeyId = Environment.GetEnvironmentVariable("ACCESS_KEY");
        var awsSecretAccessKey = Environment.GetEnvironmentVariable("SECRET_KEY");

        _s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey);
        _dynamoDbClient = new AmazonDynamoDBClient(awsAccessKeyId, awsSecretAccessKey);
    }

    public async Task<JObject> GetUsersData()
    {
        var response = await _s3Client.GetObjectAsync(BucketName, "users.json");
        StreamReader stream;
        using (stream = new StreamReader(response.ResponseStream))
        {
            var jsonAsString = stream.ReadToEndAsync().Result;
            var json = JObject.Parse(jsonAsString);
            return json;
        }
    }

    public async Task UpdateBucket(JObject usersData)
    {
        // var mergeSettings = new JsonMergeSettings
        // {
        //     MergeArrayHandling = MergeArrayHandling.Union
        // };
        // var oldData = GetUsersData().Result;
        // oldData.Merge(usersData, mergeSettings);
        // var mergredData = oldData.SelectToken("user") as JArray;

        var inputStream = GenerateStreamFromJObject(usersData);
        var putObjectRequest = new PutObjectRequest
        {
            BucketName = BucketName,
            InputStream = inputStream,
            Key = "users.json",
        };
        await _s3Client.PutObjectAsync(putObjectRequest);
    }

    private static Stream GenerateStreamFromJObject(JObject s)
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write(s);
        writer.Flush();
        stream.Position = 0;
        return stream;
    }

   

    public async Task AddUserAsync(User user)
    {
        Dictionary<string, AttributeValue> userAttributes = new Dictionary<string, AttributeValue>
        {
            ["Id"] = new() { N = user.Id.ToString() },
            ["ChannelId"] = new() { N = user.ChannelId.ToString() },
            ["IsPostedToday"] = new() { BOOL = user.IsPostedToday },
            ["IsPostingNow"] = new() { BOOL = user.IsPostingNow },
            ["IsRegistered"] = new() { BOOL = user.IsRegistered },
            ["PostCount"] = new() { N = user.PostCount.ToString() },
            ["CurrentPostText"] = new() { S = user.CurrentPostText }
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
        if (response.Items.Count() != 0)
        {
            User user = new User()
            {
                Id = long.Parse(response.Items[0]["Id"].N),
                ChannelId = long.Parse(response.Items[0]["ChannelId"].N),
                IsPostedToday = response.Items[0]["IsPostedToday"].BOOL,
                IsPostingNow = response.Items[0]["IsPostingNow"].BOOL,
                PostCount = int.Parse(response.Items[0]["PostCount"].N),
                CurrentPostText = response.Items[0]["CurrentPostText"].S,
                IsRegistered = response.Items[0]["IsRegistered"].BOOL
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

    public async Task<bool> IsUserRegistered(long id)
    {
        if (!await IsUserExists(id))
        {
            return false;
        }

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
        if (response.Items[0]["IsRegistered"].BOOL)
        {
            return true;
        }

        return false;
    }
}