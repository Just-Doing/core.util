using Autofac;
using Autofac.Core;
using core.util.IocTest;
using System;
using Util.Core.Helpers;
using Util.Dependency;
using Xunit;

namespace Util.Test
{
    public class TestHelper
    {
        [Fact]
        public void IocTest()
        {
            IConfig[] configs = new IConfig[] {
                new CalcConfig()
            };
            Ioc.Register(configs);
            var service = Ioc.Create<IClass>();
            Assert.Equal<int>(7, service.Calc(1, 2));
        }

    }
    public class CalcConfig: Module, IConfig
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MyClass2>().As<MyClass2>();
            builder.RegisterType<MyClass>().As<IClass>();
        }
    }
}
