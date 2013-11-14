using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JBig2Decoder
{
    public class DecodeIntResult
    {

        private long _intResult;
        private bool _booleanResult;

        public DecodeIntResult(long intResult, bool booleanResult)
        {
            this._intResult = intResult;
            this._booleanResult = booleanResult;
        }

        public long intResult()
        {
            return _intResult;
        }

        public bool booleanResult()
        {
            return _booleanResult;
        }
    }
}
