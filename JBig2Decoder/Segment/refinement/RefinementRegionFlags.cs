using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class RefinementRegionFlags : Flags {

	public const string GR_TEMPLATE = "GR_TEMPLATE";
    public const string TPGDON = "TPGDON";

	public override void setFlags(int flagsAsInt) {
		this.flagsAsInt = flagsAsInt;

		/** extract GR_TEMPLATE */
		flags[GR_TEMPLATE] = flagsAsInt & 1;

		/** extract TPGDON */
		flags[TPGDON] = (flagsAsInt >> 1) & 1;

		if (JBIG2StreamDecoder.debug)
            Console.WriteLine(flags);
	}
}
}
