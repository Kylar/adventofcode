<Query Kind="Program">
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

//public string SessionCookie = "53616c7465645f5fde201829a0bd96d0427d00b6e017e3ba44d69722509c8353e6a84866d37e0159d4e1960be57ecc7a"; // Generated 2021-11-xx - A few days before the event started on 2021-12-01
public string SessionCookie = "53616c7465645f5fcb7b9833b130dd186534919fd1fef9485919711483a133b113726deaea777db55b84a5134eb1894f"; // Generated 2021-12-10

public enum AdventOfCodeYear
{
	Year2020 = 2020,
	Year2021 = 2021,
};

public record AdventOfCodeConfig(AdventOfCodeYear Year, string SessionCookie);

public class AdventOfCodeClient : IDisposable
{
	private HttpClient _client;
	private string _cacheBaseDir = Path.Combine(Path.GetDirectoryName(Util.CurrentQueryPath), "..", "cache");

	public AdventOfCodeConfig Config { get; }

	public AdventOfCodeClient(AdventOfCodeConfig config)
	{
		Config = config;
		var handler = new HttpClientHandler() { UseCookies = false };
		var client = new HttpClient(handler, true)
		{
			BaseAddress = new Uri($"https://adventofcode.com/{(int)config.Year}/"),
		};
		_client = client;
	}
	
	public async Task<string> GetInput(int dayNumber)
	{
		if (dayNumber < 1 || dayNumber > 25)
		{
			throw new ArgumentException("Invalid day number");
		}
		
		var request = new HttpRequestMessage(HttpMethod.Get, $"day/{dayNumber}/input");
		var bytes = await SendRequestWithCache(request);
		var str = Encoding.ASCII.GetString(bytes);
		return str;
	}
	
	public HttpRequestMessage CreateInputRequest(int dayNumber, int partNumber = 1)
	{
		if (dayNumber < 1 || dayNumber > 25)
		{
			throw new ArgumentException("Invalid day number");
		}
		
		if (partNumber < 1 || partNumber > 2)
		{
			throw new ArgumentException("Invalid part number");
		}

		return new HttpRequestMessage(HttpMethod.Get, $"day/{dayNumber}/input");
	}

	private async Task<byte[]> SendRequestWithCache(HttpRequestMessage request)
	{
		var cacheKey = $"{request.Method}_{(int)Config.Year}_{request.RequestUri.ToString().Replace("/", "_").Replace("#", "_")}.txt";
		var cacheFile = Path.Combine(_cacheBaseDir, cacheKey);
		if (File.Exists(cacheFile))
		{
			$"{cacheKey} - Found response in cache".Dump();
			return await File.ReadAllBytesAsync(cacheFile);
		}

		request.Headers.Add("Cookie", $"session={Config.SessionCookie}");

		$"{cacheKey} - Sending real request to be cached".Dump();
		
		var response = await _client.SendAsync(request);
		if (response.StatusCode != System.Net.HttpStatusCode.OK)
		{
			response.Dump();
			throw new Exception("Response status not OK");
		}

		var responseBytes = await response.Content.ReadAsByteArrayAsync();
		Directory.CreateDirectory(Path.GetDirectoryName(cacheFile));
		await File.WriteAllBytesAsync(cacheFile, responseBytes);
		return responseBytes;
	}

	public void Dispose()
	{
		_client.Dispose();
	}
}

public AdventOfCodeClient Client2020 => new AdventOfCodeClient(new AdventOfCodeConfig(AdventOfCodeYear.Year2020, SessionCookie));
public AdventOfCodeClient Client2021 => new AdventOfCodeClient(new AdventOfCodeConfig(AdventOfCodeYear.Year2020, SessionCookie));

public static AdventOfCodeClient Client;

public AdventOfCodeClient Configure2020()
{
	Client = new AdventOfCodeClient(new AdventOfCodeConfig(AdventOfCodeYear.Year2020, SessionCookie));
	return Client;
}
public AdventOfCodeClient Configure2021()
{
	Client = new AdventOfCodeClient(new AdventOfCodeConfig(AdventOfCodeYear.Year2021, SessionCookie));
	return Client;
}

async Task Main()
{
	Configure2020();
	using var c = Client;
	var input = await c.GetInput(1).Dump();
}


