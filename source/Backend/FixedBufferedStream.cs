using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace globalwaves.Player
{
    public class FixedBufferedStream : Stream
    {
        /*
         * The following is code edited from NAudio's Mp3StreamingDemo.ReadFullyStream class.
         * I just didn't like the name, but I also didn't want to reinvent the wheel.
         * The only thing I added myself is the metadata handling for icecast.
         */

        private Stream sourceStream;
        private long pos; // read-ahead position
        private long pos_real; // read by external position ("real" position)
        private byte[] readAheadBuffer;
        private int readAheadLength;
        private int readAheadOffset;
        private long metaint;

        public FixedBufferedStream(Stream sourceStream, long metaint)
        {
            this.sourceStream = sourceStream;
            this.readAheadBuffer = new byte[128];
            this.metaint = metaint;
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void Flush()
        {
            throw new InvalidOperationException();
        }

        public override long Length
        {
            get { return pos_real; }
        }

        public override long Position
        {
            get
            {
                return pos_real;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }


        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = 0;
            while (bytesRead < count)
            {
                int readAheadAvailableBytes = readAheadLength - readAheadOffset;
                int bytesRequired = count - bytesRead;
                if (readAheadAvailableBytes > 0)
                {
                    int toCopy = Math.Min(readAheadAvailableBytes, bytesRequired);
                    Array.Copy(readAheadBuffer, readAheadOffset, buffer, offset + bytesRead, toCopy);
                    bytesRead += toCopy;
                    pos_real += toCopy;
                    readAheadOffset += toCopy;
                }
                else
                {
                    readAheadOffset = 0;
                    int readAheadMust = readAheadBuffer.Length;
                    if (metaint > 0)
                        readAheadMust = (int)Math.Min(readAheadMust, 16000 - (pos % metaint));
                    //Console.WriteLine("Must read " + readAheadMust + " bytes, buffer length is " + readAheadBuffer.Length + ", bytes until metaint: " + (16000 - (pos % metaint)));
                    readAheadLength = sourceStream.Read(readAheadBuffer, 0, readAheadMust);
                    pos += readAheadLength;
                    if (metaint > 0 && pos % metaint == 0)
                    {
                        _readMetadata();
                        continue;
                    }
                    if (readAheadLength == 0)
                    {
                        break;
                    }
                }
            }
            return bytesRead;
        }

        private void _readMetadata()
        {
            int length = 16 * sourceStream.ReadByte();
            if (length == 0)
            {
                return; // No metadata change
            }
            if (length < 0)
            {
                Console.WriteLine("Could not read from stream");
                return; // TODO: Throw error here?
            }
            int posi = 0;
            byte[] metabuff = new byte[length];
            while(posi < length)
                posi += sourceStream.Read(metabuff, posi, length - posi);
            var metabuffs = Encoding.UTF8.GetString(metabuff).Trim();
            Metadata = System.Text.RegularExpressions.Regex.Match(metabuffs, "StreamTitle='(.+)';").Groups[1].Value.Trim();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new InvalidOperationException();
        }

        public override void SetLength(long value)
        {
            throw new InvalidOperationException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new InvalidOperationException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        public string Metadata
        {
            get;
            set;
        }
    }
}
