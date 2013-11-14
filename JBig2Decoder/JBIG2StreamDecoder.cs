using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Runtime.InteropServices.WindowsRuntime;
namespace JBig2Decoder
{
    public class JBIG2StreamDecoder
    {
        public static bool debug = false;
        private Big2StreamReader reader;
        private ArithmeticDecoder arithmeticDecoder;
        private HuffmanDecoder huffmanDecoder;
        private MMRDecoder mmrDecoder;
        private bool noOfPagesKnown;
        private bool randomAccessOrganisation;
        private int noOfPages = -1;
        private List<Segment> segments = new List<Segment>();
        private List<JBIG2Bitmap> bitmaps = new List<JBIG2Bitmap>();
        private byte[] globalData;

        public void movePointer(int i)
        {
            reader.movePointer(i);
        }
        public void setGlobalData(byte[] data)
        {
            globalData = data;
        }
        public byte[] decodeJBIG2(byte[] data)
        {
            reader = new Big2StreamReader(data);
            resetDecoder();
            bool validFile = checkHeader();
		    if (JBIG2StreamDecoder.debug)
			    Console.WriteLine("validFile = " + validFile);
		    if (!validFile) {
			    /**
			     * Assume this is a stream from a PDF so there is no file header,
			     * end of page segments, or end of file segments. Organisation must
			     * be sequential, and the number of pages is assumed to be 1.
			     */
			    noOfPagesKnown = true;
			    randomAccessOrganisation = false;
			    noOfPages = 1;
			    /** check to see if there is any global data to be read */
			    if (globalData != null) {
				    /** set the reader to read from the global data */
				    reader = new Big2StreamReader(globalData);

				    huffmanDecoder = new HuffmanDecoder(reader);
				    mmrDecoder = new MMRDecoder(reader);
				    arithmeticDecoder = new ArithmeticDecoder(reader);
				
				    /** read in the global data segments */
				    readSegments();

				    /** set the reader back to the main data */
				    reader = new Big2StreamReader(data);
			    } else {
				    /**
				     * There's no global data, so move the file pointer back to the
				     * start of the stream
				     */
				    reader.movePointer(-8);
			    }
		    } else {
			    /**
			     * We have the file header, so assume it is a valid stand-alone
			     * file.
			     */

			    if (JBIG2StreamDecoder.debug)
				    Console.WriteLine("==== File Header ====");

			    setFileHeaderFlags();

			    if (JBIG2StreamDecoder.debug) {
				    Console.WriteLine("randomAccessOrganisation = " + randomAccessOrganisation);
				    Console.WriteLine("noOfPagesKnown = " + noOfPagesKnown);
			    }

			    if (noOfPagesKnown) {
				    noOfPages = getNoOfPages();

				    if (JBIG2StreamDecoder.debug)
                        Console.WriteLine("noOfPages = " + noOfPages);
			    }
		    }

		    huffmanDecoder = new HuffmanDecoder(reader);
		    mmrDecoder = new MMRDecoder(reader);
		    arithmeticDecoder = new ArithmeticDecoder(reader);
		
		    /** read in the main segment data */
		    readSegments();

            //Create Image
            var rawimage =  findPageSegement(1).getPageBitmap();
            int width = (int)rawimage.getWidth();
            int height = (int)rawimage.getHeight();
            var dataStream =  rawimage.getData(true);
           
            var newarray = new byte[dataStream.Length];
            Array.Copy(dataStream, newarray, dataStream.Length);
            int stride = (width * 1 + 7) / 8;
         
            var bitmap = new WriteableBitmap(width, height, 96, 96, System.Windows.Media.PixelFormats.BlackWhite, null);
            bitmap.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), newarray, stride, 0);     

            MemoryStream stream3 = new MemoryStream();
            var encoder = new TiffBitmapEncoder ();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            encoder.Save(stream3);
            return stream3.ToArray();
        }

        public HuffmanDecoder getHuffmanDecoder()
        {
            return huffmanDecoder;
        }
        public MMRDecoder getMMRDecoder()
        {
            return mmrDecoder;
        }
        public ArithmeticDecoder getArithmeticDecoder()
        {
            return arithmeticDecoder;
        }
        private void resetDecoder()
        {
            noOfPagesKnown = false;
            randomAccessOrganisation = false;

            noOfPages = -1;
            segments.Clear();
            bitmaps.Clear();
        }
        private void readSegments()
        {
            bool finished = false;
            while (!reader.isFinished() && !finished)
            {

                SegmentHeader segmentHeader = new SegmentHeader();
                readSegmentHeader(segmentHeader);

                // read the Segment data
                Segment segment = null;

                int segmentType = segmentHeader.getSegmentType();
                int[] referredToSegments = segmentHeader.getReferredToSegments();
                int noOfReferredToSegments = segmentHeader.getReferredToSegmentCount();

                switch (segmentType)
                {
                    case Segment.SYMBOL_DICTIONARY:

                        segment = new SymbolDictionarySegment(this);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.INTERMEDIATE_TEXT_REGION:

                        segment = new TextRegionSegment(this, false);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_TEXT_REGION:

                        segment = new TextRegionSegment(this, true);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_LOSSLESS_TEXT_REGION:

                        segment = new TextRegionSegment(this, true);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.PATTERN_DICTIONARY:

                        segment = new PatternDictionarySegment(this);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.INTERMEDIATE_HALFTONE_REGION:

                        segment = new HalftoneRegionSegment(this, false);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_HALFTONE_REGION:

                        segment = new HalftoneRegionSegment(this, true);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_LOSSLESS_HALFTONE_REGION:

                        segment = new HalftoneRegionSegment(this, true);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.INTERMEDIATE_GENERIC_REGION:

                        segment = new GenericRegionSegment(this, false);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_GENERIC_REGION:

                        segment = new GenericRegionSegment(this, true);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_LOSSLESS_GENERIC_REGION:

                        segment = new GenericRegionSegment(this, true);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.INTERMEDIATE_GENERIC_REFINEMENT_REGION:

                        segment = new RefinementRegionSegment(this, false, referredToSegments, noOfReferredToSegments);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_GENERIC_REFINEMENT_REGION:

                        segment = new RefinementRegionSegment(this, true, referredToSegments, noOfReferredToSegments);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.IMMEDIATE_LOSSLESS_GENERIC_REFINEMENT_REGION:

                        segment = new RefinementRegionSegment(this, true, referredToSegments, noOfReferredToSegments);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.PAGE_INFORMATION:

                        segment = new PageInformationSegment(this);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    case Segment.END_OF_PAGE:
                        continue;

                    case Segment.END_OF_STRIPE:

                        segment = new EndOfStripeSegment(this);

                        segment.setSegmentHeader(segmentHeader);
                        break;

                    case Segment.END_OF_FILE:

                        finished = true;

                        continue;

                    case Segment.PROFILES:
                        break;

                    case Segment.TABLES:
                        break;

                    case Segment.EXTENSION:

                        segment = new ExtensionSegment(this);

                        segment.setSegmentHeader(segmentHeader);

                        break;

                    default:
                        break;
                }

                if (!randomAccessOrganisation)
                {                   
                        segment.readSegment();
                }             
                segments.Add(segment);
            }

            if (randomAccessOrganisation)
            {
                foreach (Segment segment in segments)
                {
                    segment.readSegment();
                }
            }
        }

        public PageInformationSegment findPageSegement(int page)
        {
            foreach (Segment segment in segments)
            {
                SegmentHeader segmentHeader = segment.getSegmentHeader();
                if (segmentHeader.getSegmentType() == Segment.PAGE_INFORMATION && segmentHeader.getPageAssociation() == page)
                {
                    return (PageInformationSegment)segment;
                }
            }

            return null;
        }
        public Segment findSegment(int segmentNumber)
        {
            foreach (Segment segment in segments)
            {
                if (segment.getSegmentHeader().getSegmentNumber() == segmentNumber)
                {
                    return segment;
                }
            }
            return null;
        }
        private void readSegmentHeader(SegmentHeader segmentHeader)
        {
            handleSegmentNumber(segmentHeader);

            handleSegmentHeaderFlags(segmentHeader);

            handleSegmentReferredToCountAndRententionFlags(segmentHeader);

            handleReferedToSegmentNumbers(segmentHeader);

            handlePageAssociation(segmentHeader);

            if (segmentHeader.getSegmentType() != Segment.END_OF_FILE)
                handleSegmentDataLength(segmentHeader);
        }
        private void handlePageAssociation(SegmentHeader segmentHeader)
        {
            int pageAssociation;

            bool isPageAssociationSizeSet = segmentHeader.isPageAssociationSizeSet();
            if (isPageAssociationSizeSet)
            { // field is 4 bytes long
                short[] buf = new short[4];
                reader.readbyte(buf);
                pageAssociation = BinaryOperation.getInt32(buf);
            }
            else
            { // field is 1 byte long
                pageAssociation = reader.readbyte();
            }

            segmentHeader.setPageAssociation(pageAssociation);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("pageAssociation = " + pageAssociation);
        }
        private void handleSegmentNumber(SegmentHeader segmentHeader)
        {
            short[] segmentbytes = new short[4];
            reader.readbyte(segmentbytes);

            int segmentNumber = BinaryOperation.getInt32(segmentbytes);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("SegmentNumber = " + segmentNumber);
            segmentHeader.setSegmentNumber(segmentNumber);
        }
        private void handleSegmentHeaderFlags(SegmentHeader segmentHeader)
        {
            short segmentHeaderFlags = reader.readbyte();
            // System.out.println("SegmentHeaderFlags = " + SegmentHeaderFlags);
            segmentHeader.setSegmentHeaderFlags(segmentHeaderFlags);
        }
        private void handleSegmentReferredToCountAndRententionFlags(SegmentHeader segmentHeader)
        {
            short referedToSegmentCountAndRetentionFlags = reader.readbyte();

            int referredToSegmentCount = (referedToSegmentCountAndRetentionFlags & 224) >> 5; // 224
            // =
            // 11100000
            short[] retentionFlags = null;
            /** take off the first three bits of the first byte */
            short firstbyte = (short)(referedToSegmentCountAndRetentionFlags & 31); // 31 =
            // 00011111

            if (referredToSegmentCount <= 4)
            { // short form

                retentionFlags = new short[1];
                retentionFlags[0] = firstbyte;

            }
            else if (referredToSegmentCount == 7)
            { // long form

                short[] longFormCountAndFlags = new short[4];
                /** add the first byte of the four */
                longFormCountAndFlags[0] = firstbyte;

                for (int i = 1; i < 4; i++)
                    // add the next 3 bytes to the array
                    longFormCountAndFlags[i] = reader.readbyte();

                /** get the count of the referred to Segments */
                referredToSegmentCount = BinaryOperation.getInt32(longFormCountAndFlags);

                /** calculate the number of bytes in this field */
                int noOfbytesInField = (int)Math.Ceiling(4 + ((referredToSegmentCount + 1) / 8d));
                // System.out.println("noOfbytesInField = " + noOfbytesInField);

                int noOfRententionFlagbytes = noOfbytesInField - 4;
                retentionFlags = new short[noOfRententionFlagbytes];
                reader.readbyte(retentionFlags);

            }
            else
            { // error
                //throw new JBIG2Exception("Error, 3 bit Segment count field = " + referredToSegmentCount);
            }

            segmentHeader.setReferredToSegmentCount(referredToSegmentCount);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("referredToSegmentCount = " + referredToSegmentCount);

            segmentHeader.setRententionFlags(retentionFlags);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("retentionFlags = ");

            if (JBIG2StreamDecoder.debug)
            {
                for (int i = 0; i < retentionFlags.Length; i++)
                    Console.WriteLine(retentionFlags[i] + " ");
                Console.WriteLine("");
            }
        }
        private void handleReferedToSegmentNumbers(SegmentHeader segmentHeader)
        {
            int referredToSegmentCount = segmentHeader.getReferredToSegmentCount();
            int[] referredToSegments = new int[referredToSegmentCount];

            int segmentNumber = segmentHeader.getSegmentNumber();

            if (segmentNumber <= 256)
            {
                for (int i = 0; i < referredToSegmentCount; i++)
                    referredToSegments[i] = reader.readbyte();
            }
            else if (segmentNumber <= 65536)
            {
                short[] buf = new short[2];
                for (int i = 0; i < referredToSegmentCount; i++)
                {
                    reader.readbyte(buf);
                    referredToSegments[i] = BinaryOperation.getInt16(buf);
                }
            }
            else
            {
                short[] buf = new short[4];
                for (int i = 0; i < referredToSegmentCount; i++)
                {
                    reader.readbyte(buf);
                    referredToSegments[i] = BinaryOperation.getInt32(buf);
                }
            }

            segmentHeader.setReferredToSegments(referredToSegments);

            if (JBIG2StreamDecoder.debug)
            {
                Console.WriteLine("referredToSegments = ");
                for (int i = 0; i < referredToSegments.Length; i++)
                    Console.WriteLine(referredToSegments[i] + " ");
                Console.WriteLine("");
            }
        }

        private int getNoOfPages()  {
		    short[] noOfPages = new short[4];
		    reader.readbyte(noOfPages);
		    return BinaryOperation.getInt32(noOfPages);
	    }
	    private void handleSegmentDataLength(SegmentHeader segmentHeader) {
		    short[] buf = new short[4];
		    reader.readbyte(buf);
		
		    int dateLength = BinaryOperation.getInt32(buf);
		    segmentHeader.setDataLength(dateLength);

		    if (JBIG2StreamDecoder.debug)
			    Console.WriteLine("dateLength = " + dateLength);
	    }
	    private void setFileHeaderFlags()  {
		    short headerFlags = reader.readbyte();

		    if ((headerFlags & 0xfc) != 0) {
			    Console.WriteLine("Warning, reserved bits (2-7) of file header flags are not zero " + headerFlags);
		    }

		    int fileOrganisation = headerFlags & 1;
		    randomAccessOrganisation = fileOrganisation == 0;

		    int pagesKnown = headerFlags & 2;
		    noOfPagesKnown = pagesKnown == 0;
	    }
	    private bool checkHeader()  {
		    short[] controlHeader = new short[] { 151, 74, 66, 50, 13, 10, 26, 10 };
		    short[] actualHeader = new short[8];
		    reader.readbyte(actualHeader);

            return controlHeader.SequenceEqual(actualHeader);
	    }
        public int readBits(long num)
        {
            return reader.readBits(num);
        }
        public int readBit()
        {
            return reader.readBit();
        }
        public void readbyte(short[] buff)
        {
            reader.readbyte(buff);
        }
        public void consumeRemainingBits()
        {
            reader.consumeRemainingBits();
        }
        public short readbyte()
        {
            return reader.readbyte();
        }
        public void appendBitmap(JBIG2Bitmap bitmap)
        {
            bitmaps.Add(bitmap);
        }

        public JBIG2Bitmap findBitmap(int bitmapNumber) {
		    foreach (JBIG2Bitmap bitmap in bitmaps) {
			    if (bitmap.getBitmapNumber() == bitmapNumber) {
				    return bitmap;
			    }
		    }

		    return null;
	    }
        public JBIG2Bitmap getPageAsJBIG2Bitmap(int i)
        {
            JBIG2Bitmap pageBitmap = findPageSegement(1).getPageBitmap();
            return pageBitmap;
        }
        public bool isNumberOfPagesKnown()
        {
            return noOfPagesKnown;
        }
        public int getNumberOfPages()
        {
            return noOfPages;
        }
        public bool isRandomAccessOrganisationUsed()
        {
            return randomAccessOrganisation;
        }
        public List<Segment> getAllSegments()
        {
            return segments;
        }


    }
	
}
