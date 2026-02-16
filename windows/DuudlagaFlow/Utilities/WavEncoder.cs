using System.IO;

namespace DuudlagaFlow.Utilities;

public static class WavEncoder
{
    public static byte[] Encode(byte[] pcmData, int sampleRate = 16000, int channels = 1, int bitDepth = 16)
    {
        int dataSize = pcmData.Length;
        int fileSize = 44 + dataSize;

        using var ms = new MemoryStream(fileSize);
        using var writer = new BinaryWriter(ms);

        // RIFF header
        writer.Write("RIFF"u8);
        writer.Write(fileSize - 8);
        writer.Write("WAVE"u8);

        // fmt subchunk
        writer.Write("fmt "u8);
        writer.Write(16);                                    // subchunk size
        writer.Write((short)1);                              // PCM format
        writer.Write((short)channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitDepth / 8);  // byte rate
        writer.Write((short)(channels * bitDepth / 8));      // block align
        writer.Write((short)bitDepth);

        // data subchunk
        writer.Write("data"u8);
        writer.Write(dataSize);
        writer.Write(pcmData);

        return ms.ToArray();
    }

    public static byte[] EncodeSilence(double durationSeconds, int sampleRate = 16000)
    {
        int sampleCount = (int)(sampleRate * durationSeconds);
        var pcm = new byte[sampleCount * 2]; // 16-bit = 2 bytes per sample
        // Fill with near-silence (tiny value to avoid API rejection)
        for (int i = 0; i < sampleCount; i++)
        {
            short value = 1;
            pcm[i * 2] = (byte)(value & 0xFF);
            pcm[i * 2 + 1] = (byte)(value >> 8);
        }
        return Encode(pcm, sampleRate);
    }
}
