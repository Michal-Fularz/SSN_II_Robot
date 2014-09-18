using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAF_Robot
{
    public class CSound : IDisposable
    {
        // czy duration tutaj? czy wówczas czas trwania dodać do wszystkich elementów wyjściowych (np ustawia się jak długo silniki mają wysyłać informację...)
        public string SoundName { get; private set; }
        public int DurationInMiliseconds { get; private set; }

        private bool flagSoundInProgress;

        private NAudio.Wave.WaveFileReader wfr;
        private NAudio.Wave.WaveChannel32 wc;
        private NAudio.Wave.WaveOutEvent audioOutput;

        public CSound()
        {
            this.SoundName = "";
            this.DurationInMiliseconds = -1;

            this.flagSoundInProgress = false;
        }

        public bool IsSoundInProgress()
        {
            return flagSoundInProgress;
        }

        public void Play(string filename)
        {
            if (this.flagSoundInProgress)
            {
                this.StopPlayingSound();
            }

            try
            {
                this.wfr = new NAudio.Wave.WaveFileReader(filename);
                this.wc = new NAudio.Wave.WaveChannel32(this.wfr) { PadWithZeroes = false };

                this.audioOutput = new NAudio.Wave.WaveOutEvent();
                this.audioOutput.Init(wc);
                this.audioOutput.Play();
                this.audioOutput.PlaybackStopped += PlaybackStopped;

                this.flagSoundInProgress = true;
            }
            catch (Exception e)
            {
                AminExceptions.CAminExceptions.ThrowException(e, "Błąd przy próbie odtwarzania pliku dźwiękowego.");
                throw;
            }
        }

        public void StopPlayingSound()
        {
            try
            {
                this.audioOutput.Stop();
                this.flagSoundInProgress = false;
            }
            catch (Exception e)
            {
                AminExceptions.CAminExceptions.ThrowException(e, "Błąd przy próbie zatrzymania odtwarzania pliku dźwiękowego.");
            }
        }

        private void PlaybackStopped(Object sender, NAudio.Wave.StoppedEventArgs args)
        {
            this.StopPlayingSound();
        }

        public void Dispose()
        {
            if (this.wfr != null)
            {
                this.wfr.Dispose();
            }
            if (this.audioOutput != null)
            {
                this.audioOutput.Dispose();
            }
        }
    }
}
