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
        public static string NoClass => GetManifestText(nameof(NoClass));
        public static string MultipleClasses => GetManifestText(nameof(MultipleClasses));

        public static string EntityWithBase => GetManifestText(nameof(EntityWithBase));
        public static string EntityWithBaseDTO => GetManifestText(nameof(EntityWithBaseDTO));

        public static string ComplexEntity => GetManifestText(nameof(ComplexEntity));
        public static string OtherEntity => GetManifestText(nameof(OtherEntity));
        public static string ComplexEntityDto => GetManifestText(nameof(ComplexEntityDto));

        public static string EntityOnlySimpleProperties => GetManifestText(nameof(EntityOnlySimpleProperties));
        public static string EntityOnlySimplePropertiesDto => GetManifestText(nameof(EntityOnlySimplePropertiesDto));
        
        public static string EntityWithCollectionProperties => GetManifestText(nameof(EntityWithCollectionProperties));

        public static string SampleTable1 => GetManifestText(nameof(SampleTable1));
        public static string SampleTable2 => GetManifestText(nameof(SampleTable2));
        public static string SampleTable3 => GetManifestText(nameof(SampleTable3));

        public static string NestedEntity => GetManifestText(nameof(NestedEntity));

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
