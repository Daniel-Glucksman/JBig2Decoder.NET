using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class FastBitSet {
		public long[] w;
		public static int pot =  6 ;
        public static int mask = unchecked((int)(((ulong)(-1L)) >> (64 - pot)));
		public long length;
		
		
		public FastBitSet(long length) {
			this.length = length;
			long wcount = length / 64; 
			if (length % 64 != 0) wcount++;
			w = new long[wcount];
		}
		
		public long size() {
			return length;
		}

		public void setAll(bool value) {
			if (value)
				for (long i = 0; i<w.Length; i++){
					w[i] = -1L;
				}
			else
				for (long i = 0; i<w.Length; i++){
					w[i] = 0;
				}
				
		}
		
		public void set(long start, long end, bool value) {
			if (value) {
				for (long i=start; i<end; i++) {
					set(i);
				}
			}
			else {
				for (long i=start; i<end; i++) {
					clear(i);
				}				
			}
		}

        public void or(long startindex, FastBitSet set, long setStartIndex, long length)
        {           
            long shift = startindex - setStartIndex;
			long k = set.w[setStartIndex >> pot];            
			//Cyclic shift
            k = (k << (int)shift) | (long)((ulong)k >> (64 - (int)shift));            
			if ((setStartIndex & mask) + length <= 64) {
				setStartIndex += shift;
				for (long i=0; i<length; i++) {
                    w[(long)((ulong)startindex) >> pot] |= k & (1L << (int)setStartIndex);
					setStartIndex++;
					startindex++;
				}				
			}
			else{
				for (long i=0; i<length; i++) {                    
					if ((setStartIndex & mask) == 0){ 
						k = set.w[(setStartIndex) >> pot];
						k = (k << (int)shift) | (long)((ulong)k >> (64 - (int)shift));
					}          
                    w[(long)((ulong)startindex >> pot)] |= k & (1L << (int)(setStartIndex + shift));
					setStartIndex++;
					startindex++;
				}
			}
		}
		
		public void set(long index, bool value) {
			if (value) set(index);
			else clear(index);
		}
		
		public void set(long index) {
            long windex = (long)((ulong)index >> pot);
			w[windex] |= (1L<<(int)index);
		}
		
		public void clear(long index) {
            long windex = (long)((ulong)index >> pot);
			w[windex] &= ~(1L<<(int)index);
		}

		public bool get(long index) {
            long windex = (long)((ulong)index >> pot);
			return ((w[windex] & (1L<<(int)index)) != 0);
		}		
	}
}
