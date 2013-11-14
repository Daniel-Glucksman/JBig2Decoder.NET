using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class Big2StreamReader {
	    private byte[] data;

	    private int bitPointer = 7;

	    private int bytePointer = 0;

	    public Big2StreamReader(byte[] data) {
		    this.data = data;
	    }	    

	    public short readbyte() {
		    short bite = (short) (data[bytePointer++] & 255);

		    return bite;
	    }

	    public void readbyte(short[] buf) {
		    for (int i = 0; i < buf.Length; i++) {
			    buf[i] = (short) (data[bytePointer++] & 255);
		    }
	    }

	    public int readBit() {
		    short buf = readbyte();
		    short mask = (short) (1 << bitPointer);

		    int bit = (buf & mask) >> bitPointer;

		    bitPointer--;
		    if (bitPointer == -1) {
			    bitPointer = 7;
		    } else {
			    movePointer(-1);
		    }

		    return bit;
	    }

	    public int readBits(long num) {
		    int result = 0;

		    for (int i = 0; i < num; i++) {
			    result = (result << 1) | readBit();
		    }

		    return result;
	    }

	    public void movePointer(int ammount) {
		    bytePointer += ammount;
	    }

	    public void consumeRemainingBits() {
		    if (bitPointer != 7)
			    readBits(bitPointer + 1);
	    }

	    public bool isFinished() {
		    return bytePointer == data.Length;
	    }
}
}
