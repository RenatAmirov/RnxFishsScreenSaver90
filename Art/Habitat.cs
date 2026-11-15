using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Windows.UI;
using static Aquarium90.Art.Ink;

namespace Aquarium90.Art;

internal static class Habitat
{
    internal const float Height = 720;

    internal static void DrawWater(CanvasDrawingSession g, float w)
    {
        using var water = Linear(g, V(0, 0), V(w * .15f, Height),
            Stop(0, C(0x125963)), Stop(.085f, C(0x0C555E)), Stop(.27f, C(0x073E4B)),
            Stop(.62f, C(0x052A36)), Stop(.88f, C(0x082B2B)), Stop(1, C(0x041519)));
        g.FillRectangle(0, 0, w, Height, water);

        using var light = new CanvasRadialGradientBrush(g,
            [Stop(0, C(0x80F4D8, 130)), Stop(.25f, C(0x42C6B5, 62)),
                Stop(.65f, C(0x189D9B, 25)), Stop(1, C(0x189D9B, 0))])
        { Center = V(w * .37f, 0), RadiusX = w * .68f, RadiusY = 700 };
        g.FillRectangle(0, 0, w, Height, light);

        var r = new Random(216);
        // Far plants and mounds sit in a layer of blue-green haze.
        for (int i = 0; i < 22; i++)
        {
            float x = r.Between(-20, w + 20);
            float y = r.Between(558, 629);
            float h = r.Between(50, 230) * (x > w * .24f && x < w * .7f ? .68f : 1);
            Ribbon(g, r, x, y, h, r.Between(6, 24), C(0x144A50, (byte)r.Next(70, 140)), 3);
        }
        Fill(g, C(0x103B40), 0, 576, w * .10f, 499, w * .14f, 590, w * .30f, 562,
            w * .45f, 542, w * .57f, 598, w * .67f, 565, w * .78f, 526, w * .93f, 534, w, 553,
            w, 593, w, 640, w, 675, w * .6f, 675, w * .3f, 675, 0, 675);

        for (int i = 0; i < 2400; i++)
        {
            float x = r.Between(0, w), y = r.Between(15, 615);
            float size = r.Between(.2f, 1.1f);
            g.FillEllipse(x, y, size, size * .7f, C(0xC3DCC0, (byte)r.Next(4, 22)));
        }

        DrawFloor(g, w, r);
        for (int i = 0; i < 33; i++)
        {
            float x = r.Between(0, w), y = r.Between(586, 687);
            Rock(g, r, x, y, r.Between(6, 21), r.Between(2.6f, 8), C(0x557267), C(0x1D3735));
        }

        Ribbon(g, r, w * .87f, 654, 244, 15, C(0x296C5C), 10);
        Ribbon(g, r, w * .94f, 686, 201, 14, C(0x37694C), 11);
        Ribbon(g, r, w * .115f, 647, 220, 13, C(0x2A715E), 10);
        Rock(g, r, w * .14f, 640, w * .12f, 63, C(0x5D7773), C(0x162D32));
        Rock(g, r, w * .245f, 655, w * .082f, 37, C(0x7A8773), C(0x25373A));
        Rock(g, r, w * .085f, 673, w * .07f, 32, C(0x63766C), C(0x172F36));
        Rock(g, r, w * .805f, 662, w * .11f, 44, C(0x63816D), C(0x1C3330));
        Rock(g, r, w * .91f, 678, w * .08f, 51, C(0x7A8D71), C(0x263B32));
        Driftwood(g, w);
        LeafCluster(g, r, w * .14f, 665, .82f, C(0x5EA675), 11);
        LeafCluster(g, r, w * .79f, 668, .64f, C(0x799F62), 8);
        LeafCluster(g, r, w * .96f, 681, 1.05f, C(0x428755), 9);
    }

    internal static void DrawForeground(CanvasDrawingSession g, float w)
    {
        var random = new Random(6132);
        LeafCluster(g, random, w * .015f, 728, 1.14f, C(0x6AAB74), 14);
        LeafCluster(g, random, w * .245f, 713, .55f, C(0x5B8B62), 9);
        LeafCluster(g, random, w * .883f, 731, .87f, C(0x78A259), 11);
        Ribbon(g, random, w * .996f, 733, 262, 13, C(0x447754), 9);
        Ribbon(g, random, w * .049f, 718, 184, 8, C(0x48936B), 6);

        using var vignette = new CanvasRadialGradientBrush(g,
            [Stop(0, C(0x020D16, 0)), Stop(.56f, C(0x020D16, 0)),
                Stop(.82f, C(0x020D16, 65)), Stop(1, C(0x020D16, 172))])
        { Center = V(w * .48f, 342), RadiusX = w * .73f, RadiusY = 510 };
        g.FillRectangle(0, 0, w, Height, vignette);
    }

    private static void DrawFloor(CanvasDrawingSession g, float w, Random r)
    {
        using var floor = Curve(g, true, 0, 594,
            w * .22f, 563, w * .32f, 623, w * .55f, 588,
            w * .68f, 564, w * .9f, 579, w, 599, w, 642, w, 708, w, 720,
            w * .72f, 720, w * .23f, 720, 0, 720);
        using var sand = Linear(g, V(0, 579), V(0, 720),
            Stop(0, C(0x224848)), Stop(.26f, C(0x436757)), Stop(.60f, C(0x627960)),
            Stop(.82f, C(0x566953)), Stop(1, C(0x263C35)));
        g.FillGeometry(floor, sand);
        using var clip = g.CreateLayer(1, floor);
        using var patch = new CanvasRadialGradientBrush(g, C(0x79A786, 135), C(0x79A786, 0))
        { Center = V(w * .51f, 649), RadiusX = w * .47f, RadiusY = 85 };
        g.FillRectangle(0, 570, w, 150, patch);
        for (int i = 0; i < 46; i++)
        {
            float x = r.Between(0, w), y = r.Between(580, 716), length = r.Between(6, w * .11f);
            Stroke(g, C(0xBACB9C, (byte)r.Next(6, 26)), r.Between(.5f, 2),
                x, y, x + length * .2f, y - 4, x + length * .75f, y + 2, x + length, y - 2);
        }
        for (int i = 0; i < 4000; i++)
        {
            float y = r.Between(578, 725), x = r.Between(0, w);
            float a = r.Between(.55f, 3.2f) * (.38f + (y - 578) / 148);
            float b = a * r.Between(.31f, .69f);
            g.FillEllipse(x, y, a, b, r.Next(3) == 0 ? C(0xBED2AA, (byte)r.Next(25, 95))
                : C(0x112D2C, (byte)r.Next(30, 118)));
        }
    }

    private static void Ribbon(CanvasDrawingSession g, Random r, float x, float y, float height,
        float width, Color color, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float bx = x + r.Between(-43, 43);
            float h = height * r.Between(.40f, 1.08f);
            float lean = r.Between(-100, 100);
            float bend = r.Between(-39, 39);
            float leaf = width * r.Between(.55f, 1.27f);
            float tip = bx + lean;
            using var path = Curve(g, true, bx, y,
                bx + bend - leaf, y - h * .37f, tip - bend - leaf, y - h * .73f, tip, y - h,
                tip - bend + leaf, y - h * .72f, bx + bend + leaf, y - h * .33f, bx + 2, y);
            using var brush = Linear(g, V(bx, y - h), V(bx + 20, y),
                Stop(0, Mix(color, C(0x8CD9A2, color.A), r.Between(.08f, .31f))),
                Stop(.44f, color), Stop(1, Mix(color, C(0x10372F, color.A), .75f)));
            g.FillGeometry(path, brush);
            if (count > 3)
                Stroke(g, C(0xBCE6A5, 43), .7f, bx + 1, y - 5,
                    bx + bend, y - h * .36f, tip - bend, y - h * .72f, tip, y - h);
        }
    }

    private static void LeafCluster(CanvasDrawingSession g, Random r, float x, float y,
        float scale, Color light, int count)
    {
        for (int i = 0; i < count; i++)
        {
            float angle = -2.97f + 2.75f * (i / (float)(count - 1)) + r.Between(-.22f, .22f);
            Vector2 start = V(x + r.Between(-28, 28) * scale, y + r.Between(-10, 8) * scale);
            Vector2 end = start + V(MathF.Cos(angle), MathF.Sin(angle)) * r.Between(52, 172) * scale;
            Vector2 axis = Vector2.Normalize(end - start);
            Vector2 side = V(-axis.Y, axis.X);
            float width = r.Between(11, 29) * scale;
            Vector2 root = Vector2.Lerp(start, end, r.Between(.24f, .51f));
            Vector2 a = Vector2.Lerp(root, end, .13f), b = Vector2.Lerp(root, end, .63f);
            g.DrawLine(start, root, Mix(light, C(0x14352E), .68f), 2.3f * scale);
            using var leaf = Curve(g, true, root.X, root.Y,
                (a + side * width).X, (a + side * width).Y,
                (b + side * width).X, (b + side * width).Y, end.X, end.Y,
                (b - side * width * .86f).X, (b - side * width * .86f).Y,
                (a - side * width * .84f).X, (a - side * width * .84f).Y, root.X, root.Y);
            Vector2 middle = Vector2.Lerp(root, end, .45f);
            var tone = Mix(light, C(0x164C46), r.Between(.12f, .62f));
            using var paint = Linear(g, middle - side * width, middle + side * width,
                Stop(0, C(0x153F39)), Stop(.37f, tone), Stop(.5f, Mix(tone, C(0xB9DD85), .27f)),
                Stop(.55f, Mix(tone, C(0x317256), .35f)), Stop(1, C(0x234738)));
            g.FillGeometry(leaf, paint);
            g.DrawGeometry(leaf, C(0x12372D, 110), .9f);
            g.DrawLine(root, end, C(0xC6D890, 69), .8f * scale);
            for (int j = 1; j <= 4; j++)
            {
                float t = j / 6f;
                Vector2 vein = Vector2.Lerp(root, end, t);
                Vector2 reach = side * (width * .60f * MathF.Sin(t * MathF.PI));
                g.DrawLine(vein, vein + reach + axis * 8 * scale, C(0xC6D890, 41), .6f);
                g.DrawLine(vein, vein - reach + axis * 8 * scale, C(0x0E322B, 90), .6f);
            }
        }
    }

    private static void Rock(CanvasDrawingSession g, Random r, float x, float y, float rx, float ry,
        Color cap, Color baseColor)
    {
        g.FillEllipse(x + rx * .05f, y - ry * .08f, rx * 1.14f, ry * .27f, C(0x061D22, 136));
        float crest = x + rx * r.Between(-.55f, .24f);
        using var shape = Curve(g, true, x - rx, y - .08f * ry,
            x - rx * 1.09f, y - ry * .55f, crest - rx * .48f, y - ry * .91f, crest, y - ry,
            crest + rx * .42f, y - ry * 1.17f, x + rx * .79f, y - ry * .63f, x + rx, y - ry * .2f,
            x + rx * 1.15f, y + ry * .26f, x - rx * .84f, y + ry * .19f, x - rx, y - .08f * ry);
        using var brush = Linear(g, V(crest - rx * .38f, y - ry), V(crest + rx * .21f, y),
            Stop(0, cap), Stop(.41f, Mix(cap, baseColor, .27f)), Stop(1, baseColor));
        g.FillGeometry(shape, brush);
        using var clip = g.CreateLayer(1, shape);
        int count = (int)Math.Clamp(rx * ry / 13, 9, 430);
        for (int i = 0; i < count; i++)
        {
            float p = r.Between(.4f, MathF.Max(1.35f, rx * .044f));
            g.FillEllipse(x + r.Between(-rx, rx), y - r.Between(-.12f, 1.08f) * ry, p, p * .47f,
                r.Next(2) == 0 ? C(0xC2CEAA, 34) : C(0x0C2023, 57));
        }
        if (rx > 30)
        {
            Fill(g, C(0xB5C29E, 33), crest - rx * .38f, y - ry * .89f,
                crest, y - ry, crest + rx * .42f, y - ry * 1.07f, x + rx * .61f, y - ry * .51f,
                x + rx * .11f, y - ry * .66f, x - rx * .13f, y - ry * .55f, crest - rx * .38f, y - ry * .89f);
            Stroke(g, C(0x162A2B, 111), 1.2f, x - rx * .95f, y - ry * .26f,
                x - rx * .17f, y - ry * .53f, x + rx * .04f, y - ry * .34f, x + rx * .7f, y - ry * .59f);
        }
    }

    private static void Driftwood(CanvasDrawingSession g, float w)
    {
        // A forked root partially buried in the gravel.
        using var root = Curve(g, true, w * .16f, 657,
            w * .22f, 650, w * .25f, 646, w * .29f, 617,
            w * .32f, 591, w * .345f, 555, w * .345f, 526,
            w * .345f, 500, w * .328f, 473, w * .317f, 465,
            w * .346f, 471, w * .376f, 509, w * .37f, 545,
            w * .393f, 533, w * .416f, 527, w * .434f, 532,
            w * .453f, 535, w * .465f, 521, w * .486f, 512,
            w * .475f, 539, w * .451f, 554, w * .43f, 553,
            w * .399f, 555, w * .384f, 571, w * .366f, 597,
            w * .346f, 627, w * .403f, 631, w * .431f, 652,
            w * .379f, 642, w * .335f, 652, w * .306f, 657,
            w * .259f, 674, w * .226f, 663, w * .16f, 657);
        using var wood = Linear(g, V(w * .326f, 514), V(w * .38f, 658),
            Stop(0, C(0x775F45)), Stop(.41f, C(0x655C48)), Stop(.75f, C(0x423B2E)),
            Stop(1, C(0x272D27)));
        g.FillGeometry(root, wood);
        g.DrawGeometry(root, C(0x102B29, 166), 1.3f);
        using var clip = g.CreateLayer(1, root);
        for (int i = 0; i < 11; i++)
        {
            float k = i * 1.9f - 11;
            Stroke(g, i % 3 == 0 ? C(0xAD9565, 103) : C(0x201E19, 120), i % 3 == 0 ? 1.1f : 1.6f,
                w * .19f, 659 + k, w * .32f, 652 + k, w * .332f, 575 + k, w * .357f, 548 + k,
                w * .364f, 523 + k, w * .343f, 485 + k, w * .317f, 466 + k);
            Stroke(g, i % 3 == 0 ? C(0xAD9565, 103) : C(0x201E19, 120), .9f,
                w * .327f, 610 + k, w * .36f, 569 + k, w * .402f, 535 + k, w * .435f, 542 + k,
                w * .45f, 547 + k, w * .471f, 529 + k, w * .489f, 511 + k);
        }
    }
}
