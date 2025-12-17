using System.Text.Json;
using System.Linq;

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
        return Results.Content(AdminHtml(new List<string>()), "text/html");

    var json = System.IO.File.ReadAllText(filePath);
    var gifts = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? new();

    return Results.Content(AdminHtml(gifts), "text/html");
});

string AdminHtml(List<string> gifts)
{
    var listItems = gifts.Count == 0
        ? "<p>Henüz hiç hediye girilmemiş 🎁</p>"
        : string.Join("", gifts.Select(g => $"<li>{g}</li>"));

    return $@"
<!DOCTYPE html>
<html lang='tr'>
<head>
    <meta charset='UTF-8'>
    <title>Secret Santa – Admin</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background: #f5f6fa;
            display: flex;
            justify-content: center;
            padding-top: 50px;
        }}
        .card {{
            background: white;
            width: 400px;
            border-radius: 12px;
            padding: 20px;
            box-shadow: 0 10px 25px rgba(0,0,0,0.1);
        }}
        h2 {{
            text-align: center;
            margin-bottom: 20px;
        }}
        ul {{
            list-style: none;
            padding: 0;
        }}
        li {{
            background: #f1f2f6;
            margin-bottom: 10px;
            padding: 10px;
            border-radius: 8px;
            text-align: center;
            font-weight: 500;
        }}
        .count {{
            text-align: center;
            margin-top: 15px;
            color: #555;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class='card'>
        <h2>🎁 Girilen Hediyeler</h2>
        <ul>
            {listItems}
        </ul>
        <div class='count'>
            Toplam: {gifts.Count} hediye
        </div>
    </div>
</body>
</html>
";
}

app.Run();

record GiftRequest(string Gift);
