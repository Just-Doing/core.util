using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace core.util.IocTest
{
    public class MyClass : IClass
    {
        private MyClass2 _m;
        public MyClass(MyClass2 c) {
            _m = c;
        }
        public int Calc(int a, int b)
        {
            return _m.Calc(a, b) + a + b;
        }
    }
}
