using QRCoder;
//using SixLabors.ImageSharp.Drawing.Processing;
//using SixLabors.ImageSharp;
//using SixLabors.ImageSharp.PixelFormats;
//using SixLabors.ImageSharp.Processing;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

public class QrCodeHelper
{
    public static Bitmap GenerateQRCodeWithLogo(string text, string logoPath)
    {
        if (!File.Exists(logoPath))
        {
            throw new FileNotFoundException("Logo file not found", logoPath);
        }

        // Generate QR Code dengan ukuran lebih besar
        using var qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.H);
        using var qrCode = new PngByteQRCode(qrCodeData);

        // Perbesar ukuran QR Code (300x300 px)
        byte[] qrCodeBytes = qrCode.GetGraphic(10); // Skala lebih besar (default biasanya 4)

        // Load QR code ke dalam Bitmap
        Bitmap qrBitmap;
        using (var ms = new MemoryStream(qrCodeBytes))
        {
            qrBitmap = new Bitmap(ms);
        }

        // Pastikan QR Code dalam format yang bisa ditulis ulang
        Bitmap writableQrBitmap = new Bitmap(qrBitmap, new Size(300, 300));

        using (var graphics = Graphics.FromImage(writableQrBitmap))
        {
            graphics.DrawImage(qrBitmap, 0, 0, 300, 300);
        }

        // Tambahkan logo di tengah QR Code
        using var logoBitmap = new Bitmap(logoPath);
        using (var graphics = Graphics.FromImage(writableQrBitmap))
        {
            int logoSize = writableQrBitmap.Width / 3; // Logo 20% dari QR Code
            int x = (writableQrBitmap.Width - logoSize) / 2;
            int y = (writableQrBitmap.Height - logoSize) / 2;
            graphics.DrawImage(logoBitmap, new Rectangle(x, y, logoSize, logoSize));
        }

        return writableQrBitmap;
    }


    //public static byte[] GenerateQrCodeWithLogoPngBytes(string text, string logoPath)
    //{
    //    // 1. Generate QR code data
    //    var qrGenerator = new QRCodeGenerator();
    //    var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.H);
    //    var qrCode = new PngByteQRCode(qrData);

    //    // 2. Generate the QR code as byte[] PNG
    //    byte[] qrCodeBytes = qrCode.GetGraphic(10); // Larger scale for higher quality

    //    // 3. Load the QR code into ImageSharp Image
    //    using var qrImage = SixLabors.ImageSharp.Image.Load<Rgba32>(qrCodeBytes);

    //    // 4. Resize the QR code to 300x300
    //    qrImage.Mutate(x => x.Resize(300, 300));

    //    // 5. Add logo in the center of the QR code if logo exists
    //    if (File.Exists(logoPath))
    //    {
    //        using var logoImage = SixLabors.ImageSharp.Image.Load<Rgba32>(logoPath);

    //        // Resize logo to 33% of QR Code size
    //        int logoSize = qrImage.Width / 3;
    //        logoImage.Mutate(x => x.Resize(logoSize, logoSize));

    //        // Calculate the position to place the logo at the center
    //        int xPos = (qrImage.Width - logoSize) / 2;
    //        int yPos = (qrImage.Height - logoSize) / 2;

    //        // Draw the logo on the QR Code without changing its colors
    //        qrImage.Mutate(ctx =>
    //        {
    //            var options = new DrawingOptions
    //            {
    //                GraphicsOptions = new GraphicsOptions { Antialias = true }
    //            };
    //            ctx.DrawImage(logoImage, new SixLabors.ImageSharp.Point(xPos, yPos), 1f); // Keep logo colors intact
    //        });
    //    }

    //    // 6. Save the final QR Code with logo as PNG in memory stream
    //    using var memoryStream = new MemoryStream();
    //    qrImage.Save(memoryStream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
    //    return memoryStream.ToArray(); // Return as byte[]
    //}
}


