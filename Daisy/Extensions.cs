using System;
using System.Diagnostics;

namespace Daisy
{
    public static class Extensions
    {
        public static void Invoke(this Process process, Type type, string methodName, string arg = null)
        {
            var injector = new Injector(process.Id);
            injector.Invoke(type,methodName,arg);
        }
    }
}
