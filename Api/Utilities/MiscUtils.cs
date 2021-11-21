using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api.Utilities;

public static class MiscUtils
{
    public static async Task<T?> ParseJsonAsync<T>(this HttpResponseMessage message)
    {
        var content = await message.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return result;
    }

    public static async Task<HttpResponseMessage> PatchAsJsonAsync<T>(this HttpClient client, string url, T data)
    {
        var response = await client.PatchAsync(url, 
            new StringContent(JsonSerializer.Serialize(data), Encoding.Default, mediaType: "application/json"));

        return response;
    }
}