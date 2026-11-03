using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace BookSpace.ApiTests;

/// <summary>HTTP კლიენტის დამხმარე: რეგისტრაცია/შესვლა ვებ-ფორმით (antiforgery ტოკენით), cookie ინახება.</summary>
public static partial class ApiClient
{
    public static HttpClient CreateClient(BookSpaceApiFactory factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            HandleCookies = true
        });

    public static async Task<HttpResponseMessage> RegisterAsync(HttpClient client, string email,
        string password = "Password1")
    {
        var token = await GetAntiforgeryTokenAsync(client, "/Account/Register");
        return await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["ConfirmPassword"] = password,
            ["__RequestVerificationToken"] = token
        }));
    }

    public static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password)
    {
        var token = await GetAntiforgeryTokenAsync(client, "/Account/Login");
        return await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        }));
    }

    private static async Task<string> GetAntiforgeryTokenAsync(HttpClient client, string path)
    {
        var response = await client.GetAsync(path);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            throw new InvalidOperationException($"GET {path} returned {(int)response.StatusCode}");
        }

        var html = await response.Content.ReadAsStringAsync();
        var match = TokenRegex().Match(html);
        if (!match.Success)
        {
            throw new InvalidOperationException($"No antiforgery token on {path}");
        }

        return WebUtility.HtmlDecode(match.Groups[1].Value);
    }

    [GeneratedRegex("name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"")]
    private static partial Regex TokenRegex();
}
