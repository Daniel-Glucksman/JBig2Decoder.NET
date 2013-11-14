using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public abstract class Segment {
	    public const int SYMBOL_DICTIONARY = 0;
	    public const int INTERMEDIATE_TEXT_REGION = 4;
	    public const int IMMEDIATE_TEXT_REGION = 6;
	    public const int IMMEDIATE_LOSSLESS_TEXT_REGION = 7;
	    public const int PATTERN_DICTIONARY = 16;
	    public const int INTERMEDIATE_HALFTONE_REGION = 20;
	    public const int IMMEDIATE_HALFTONE_REGION = 22;
	    public const int IMMEDIATE_LOSSLESS_HALFTONE_REGION = 23;
	    public const int INTERMEDIATE_GENERIC_REGION = 36;
	    public const int IMMEDIATE_GENERIC_REGION = 38;
	    public const int IMMEDIATE_LOSSLESS_GENERIC_REGION = 39;
	    public const int INTERMEDIATE_GENERIC_REFINEMENT_REGION = 40;
	    public const int IMMEDIATE_GENERIC_REFINEMENT_REGION = 42;
	    public const int IMMEDIATE_LOSSLESS_GENERIC_REFINEMENT_REGION = 43;
	    public const int PAGE_INFORMATION = 48;
	    public const int END_OF_PAGE = 49;
	    public const int END_OF_STRIPE = 50;
	    public const int END_OF_FILE = 51;
	    public const int PROFILES = 52;
	    public const int TABLES = 53;
	    public const int EXTENSION = 62;
	    public const int BITMAP = 70;

	    protected SegmentHeader segmentHeader;

	    protected HuffmanDecoder huffmanDecoder;

	    protected ArithmeticDecoder arithmeticDecoder;

	    protected MMRDecoder mmrDecoder;

	    protected JBIG2StreamDecoder decoder;

	    public Segment(JBIG2StreamDecoder streamDecoder) {
		    this.decoder = streamDecoder;			
			huffmanDecoder = decoder.getHuffmanDecoder();
			arithmeticDecoder = decoder.getArithmeticDecoder();
			mmrDecoder = decoder.getMMRDecoder();		

	    }
	    protected short readATValue() {
		    short atValue;
		    short c0 = atValue = decoder.readbyte();

		    if ((c0 & 0x80) != 0) {
			    atValue |= -1 - 0xff;
		    }

		    return atValue;
	    }

	    public SegmentHeader getSegmentHeader() {
		    return segmentHeader;
	    }

	    public void setSegmentHeader(SegmentHeader segmentHeader) {
		    this.segmentHeader = segmentHeader;
	    }

        public abstract void readSegment();
    }

}
