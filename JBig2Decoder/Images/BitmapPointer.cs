using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class BitmapPointer
    {
        private long x, y, width, height, bits, count;
        private bool output;
        private JBIG2Bitmap bitmap;

        public BitmapPointer(JBIG2Bitmap bitmap)
        {
            this.bitmap = bitmap;
            this.height = bitmap.getHeight();
            this.width = bitmap.getWidth();
        }

        public void setPointer(long x, long y)
        {
            this.x = x;
            this.y = y;
            output = true;
            if (y < 0 || y >= height || x >= width)
            {
                output = false;
            }
            count = y * width;
        }

        public int nextPixel()
        {

            // fairly certain the byte can be cached here - seems to work fine. only
            // problem would be if cached pixel was modified, and the modified
            // version needed.
            //		if (y < 0 || y >= height || x >= width) {
            //			return 0;
            //		} else if (x < 0) {
            //			x++;
            //			return 0;
            //		}
            //
            //		if (count == 0 && width - x >= 8) {
            //			bits = bitmap.getPixelbyte(x, y);
            //			count = 8;
            //		} else {
            //			count = 0;
            //		}
            //
            //		if (count > 0) {
            //			int b = bits & 0x01;
            //			count--;
            //			bits >>= 1;
            //			x++;
            //			return b;
            //		}
            //
            //		int pixel = bitmap.getPixel(x, y);
            //		x++;
            //
            //		return pixel;

            if (!output)
            {
                return 0;
            }
            else if (x < 0 || x >= width)
            {
                x++;
                return 0;
            }
            /*if ((output == false) || x < 0) {
                x++;
                return 0;
            }*/
            //int pixel = bitmap.getPixel(x, y);
            //x++;
            return bitmap.data.get((int)(count + x++)) ? 1 : 0;


            //return pixel;
        }
    }
}
