using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using DuudlagaFlow.Utilities;

namespace DuudlagaFlow.Services;

public class ChimegeApiException : Exception
{
    public ChimegeApiException(string message) : base(message) { }
}

internal class SttLongSubmitResponse
{
    [JsonPropertyName("uuid")]
    public string Uuid { get; set; } = "";

    [JsonPropertyName("duration")]
    public double Duration { get; set; }
}

internal class SttLongTranscriptResponse
{
    [JsonPropertyName("done")]
    public bool Done { get; set; }

    [JsonPropertyName("transcription")]
    public string? Transcription { get; set; }

    [JsonPropertyName("duration")]
    public double? Duration { get; set; }
}

public class ChimegeApiClient
{
    private readonly HttpClient _httpClient;

    public ChimegeApiClient()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public async Task<string> TranscribeAsync(byte[] audioData, string token, bool punctuate, CancellationToken ct = default)
    {
        var cleanToken = token.Trim();
        var uuid = await SubmitAudioAsync(audioData, cleanToken, ct);
        return await PollForTranscriptAsync(uuid, cleanToken, ct);
    }

    private async Task<string> SubmitAudioAsync(byte[] audioData, string token, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, Constants.SttLongUrl);
        request.Headers.Add("Token", token);
        request.Content = new ByteArrayContent(audioData);
        request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(request, ct);
        }
        catch (TaskCanceledException)
        {
            throw new ChimegeApiException("Хугацаа дууссан, сүлжээ шалгана уу");
        }
        catch (HttpRequestException ex)
        {
            throw new ChimegeApiException($"Сүлжээний алдаа: {ex.Message}");
        }

        var body = await response.Content.ReadAsStringAsync(ct);

        return response.StatusCode switch
        {
            HttpStatusCode.OK => JsonSerializer.Deserialize<SttLongSubmitResponse>(body)?.Uuid
                ?? throw new ChimegeApiException("Серверийн хариу буруу"),
            HttpStatusCode.BadRequest => throw new ChimegeApiException($"Аудио алдаа: {body}"),
            HttpStatusCode.Forbidden => throw new ChimegeApiException($"Token алдаа: {body}"),
            HttpStatusCode.InternalServerError => throw new ChimegeApiException($"Серверийн алдаа: {body}"),
            HttpStatusCode.ServiceUnavailable => throw new ChimegeApiException("Сервер ачаалалтай, дахин оролдоно уу"),
            _ => throw new ChimegeApiException($"Алдаа {(int)response.StatusCode}: {body}")
        };
    }

    private async Task<string> PollForTranscriptAsync(string uuid, string token, CancellationToken ct)
    {
        const int maxAttempts = 60;

        for (int i = 0; i < maxAttempts; i++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, Constants.SttLongTranscriptUrl);
            request.Headers.Add("Token", token);
            request.Headers.Add("UUID", uuid);

            try
            {
                var response = await _httpClient.SendAsync(request, ct);

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    var result = JsonSerializer.Deserialize<SttLongTranscriptResponse>(body);

                    if (result?.Done == true)
                    {
                        var text = result.Transcription?.Trim() ?? "";
                        if (string.IsNullOrEmpty(text))
                            throw new ChimegeApiException("Хоосон хариу");
                        return text;
                    }
                }
                else if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    var body = await response.Content.ReadAsStringAsync(ct);
                    throw new ChimegeApiException($"Token алдаа: {body}");
                }
            }
            catch (ChimegeApiException)
            {
                throw;
            }
            catch (TaskCanceledException)
            {
                throw new ChimegeApiException("Цуцлагдсан");
            }
            catch
            {
                // Ignore transient errors during polling
            }

            await Task.Delay(1000, ct);
        }

        throw new ChimegeApiException("Хугацаа дууссан, дахин оролдоно уу");
    }

    public async Task<(bool Success, string Message)> TestTokenAsync(string token)
    {
        var cleanToken = token.Trim();
        var silentWav = WavEncoder.EncodeSilence(0.5);

        try
        {
            var uuid = await SubmitAudioAsync(silentWav, cleanToken, CancellationToken.None);
            return (true, $"Token зөв! (UUID: {uuid[..Math.Min(8, uuid.Length)]}...)");
        }
        catch (ChimegeApiException ex)
        {
            return (false, ex.Message);
        }
    }
}
