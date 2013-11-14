using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class SegmentHeader
    {
        private int segmentNumber;
        private int segmentType;
        private bool pageAssociationSizeSet;
        private bool deferredNonRetainSet;
        private int referredToSegmentCount;
        private short[] rententionFlags;
        private int[] referredToSegments;
        private int pageAssociation;
        private int dataLength;

        public void setSegmentNumber(int SegmentNumber)
        {
            this.segmentNumber = SegmentNumber;
        }
        public void setSegmentHeaderFlags(short SegmentHeaderFlags) {
		    segmentType = SegmentHeaderFlags & 63; // 63 = 00111111
		    pageAssociationSizeSet = (SegmentHeaderFlags & 64) == 64; // 64 = // 01000000
		    deferredNonRetainSet = (SegmentHeaderFlags & 80) == 80; // 64 = 10000000		
	    }
        public void setReferredToSegmentCount(int referredToSegmentCount)
        {
            this.referredToSegmentCount = referredToSegmentCount;
        }

        public void setRententionFlags(short[] rententionFlags)
        {
            this.rententionFlags = rententionFlags;
        }

        public void setReferredToSegments(int[] referredToSegments)
        {
            this.referredToSegments = referredToSegments;
        }

        public int[] getReferredToSegments()
        {
            return referredToSegments;
        }

        public int getSegmentType()
        {
            return segmentType;
        }

        public int getSegmentNumber()
        {
            return segmentNumber;
        }

        public bool isPageAssociationSizeSet()
        {
            return pageAssociationSizeSet;
        }

        public bool isDeferredNonRetainSet()
        {
            return deferredNonRetainSet;
        }

        public int getReferredToSegmentCount()
        {
            return referredToSegmentCount;
        }

        public short[] getRententionFlags()
        {
            return rententionFlags;
        }

        public int getPageAssociation()
        {
            return pageAssociation;
        }

        public void setPageAssociation(int pageAssociation)
        {
            this.pageAssociation = pageAssociation;
        }

        public void setDataLength(int dataLength)
        {
            this.dataLength = dataLength;
        }

        public void setSegmentType(int type)
        {
            this.segmentType = type;
        }

        public int getSegmentDataLength()
        {
            return dataLength;
        }
    }
}
