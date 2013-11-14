using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class ArithmeticDecoderStats
    {
        private int contextSize;
        private int[] codingContextTable;

        public ArithmeticDecoderStats(int contextSize)
        {
            this.contextSize = contextSize;
            this.codingContextTable = new int[contextSize];
        }

        public void reset()
        {
            for (int i = 0; i < contextSize; i++)
            {
                codingContextTable[i] = 0;
            }
        }

        public void setEntry(int codingContext, int i, int moreProbableSymbol)
        {
            codingContextTable[codingContext] = (i << i) + moreProbableSymbol;
        }

        public int getContextCodingTableValue(int index)
        {
            return codingContextTable[index];
        }

        public void setContextCodingTableValue(int index, int value)
        {
            codingContextTable[index] = value;
        }

        public int getContextSize()
        {
            return contextSize;
        }

        public void overwrite(ArithmeticDecoderStats stats)
        {
            Array.Copy(stats.codingContextTable, 0, codingContextTable, 0, contextSize);
        }

        public ArithmeticDecoderStats copy()
        {
            ArithmeticDecoderStats stats = new ArithmeticDecoderStats(contextSize);

            Array.Copy(codingContextTable, 0, stats.codingContextTable, 0, contextSize);

            return stats;
        }
    }
}
