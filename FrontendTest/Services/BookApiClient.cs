using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FrontendTest.Models;

namespace FrontendTest.Services
{
    // Single entry point to the backend API
    public class BookApiClient
    {
        private readonly HttpClient _http;

        // Enums are sent and read as text ("Fiction", "ToRead"...)
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        public BookApiClient(HttpClient http)
        {
            _http = http;
        }

        // List with search and pagination (skip / take)
        public async Task<List<Book>> GetAsync(string? search, int skip, int take)
        {
            var url = $"api/books?search={Uri.EscapeDataString(search ?? "")}&skip={skip}&take={take}";
            return await _http.GetFromJsonAsync<List<Book>>(url, JsonOptions) ?? new List<Book>();
        }

        // The 3 methods below return null on success,
        // otherwise the error message to show to the user
        public async Task<string?> CreateAsync(Book book)
        {
            var response = await _http.PostAsJsonAsync("api/books", book, JsonOptions);
            return response.IsSuccessStatusCode ? null : await ReadErrorAsync(response);
        }

        public async Task<string?> UpdateAsync(Book book)
        {
            var response = await _http.PutAsJsonAsync($"api/books/{book.Id}", book, JsonOptions);
            return response.IsSuccessStatusCode ? null : await ReadErrorAsync(response);
        }

        public async Task<string?> DeleteAsync(List<int> ids)
        {
            var response = await _http.PostAsJsonAsync("api/books/delete", ids, JsonOptions);
            return response.IsSuccessStatusCode ? null : await ReadErrorAsync(response);
        }

        // Reads the error format of the API: { "message": "..." }
        // (also handles the automatic validation format: { "errors": { "Field": ["..."] } })
        private static async Task<string> ReadErrorAsync(HttpResponseMessage response)
        {
            try
            {
                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var message))
                    return message.GetString() ?? "Error.";

                if (root.TryGetProperty("errors", out var errors))
                {
                    var list = new List<string>();
                    foreach (var field in errors.EnumerateObject())
                        foreach (var item in field.Value.EnumerateArray())
                            list.Add(item.GetString() ?? "");
                    return string.Join("\n", list);
                }
            }
            catch (JsonException)
            {
                // not JSON: fall back to the default message
            }

            return "An unexpected error occurred.";
        }
    }
}