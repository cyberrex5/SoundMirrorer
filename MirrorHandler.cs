using System.Diagnostics;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace SoundMirrorer
{
    class MirrorHandler
    {
        public MMDevice OutputDevice { get; private set; }
        public bool IsMirroring { get; private set; }

        private WasapiLoopbackCapture capture = null;
        private BufferedWaveProvider bufferedWaveProvider = null;
        private WasapiOut player = null;

        public MirrorHandler(ref WasapiLoopbackCapture captureRef, MMDevice outputDevice)
        {
            if (captureRef == null)
            {
                throw new System.ArgumentNullException(nameof(captureRef), "Cannot be null.");
            }
            this.OutputDevice = outputDevice;
            this.capture = captureRef;
        }

        public void StartMirroring()
        {
            if (IsMirroring)
            {
                Debug.WriteLine($"\nAlready mirroring to {OutputDevice.FriendlyName}\n");
                return;
            }
            capture.DataAvailable += (s, a) => bufferedWaveProvider.AddSamples(a.Buffer, 0, a.BytesRecorded);

            bufferedWaveProvider = new BufferedWaveProvider(capture.WaveFormat);

            player = new WasapiOut(OutputDevice, AudioClientShareMode.Shared, false, 0);
            player.Init(bufferedWaveProvider);
            player.Play();

            Debug.WriteLine($"\nStarted mirroring to {OutputDevice.FriendlyName}\n");
            IsMirroring = true;
        }

        public void StopMirroring()
        {
            if (!IsMirroring)
            {
                Debug.WriteLine($"\nNot mirroring\n");
                return;
            }

            player.Stop();

            bufferedWaveProvider = null;
            player = null;

            Debug.WriteLine("\nStopped mirroring audio\n");
            IsMirroring = false;
        }
    }
}
