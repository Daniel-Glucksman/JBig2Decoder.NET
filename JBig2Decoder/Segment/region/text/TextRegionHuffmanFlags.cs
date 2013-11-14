using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class TextRegionHuffmanFlags : Flags {

	    public const string SB_HUFF_FS = "SB_HUFF_FS";
	    public const string SB_HUFF_DS = "SB_HUFF_DS";
	    public const string SB_HUFF_DT = "SB_HUFF_DT";
	    public const string SB_HUFF_RDW = "SB_HUFF_RDW";
	    public const string SB_HUFF_RDH = "SB_HUFF_RDH";
	    public const string SB_HUFF_RDX = "SB_HUFF_RDX";
	    public const string SB_HUFF_RDY = "SB_HUFF_RDY";
	    public const string SB_HUFF_RSIZE = "SB_HUFF_RSIZE";

	    public override void setFlags(int flagsAsInt) {
		    this.flagsAsInt = flagsAsInt;

		    /** extract SB_HUFF_FS */
		    flags[SB_HUFF_FS] =  flagsAsInt & 3;

		    /** extract SB_HUFF_DS */
		    flags[SB_HUFF_DS] = (flagsAsInt >> 2) & 3;

		    /** extract SB_HUFF_DT */
		    flags[SB_HUFF_DT] = (flagsAsInt >> 4) & 3;

		    /** extract SB_HUFF_RDW */
		    flags[SB_HUFF_RDW] =  (flagsAsInt >> 6) & 3;

		    /** extract SB_HUFF_RDH */
		    flags[SB_HUFF_RDH] =  (flagsAsInt >> 8) & 3;

		    /** extract SB_HUFF_RDX */
		    flags[SB_HUFF_RDX] =  (flagsAsInt >> 10) & 3;

		    /** extract SB_HUFF_RDY */
		    flags[SB_HUFF_RDY] = (flagsAsInt >> 12) & 3;

		    /** extract SB_HUFF_RSIZE */
		    flags[SB_HUFF_RSIZE] = (flagsAsInt >> 14) & 1;

		    if (JBIG2StreamDecoder.debug)
			    Console.WriteLine(flags);
	    }
    }
}
