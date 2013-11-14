using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class RegionFlags : Flags
    {
        public static string EXTERNAL_COMBINATION_OPERATOR = "EXTERNAL_COMBINATION_OPERATOR";
        public override void setFlags(int flagsAsInt)
        {
            this.flagsAsInt = flagsAsInt;

            /** extract EXTERNAL_COMBINATION_OPERATOR */
            flags[EXTERNAL_COMBINATION_OPERATOR] = flagsAsInt & 7;

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine(flags);
        }
    }
}
