using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class GenericRegionSegment : RegionSegment {
    private GenericRegionFlags genericRegionFlags = new GenericRegionFlags();

    private bool inlineImage;
    private bool unknownLength = false;
    
    public GenericRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage):base(streamDecoder) {
        this.inlineImage = inlineImage;
    }

    public override void readSegment() {
    	
    	if(JBIG2StreamDecoder.debug)
    		Console.WriteLine("==== Reading Immediate Generic Region ====");
    	
    	base.readSegment();
        
        /** read text region Segment flags */
        readGenericRegionFlags();

        bool useMMR = genericRegionFlags.getFlagValue(GenericRegionFlags.MMR) != 0;
        int template = genericRegionFlags.getFlagValue(GenericRegionFlags.GB_TEMPLATE);
        
        short[] genericBAdaptiveTemplateX = new short[4];
    	short[] genericBAdaptiveTemplateY = new short[4];
        
        if (!useMMR) {
        	if (template == 0) {
        		genericBAdaptiveTemplateX[0] = readATValue();
        		genericBAdaptiveTemplateY[0] = readATValue();
        		genericBAdaptiveTemplateX[1] = readATValue();
        		genericBAdaptiveTemplateY[1] = readATValue();
        		genericBAdaptiveTemplateX[2] = readATValue();
        		genericBAdaptiveTemplateY[2] = readATValue();
        		genericBAdaptiveTemplateX[3] = readATValue();
        		genericBAdaptiveTemplateY[3] = readATValue();
        	} else {
        		genericBAdaptiveTemplateX[0] = readATValue();
        		genericBAdaptiveTemplateY[0] = readATValue();
        	}
        	
        	arithmeticDecoder.resetGenericStats(template, null);
        	arithmeticDecoder.start();
        }
        
        bool typicalPredictionGenericDecodingOn = genericRegionFlags.getFlagValue(GenericRegionFlags.TPGDON) != 0;
        int length = segmentHeader.getSegmentDataLength();

        if(length == -1) { 
        	/** 
        	 * length of data is unknown, so it needs to be determined through examination of the data.
        	 * See 7.2.7 - Segment data length of the JBIG2 specification.
        	 */
        	
        	unknownLength = true;
        	
        	short match1;
        	short match2;
        	
        	if(useMMR) {
        		// look for 0x00 0x00 (0, 0)
        		
        		match1 = 0;
        		match2 = 0;
        	} else {
        		// look for 0xFF 0xAC (255, 172)
        		
        		match1 = 255;
        		match2 = 172;
        	}
        	
        	int bytesRead = 0;
    		while(true) {
    			short bite1 = decoder.readbyte();
    			bytesRead++;
    			
    			if(bite1 == match1){
    				short bite2 = decoder.readbyte();
    				bytesRead++;
    				
    				if(bite2 == match2){
    					length = bytesRead - 2;
    					break;
    				}
    			}
    		}
    		
    		decoder.movePointer(-bytesRead);
        }
        
        JBIG2Bitmap bitmap = new JBIG2Bitmap(regionBitmapWidth, regionBitmapHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
        bitmap.clear(0);
        bitmap.readBitmap(useMMR, template, typicalPredictionGenericDecodingOn, false, null, genericBAdaptiveTemplateX, genericBAdaptiveTemplateY, useMMR ? 0 : length - 18);
        
        
        
        if (inlineImage) {
            PageInformationSegment pageSegment = decoder.findPageSegement(segmentHeader.getPageAssociation());
            JBIG2Bitmap pageBitmap = pageSegment.getPageBitmap();

            int extCombOp = regionFlags.getFlagValue(RegionFlags.EXTERNAL_COMBINATION_OPERATOR);
            
            if(pageSegment.getPageBitmapHeight() == -1 && regionBitmapYLocation + regionBitmapHeight > pageBitmap.getHeight()) {
            	pageBitmap.expand(regionBitmapYLocation + regionBitmapHeight, 
            			pageSegment.getPageInformationFlags().getFlagValue(PageInformationFlags.DEFAULT_PIXEL_VALUE));
            }
            
            pageBitmap.combine(bitmap, regionBitmapXLocation, regionBitmapYLocation, extCombOp);
        } else {
			bitmap.setBitmapNumber(getSegmentHeader().getSegmentNumber());
			decoder.appendBitmap(bitmap);
		}

        
        if(unknownLength) {
        	decoder.movePointer(4);
        }
        
    }

    private void readGenericRegionFlags() {
        /** extract text region Segment flags */
        short genericRegionFlagsField = decoder.readbyte();

        genericRegionFlags.setFlags(genericRegionFlagsField);
        
        if(JBIG2StreamDecoder.debug)
            Console.WriteLine("generic region Segment flags = " + genericRegionFlagsField);
    }

    public GenericRegionFlags getGenericRegionFlags() {
        return genericRegionFlags;
    }
}
}
