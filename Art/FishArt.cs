using System.Numerics;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using static Aquarium90.Art.Ink;

namespace Aquarium90.Art;

internal enum Species { Goldfish, Discus, Tang, Angelfish, Neon, Guppy }

internal sealed class FishArt : IDisposable
{
    private const float Ox = 76, Oy = 90;
    private static readonly Windows.Foundation.Rect Bounds = new(-Ox, -Oy, 148, 180);
    private readonly CanvasRenderTarget body;
    private readonly CanvasRenderTarget tail;

    internal FishArt(CanvasDevice device, Species kind)
    {
        tail = MakeSprite(device, g => PaintTail(g, kind));
        body = MakeSprite(device, g => PaintBody(g, kind));
    }

    internal void Draw(CanvasDrawingSession g, Matrix3x2 pose, float beat, float opacity)
    {
        var stage = g.Transform;
        g.Transform = new Matrix3x2(.85f + .15f * MathF.Cos(beat), .115f * MathF.Sin(beat), 0, 1, 0, 0)
            * Matrix3x2.CreateTranslation(-32, 0) * pose * stage;
        g.DrawImage(tail, Bounds, tail.Bounds, opacity);
        g.Transform = pose * stage;
        g.DrawImage(body, Bounds, body.Bounds, opacity);
        g.Transform = stage;
    }

    private static CanvasRenderTarget MakeSprite(CanvasDevice device, Action<CanvasDrawingSession> paint)
    {
        var bitmap = new CanvasRenderTarget(device, (float)Bounds.Width, (float)Bounds.Height, 192);
        using var g = bitmap.CreateDrawingSession();
        g.Clear(C(0, 0));
        g.Transform = Matrix3x2.CreateTranslation(Ox, Oy);
        paint(g);
        return bitmap;
    }

    private static void PaintTail(CanvasDrawingSession g, Species kind)
    {
        if (kind == Species.Goldfish)
        {
            Fin(g, C(0xE45930, 201), C(0xFFC270, 143), V(1, 0), V(-58, -20), -64, -37,
                9, -4, -24, -11, -41, -43, -67, -38, -52, -20, -60, 11, -34, 17,
                -14, 20, -14, -2, 9, -4);
            Fin(g, C(0xE6683A, 248), C(0xFFD390, 155), V(1, 0), V(-58, 27), -67, 45,
                9, -4, -19, -8, -46, -27, -52, -24, -41, -3, -62, 18, -73, 45,
                -53, 46, -26, 23, -17, 11, -7, 4, 1, 10, 9, 4);
        }
        else if (kind is Species.Tang or Species.Discus)
        {
            bool blue = kind == Species.Tang;
            Fin(g, blue ? C(0xFFBE23) : C(0xB14924), blue ? C(0xFFE987) : C(0xFFA157, 171),
                V(2, 0), V(-36, 0), -39, 26,
                8, -4, -13, -6, -19, -18, -39, -23, -34, -10, -34, 8, -38, 23,
                -26, 20, -11, 7, 8, 5);
        }
        else if (kind == Species.Guppy)
        {
            Fin(g, C(0xA7266E), C(0xC88DE7, 224), V(3, 0), V(-51, 3), -50, 40,
                5, -4, -12, -6, -29, -36, -49, -43, -54, -32, -49, -20, -59, -12,
                -55, 5, -56, 18, -54, 31, -49, 37, -49, 42, -48, 43,
                -29, 40, -10, 9, 5, 5);
            var r = new Random(241);
            for (int i = 0; i < 26; i++)
            {
                float x = r.Between(-49, -20), y = r.Between(-1, 1) * (-x - 12);
                g.FillEllipse(x, y, r.Between(.8f, 2.6f), r.Between(.85f, 2.6f), C(0x29193D, 142));
            }
        }
        else
        {
            bool silver = kind == Species.Angelfish;
            Fin(g, silver ? C(0x385953) : C(0xA52B54), silver ? C(0xA5CDAB, 150) : C(0xE278A0, 154),
                V(6, 0), V(-44, 2), -45, 27,
                8, -4, -18, -5, -30, -16, -44, -25, -41, -13, -31, -1, -27, 0,
                -30, 3, -41, 14, -45, 24, -32, 19, -21, 8, 8, 5);
        }
    }

    private static void PaintBody(CanvasDrawingSession g, Species kind)
    {
        var (top, bright, middle, bottom, depth, eyeX, eyeY) = kind switch
        {
            Species.Goldfish => (0xA84426u, 0xFFC177u, 0xF37B2Du, 0x8F321Eu, 34f, 33f, -8f),
            Species.Discus => (0x932B2Au, 0xFC9A51u, 0xE16E36u, 0x843031u, 44f, 31f, -15f),
            Species.Tang => (0x182E63u, 0x55C8F9u, 0x2678DFu, 0x172970u, 33f, 33f, -9f),
            Species.Angelfish => (0x66796Fu, 0xEDF5D6u, 0xB0C1A3u, 0x4C645Fu, 32f, 29f, -6f),
            Species.Neon => (0x183645u, 0xD0E7C4u, 0x37849Cu, 0x391D46u, 15f, 33f, -4f),
            _ => (0x48455Eu, 0xEFE1B5u, 0xA6BBB5u, 0x625086u, 19f, 34f, -4f)
        };
        BackFins(g, kind, C(bright), C(bottom));
        using var shape = kind switch
        {
            Species.Discus => Curve(g, true, -40, 2, -36, -17, -28, -44, -6, -45,
                15, -52, 28, -38, 37, -23, 42, -14, 38, -10, 47, -3,
                51, 0, 45, 8, 39, 11, 34, 30, 22, 46, 1, 45, -23, 45, -32, 28, -40, 2),
            Species.Tang => Curve(g, true, -39, 2, -34, -14, -25, -29, -6, -35,
                14, -42, 33, -25, 40, -14, 50, -5, 49, 4, 43, 8,
                34, 16, 23, 33, 0, 32, -19, 31, -30, 14, -39, 2),
            Species.Angelfish => Curve(g, true, -38, 0, -23, -10, -14, -27, -6, -32,
                7, -35, 16, -18, 29, -15, 38, -12, 45, -5, 44, 0,
                41, 4, 33, 3, 26, 10, 19, 23, 7, 36, -2, 31, -17, 28, -23, 9, -38, 0),
            Species.Neon or Species.Guppy => Curve(g, true, -38, 1, -21, -5, -16, -16, 3, -16,
                21, -18, 41, -13, 44, -7, 54, -4, 49, 4, 44, 6,
                31, 19, -5, 18, -16, 10, -25, 6, -30, 5, -38, 5),
            _ => Curve(g, true, -38, 1, -29, -14, -21, -32, 1, -34,
                19, -38, 34, -28, 40, -19, 45, -14, 48, -8, 47, -4,
                57, 1, 52, 10, 44, 13, 31, 26, 21, 35, 5, 35, -16, 36, -30, 14, -38, 4)
        };
        using var enamel = Linear(g, V(-14, -depth), V(9, depth),
            Stop(0, C(top)), Stop(.26f, C(bright)), Stop(.56f, C(middle)), Stop(.8f, C(middle)),
            Stop(1, C(bottom)));
        g.FillGeometry(shape, enamel);
        g.DrawGeometry(shape, C(top, 165), 1.3f);

        using (g.CreateLayer(1, shape))
        {
            Markings(g, kind);
            if (kind is not Species.Neon)
            {
                for (int row = -9; row <= 9; row++)
                for (int col = 0; col < 11; col++)
                {
                    float x = -35 + col * 5.1f + (row % 2) * 2.7f, y = row * 4.7f;
                    byte a = (byte)(row is >= -5 and <= 2 ? 55 : 25);
                    Stroke(g, C(bright, a), .85f, x, y - 2.1f, x - 3.5f, y - .7f,
                        x - 3.5f, y + .9f, x, y + 2.2f);
                }
            }
            using var sheen = new Microsoft.Graphics.Canvas.Brushes.CanvasRadialGradientBrush(g,
                C(0xFFF4C0, 101), C(0xFFF4C0, 0))
            { Center = V(9, -depth * .5f), RadiusX = 36, RadiusY = depth * .55f };
            g.FillRectangle(-54, -48, 115, 97, sheen);
        }
        Stroke(g, C(bottom, 149), 1.25f, eyeX - 11, eyeY - 10, eyeX - 17, eyeY + 5,
            eyeX - 11, eyeY + 18, eyeX - 1, eyeY + 23);
        Stroke(g, C(bright, 124), .9f, eyeX - 8, eyeY - 9, eyeX - 13, eyeY + 6,
            eyeX - 9, eyeY + 17, eyeX + 1, eyeY + 21);
        if (kind is not Species.Neon)
            Fin(g, C(middle, 133), C(bright, 95), V(17, 6), V(-14, 24), -13, 34,
                17, 4, 8, 4, 1, 16, -17, 21, -13, 24, -10, 27, -14, 32,
                -2, 31, 19, 16, 17, 4);
        Eye(g, eyeX, eyeY, kind == Species.Goldfish ? 6.6f : 5.5f,
            kind == Species.Discus ? C(0xFF953F) : C(0xE9D0A0));
        Stroke(g, C(bottom, 181), 1.15f, eyeX + 8, eyeY + 9, eyeX + 12, eyeY + 10,
            eyeX + 15, eyeY + 8, eyeX + 17, eyeY + 7);
    }

    private static void Markings(CanvasDrawingSession g, Species kind)
    {
        switch (kind)
        {
            case Species.Goldfish:
                Fill(g, C(0xFFF0BE, 166), -23, 17, -9, 7, -5, 8, 4, 15,
                    13, 23, 19, 17, 31, 20, 23, 36, 1, 41, -26, 26);
                Fill(g, C(0xFFF0C3, 102), -24, -25, -16, -29, -12, -23, -4, -25,
                    6, -30, 13, -27, 17, -33, -4, -43, -17, -38, -24, -25);
                break;
            case Species.Discus:
            case Species.Angelfish:
                bool angel = kind == Species.Angelfish;
                for (int i = 0; i < (angel ? 4 : 7); i++)
                {
                    float x = (angel ? -31 + i * 20 : -35 + i * 10);
                    Fill(g, angel ? C(0x24373C, 192) : C(0x692F36, 100), x - 4, -53,
                        x + 6, -26, x + 4, -2, x - 1, 14, x - 4, 37, x + 4, 46, x + 3, 53,
                        x + 9, 39, x + 1, 21, x + 6, 8, x + 13, -15, x + 8, -41, x + 4, -54);
                }
                if (!angel)
                    for (int row = 0; row < 13; row++)
                    {
                        float y = -37 + row * 5.8f;
                        Stroke(g, C(0x65DBC5, (byte)(row % 2 == 0 ? 188 : 110)), 1.75f,
                            -27, y + 6, -18, y - 5, -14, y - 4, -5, y - .5f,
                            5, y + 4, 13, y - 3, 21, y - 2, 27, y, 33, y + 6, 42, y + 10);
                    }
                break;
            case Species.Tang:
                Fill(g, C(0x131F51, 230), -39, -5, -29, -13, -28, -22, -17, -26,
                    3, -42, 23, -26, 34, -17, 19, -21, 6, -24, -7, -14,
                    -17, -8, -17, -2, -10, 6, -2, 15, 9, 13, 21, 5,
                    15, 29, -16, 26, -23, 13, -30, 7, -34, 7, -39, 6);
                break;
            case Species.Neon:
                Fill(g, C(0xFA315B, 224), -39, 0, -23, 3, -11, 1, 23, 1,
                    16, 16, 1, 19, -15, 11, -22, 9, -34, 5, -39, 4);
                Stroke(g, C(0x1DE8FF, 124), 6, -34, -1, -9, -5, 11, -7, 38, -5);
                Stroke(g, C(0xA8FAF0), 2.25f, -32, -2, -7, -5, 11, -7, 36, -6);
                break;
            case Species.Guppy:
                Stroke(g, C(0x5AF1C9, 183), 4, -27, -3, -8, -12, 8, -13, 26, -7);
                g.FillEllipse(-17, 4, 7, 3, C(0xB62675, 202));
                break;
        }
    }

    private static void BackFins(CanvasDrawingSession g, Species kind, Color light, Color dark)
    {
        if (kind == Species.Angelfish)
        {
            Fin(g, dark, C(0xC5DAB5, 172), V(-9, -19), V(-44, -71), -46, -83,
                11, -21, -1, -49, -22, -67, -47, -82, -35, -62, -34, -32, -32, -4,
                -15, -10, 0, -12, 11, -21);
            Fin(g, dark, C(0xB8CCAA, 160), V(-3, 20), V(-32, 66), -43, 66,
                11, 24, -3, 47, -22, 56, -43, 74, -33, 53, -31, 28, -32, 7,
                -12, 12, 1, 19, 11, 24);
            Stroke(g, C(0xC7D8B6, 218), 1.5f, 20, 17, 22, 43, 18, 61, 8, 86);
            Stroke(g, C(0xABBFA2, 151), 1.1f, 24, 16, 28, 41, 28, 53, 21, 77);
        }
        else
        {
            float h = kind switch { Species.Discus => 43, Species.Tang => 34, Species.Goldfish => 31, _ => 14 };
            Fin(g, dark, C(((uint)light.R << 16) | ((uint)light.G << 8) | light.B, 190),
                V(-10, -h * .8f), V(-5, -h - 10), 22, -h - 12,
                -36, -h * .16f, -31, -h * .68f, -21, -h - 10, -6, -h - 10,
                1, -h - 15, 12, -h - 9, 21, -h * .72f, 0, -h * .59f, -16, -h * .11f, -36, -h * .16f);
            Fin(g, dark, light, V(-8, h * .66f), V(-17, h + 4), -19, h + 12,
                13, h * .77f, 4, h * 1.08f, -12, h + 7, -21, h + 4,
                -19, h * .69f, -24, h * .41f, -29, h * .33f, -13, h * .49f, -2, h * .58f, 13, h * .77f);
        }
    }

    private static void Fin(CanvasDrawingSession g, Color baseColor, Color tipColor, Vector2 start,
        Vector2 end, float rayX, float rayY, params ReadOnlySpan<float> points)
    {
        using var shape = Curve(g, true, points);
        using var paint = Linear(g, start, end, Stop(0, baseColor), Stop(.60f, Mix(baseColor, tipColor, .75f)),
            Stop(1, tipColor));
        g.FillGeometry(shape, paint);
        using var clip = g.CreateLayer(1, shape);
        var tint = Mix(tipColor, C(0xFFF1C7, 120), .56f);
        for (int i = 0; i < 11; i++)
        {
            float t = (i - 5) * 6.8f;
            Stroke(g, i % 2 == 0 ? tint : Color.FromArgb(66, baseColor.R, baseColor.G, baseColor.B),
                i % 2 == 0 ? .8f : 1.3f, start.X, start.Y, (start.X * .62f + rayX * .38f), start.Y + t * .1f,
                rayX * .91f + t * .33f, rayY * .69f + t * .55f, rayX + t * .45f, rayY * .5f + t * 1.9f);
        }
    }

    private static void Eye(CanvasDrawingSession g, float x, float y, float r, Color iris)
    {
        g.FillEllipse(x, y, r + 1.35f, r + 1.35f, C(0x282B2E, 154));
        g.FillEllipse(x, y, r, r, iris);
        g.DrawEllipse(x, y, r - .75f, r - .75f, C(0xFFF0C0, 153), .85f);
        g.FillEllipse(x + r * .16f, y, r * .64f, r * .74f, C(0x11212B));
        g.FillEllipse(x - r * .09f, y - r * .38f, r * .28f, r * .21f, C(0xFFFCE6));
        g.FillEllipse(x + r * .4f, y + r * .33f, r * .11f, r * .11f, C(0xBBE5DB, 153));
    }

    public void Dispose() { body.Dispose(); tail.Dispose(); }
}
