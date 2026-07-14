using SkiaSharp;
using SmartSociety.Application.DTOs;

namespace SmartSociety.Application.Services;

public static class BillStatementPdfGenerator
{
    private const float PageWidth  = 595f;
    private const float PageHeight = 842f;
    private const float Margin     = 48f;
    private const float LineHeight = 22f;

    public static byte[] Generate(
        string residentName,
        string residentEmail,
        string apartmentRef,
        DateTime moveInDate,
        DateTime moveOutDate,
        IEnumerable<BillResponseDto> bills)
    {
        var billList = bills.ToList();
        using var ms = new System.IO.MemoryStream();
        
        using var document = SKDocument.CreatePdf(ms);

        using var canvas = document.BeginPage(PageWidth, PageHeight);
        DrawPage(canvas, residentName, residentEmail, apartmentRef, moveInDate, moveOutDate, billList);
        document.EndPage();
        document.Close();

        return ms.ToArray();
    }

    private static void DrawPage(
        SKCanvas canvas,
        string name, string email, string apt,
        DateTime moveIn, DateTime moveOut,
        List<BillResponseDto> bills)
    {
        float y = Margin;

        using var headerPaint = new SKPaint { Color = new SKColor(30, 64, 175), Style = SKPaintStyle.Fill };
        canvas.DrawRect(0, 0, PageWidth, 80, headerPaint);

        using var titlePaint = new SKPaint { Color = SKColors.White };
        using var titleFont  = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 22);
        canvas.DrawText("SmartSociety Management", Margin, 38, SKTextAlign.Left, titleFont, titlePaint);

        using var subFont = new SKFont(SKTypeface.FromFamilyName("Arial"), 11);
        canvas.DrawText("Maintenance Bill Statement on Move-Out", Margin, 62, SKTextAlign.Left, subFont, titlePaint);

        y = 100;

        using var labelPaint  = new SKPaint { Color = new SKColor(107, 114, 128) };
        using var labelFont   = new SKFont(SKTypeface.FromFamilyName("Arial"), 9);
        using var valuePaint  = new SKPaint { Color = new SKColor(17, 24, 39) };
        using var valueFont   = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 11);
        using var normalFont  = new SKFont(SKTypeface.FromFamilyName("Arial"), 11);

        DrawLabelValue(canvas, Margin,          y, "Resident",   name,            labelFont, labelPaint, valueFont, valuePaint);
        DrawLabelValue(canvas, Margin + 180,    y, "Email",      email,           labelFont, labelPaint, valueFont, valuePaint);
        DrawLabelValue(canvas, Margin + 380,    y, "Apartment",  apt,             labelFont, labelPaint, valueFont, valuePaint);
        y += LineHeight * 2.2f;
        DrawLabelValue(canvas, Margin,          y, "Move-in",    moveIn.ToString("dd MMM yyyy"),  labelFont, labelPaint, valueFont, valuePaint);
        DrawLabelValue(canvas, Margin + 180,    y, "Move-out",   moveOut.ToString("dd MMM yyyy"), labelFont, labelPaint, valueFont, valuePaint);
        DrawLabelValue(canvas, Margin + 380,    y, "Generated",  DateTime.UtcNow.ToString("dd MMM yyyy"), labelFont, labelPaint, valueFont, valuePaint);
        y += LineHeight * 2f;

        using var divPaint = new SKPaint { Color = new SKColor(229, 231, 235), StrokeWidth = 1 };
        canvas.DrawLine(Margin, y, PageWidth - Margin, y, divPaint);
        y += 12;

        float[] cols = { Margin, Margin + 90, Margin + 185, Margin + 260, Margin + 330, Margin + 405 };
        string[] headers = { "Period", "Status", "Base (₹)", "Penalty (₹)", "Total (₹)", "Paid At" };

        using var thBg  = new SKPaint { Color = new SKColor(243, 244, 246), Style = SKPaintStyle.Fill };
        canvas.DrawRect(Margin - 4, y - 14, PageWidth - Margin * 2 + 8, LineHeight + 4, thBg);

        using var thFont  = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 9);
        using var thPaint = new SKPaint { Color = new SKColor(55, 65, 81) };
        for (int i = 0; i < headers.Length; i++)
            canvas.DrawText(headers[i], cols[i], y, SKTextAlign.Left, thFont, thPaint);

        y += LineHeight;

        bool alt = false;
        using var altBg = new SKPaint { Color = new SKColor(249, 250, 251), Style = SKPaintStyle.Fill };
        using var rowPaint = new SKPaint { Color = new SKColor(17, 24, 39) };
        using var rowFont  = new SKFont(SKTypeface.FromFamilyName("Arial"), 9);
        using var redPaint = new SKPaint { Color = new SKColor(185, 28, 28) };
        using var greenPaint = new SKPaint { Color = new SKColor(21, 128, 61) };

        decimal grandTotal = 0;
        decimal totalPenalty = 0;
        decimal totalUnpaid = 0;

        foreach (var bill in bills.OrderBy(b => b.Period))
        {
            if (y > PageHeight - Margin - 60) break; // safety

            if (alt)
                canvas.DrawRect(Margin - 4, y - 14, PageWidth - Margin * 2 + 8, LineHeight, altBg);

            var statusPaint = bill.Status.ToString() is "Paid" ? greenPaint :
                            bill.Status.ToString() is "Unpaid" or "Overdue" ? redPaint : rowPaint;

            canvas.DrawText(bill.Period,                                   cols[0], y, SKTextAlign.Left, rowFont, rowPaint);
            canvas.DrawText(bill.Status.ToString(),                        cols[1], y, SKTextAlign.Left, rowFont, statusPaint);
            canvas.DrawText(bill.BaseAmount.ToString("N2"),                cols[2], y, SKTextAlign.Left, rowFont, rowPaint);
            canvas.DrawText(bill.Penalty.ToString("N2"),                   cols[3], y, SKTextAlign.Left, rowFont, bill.Penalty > 0 ? redPaint : rowPaint);
            canvas.DrawText(bill.Total.ToString("N2"),                     cols[4], y, SKTextAlign.Left, rowFont, rowPaint);
            canvas.DrawText(bill.PaidAt.HasValue ? bill.PaidAt.Value.ToString("dd MMM yy") : "—",
                                                                        cols[5], y, SKTextAlign.Left, rowFont, rowPaint);
            y += LineHeight;
            alt = !alt;

            grandTotal   += bill.Total;
            totalPenalty += bill.Penalty;
            if (bill.Status.ToString() is "Unpaid" or "Overdue")
                totalUnpaid += bill.Total;
        }

        y += 8;
        canvas.DrawLine(Margin, y, PageWidth - Margin, y, divPaint);
        y += 16;

        using var totFont  = new SKFont(SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold), 10);
        using var totPaint = new SKPaint { Color = new SKColor(17, 24, 39) };
        canvas.DrawText("Total Billed:", cols[3], y, SKTextAlign.Left, totFont, totPaint);
        canvas.DrawText($"₹ {grandTotal:N2}", cols[4], y, SKTextAlign.Left, totFont, totPaint);
        y += LineHeight;

        if (totalUnpaid > 0)
        {
            using var unpaidPaint = new SKPaint { Color = new SKColor(185, 28, 28) };
            canvas.DrawText("Unpaid / Overdue:", cols[3], y, SKTextAlign.Left, totFont, unpaidPaint);
            canvas.DrawText($"₹ {totalUnpaid:N2}", cols[4], y, SKTextAlign.Left, totFont, unpaidPaint);
            y += LineHeight;
        }

        y += 16;
        using var notePaint = new SKPaint { Color = new SKColor(107, 114, 128) };
        using var noteFont  = new SKFont(SKTypeface.FromFamilyName("Arial"), 8);
        canvas.DrawText(
            "This statement was automatically generated by SmartSociety on move-out. " +
            "Unpaid dues have been transferred to the apartment owner.",
            Margin, y, SKTextAlign.Left, noteFont, notePaint);
    }

    private static void DrawLabelValue(
        SKCanvas canvas,
        float x, float y,
        string label, string value,
        SKFont labelFont, SKPaint labelPaint,
        SKFont valueFont, SKPaint valuePaint)
    {
        canvas.DrawText(label.ToUpper(), x, y, SKTextAlign.Left, labelFont, labelPaint);
        canvas.DrawText(value,           x, y + 14, SKTextAlign.Left, valueFont, valuePaint);
    }
}
