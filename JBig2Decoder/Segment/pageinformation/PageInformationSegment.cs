using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class PageInformationSegment : Segment
    {

        private int pageBitmapHeight, pageBitmapWidth;
        private int yResolution, xResolution;

        PageInformationFlags pageInformationFlags = new PageInformationFlags();
        private int pageStriping;

        private JBIG2Bitmap pageBitmap;

        public PageInformationSegment(JBIG2StreamDecoder streamDecoder) : base(streamDecoder) { }

        public PageInformationFlags getPageInformationFlags()
        {
            return pageInformationFlags;
        }

        public JBIG2Bitmap getPageBitmap()
        {
            return pageBitmap;
        }

        public override void readSegment()  {

		if (JBIG2StreamDecoder.debug)
			Console.WriteLine("==== Reading Page Information Dictionary ====");

		    short[] buff = new short[4];
		    decoder.readbyte(buff);
		    pageBitmapWidth = BinaryOperation.getInt32(buff);

		    buff = new short[4];
		    decoder.readbyte(buff);
		    pageBitmapHeight = BinaryOperation.getInt32(buff);

		    if (JBIG2StreamDecoder.debug)
			    Console.WriteLine("Bitmap size = " + pageBitmapWidth + 'x' + pageBitmapHeight);

		    buff = new short[4];
		    decoder.readbyte(buff);
		    xResolution = BinaryOperation.getInt32(buff);

		    buff = new short[4];
		    decoder.readbyte(buff);
		    yResolution = BinaryOperation.getInt32(buff);

		    if (JBIG2StreamDecoder.debug)
			    Console.WriteLine("Resolution = " + xResolution + 'x' + yResolution);

		    /** extract page information flags */
		    short pageInformationFlagsField = decoder.readbyte();

		    pageInformationFlags.setFlags(pageInformationFlagsField);

		    if (JBIG2StreamDecoder.debug)
			    Console.WriteLine("symbolDictionaryFlags = " + pageInformationFlagsField);

		    buff = new short[2];
		    decoder.readbyte(buff);
		    pageStriping = BinaryOperation.getInt16(buff);

		    if (JBIG2StreamDecoder.debug)
                Console.WriteLine("Page Striping = " + pageStriping);

		    int defPix = pageInformationFlags.getFlagValue(PageInformationFlags.DEFAULT_PIXEL_VALUE);

		    int height;

		    if (pageBitmapHeight == -1) {
			    height = pageStriping & 0x7fff;
		    } else {
			    height = pageBitmapHeight;
		    }

		    pageBitmap = new JBIG2Bitmap(pageBitmapWidth, height, arithmeticDecoder, huffmanDecoder, mmrDecoder);
		    pageBitmap.clear(defPix);
	    }

        public int getPageBitmapHeight()
        {
            return pageBitmapHeight;
        }
    }
}
