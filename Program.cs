using System;
using System.IO;
using System.Speech.Synthesis;

// removed wavPath

using var synth = new SpeechSynthesizer();
synth.Rate = 1;
synth.Volume = 100;

string line;
while ((line = Console.ReadLine()) != null)
{
    if (string.IsNullOrWhiteSpace(line)) continue;

    string wavPath = Path.Combine(Path.GetTempPath(), $"ttsgui_{Guid.NewGuid():N}.wav");

    synth.SetOutputToWaveFile(wavPath);
    synth.Speak(line);
    synth.SetOutputToNull();

    AmplifyWav(wavPath, 3.5f);

    Console.WriteLine(wavPath);
    Console.Out.Flush();
}

static void AmplifyWav(string path, float factor)
{
    byte[] wav = File.ReadAllBytes(path);
    int offset = FindDataChunk(wav);
    if (offset < 0) return;

    for (int i = offset; i < wav.Length - 1; i += 2)
    {
        short sample = (short)(wav[i] | (wav[i + 1] << 8));
        float normalized = sample / 32768f;
        
        normalized = (float)Math.Tanh(normalized * factor);
        
        short boosted = (short)(normalized * 32767f);
        wav[i] = (byte)(boosted & 0xFF);
        wav[i + 1] = (byte)((boosted >> 8) & 0xFF);
    }

    File.WriteAllBytes(path, wav);
}

static int FindDataChunk(byte[] wav)
{
    for (int i = 12; i < wav.Length - 8; i++)
    {
        if (wav[i] == 'd' && wav[i + 1] == 'a' && wav[i + 2] == 't' && wav[i + 3] == 'a')
            return i + 8;
    }
    return -1;
}
