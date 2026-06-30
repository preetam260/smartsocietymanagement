using QRCoder;
using ZXing;
using ZXing.Common;
using SkiaSharp;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace SmartSociety.Application.Services;

public class QRService : IQRService
{
    public Task<string> GenerateTokenAsync()
    {
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public Task<byte[]> GenerateImageAsync(string token)
    {
        using var qrGenerator = new QRCodeGenerator();

        var qrData = qrGenerator.CreateQrCode(
            token,
            QRCodeGenerator.ECCLevel.Q
        );

        using var qrCode = new PngByteQRCode(qrData);

        return Task.FromResult(qrCode.GetGraphic(20));
    }

    public async Task<string> DecodeImageAsync(IFormFile image)
    {
        await using var stream = image.OpenReadStream();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var bitmap = SKBitmap.Decode(memoryStream);

        if (bitmap == null)
            throw new BadRequestException("Unable to read the provided image.");

        int width = bitmap.Width;
        int height = bitmap.Height;

        var pixels = new byte[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var color = bitmap.GetPixel(x, y);

                byte gray = (byte)(
                    (color.Red + color.Green + color.Blue) / 3
                );

                pixels[y * width + x] = gray;
            }
        }

        var source = new RGBLuminanceSource(
            pixels,
            width,
            height,
            RGBLuminanceSource.BitmapFormat.Gray8
        );

        var binaryBitmap = new BinaryBitmap(
            new HybridBinarizer(source)
        );

        var reader = new MultiFormatReader();

        var result = reader.decode(binaryBitmap);

        if (result == null)
        {
            throw new BadRequestException(
                "Unable to decode QR code from the provided image."
            );
        }

        return result.Text;
    }
}