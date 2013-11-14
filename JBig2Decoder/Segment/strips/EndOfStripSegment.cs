using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class EndOfStripeSegment : Segment {

	public EndOfStripeSegment(JBIG2StreamDecoder streamDecoder):base(streamDecoder) {}

	public override void readSegment()  {
		for (int i = 0; i < this.getSegmentHeader().getSegmentDataLength(); i++) {
			decoder.readbyte();
		}
	}
}
}
