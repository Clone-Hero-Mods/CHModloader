using NAudio.Wave;
using System;
using System.IO;
using UnityEngine;

namespace CHModloader
{
    public class Sounds : MonoBehaviour
    {
        public static AudioClip LoadAudio(Mod mod, string fileName)
        {
            string FilePath = ModLoader.ModsPath + mod.ID + "/Sounds";

            if (!File.Exists(FilePath))
            {
                ModLogs.Log(string.Format("ERROR in LoadAudio(): File not found {0}", FilePath));
                return null;
            }

            string Extension = Path.GetExtension(FilePath).ToLower();

            if (Extension == ".wav" || Extension == ".mp3" || Extension == ".ogg")
            {
                AudioClip audio;
                float[] floatBuffer;

                using (MediaFoundationReader media = new MediaFoundationReader(FilePath))
                {
                    int _byteBuffer32_length = (int)media.Length * 2;
                    int _floatBuffer_length = _byteBuffer32_length / sizeof(float);

                    IWaveProvider stream32 = new Wave16ToFloatProvider(media);
                    WaveBuffer _waveBuffer = new WaveBuffer(_byteBuffer32_length);
                    stream32.Read(_waveBuffer, 0, (int)_byteBuffer32_length);
                    floatBuffer = new float[_floatBuffer_length];

                    for (int i = 0; i < _floatBuffer_length; i++)
                    {
                        floatBuffer[i] = _waveBuffer.FloatBuffer[i];
                    }

                    audio = AudioClip.Create(fileName, _floatBuffer_length, media.WaveFormat.Channels, media.WaveFormat.SampleRate, false);
                }
                audio.SetData(floatBuffer, 0);

                return audio;
            }
            else
            {
                ModLogs.Log(string.Format("ERROR in LoadAudio(): Audio extension not supported: {0}", Environment.NewLine));
            }
            return null;
        }
    }
}
