using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Threading;
using NAudio.Wave;
using WPFSoundVisualizationLib;

namespace globalwaves.Player.WpfGui.Compatibility
{
    class SampleEngine : ISpectrumPlayer, IDisposable, INotifyPropertyChanged
    {
        #region Fields
        private static SampleEngine instance;
        private readonly BackgroundWorker waveformGenerateWorker = new BackgroundWorker();
        private readonly int fftDataSize = (int)FFTDataSize.FFT2048;
        private SampleAggregator sampleAggregator = new SampleAggregator((int)FFTDataSize.FFT2048);
        #endregion

        public void AddSample(float sampleL, float sampleR)
        {
            sampleAggregator.Add(sampleL, sampleR);
        }

        #region Constants
        private const int waveformCompressedPointCount = 2000;
        #endregion

        #region Singleton Pattern
        public static SampleEngine Instance
        {
            get
            {
                if (instance == null)
                    instance = new SampleEngine();
                return instance;
            }
        }
        #endregion

        #region Constructor
        private SampleEngine()
        {
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }
        #endregion

        #region IDisposable
        private bool disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                disposed = true;
            }
        }
        #endregion

        #region ISoundPlayer

        private bool _isPlaying = false;
        public bool IsPlaying { get { return _isPlaying; } set { _isPlaying = value; NotifyPropertyChanged("IsPlaying"); } }

        #endregion

        #region ISpectrumPlayer

        public bool GetFFTData(float[] fftDataBuffer)
        {
            sampleAggregator.GetFFTResults(fftDataBuffer);
            return IsPlaying;
        }

        public int GetFFTFrequencyIndex(int frequency)
        {
            double maxFrequency;
            //if (ActiveStream != null)
            //    maxFrequency = ActiveStream.WaveFormat.SampleRate / 2.0d;
            //else
                maxFrequency = 22050; // Assume a default 44.1 kHz sample rate.
            return (int)((frequency / maxFrequency) * (fftDataSize / 2));
        }

        #endregion
    }
}
