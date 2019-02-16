using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace core.util.IocTest
{
    public class MyClass : IClass
    {
        public int Calc(int a, int b)
        {
            return a + b;
        }
    }
}
