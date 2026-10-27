using System.Text;

// Original, deterministic short effects. No samples or third-party audio.
const int rate = 22050;
string output = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Assets/Audio/Sfx"));
Directory.CreateDirectory(output);
var sounds = new (string name, double duration, int seed, Func<double, double, Random, double> sample)[]
{
    ("step_a", .105, 11, (t, u, r) => (Noise(r) * .34 + Math.Sin(2 * Math.PI * (76 - 30*u) * t) * .52) * Env(u, .04, 4.4)),
    ("step_b", .115, 13, (t, u, r) => (Noise(r) * .28 + Math.Sin(2 * Math.PI * (63 - 22*u) * t) * .48) * Env(u, .035, 4.0)),
    ("jump", .17, 17, (t, u, r) => (Math.Sin(2 * Math.PI * (150 + 180*u) * t) * .54 + Noise(r) * .12) * Env(u, .08, 3.2)),
    ("dash", .20, 19, (t, u, r) => (Noise(r) * .40 + Math.Sin(2 * Math.PI * (280 - 150*u) * t) * .42) * Env(u, .02, 2.2)),
    ("sword_whoosh", .19, 23, (t, u, r) => (Noise(r) * (.15 + .72*Math.Sin(Math.PI*u)) + Math.Sin(2 * Math.PI * (500 - 270*u) * t) * .17) * Env(u, .12, 2.0)),
    ("sword_hit", .14, 29, (t, u, r) => (Noise(r) * .41 + Math.Sin(2 * Math.PI * (170 - 75*u) * t) * .55) * Env(u, .015, 6.0)),
    ("hero_hurt", .21, 31, (t, u, r) => (Math.Sin(2 * Math.PI * (225 - 150*u) * t) * .65 + Noise(r) * .16) * Env(u, .01, 2.6)),
    ("heal", .34, 37, (t, u, r) => (Math.Sin(2 * Math.PI * 392*t) + .7*Math.Sin(2 * Math.PI * 523.25*t) + .45*Math.Sin(2 * Math.PI * 783.99*t)) * .28 * Env(u, .10, 1.8)),
    ("mage_warning", .31, 41, (t, u, r) => (Math.Sin(2 * Math.PI * (195 + 260*u) * t) * .43 + Math.Sin(2 * Math.PI * 610*t) * .14 + Noise(r) * .08) * Env(u, .12, 1.8)),
    ("mage_impact", .31, 43, (t, u, r) => (Noise(r) * .37 + Math.Sin(2 * Math.PI * (105 - 65*u) * t) * .65) * Env(u, .01, 3.8)),
    ("ring_attack", .27, 47, (t, u, r) => (Math.Sin(2 * Math.PI * (310 - 210*u) * t) * .46 + Noise(r) * .26) * Env(u, .03, 3.1)),
    ("trap_cast", .25, 53, (t, u, r) => (Noise(r) * .28 + Math.Sin(2 * Math.PI * (90 + 70*u) * t) * .36) * Env(u, .08, 2.3)),
    ("io_launch", .19, 59, (t, u, r) => (Math.Sin(2 * Math.PI * (430 + 210*u) * t) * .53 + Math.Sin(2 * Math.PI * (645 + 310*u) * t) * .19) * Env(u, .06, 2.7)),
    ("enemy_strike", .19, 61, (t, u, r) => (Noise(r) * .35 + Math.Sin(2 * Math.PI * (145 - 85*u) * t) * .58) * Env(u, .015, 4.1)),
};
foreach (var sound in sounds)
{
    string path = Path.Combine(output, sound.name + ".wav");
    int count = (int)(sound.duration * rate);
    short[] pcm = new short[count];
    var random = new Random(sound.seed);
    for (int i = 0; i < count; i++)
    {
        double t = (double)i / rate, u = (double)i / count;
        double edge = Math.Min(1, Math.Min(i, count - 1 - i) / (rate * .004));
        double value = Math.Clamp(sound.sample(t, u, random) * edge * .68, -1, 1);
        pcm[i] = (short)Math.Round(value * short.MaxValue);
    }
    using var stream = File.Create(path);
    using var writer = new BinaryWriter(stream, Encoding.ASCII);
    writer.Write(Encoding.ASCII.GetBytes("RIFF")); writer.Write(36 + count * 2);
    writer.Write(Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
    writer.Write((short)1); writer.Write((short)1); writer.Write(rate);
    writer.Write(rate * 2); writer.Write((short)2); writer.Write((short)16);
    writer.Write(Encoding.ASCII.GetBytes("data")); writer.Write(count * 2);
    foreach (short sample in pcm) writer.Write(sample);
    Console.WriteLine(Path.GetFileName(path));
}
static double Noise(Random r) => r.NextDouble() * 2 - 1;
static double Env(double u, double attack, double decay) => Math.Min(1, u / attack) * Math.Pow(1 - u, decay);
