using QRCoder;
using SkiaSharp;

public class QrCodeHelper
{
    public static byte[] GenerateQrCodeWithLogoPngBytes(string text, string logoPath)
    {
        // 1. Generate QR code data using QRCoder library
        var qrGenerator = new QRCodeGenerator();
        var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.H);
        var qrCode = new PngByteQRCode(qrData);

        // 2. Generate the QR code as byte[] PNG
        byte[] qrCodeBytes = qrCode.GetGraphic(10); // Larger scale for higher quality

        // 3. Load the QR code into SkiaSharp SKImage
        using var qrStream = new MemoryStream(qrCodeBytes);
        using var qrBitmap = SKBitmap.Decode(qrStream);

        // 4. Resize the QR code to 300x300
        var qrImage = qrBitmap.Resize(new SKImageInfo(300, 300), SKFilterQuality.High);

        // 5. Create surface to draw QR code and logo
        SKSurface surface = SKSurface.Create(new SKImageInfo(qrImage.Width, qrImage.Height));
        var canvas = surface.Canvas;

        // Draw the QR code on the canvas
        canvas.DrawBitmap(qrImage, 0, 0);

        // 6. Add logo in the center of the QR code if logo exists
        if (File.Exists(logoPath))
        {
            using var logoImage = SKBitmap.Decode(logoPath);

            // Resize logo to 33% of QR Code size
            int logoSize = qrImage.Width / 3;
            var logoResized = logoImage.Resize(new SKImageInfo(logoSize, logoSize), SKFilterQuality.High);

            // Calculate the position to place the logo at the center
            int xPos = (qrImage.Width - logoSize) / 2;
            int yPos = (qrImage.Height - logoSize) / 2;

            // Draw the logo on the QR code in the center
            canvas.DrawBitmap(logoResized, xPos, yPos);
        }

        // 7. Capture the final image from the surface and encode as PNG
        using var finalImage = surface.Snapshot();
        using var finalMemoryStream = new MemoryStream();
        finalImage.Encode(SKEncodedImageFormat.Png, 100).SaveTo(finalMemoryStream);

        return finalMemoryStream.ToArray(); // Return as byte[]
    }
}


