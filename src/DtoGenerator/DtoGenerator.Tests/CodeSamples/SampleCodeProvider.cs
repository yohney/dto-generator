using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Tests.CodeSamples
{
    public static class SampleCodeProvider
    {
        public static string EntityOnlySimpleProperties
        {
            get
            {
                using (var stream = typeof(SampleCodeProvider).Assembly.GetManifestResourceStream("DtoGenerator.Tests.CodeSamples.EntityOnlySimpleProperties.cs"))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
