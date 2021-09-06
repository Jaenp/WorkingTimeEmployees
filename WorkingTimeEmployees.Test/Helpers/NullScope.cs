using System;
using System.Collections.Generic;
using System.Text;

namespace WorkingTimeEmployees.Test.Helpers
{
    public class NullScope : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
