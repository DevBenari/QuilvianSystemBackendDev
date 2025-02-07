using QRCoder;
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
}
