using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

[ApiController]
[Route("api/[controller]")]
public class RetellController : ControllerBase
{
    //  Store full chat history per user//i am vinay singh
    //private static readonly Dictionary<string, List<object>> UserHistory = new();
    private static readonly Dictionary<string, List<object>> UserHistory = new();
    // Ye Constructor zaroori hai:
    private readonly IConfiguration _configuration;

    // Ye Constructor zaroori hai:
    public RetellController(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] JsonElement data)
    {
        Debug.WriteLine("🔥 Webhook HIT 🔥");

        string userId = "default_user"; // Replace with dynamic user ID if needed
        string message = "";

        //  Extract user message
        if (data.TryGetProperty("data", out JsonElement inner) &&
            inner.TryGetProperty("text", out JsonElement text))
        {
            message = text.GetString();
        }

        Debug.WriteLine("User Message: " + message);

        if (string.IsNullOrEmpty(message))
        {
            return BadRequest(new { error = "Message is empty" });
        }

        //  Initialize history if not exists
        if (!UserHistory.ContainsKey(userId))
        {
            UserHistory[userId] = new List<object>();
        }

        //  Add user message to history
        UserHistory[userId].Add(new
        {
            role = "user",
            content = message
        });

        using var client = new HttpClient();

        //  Move to env variable in production
        var apiKey = _configuration["ApiSettings:OpenRouterKey"];
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        client.DefaultRequestHeaders.Add("HTTP-Referer", "https://your-site.com");
        client.DefaultRequestHeaders.Add("X-OpenRouter-Title", "AI Chatbot");

        //  Request body
        var body = new
        {
            model = "meta-llama/llama-3.3-70b-instruct",
            messages = UserHistory[userId]
        };

        var httpContent = new StringContent(
            JsonSerializer.Serialize(body),
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage res;

        try
        {
            res = await client.PostAsync(
                "https://openrouter.ai/api/v1/chat/completions",
                httpContent
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = "Connection error: " + ex.Message });
        }

        var resultString = await res.Content.ReadAsStringAsync();
        Debug.WriteLine("RAW AI Response: " + resultString);

        string aiReply = "Error in AI response";

        //  Parse response safely
        try
        {
            using var json = JsonDocument.Parse(resultString);
            var root = json.RootElement;

            if (root.TryGetProperty("choices", out JsonElement choices))
            {
                aiReply = choices[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();
            }
            else if (root.TryGetProperty("error", out JsonElement error))
            {
                aiReply = error.GetProperty("message").GetString();
            }
        }
        catch (Exception ex)
        {
            aiReply = $"Parsing error: {ex.Message}";
        }

        Debug.WriteLine("AI Reply: " + aiReply);

        //  Save assistant response in history
        UserHistory[userId].Add(new
        {
            role = "assistant",
            content = aiReply
        });
        // Remove repeated greetings (optional cleanup)
        UserHistory[userId] = UserHistory[userId]
            .Where((msg, index) =>
            {
                if (index == 0) return true;
                var prev = UserHistory[userId][index - 1].ToString();
                return msg.ToString() != prev;
            })
            .ToList();
        return Ok(new { response = aiReply });
    }


    [HttpPost("reset-session")]
    public IActionResult ResetSession(string userId)
    {
        if (UserHistory.ContainsKey(userId))
        {
            UserHistory[userId].Clear();
        }
        return Ok();
    }
}