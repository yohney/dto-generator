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
        public static string EntityOnlySimpleProperties => GetManifestText(nameof(EntityOnlySimpleProperties));
        public static string NoClass => GetManifestText(nameof(NoClass));
        public static string MultipleClasses => GetManifestText(nameof(MultipleClasses));

        private static string GetManifestText(string name)
        {
            using (var stream = typeof(SampleCodeProvider).Assembly.GetManifestResourceStream($"DtoGenerator.Tests.CodeSamples.{name}.cs"))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
