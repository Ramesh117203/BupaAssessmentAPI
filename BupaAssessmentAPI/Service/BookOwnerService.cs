using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using BookApi.Models;
using BookApi;
using System;

public class BookOwnerService
{
    private readonly HttpClient _httpClient;

    public BookOwnerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    public async Task<List<BookOwner>> GetBookOwnersAsync()
    {
        var response = await _httpClient.GetAsync("https://digitalcodingtest.bupa.com.au/api/v1/bookowners");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            var bookOwners = JsonSerializer.Deserialize<List<BookOwner>>(content);
            return bookOwners;
        }

        throw new HttpRequestException($"Error fetching data from  API. Status code: {response.StatusCode}");
    }
}
