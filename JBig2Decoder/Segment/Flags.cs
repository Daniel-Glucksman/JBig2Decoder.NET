using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public abstract class Flags
    {        
        protected int flagsAsInt;
        protected Dictionary<string, int> flags = new Dictionary<string, int>();

        public int getFlagValue(string key)
        {
            int value = flags[key];
            return value;
        }
        public abstract void setFlags(int flagsAsInt);
    }
}
