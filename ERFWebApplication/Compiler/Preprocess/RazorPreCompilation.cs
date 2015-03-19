using System;
using Microsoft.AspNet.Mvc;

namespace WebApplicationNet5.Compiler.Preprocess
{
    public class RazorPreCompilation : RazorPreCompileModule
    {
        public RazorPreCompilation(IServiceProvider provider) : base(provider)
        {
        }
    }
}
