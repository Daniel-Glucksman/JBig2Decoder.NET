using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class PageInformationFlags : Flags
    {

        public static string DEFAULT_PIXEL_VALUE = "DEFAULT_PIXEL_VALUE";
        public static string DEFAULT_COMBINATION_OPERATOR = "DEFAULT_COMBINATION_OPERATOR";

        public override void setFlags(int flagsAsInt) {
		    this.flagsAsInt = flagsAsInt;
		    /** extract DEFAULT_PIXEL_VALUE */
		    flags[DEFAULT_PIXEL_VALUE] = (flagsAsInt >> 2) & 1;

		    /** extract DEFAULT_COMBINATION_OPERATOR */
		    flags[DEFAULT_COMBINATION_OPERATOR] = (flagsAsInt >> 3) & 3;

		    if (JBIG2StreamDecoder.debug)
                Console.WriteLine(flags);
	    }
    }
}
