using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public abstract class RegionSegment : Segment
    {
        protected int regionBitmapWidth, regionBitmapHeight;
        protected int regionBitmapXLocation, regionBitmapYLocation;

        protected RegionFlags regionFlags = new RegionFlags();

        public RegionSegment(JBIG2StreamDecoder streamDecoder): base(streamDecoder)  { }

        public override void readSegment()
        {
            short[] buff = new short[4];
            decoder.readbyte(buff);
            regionBitmapWidth = BinaryOperation.getInt32(buff);

            buff = new short[4];
            decoder.readbyte(buff);
            regionBitmapHeight = BinaryOperation.getInt32(buff);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("Bitmap size = " + regionBitmapWidth + 'x' + regionBitmapHeight);

            buff = new short[4];
            decoder.readbyte(buff);
            regionBitmapXLocation = BinaryOperation.getInt32(buff);

            buff = new short[4];
            decoder.readbyte(buff);
            regionBitmapYLocation = BinaryOperation.getInt32(buff);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("Bitmap location = " + regionBitmapXLocation + ',' + regionBitmapYLocation);

            /** extract region Segment flags */
            short regionFlagsField = decoder.readbyte();

            regionFlags.setFlags(regionFlagsField);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("region Segment flags = " + regionFlagsField);
        }
    }
}
