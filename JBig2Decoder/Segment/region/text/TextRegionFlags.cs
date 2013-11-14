using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class TextRegionFlags : Flags {

	public const string SB_HUFF = "SB_HUFF";
	public const string SB_REFINE = "SB_REFINE";
	public const string LOG_SB_STRIPES = "LOG_SB_STRIPES";
	public const string REF_CORNER = "REF_CORNER";
	public const string TRANSPOSED = "TRANSPOSED";
	public const string SB_COMB_OP = "SB_COMB_OP";
	public const string SB_DEF_PIXEL = "SB_DEF_PIXEL";
	public const string SB_DS_OFFSET = "SB_DS_OFFSET";
	public const string SB_R_TEMPLATE = "SB_R_TEMPLATE";
	public override void setFlags(int flagsAsInt) {
		this.flagsAsInt = flagsAsInt;

		/** extract SB_HUFF */
		flags[SB_HUFF] = flagsAsInt & 1;

		/** extract SB_REFINE */
		flags[SB_REFINE] = (flagsAsInt >> 1) & 1;

		/** extract LOG_SB_STRIPES */
		flags[LOG_SB_STRIPES] = (flagsAsInt >> 2) & 3;

		/** extract REF_CORNER */
		flags[REF_CORNER] = (flagsAsInt >> 4) & 3;

		/** extract TRANSPOSED */
		flags[TRANSPOSED] = (flagsAsInt >> 6) & 1;

		/** extract SB_COMB_OP */
		flags[SB_COMB_OP] = (flagsAsInt >> 7) & 3;

		/** extract SB_DEF_PIXEL */
		flags[SB_DEF_PIXEL] = (flagsAsInt >> 9) & 1;

		int sOffset = (flagsAsInt >> 10) & 0x1f;
		if ((sOffset & 0x10) != 0) {
			sOffset |= -1 - 0x0f;
		}
		flags[SB_DS_OFFSET] = sOffset;

		/** extract SB_R_TEMPLATE */
		flags[SB_R_TEMPLATE]=(flagsAsInt >> 15) & 1;

		if (JBIG2StreamDecoder.debug)
			Console.WriteLine(flags);
	}
}
}
