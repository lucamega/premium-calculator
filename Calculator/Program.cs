using Calculator.Controllers;
using Calculator.Models.Http;

using Newtonsoft.Json;

namespace Calculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton(new CalculatorController());
            builder.Services.AddSingleton(new PremiumController());

            var app = builder.Build();
            app.MapGet("/", async (CalculatorController calculatorController, HttpContext context) =>
            {
                var path = context.Request.Path.Value;
                return await calculatorController.GetAsset(path);
            });
            app.MapGet("/assets/{asset}", async (CalculatorController calculatorController, HttpContext context) =>
            {
                var path = context.Request.Path.Value;
                return await calculatorController.GetAsset(path);
            });
            app.MapPost("/premium", async (PremiumController premiumController, HttpContext context) =>
            {
                try
                {
                    // Simulate a slow connection
                    await Task.Delay(1000);

                    string requestJson;
                    var request = context.Request;
                    using (var streamReader = new StreamReader(request.Body))
                    {
                        requestJson = await streamReader.ReadToEndAsync();
                    }
                    var premiumRequest = JsonConvert.DeserializeObject<PremiumRequest>(requestJson.Replace("\"*\"", "\"Any\""));
                    return premiumController.GetPremiums(premiumRequest);
                }
                catch (Exception)
                {
                    // Log the exception

                    var error = new ErrorResponse()
                    {
                        Message = "Internal server error."
                    };
                    return Results.Json(error, null, null, 500);
                }
            });
            app.Run();
        }
    }
}
