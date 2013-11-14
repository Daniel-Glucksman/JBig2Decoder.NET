using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class HalftoneRegionSegment : RegionSegment
    {
        private HalftoneRegionFlags halftoneRegionFlags = new HalftoneRegionFlags();
        private bool inlineImage;

        public HalftoneRegionSegment(JBIG2StreamDecoder streamDecoder, bool inlineImage)
            : base(streamDecoder)
        {
            this.inlineImage = inlineImage;
        }

        public override void readSegment()
        {
            readSegment();

            /** read text region Segment flags */
            readHalftoneRegionFlags();

            short[] buf = new short[4];
            decoder.readbyte(buf);
            int gridWidth = BinaryOperation.getInt32(buf);

            buf = new short[4];
            decoder.readbyte(buf);
            int gridHeight = BinaryOperation.getInt32(buf);

            buf = new short[4];
            decoder.readbyte(buf);
            int gridX = BinaryOperation.getInt32(buf);

            buf = new short[4];
            decoder.readbyte(buf);
            int gridY = BinaryOperation.getInt32(buf);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("grid pos and size = " + gridX + ',' + gridY + ' ' + gridWidth + ',' + gridHeight);

            buf = new short[2];
            decoder.readbyte(buf);
            int stepX = BinaryOperation.getInt16(buf);

            buf = new short[2];
            decoder.readbyte(buf);
            int stepY = BinaryOperation.getInt16(buf);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("step size = " + stepX + ',' + stepY);

            int[] referedToSegments = segmentHeader.getReferredToSegments();
            if (referedToSegments.Length != 1)
            {
                Console.WriteLine("Error in halftone Segment. refSegs should == 1");
            }

            Segment segment = decoder.findSegment(referedToSegments[0]);
            if (segment.getSegmentHeader().getSegmentType() != Segment.PATTERN_DICTIONARY)
            {
                if (JBIG2StreamDecoder.debug)
                    Console.WriteLine("Error in halftone Segment. bad symbol dictionary reference");
            }

            PatternDictionarySegment patternDictionarySegment = (PatternDictionarySegment)segment;

            int bitsPerValue = 0, i = 1;
            while (i < patternDictionarySegment.getSize())
            {
                bitsPerValue++;
                i <<= 1;
            }

            JBIG2Bitmap bitmap = patternDictionarySegment.getBitmaps()[0];
            long patternWidth = bitmap.getWidth();
            long patternHeight = bitmap.getHeight();

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("pattern size = " + patternWidth + ',' + patternHeight);

            bool useMMR = halftoneRegionFlags.getFlagValue(HalftoneRegionFlags.H_MMR) != 0;
            int template = halftoneRegionFlags.getFlagValue(HalftoneRegionFlags.H_TEMPLATE);

            if (!useMMR)
            {
                arithmeticDecoder.resetGenericStats(template, null);
                arithmeticDecoder.start();
            }

            int halftoneDefaultPixel = halftoneRegionFlags.getFlagValue(HalftoneRegionFlags.H_DEF_PIXEL);
            bitmap = new JBIG2Bitmap(regionBitmapWidth, regionBitmapHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
            bitmap.clear(halftoneDefaultPixel);

            bool enableSkip = halftoneRegionFlags.getFlagValue(HalftoneRegionFlags.H_ENABLE_SKIP) != 0;

            JBIG2Bitmap skipBitmap = null;
            if (enableSkip)
            {
                skipBitmap = new JBIG2Bitmap(gridWidth, gridHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);
                skipBitmap.clear(0);
                for (int y = 0; y < gridHeight; y++)
                {
                    for (int x = 0; x < gridWidth; x++)
                    {
                        int xx = gridX + y * stepY + x * stepX;
                        int yy = gridY + y * stepX - x * stepY;

                        if (((xx + patternWidth) >> 8) <= 0 || (xx >> 8) >= regionBitmapWidth || ((yy + patternHeight) >> 8) <= 0 || (yy >> 8) >= regionBitmapHeight)
                        {
                            skipBitmap.setPixel(y, x, 1);
                        }
                    }
                }
            }

            int[] grayScaleImage = new int[gridWidth * gridHeight];

            short[] genericBAdaptiveTemplateX = new short[4], genericBAdaptiveTemplateY = new short[4];

            genericBAdaptiveTemplateX[0] = (short)(template <= 1 ? 3 : 2);
            genericBAdaptiveTemplateY[0] = -1;
            genericBAdaptiveTemplateX[1] = -3;
            genericBAdaptiveTemplateY[1] = -1;
            genericBAdaptiveTemplateX[2] = 2;
            genericBAdaptiveTemplateY[2] = -2;
            genericBAdaptiveTemplateX[3] = -2;
            genericBAdaptiveTemplateY[3] = -2;

            JBIG2Bitmap grayBitmap;

            for (int j = bitsPerValue - 1; j >= 0; --j)
            {
                grayBitmap = new JBIG2Bitmap(gridWidth, gridHeight, arithmeticDecoder, huffmanDecoder, mmrDecoder);

                grayBitmap.readBitmap(useMMR, template, false, enableSkip, skipBitmap, genericBAdaptiveTemplateX, genericBAdaptiveTemplateY, -1);

                i = 0;
                for (int row = 0; row < gridHeight; row++)
                {
                    for (int col = 0; col < gridWidth; col++)
                    {
                        int bit = grayBitmap.getPixel(col, row) ^ grayScaleImage[i] & 1;
                        grayScaleImage[i] = (grayScaleImage[i] << 1) | bit;
                        i++;
                    }
                }
            }

            int combinationOperator = halftoneRegionFlags.getFlagValue(HalftoneRegionFlags.H_COMB_OP);

            i = 0;
            for (int col = 0; col < gridHeight; col++)
            {
                int xx = gridX + col * stepY;
                int yy = gridY + col * stepX;
                for (int row = 0; row < gridWidth; row++)
                {
                    if (!(enableSkip && skipBitmap.getPixel(col, row) == 1))
                    {
                        JBIG2Bitmap patternBitmap = patternDictionarySegment.getBitmaps()[grayScaleImage[i]];
                        bitmap.combine(patternBitmap, xx >> 8, yy >> 8, combinationOperator);
                    }

                    xx += stepX;
                    yy -= stepY;

                    i++;
                }
            }

            if (inlineImage)
            {
                PageInformationSegment pageSegment = decoder.findPageSegement(segmentHeader.getPageAssociation());
                JBIG2Bitmap pageBitmap = pageSegment.getPageBitmap();

                int externalCombinationOperator = regionFlags.getFlagValue(RegionFlags.EXTERNAL_COMBINATION_OPERATOR);
                pageBitmap.combine(bitmap, regionBitmapXLocation, regionBitmapYLocation, externalCombinationOperator);
            }
            else
            {
                bitmap.setBitmapNumber(getSegmentHeader().getSegmentNumber());
                decoder.appendBitmap(bitmap);
            }

        }

        private void readHalftoneRegionFlags()
        {
            /** extract text region Segment flags */
            short halftoneRegionFlagsField = decoder.readbyte();

            halftoneRegionFlags.setFlags(halftoneRegionFlagsField);

            if (JBIG2StreamDecoder.debug)
                Console.WriteLine("generic region Segment flags = " + halftoneRegionFlagsField);
        }

        public HalftoneRegionFlags getHalftoneRegionFlags()
        {
            return halftoneRegionFlags;
        }
    }

}