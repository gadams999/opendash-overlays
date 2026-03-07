using Svg.Skia;
using SkiaSharp;

// Windows 10/11 icon sizes for desktop apps:
// 16x16   - Small icon (title bar, taskbar small)
// 24x24   - Taskbar notification area
// 32x32   - Alt+Tab, taskbar medium
// 48x48   - Taskbar large, Explorer details
// 64x64   - Explorer extra large
// 256x256 - Explorer jumbo, high-DPI
//
// Windows 10/11 tile sizes (Start menu):
// 44x44   - App list icon
// 50x50   - Small tile (unpaneled)
// 70x70   - Small tile
// 150x150 - Medium tile
// 310x150 - Wide tile
// 310x310 - Large tile

var sizes = new (int Width, int Height, string Suffix)[]
{
    (16, 16, "16x16"),
    (24, 24, "24x24"),
    (32, 32, "32x32"),
    (48, 48, "48x48"),
    (64, 64, "64x64"),
    (256, 256, "256x256"),
    // Windows tile sizes
    (44, 44, "44x44_applist"),
    (50, 50, "50x50_small_tile"),
    (70, 70, "70x70_small_tile"),
    (150, 150, "150x150_medium_tile"),
    (310, 150, "310x150_wide_tile"),
    (310, 310, "310x310_large_tile"),
};

var variants = new (string SvgFile, string Prefix)[]
{
    ("wheel_overlay_light.svg", "light"),
    ("wheel_overlay_dark.svg", "dark"),
};

// Resolve paths relative to repo root
var repoRoot = args.Length > 0 ? args[0] : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
var svgDir = Path.Combine(repoRoot, "assets", "icons");
var outputDir = Path.Combine(repoRoot, "assets", "icons", "generated");

Directory.CreateDirectory(outputDir);

Console.WriteLine($"SVG source:  {svgDir}");
Console.WriteLine($"Output dir:  {outputDir}");
Console.WriteLine();

foreach (var (svgFile, prefix) in variants)
{
    var svgPath = Path.Combine(svgDir, svgFile);
    if (!File.Exists(svgPath))
    {
        Console.WriteLine($"WARNING: SVG not found: {svgPath}");
        continue;
    }

    using var svg = new SKSvg();
    svg.Load(svgPath);

    if (svg.Picture == null)
    {
        Console.WriteLine($"WARNING: Failed to parse SVG: {svgPath}");
        continue;
    }

    var svgBounds = svg.Picture.CullRect;
    Console.WriteLine($"Processing {svgFile} ({svgBounds.Width}x{svgBounds.Height})");

    foreach (var (width, height, suffix) in sizes)
    {
        var outputFile = Path.Combine(outputDir, $"wheel_overlay_{prefix}_{suffix}.png");

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.Transparent);

        // Scale SVG to fit within target dimensions, centered
        var scaleX = width / svgBounds.Width;
        var scaleY = height / svgBounds.Height;
        var scale = Math.Min(scaleX, scaleY);

        var scaledW = svgBounds.Width * scale;
        var scaledH = svgBounds.Height * scale;
        var offsetX = (width - scaledW) / 2f;
        var offsetY = (height - scaledH) / 2f;

        canvas.Translate(offsetX, offsetY);
        canvas.Scale(scale, scale);
        canvas.DrawPicture(svg.Picture);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(outputFile);
        data.SaveTo(stream);

        Console.WriteLine($"  -> {Path.GetFileName(outputFile)} ({width}x{height})");
    }

    Console.WriteLine();
}

Console.WriteLine("Done! Generated icons are in: assets/icons/generated/");
