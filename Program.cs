using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// index.html otomatik açılsın
app.UseDefaultFiles();
app.UseStaticFiles();

string dbPath = "gifts.json";

List<string> ReadGifts()
{
    if (!File.Exists(dbPath))
        return new List<string>();

    var json = File.ReadAllText(dbPath);
    return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
}

void WriteGifts(List<string> gifts)
{
    var json = JsonSerializer.Serialize(gifts, new JsonSerializerOptions
    {
        WriteIndented = true
    });
    File.WriteAllText(dbPath, json);
}

app.MapPost("/add-gift", (GiftRequest body) =>
{
    if (body == null || string.IsNullOrWhiteSpace(body.Gift))
        return Results.BadRequest(new { message = "Hediye boş olamaz" });

    var gift = body.Gift.Trim().ToLower();
    var gifts = ReadGifts();

    if (gifts.Contains(gift))
    {
        return Results.Ok(new
        {
            exists = true,
            message = "⚠️ Bu hediye daha önce girilmiş olabilir"
        });
    }

    gifts.Add(gift);
    WriteGifts(gifts);

    return Results.Ok(new
    {
        exists = false,
        message = "✅ Hediye kaydedildi"
    });
});

app.MapGet("/admin", () =>
{
    var filePath = "gifts.json";

    if (!System.IO.File.Exists(filePath))
        return Results.Text("Henüz hiç hediye girilmemiş.");

    var json = System.IO.File.ReadAllText(filePath);
    var gifts = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new();

    var html = "<h2>🎁 Girilen Hediyeler</h2><ul>";

    foreach (var gift in gifts)
    {
        html += $"<li>{gift}</li>";
    }

    html += "</ul>";

    return Results.Content(html, "text/html");
});
app.Run();

record GiftRequest(string Gift);
