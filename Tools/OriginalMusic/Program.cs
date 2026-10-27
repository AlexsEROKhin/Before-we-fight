using System;
using System.IO;

// Original score made entirely from oscillators and deterministic noise.
// No recordings, sound libraries, borrowed melodies, or third-party samples.
const int sampleRate = 32000;
const int beatsPerBar = 4;
const int bars = 32;
const double bpm = 144.0;
const double secondsPerBeat = 60.0 / bpm;
int sampleCount = (int)Math.Round(bars * beatsPerBar * secondsPerBeat * sampleRate);
float[] mix = new float[sampleCount];

double Frequency(int midi) => 440.0 * Math.Pow(2.0, (midi - 69) / 12.0);

void AddTone(double beat, double durationBeats, int midi, float level, int voice)
{
    int start = (int)Math.Round(beat * secondsPerBeat * sampleRate) % sampleCount;
    int length = (int)Math.Round(durationBeats * secondsPerBeat * sampleRate);
    double frequency = Frequency(midi);
    int attack = voice == 3 ? (int)(0.08 * sampleRate) : (int)(0.007 * sampleRate);
    int release = voice == 3 ? (int)(0.14 * sampleRate) : (int)(0.065 * sampleRate);
    for (int i = 0; i < length; i++)
    {
        double time = (double)i / sampleRate;
        double phase = 2.0 * Math.PI * frequency * time;
        double envelope = Math.Min(1.0, (double)i / Math.Max(1, attack)) *
            Math.Min(1.0, (double)(length - i) / Math.Max(1, release));
        double wave = voice switch
        {
            0 => Math.Sin(phase) + 0.27 * Math.Sin(2 * phase), // rounded bass
            1 => Math.Sin(phase) + 0.42 * Math.Sin(3 * phase) +
                0.19 * Math.Sin(5 * phase), // heroic pulse lead
            2 => Math.Sin(phase) + 0.25 * Math.Sin(2 * phase) +
                0.12 * Math.Sin(4 * phase), // plucked arpeggio
            _ => Math.Sin(phase) + 0.16 * Math.Sin(2 * phase) // low pad
        };
        int index = start + i;
        if (index >= sampleCount) index -= sampleCount;
        mix[index] += (float)(wave * envelope * level);
    }
}

double Noise(int index)
{
    uint value = unchecked((uint)index * 747796405u + 2891336453u);
    value = unchecked((value >> ((int)(value >> 28) + 4)) ^ value) * 277803737u;
    value = (value >> 22) ^ value;
    return (value / (double)uint.MaxValue) * 2.0 - 1.0;
}

void AddDrum(double beat, int kind, float level)
{
    int start = (int)Math.Round(beat * secondsPerBeat * sampleRate) % sampleCount;
    double duration = kind == 0 ? 0.34 : kind == 1 ? 0.22 : 0.085;
    int length = (int)(duration * sampleRate);
    double previousNoise = 0.0;
    for (int i = 0; i < length; i++)
    {
        double time = (double)i / sampleRate;
        double noise = Noise(i + kind * 1217);
        double highNoise = noise - previousNoise * 0.72;
        previousNoise = noise;
        double sample = kind switch
        {
            0 => Math.Sin(2.0 * Math.PI *
                    (44.0 * time + 11.0 * (1.0 - Math.Exp(-13.0 * time)))) *
                Math.Exp(-17.0 * time),
            1 => (0.82 * highNoise + 0.18 * Math.Sin(2.0 * Math.PI * 180.0 * time)) *
                Math.Exp(-19.0 * time),
            _ => highNoise * Math.Exp(-67.0 * time)
        };
        sample *= Math.Min(1.0, i / 55.0); // zero-valued onset prevents loop clicks
        int index = start + i;
        if (index >= sampleCount) index -= sampleCount;
        mix[index] += (float)(sample * level);
    }
}

// D minor, with an A7 turnaround back to the first bar. Four phrases, each
// with small changes in melody and percussion so the full-time loop breathes.
int[] roots = [50, 46, 41, 48, 50, 46, 43, 45];
int[][] chords =
[
    [62, 65, 69, 72], [58, 62, 65, 69], [53, 57, 60, 64], [60, 64, 67, 70],
    [62, 65, 69, 72], [58, 62, 65, 69], [55, 58, 62, 65], [57, 61, 64, 67]
];
int[][] phraseA =
[
    [74, 77, 81, -1, 84, 81, 77, 76],
    [74, 77, 82, 81, 77, 74, 72, 69],
    [72, 77, 81, -1, 84, 81, 79, 77],
    [76, 79, 84, 82, 79, 76, 74, 72],
    [74, 77, 81, 84, 81, 77, 76, 74],
    [77, 82, 86, -1, 84, 82, 81, 77],
    [74, 77, 82, 81, 77, 74, 72, 70],
    [73, 76, 81, -1, 79, 76, 73, -1]
];
int[][] phraseB =
[
    [69, 74, 77, 81, 79, 77, 76, 74],
    [77, 74, 70, 74, 77, 79, 77, 74],
    [69, 72, 77, 81, 84, 81, 79, 77],
    [67, 72, 76, 79, 82, 79, 76, 72],
    [74, 77, 81, 84, 86, 84, 81, 77],
    [82, 81, 77, 74, 77, 81, 82, 86],
    [79, 82, 86, 82, 79, 77, 74, 70],
    [73, 76, 81, 79, 76, 73, 69, -1]
];

for (int bar = 0; bar < bars; bar++)
{
    int harmony = bar % 8;
    int phrase = bar / 8;
    double barBeat = bar * beatsPerBar;
    int root = roots[harmony];
    int[] chord = chords[harmony];

    // Grounding bass, with an octave pickup near the end of each bar.
    AddTone(barBeat, 0.9, root, 0.22f, 0);
    AddTone(barBeat + 1.5, 0.4, root + 12, 0.14f, 0);
    AddTone(barBeat + 2.0, 0.9, root, 0.20f, 0);
    AddTone(barBeat + 3.5, 0.39, root + 7, 0.13f, 0);

    // Off-beat arpeggio and quiet harmony leave space for combat sounds.
    AddTone(barBeat, 3.99, chord[0] - 12, 0.032f, 3);
    AddTone(barBeat, 3.99, chord[2] - 12, 0.025f, 3);
    for (int step = 0; step < 8; step++)
        AddTone(barBeat + step * 0.5 + 0.25, 0.22,
            chord[(step + (bar / 4)) % 4], 0.048f, 2);

    int[] melody = phrase % 2 == 0 ? phraseA[harmony] : phraseB[harmony];
    for (int step = 0; step < melody.Length; step++)
    {
        int note = melody[step];
        if (note < 0) continue;
        float strength = phrase == 3 ? 0.13f : 0.11f;
        if (step % 2 == 1) strength *= 0.72f;
        AddTone(barBeat + step * 0.5, 0.43, note, strength, 1);
        if (phrase == 3 && step == 0)
            AddTone(barBeat, 0.72, note - 12, 0.034f, 3);
    }

    AddDrum(barBeat, 0, 0.37f);
    AddDrum(barBeat + 2.0, 0, 0.31f);
    AddDrum(barBeat + 1.0, 1, 0.19f);
    AddDrum(barBeat + 3.0, 1, 0.22f);
    for (int eighth = 0; eighth < 8; eighth++)
        AddDrum(barBeat + eighth * 0.5, 2,
            eighth % 2 == 0 ? 0.057f : 0.038f);
    if (phrase > 0 && harmony == 7)
        for (int fill = 0; fill < 4; fill++)
            AddDrum(barBeat + 3.0 + fill * 0.25, 1, 0.055f + fill * 0.02f);
}

// Gentle limiting, then a fixed peak. This is intentionally quieter than a
// mastered pop track so sword and enemy cues can remain audible in Unity.
float peak = 0f;
for (int i = 0; i < mix.Length; i++)
{
    mix[i] = (float)Math.Tanh(mix[i] * 1.15f);
    peak = Math.Max(peak, Math.Abs(mix[i]));
}
float gain = 0.82f / Math.Max(peak, 0.001f);

string destination = args.Length > 0 ? args[0] :
    Path.Combine("Assets", "Audio", "Music", "before_we_fight_original.wav");
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(destination))!);
using (FileStream stream = File.Create(destination))
using (BinaryWriter writer = new(stream))
{
    int bytes = mix.Length * sizeof(short);
    writer.Write("RIFF"u8);
    writer.Write(36 + bytes);
    writer.Write("WAVE"u8);
    writer.Write("fmt "u8);
    writer.Write(16);
    writer.Write((short)1); // PCM
    writer.Write((short)1); // mono
    writer.Write(sampleRate);
    writer.Write(sampleRate * sizeof(short));
    writer.Write((short)sizeof(short));
    writer.Write((short)16);
    writer.Write("data"u8);
    writer.Write(bytes);
    foreach (float value in mix)
        writer.Write((short)Math.Clamp((int)Math.Round(value * gain * 32767),
            short.MinValue, short.MaxValue));
}
Console.WriteLine($"Created {destination}: {bars} bars, {bpm:0} BPM, " +
    $"{sampleCount / (double)sampleRate:0.00}s, original synthesized audio.");
