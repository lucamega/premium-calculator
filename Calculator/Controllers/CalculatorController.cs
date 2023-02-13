using System;
using System.Text;

namespace Calculator.Controllers
{
    public class CalculatorController
    {
        public async Task<IResult> GetAsset(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/")
            {
                var indexText = await File.ReadAllTextAsync(@"Assets\Calculator.html", Encoding.UTF8);
                return Results.Text(indexText, "text/html", Encoding.UTF8);
            }

            var segments = path.Split("/");
            if (segments.Length <= 2)
            {
                return Results.NotFound();
            }

            var folder = segments[1];
            if (folder == "assets")
            {
                var fileName = segments[2];
                var mimeType = MimeTypes.GetMimeType(fileName);
                var assetText = await File.ReadAllTextAsync(Path.Combine("Assets", segments[2]), Encoding.UTF8);
                return Results.Text(assetText, mimeType, Encoding.UTF8);
            }

            return Results.NotFound();
        }
    }
}
