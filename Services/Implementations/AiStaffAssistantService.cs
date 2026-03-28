using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using BusinessObjects;
using Microsoft.Extensions.Configuration;
using Services.Interfaces;

namespace Services.Implementations;

public class AiStaffAssistantService : IAiStaffAssistantService
{
    private static readonly HttpClient HttpClient = new();
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IStockInService _stockInService;

    public AiStaffAssistantService()
        : this(new ProductService(), new OrderService(), new StockInService())
    {
    }

    public AiStaffAssistantService(IProductService productService, IOrderService orderService, IStockInService stockInService)
    {
        _productService = productService;
        _orderService = orderService;
        _stockInService = stockInService;
    }

    public async Task<string> AskAsync(string question, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(question))
        {
            throw new ArgumentException("Vui long nhap cau hoi cho AI Assistant.", nameof(question));
        }

        var trimmedQuestion = question.Trim();
        if (TryAnswerFromDatabase(trimmedQuestion, out var databaseAnswer))
        {
            return databaseAnswer;
        }

        var settings = LoadSettings();
        if (!settings.Enabled)
        {
            throw new InvalidOperationException("AI integration dang tat. Hay bat AiIntegration:Enabled trong appsettings.json.");
        }

        if (string.IsNullOrWhiteSpace(settings.BaseUrl) || string.IsNullOrWhiteSpace(settings.Model))
        {
            throw new InvalidOperationException("Can cau hinh AiIntegration:BaseUrl va AiIntegration:Model trong appsettings.json.");
        }

        return await AskModelWithSnapshotAsync(trimmedQuestion, settings, cancellationToken);
    }

    private bool TryAnswerFromDatabase(string question, out string answer)
    {
        var normalized = Normalize(question);

        if (IsProductStockLookupIntent(normalized))
        {
            answer = BuildProductStockLookupAnswer(question);
            return true;
        }

        if (IsHighestStockIntent(normalized))
        {
            answer = BuildHighestStockAnswer();
            return true;
        }

        if (IsLowestStockIntent(normalized))
        {
            answer = BuildLowestStockAnswer();
            return true;
        }

        if (IsLowStockIntent(normalized))
        {
            answer = BuildLowStockAnswer(question);
            return true;
        }

        if (IsInactiveProductsIntent(normalized))
        {
            answer = BuildInactiveProductsAnswer();
            return true;
        }

        if (IsRecentStockInIntent(normalized))
        {
            answer = BuildRecentStockInsAnswer();
            return true;
        }

        if (IsOrderIntent(normalized))
        {
            answer = BuildOrdersAnswer(normalized);
            return true;
        }

        if (IsProductFilterIntent(normalized))
        {
            answer = BuildFilteredProductsAnswer(normalized);
            return true;
        }

        answer = string.Empty;
        return false;
    }

    private string BuildLowStockAnswer(string question)
    {
        var threshold = ExtractThreshold(question, 5);
        var products = _productService.GetAllProducts()
            .Where(p => p.Status && p.StockQuantity <= threshold)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.ProductName)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine($"San pham sap het hang (nguong <= {threshold})");
        builder.AppendLine($"Tim thay {products.Count} san pham phu hop.");
        builder.AppendLine();

        if (products.Count == 0)
        {
            builder.AppendLine("Khong co san pham dang ban nao dang o muc ton kho bang hoac thap hon nguong nay.");
            return builder.ToString().Trim();
        }

        foreach (var product in products.Take(12))
        {
            builder.AppendLine($"- #{product.ProductId} {product.ProductName} | Ton: {product.StockQuantity} | Gia: {FormatMoney(product.Price)} | Model: {Safe(product.Model)} | Mau: {Safe(product.Color)} | Dung luong: {Safe(product.StorageCapacity)}");
        }

        return builder.ToString().Trim();
    }

    private string BuildHighestStockAnswer()
    {
        var products = _productService.GetAllProducts()
            .Where(p => p.Status)
            .OrderByDescending(p => p.StockQuantity)
            .ThenBy(p => p.ProductName)
            .Take(5)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("San pham con hang nhieu nhat");
        builder.AppendLine();

        if (products.Count == 0)
        {
            builder.AppendLine("Khong co san pham nao trong he thong.");
            return builder.ToString().Trim();
        }

        foreach (var product in products)
        {
            builder.AppendLine($"- #{product.ProductId} {product.ProductName} | Ton: {product.StockQuantity} | Mau: {Safe(product.Color)} | Dung luong: {Safe(product.StorageCapacity)}");
        }

        return builder.ToString().Trim();
    }

    private string BuildLowestStockAnswer()
    {
        var products = _productService.GetAllProducts()
            .Where(p => p.Status)
            .OrderBy(p => p.StockQuantity)
            .ThenBy(p => p.ProductName)
            .Take(5)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("San pham con it hang nhat");
        builder.AppendLine();

        if (products.Count == 0)
        {
            builder.AppendLine("Khong co san pham nao trong he thong.");
            return builder.ToString().Trim();
        }

        foreach (var product in products)
        {
            builder.AppendLine($"- #{product.ProductId} {product.ProductName} | Ton: {product.StockQuantity} | Mau: {Safe(product.Color)} | Dung luong: {Safe(product.StorageCapacity)}");
        }

        return builder.ToString().Trim();
    }

    private string BuildProductStockLookupAnswer(string question)
    {
        var matches = FindProductsFromQuestion(question);
        if (matches.Count == 0)
        {
            return "Khong xac dinh duoc ten san pham trong cau hoi, hoac khong tim thay san pham phu hop trong he thong.";
        }

        if (matches.Count > 1)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Tim thay nhieu san pham phu hop. Hay noi ro hon ten san pham can kiem tra:");
            foreach (var product in matches.Take(5))
            {
                builder.AppendLine($"- #{product.ProductId} {product.ProductName} | Mau: {Safe(product.Color)} | Dung luong: {Safe(product.StorageCapacity)}");
            }

            return builder.ToString().Trim();
        }

        var productMatch = matches[0];
        return $"San pham {productMatch.ProductName} hien con {productMatch.StockQuantity} cai trong kho.";
    }

    private string BuildInactiveProductsAnswer()
    {
        var products = _productService.GetAllProducts()
            .Where(p => !p.Status)
            .OrderBy(p => p.ProductName)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("San pham dang ngung ban");
        builder.AppendLine($"Tim thay {products.Count} san pham inactive.");
        builder.AppendLine();

        if (products.Count == 0)
        {
            builder.AppendLine("Hien tai khong co san pham inactive trong he thong.");
            return builder.ToString().Trim();
        }

        foreach (var product in products.Take(12))
        {
            builder.AppendLine($"- #{product.ProductId} {product.ProductName} | Ton: {product.StockQuantity} | Gia: {FormatMoney(product.Price)} | Mau: {Safe(product.Color)} | Dung luong: {Safe(product.StorageCapacity)}");
        }

        return builder.ToString().Trim();
    }

    private string BuildOrdersAnswer(string normalizedQuestion)
    {
        var orders = _orderService.GetAllOrders();
        var status = ExtractOrderStatus(normalizedQuestion);
        if (!string.IsNullOrWhiteSpace(status))
        {
            orders = orders.Where(o => string.Equals(o.Status, status, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var recentOnly = ContainsAny(normalizedQuestion, "today", "hom nay", "recent", "latest", "gan day", "moi nhat");
        if (ContainsAny(normalizedQuestion, "today", "hom nay"))
        {
            var today = DateTime.Today;
            orders = orders.Where(o => o.OrderDate.Date == today).ToList();
        }
        else if (recentOnly)
        {
            orders = orders.OrderByDescending(o => o.OrderDate).Take(10).ToList();
        }

        var builder = new StringBuilder();
        builder.AppendLine(string.IsNullOrWhiteSpace(status) ? "Tong quan don hang" : $"Don hang o trang thai: {status}");
        builder.AppendLine($"Tim thay {orders.Count} don hang phu hop.");
        builder.AppendLine();

        if (orders.Count == 0)
        {
            builder.AppendLine("Khong co don hang nao phu hop voi yeu cau hien tai.");
            return builder.ToString().Trim();
        }

        builder.AppendLine("Thong ke theo trang thai:");
        foreach (var group in orders.GroupBy(o => o.Status).OrderBy(g => g.Key))
        {
            builder.AppendLine($"- {group.Key}: {group.Count()} don");
        }

        builder.AppendLine();
        builder.AppendLine("Danh sach don hang:");
        foreach (var order in orders.OrderByDescending(o => o.OrderDate).Take(10))
        {
            builder.AppendLine($"- Order #{order.OrderId} | {order.OrderDate:dd/MM/yyyy HH:mm} | {order.Status} | Tong tien: {FormatMoney(order.TotalAmount)} | Nguoi nhan: {Safe(order.ReceiverName)}");
        }

        return builder.ToString().Trim();
    }

    private string BuildRecentStockInsAnswer()
    {
        var stockIns = _stockInService.GetAllStockIns()
            .OrderByDescending(s => s.StockInDate)
            .Take(10)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("Phieu nhap gan day");
        builder.AppendLine($"Tim thay {stockIns.Count} phieu nhap gan nhat.");
        builder.AppendLine();

        if (stockIns.Count == 0)
        {
            builder.AppendLine("Hien tai khong co phieu nhap nao trong he thong.");
            return builder.ToString().Trim();
        }

        foreach (var stockIn in stockIns)
        {
            builder.AppendLine($"- StockIn #{stockIn.StockInId} | {stockIn.StockInDate:dd/MM/yyyy HH:mm} | Ghi chu: {Safe(stockIn.Note)}");
        }

        return builder.ToString().Trim();
    }

    private string BuildFilteredProductsAnswer(string normalizedQuestion)
    {
        var color = ExtractColor(normalizedQuestion);
        var storage = ExtractStorage(normalizedQuestion);
        bool? status = ExtractProductStatus(normalizedQuestion);

        var products = _productService.FilterProducts(color, storage, status)
            .OrderBy(p => p.ProductName)
            .ToList();

        var builder = new StringBuilder();
        builder.AppendLine("Ket qua loc san pham");
        builder.AppendLine($"Mau: {Safe(color)}, Dung luong: {Safe(storage)}, Trang thai: {(status.HasValue ? (status.Value ? "Active" : "Inactive") : "Bat ky")}");
        builder.AppendLine($"Tim thay {products.Count} san pham phu hop.");
        builder.AppendLine();

        if (products.Count == 0)
        {
            builder.AppendLine("Khong co san pham nao khop voi bo loc hien tai.");
            return builder.ToString().Trim();
        }

        foreach (var product in products.Take(12))
        {
            builder.AppendLine($"- #{product.ProductId} {product.ProductName} | Ton: {product.StockQuantity} | Gia: {FormatMoney(product.Price)} | Mau: {Safe(product.Color)} | Dung luong: {Safe(product.StorageCapacity)} | Trang thai: {(product.Status ? "Active" : "Inactive")}");
        }

        return builder.ToString().Trim();
    }

    private List<Product> FindProductsFromQuestion(string question)
    {
        var products = _productService.GetAllProducts()
            .OrderByDescending(p => p.ProductName.Length)
            .ToList();

        var normalizedQuestion = Normalize(question);
        var directMatches = products
            .Where(p => normalizedQuestion.Contains(Normalize(p.ProductName)))
            .Take(5)
            .ToList();

        if (directMatches.Count > 0)
        {
            return directMatches;
        }

        var cleaned = Regex.Replace(normalizedQuestion, @"san pham|product|con bao nhieu|bao nhieu cai|bao nhieu chiec|so luong|ton kho|la bao nhieu|may|cai|\?|\.", " ", RegexOptions.IgnoreCase);
        cleaned = Regex.Replace(cleaned, @"\s+", " ").Trim();
        if (string.IsNullOrWhiteSpace(cleaned) || cleaned.Length < 3)
        {
            return [];
        }

        var searchResults = products
            .Where(p => ContainsAllImportantTerms(Normalize(p.ProductName), cleaned))
            .Take(5)
            .ToList();

        return searchResults;
    }

    private static bool ContainsAllImportantTerms(string normalizedProductName, string cleanedQuestion)
    {
        var terms = cleanedQuestion.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length >= 2)
            .Distinct()
            .ToList();

        return terms.Count > 0 && terms.All(normalizedProductName.Contains);
    }

    private async Task<string> AskModelWithSnapshotAsync(string question, AiIntegrationSettings settings, CancellationToken cancellationToken)
    {
        var context = BuildBusinessContext();
        var endpoint = new Uri(new Uri(AppendTrailingSlash(settings.BaseUrl)), "chat/completions");
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        }

        var payload = new ChatCompletionsRequest
        {
            Model = settings.Model,
            Temperature = settings.Temperature,
            Messages =
            [
                new ChatMessage("system", BuildSystemPrompt(settings.SystemPrompt)),
                new ChatMessage("user", $"Du lieu he thong:\n{context}\n\nCau hoi cua nhan vien:\n{question}")
            ]
        };

        request.Content = new StringContent(JsonSerializer.Serialize(payload, JsonOptions), Encoding.UTF8, "application/json");

        using var response = await HttpClient.SendAsync(request, cancellationToken);
        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Khong goi duoc AI service. Status {(int)response.StatusCode}: {rawResponse}");
        }

        var completion = JsonSerializer.Deserialize<ChatCompletionsResponse>(rawResponse, JsonOptions);
        var content = completion?.Choices?.FirstOrDefault()?.Message?.Content;
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new InvalidOperationException("AI khong tra ve noi dung hop le.");
        }

        return "Tra loi uoc luong tu LM Studio\n\n" + content.Trim();
    }

    private string BuildBusinessContext()
    {
        var products = _productService.GetAllProducts();
        var orders = _orderService.GetAllOrders();
        var stockIns = _stockInService.GetAllStockIns();

        var builder = new StringBuilder();
        builder.AppendLine($"Thoi gian snapshot: {DateTime.Now:dd/MM/yyyy HH:mm}");
        builder.AppendLine($"Tong so san pham: {products.Count}");
        builder.AppendLine($"San pham active: {products.Count(p => p.Status)}");
        builder.AppendLine($"San pham inactive: {products.Count(p => !p.Status)}");
        builder.AppendLine($"San pham het hang: {products.Count(p => p.StockQuantity == 0)}");
        builder.AppendLine($"San pham sap het hang (<=5): {products.Count(p => p.StockQuantity > 0 && p.StockQuantity <= 5)}");
        builder.AppendLine($"Tong so don hang: {orders.Count}");
        builder.AppendLine($"Tong so phieu nhap: {stockIns.Count}");
        return builder.ToString();
    }

    private static bool IsLowStockIntent(string normalizedQuestion)
    {
        return ContainsAny(normalizedQuestion,
            "low stock", "out of stock", "restock", "need restocking", "inventory low",
            "sap het hang", "het hang", "ton kho thap", "ton kho it", "nhap them", "can nhap");
    }

    private static bool IsHighestStockIntent(string normalizedQuestion)
    {
        return ContainsAny(normalizedQuestion,
            "con hang nhieu nhat", "ton kho nhieu nhat", "nhieu hang nhat", "stock nhieu nhat", "most stock", "highest stock");
    }

    private static bool IsLowestStockIntent(string normalizedQuestion)
    {
        return ContainsAny(normalizedQuestion,
            "con it hang nhat", "it hang nhat", "ton kho it nhat", "lowest stock", "least stock");
    }

    private static bool IsProductStockLookupIntent(string normalizedQuestion)
    {
        return ContainsAny(normalizedQuestion, "san pham", "product")
            && ContainsAny(normalizedQuestion,
                "con bao nhieu cai", "con bao nhieu", "ton kho bao nhieu", "bao nhieu cai", "stock bao nhieu", "how many left");
    }

    private static bool IsInactiveProductsIntent(string normalizedQuestion)
    {
        return ContainsAny(normalizedQuestion,
            "inactive product", "inactive items", "disabled products", "stopped selling",
            "inactive", "ngung ban", "tam dung", "khong hoat dong");
    }

    private static bool IsOrderIntent(string normalizedQuestion)
    {
        return ContainsAny(normalizedQuestion,
            "order", "pending", "processing", "completed", "cancelled", "canceled",
            "don hang", "dang cho", "cho xu ly", "dang xu ly", "hoan thanh", "da huy", "huy");
    }

    private static bool IsRecentStockInIntent(string normalizedQuestion)
    {
        return ContainsAny(normalizedQuestion,
            "stock in", "stock-in", "recent import", "recent received", "stockins",
            "phieu nhap", "nhap hang", "nhap kho", "gan day", "moi nhat");
    }

    private static bool IsProductFilterIntent(string normalizedQuestion)
    {
        return ContainsAny(normalizedQuestion,
            "product", "products", "color", "storage", "san pham", "mau", "dung luong", "bo nho")
            || Regex.IsMatch(normalizedQuestion, @"\b(64|128|256|512)\s?gb\b|\b1tb\b", RegexOptions.IgnoreCase);
    }

    private static bool ContainsAny(string text, params string[] keywords)
    {
        return keywords.Any(text.Contains);
    }

    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = text.Trim().ToLowerInvariant()
            .Replace((char)0x0111, 'd')
            .Replace((char)0x0110, 'd')
            .Normalize(NormalizationForm.FormD);

        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    private static int ExtractThreshold(string question, int defaultValue)
    {
        var match = Regex.Match(question, @"(?:under|below|less than|<=?|duoi|it hon|nho hon)\s*(\d+)", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    private static string? ExtractOrderStatus(string normalizedQuestion)
    {
        if (ContainsAny(normalizedQuestion, "pending", "dang cho", "cho xu ly")) return "Pending";
        if (ContainsAny(normalizedQuestion, "processing", "dang xu ly")) return "Processing";
        if (ContainsAny(normalizedQuestion, "completed", "hoan thanh")) return "Completed";
        if (ContainsAny(normalizedQuestion, "cancelled", "canceled", "da huy", "huy")) return "Cancelled";
        return null;
    }

    private string? ExtractColor(string normalizedQuestion)
    {
        var colors = _productService.GetAllColors()
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .OrderByDescending(c => c.Length)
            .ToList();

        return colors.FirstOrDefault(color => normalizedQuestion.Contains(Normalize(color)));
    }

    private static string? ExtractStorage(string normalizedQuestion)
    {
        var match = Regex.Match(normalizedQuestion, @"\b(64|128|256|512)\s?gb\b|\b1tb\b", RegexOptions.IgnoreCase);
        return match.Success ? match.Value.ToUpperInvariant().Replace(" ", string.Empty) : null;
    }

    private static bool? ExtractProductStatus(string normalizedQuestion)
    {
        if (ContainsAny(normalizedQuestion, "active", "dang ban", "hoat dong")) return true;
        if (ContainsAny(normalizedQuestion, "inactive", "ngung ban", "khong hoat dong")) return false;
        return null;
    }

    private static string BuildSystemPrompt(string? configuredPrompt)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(configuredPrompt))
        {
            builder.AppendLine(configuredPrompt.Trim());
        }

        builder.AppendLine("You are an AI assistant for warehouse and order staff in an iPhone inventory management system.");
        builder.AppendLine("Always answer in Vietnamese.");
        builder.AppendLine("Use only the provided business data snapshot.");
        builder.AppendLine("If the snapshot is insufficient, say what is missing instead of inventing details.");
        builder.AppendLine("Keep the answer concise and practical.");
        return builder.ToString();
    }

    private static AiIntegrationSettings LoadSettings()
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .Build();

        return new AiIntegrationSettings
        {
            Enabled = bool.TryParse(configuration["AiIntegration:Enabled"], out var enabled) && enabled,
            BaseUrl = configuration["AiIntegration:BaseUrl"] ?? "http://127.0.0.1:1234/v1/",
            Model = configuration["AiIntegration:Model"] ?? string.Empty,
            ApiKey = configuration["AiIntegration:ApiKey"] ?? "lm-studio",
            Temperature = float.TryParse(configuration["AiIntegration:Temperature"], out var temperature) ? temperature : 0.2f,
            SystemPrompt = configuration["AiIntegration:SystemPrompt"]
        };
    }

    private static string AppendTrailingSlash(string value)
    {
        return value.EndsWith("/", StringComparison.Ordinal) ? value : $"{value}/";
    }

    private static string Safe(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? "Bat ky" : value.Trim();
    }

    private static string FormatMoney(decimal amount)
    {
        return amount.ToString("N0", CultureInfo.InvariantCulture);
    }

    private sealed class AiIntegrationSettings
    {
        public bool Enabled { get; set; }
        public string BaseUrl { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string ApiKey { get; set; } = "lm-studio";
        public float Temperature { get; set; } = 0.2f;
        public string? SystemPrompt { get; set; }
    }

    private sealed class ChatCompletionsRequest
    {
        public string Model { get; set; } = string.Empty;
        public float Temperature { get; set; }
        public List<ChatMessage> Messages { get; set; } = [];
    }

    private sealed class ChatCompletionsResponse
    {
        public List<ChatChoice>? Choices { get; set; }
    }

    private sealed class ChatChoice
    {
        public ChatMessage? Message { get; set; }
    }

    private sealed record ChatMessage(string Role, string Content);
}


