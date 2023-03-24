using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Newtonsoft.Json.Linq;
using Telegram.Bot.Types;

namespace DiaryBotServerless;

public class S3BucketServise
{
    private readonly AmazonS3Client _s3Client;
    private const string BucketName = "diary-bot-bucket";
    

    public S3BucketServise()
    {
        var awsAccessKeyId = Environment.GetEnvironmentVariable("ACCESS_KEY");
        var awsSecretAccessKey = Environment.GetEnvironmentVariable("SECRET_KEY");

        _s3Client = new AmazonS3Client(awsAccessKeyId, awsSecretAccessKey);
        
    }

    public async Task<JObject> GetUsersData()
    {
        var response = await _s3Client.GetObjectAsync(BucketName, "test_json.json");
        StreamReader stream;
        using (stream = new StreamReader(response.ResponseStream))
        {
            var jsonAsString = stream.ReadToEndAsync().Result;
            var json = JObject.Parse(jsonAsString);
            return json;
        }
    }

    public async Task TestChangeField()
    {
        var UsersData = await GetUsersData();
        UsersData["color"] = "TestColor";
        await UpdateBucket(UsersData);
    }

    private async Task UpdateBucket(JObject UsersData)
    {
        var inputStream = GenerateStreamFromJObject(UsersData);
        var putObjectRequest = new PutObjectRequest
        {
            BucketName = BucketName,
            InputStream = inputStream,
            Key = "test_json.json",
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
}