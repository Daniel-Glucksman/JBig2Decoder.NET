using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class RefinementRegionSegment : RegionSegment {
	private RefinementRegionFlags refinementRegionFlags = new RefinementRegionFlags();

	private bool inlineImage;

	private int noOfReferedToSegments;

	int[] referedToSegments;

	public RefinementRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage, int[] referedToSegments, int noOfReferedToSegments): base(streamDecoder) {		
		this.inlineImage = inlineImage;
		this.referedToSegments = referedToSegments;
		this.noOfReferedToSegments = noOfReferedToSegments;
	}

	public void readSegment()  {
		if (JBIG2StreamDecoder.debug)
			Console.WriteLine("==== Reading Generic Refinement Region ====");

		base.readSegment();

		/** read text region segment flags */
		readGenericRegionFlags();

		short[] genericRegionAdaptiveTemplateX = new short[2];
		short[] genericRegionAdaptiveTemplateY = new short[2];
		
		int template = refinementRegionFlags.getFlagValue(RefinementRegionFlags.GR_TEMPLATE);
		if (template == 0) {
			genericRegionAdaptiveTemplateX[0] = readATValue();
			genericRegionAdaptiveTemplateY[0] = readATValue();
			genericRegionAdaptiveTemplateX[1] = readATValue();
			genericRegionAdaptiveTemplateY[1] = readATValue();
		}

		if (noOfReferedToSegments == 0 || inlineImage) {
			PageInformationSegment pageSegment = decoder.findPageSegement(segmentHeader.getPageAssociation());
			JBIG2Bitmap pageBitmap = pageSegment.getPageBitmap();

			if (pageSegment.getPageBitmapHeight() == -1 && regionBitmapYLocation + regionBitmapHeight > pageBitmap.getHeight()) {
				pageBitmap.expand(regionBitmapYLocation + regionBitmapHeight, pageSegment.getPageInformationFlags().getFlagValue(PageInformationFlags.DEFAULT_PIXEL_VALUE));
			}
		}

		if (noOfReferedToSegments > 1) {
			if(JBIG2StreamDecoder.debug)
				Console.WriteLine("Bad reference in JBIG2 generic refinement Segment");
			
			return;
		}

		JBIG2Bitmap referedToBitmap;
		if (noOfReferedToSegments == 1) {
			referedToBitmap = decoder.findBitmap(referedToSegments[0]);
		} else {
			PageInformationSegment pageSegment = decoder.findPageSegement(segmentHeader.getPageAssociation());
			JBIG2Bitmap pageBitmap = pageSegment.getPageBitmap();

			referedToBitmap = pageBitmap.getSlice(regionBitmapXLocation, regionBitmapYLocation, regionBitmapWidth, regionBitmapHeight);
		}

		arithmeticDecoder.resetRefinementStats(template, null);
		arithmeticDecoder.start();

		bool typicalPredictionGenericRefinementOn = refinementRegionFlags.getFlagValue(RefinementRegionFlags.TPGDON) != 0;

		JBIG2Bitmap bitmap = new JBIG2Bitmap(regionBitmapWidth, regionBitmapHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);

		bitmap.readGenericRefinementRegion(template, typicalPredictionGenericRefinementOn, referedToBitmap, 0, 0, genericRegionAdaptiveTemplateX, genericRegionAdaptiveTemplateY);

		if (inlineImage) {
			PageInformationSegment pageSegment = decoder.findPageSegement(segmentHeader.getPageAssociation());
			JBIG2Bitmap pageBitmap = pageSegment.getPageBitmap();

			int extCombOp = regionFlags.getFlagValue(RegionFlags.EXTERNAL_COMBINATION_OPERATOR);

			pageBitmap.combine(bitmap, regionBitmapXLocation, regionBitmapYLocation, extCombOp);
		} else {
			bitmap.setBitmapNumber(getSegmentHeader().getSegmentNumber());
			decoder.appendBitmap(bitmap);
		}
	}

	private void readGenericRegionFlags() {
		/** extract text region Segment flags */
		short refinementRegionFlagsField = decoder.readbyte();

		refinementRegionFlags.setFlags(refinementRegionFlagsField);

		if (JBIG2StreamDecoder.debug)
            Console.WriteLine("generic region Segment flags = " + refinementRegionFlagsField);
	}

	public RefinementRegionFlags getGenericRegionFlags() {
		return refinementRegionFlags;
	}
}
}
