


using StackExchange.Redis;

string connectionString = "localhost:6379";


try
{
	var redis = await ConnectionMultiplexer.ConnectAsync(connectionString);
	var db = redis.GetDatabase();



	bool setResult = await db.StringSetAsync("testKey", "testValue");
	Console.WriteLine($"Set Key: {setResult}");


	await redis.CloseAsync();
}
catch (Exception ex)
{
	Console.WriteLine($"Error: {ex.Message}");
}

