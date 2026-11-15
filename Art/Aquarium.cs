using System.Numerics;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using static Aquarium90.Art.Ink;

namespace Aquarium90.Art;

internal sealed class Aquarium(CanvasDevice device, int seed) : IDisposable
{
    private readonly FishArt[] sprites = Enum.GetValues<Species>().Select(kind => new FishArt(device, kind)).ToArray();
    private readonly Swimmer[] fish = MakeFish(seed);
    private readonly Bubble[] bubbles = MakeBubbles(seed);
    private readonly Speck[] specks = MakeSpecks(seed);
    private readonly CanvasLinearGradientBrush beamPaint = Linear(device, V(0, -12), V(0, 480),
        Stop(0, C(0xACEBD4, 17)), Stop(.23f, C(0x8BF1CF, 11)), Stop(.65f, C(0x72D9C9, 4)),
        Stop(1, C(0x56BEAE, 0)));
    private readonly CanvasRadialGradientBrush bubblePaint = new(device,
        [Stop(0, C(0x2E9DA1, 0)), Stop(.70f, C(0x2E9DA1, 5)),
            Stop(.86f, C(0x8DDBD2, 45)), Stop(1, C(0xD1FFE4, 112))]);
    private CanvasRenderTarget? backdrop, foreground;
    private float worldWidth;
    private float waterTime, swimTime;

    internal void Draw(CanvasDrawingSession g, float width, float height, float dt, AquariumOptions options)
    {
        if (width < 1 || height < 1) return;
        waterTime += dt;
        swimTime += dt * options.Speed / 100f;
        float proposed = Math.Clamp(MathF.Round(width / height * Habitat.Height / 32) * 32, 352, 2560);
        if (proposed != worldWidth)
        {
            backdrop?.Dispose();
            foreground?.Dispose();
            worldWidth = proposed;
            backdrop = MakeLayer(Habitat.DrawWater);
            foreground = MakeLayer(Habitat.DrawForeground);
        }

        var old = g.Transform;
        g.Transform = Matrix3x2.CreateScale(width / worldWidth, height / Habitat.Height) * old;
        g.DrawImage(backdrop!, 0, 0);
        DrawLight(g, worldWidth);
        DrawSpecks(g, worldWidth);
        if (options.Bubbles) DrawBubbles(g, worldWidth);
        foreach (var f in fish)
            if (f.Index < options.FishCount) DrawFish(g, f, worldWidth);
        g.DrawImage(foreground!, 0, 0);
        g.Transform = old;
    }

    private CanvasRenderTarget MakeLayer(Action<CanvasDrawingSession, float> draw)
    {
        var layer = new CanvasRenderTarget(device, worldWidth, Habitat.Height, 96);
        using var g = layer.CreateDrawingSession();
        g.Clear(C(0, 0));
        draw(g, worldWidth);
        return layer;
    }

    private void DrawLight(CanvasDrawingSession g, float w)
    {
        for (int i = 0; i < 7; i++)
        {
            float x = w * (.08f + .104f * i) + 21 * MathF.Sin(waterTime * .22f + i);
            float tip = x + 70 + 78 * MathF.Sin(i * .56f + waterTime * .31f);
            float y = 381 + 106 * MathF.Sin(i + 1);
            float span = 27 + 15 * MathF.Sin(waterTime * .4f + i * .8f);
            beamPaint.EndPoint = V(0, y - 18);
            for (int band = 0; band < 3; band++)
            {
                float feather = band * span * .42f;
                using var shaft = CanvasGeometry.CreatePolygon(g, [V(x + band * 1.4f, -10),
                    V(x + 10 + i * 2 - band * 1.4f, -9),
                    V(tip + span * 2 - feather, y), V(tip - span + feather, y + 38)]);
                g.FillGeometry(shaft, beamPaint);
            }
        }
        for (int i = 0; i < 46; i++)
        {
            float a = i * 1.793f, z = waterTime * (.32f + i % 4 * .054f);
            float x = ((i * .61371f) % 1) * w + 21 * MathF.Sin(z + a);
            float y = 3 + (i % 12) * 3.1f + 4 * MathF.Sin(z + a + .7f);
            float len = (15 + (i * 19 % 101)) * (.73f + .19f * MathF.Sin(z + a));
            byte opacity = (byte)(8 + (i % 11) * 2 + 8 * MathF.Sin(z + a));
            var pale = C(0xB6F4D8, opacity);
            g.DrawLine(x - len * .5f, y - 2, x, y, pale, .75f);
            g.DrawLine(x, y, x + len * .75f, y - 1.4f, pale, .75f);
        }
    }

    private void DrawSpecks(CanvasDrawingSession g, float w)
    {
        foreach (var p in specks)
        {
            float x = (p.X + waterTime * p.Speed) % 1;
            float y = p.Y + MathF.Sin(waterTime * .34f + p.X * 23) * 6;
            g.FillCircle(x * w, y, p.Size, C(0xC4E7C9, p.Opacity));
        }
    }

    private void DrawBubbles(CanvasDrawingSession g, float w)
    {
        foreach (var bubble in bubbles)
        {
            float t = (bubble.Start + waterTime * bubble.Speed) % 1;
            float x = bubble.X * w + MathF.Sin(t * 13 + bubble.Start * 7) * (2 + 10 * t) + t * bubble.Drift;
            float y = 656 - 652 * t;
            float radius = bubble.Radius * (.67f + t * .53f);
            float fade = MathF.Min(1, MathF.Min(t * 26, (1 - t) * 20));
            bubblePaint.Center = V(x, y);
            bubblePaint.RadiusX = radius;
            bubblePaint.RadiusY = radius * 1.02f;
            bubblePaint.Opacity = fade;
            g.FillEllipse(x, y, radius, radius * 1.02f, bubblePaint);
            g.FillEllipse(x - radius * .31f, y - radius * .51f, radius * .25f, radius * .13f,
                C(0xD9FCEE, (byte)(fade * 154)));
        }
    }

    private void DrawFish(CanvasDrawingSession g, Swimmer f, float w)
    {
        float theta = f.Phase + swimTime * f.Rate;
        float course = MathF.Cos(theta);
        float amplitude = w * f.Range;
        float x = (w * .5f + MathF.Sin(theta) * amplitude) + f.Offset;
        float d = swimTime * f.Rate * 1.54f + f.Sway;
        float y = f.Lane + f.Rise * MathF.Sin(d) + MathF.Sin(swimTime * .83f + f.Sway) * 4;
        float beat = swimTime * f.Beat + f.Sway;
        float profile = .29f + .71f * MathF.Sqrt(MathF.Abs(course));
        float heading = MathF.CopySign(1, course);
        float tilt = .115f * MathF.Cos(d) * heading;
        var pose = Matrix3x2.CreateScale(f.Size * profile * heading, f.Size)
            * Matrix3x2.CreateRotation(tilt) * Matrix3x2.CreateTranslation(x, y);
        sprites[(int)f.Kind].Draw(g, pose, beat, f.Opacity);
    }

    private static Swimmer[] MakeFish(int seed)
    {
        var random = new Random(seed);
        // The first six are a representative set even at the lowest fish count.
        var fish = new List<Swimmer>();
        Add(Species.Goldfish, .99f, 317, -.58f, .105f, .38f, 0);
        Add(Species.Discus, .83f, 409, 2.15f, .109f, .37f, 1);
        Add(Species.Tang, .83f, 235, 1.09f, .128f, .41f, 2);
        Add(Species.Angelfish, .69f, 196, 3.75f, .123f, .41f, 3);
        Add(Species.Neon, .44f, 294, 2.9f, .154f, .44f, 4);
        Add(Species.Guppy, .58f, 469, 3.65f, .133f, .38f, 5);
        for (int i = 6; i < 28; i++)
        {
            if (i is < 10 or (>= 13 and < 17) or (>= 21 and < 25))
            {
                int group = i < 10 ? 0 : i < 17 ? 1 : 2, n = i is < 10 ? i - 6 : i < 17 ? i - 13 : i - 21;
                Add(Species.Neon, random.Between(.28f, .43f), 130 + group * 130 + n * 18,
                    0.38f + group * 2.2f + n * .095f, .151f + group * .007f, .44f, i, (n - 1.5f) * 21);
            }
            else
            {
                var kind = (i % 5) switch
                {
                    0 => Species.Goldfish, 1 => Species.Discus, 2 => Species.Tang,
                    3 => Species.Angelfish, _ => Species.Guppy
                };
                Add(kind, random.Between(.48f, .72f), random.Between(137, 493),
                    random.Between(0, MathF.Tau), random.Between(.103f, .152f), random.Between(.31f, .47f), i);
            }
        }
        return [.. fish.OrderBy(f => f.Size)];

        void Add(Species kind, float size, float lane, float phase, float rate, float range, int index, float offset = 0) =>
            fish.Add(new Swimmer(index, kind, size, .68f + MathF.Min(size, 1) * .32f, lane, phase, rate,
                range, offset, random.Between(0, 6.28f), random.Between(14, 57),
                random.Between(7.0f, 10.0f) + (1 - size) * 7));
    }

    private static Bubble[] MakeBubbles(int seed)
    {
        var random = new Random(seed + 37);
        return Enumerable.Range(0, 38).Select(i => new Bubble(
            (i < 22 ? .228f : .772f) + random.Between(-.012f, .012f),
            random.NextSingle(), random.Between(.048f, .094f), random.Between(1.4f, 6.8f),
            random.Between(-16, 22))).ToArray();
    }

    private static Speck[] MakeSpecks(int seed)
    {
        var random = new Random(seed + 85);
        return Enumerable.Range(0, 54).Select(_ => new Speck(random.NextSingle(),
            random.Between(24, 627), random.Between(.001f, .004f),
            random.Between(.45f, 1.35f), (byte)random.Next(15, 65))).ToArray();
    }

    public void Dispose()
    {
        foreach (var sprite in sprites) sprite.Dispose();
        backdrop?.Dispose();
        foreground?.Dispose();
        beamPaint.Dispose();
        bubblePaint.Dispose();
    }

    private sealed record Swimmer(int Index, Species Kind, float Size, float Opacity, float Lane, float Phase,
        float Rate, float Range, float Offset, float Sway, float Rise, float Beat);
    private sealed record Bubble(float X, float Start, float Speed, float Radius, float Drift);
    private sealed record Speck(float X, float Y, float Speed, float Size, byte Opacity);
}
