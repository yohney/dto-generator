using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DtoGenerator.Tests.CodeSamples
{
    public class EntityOnlySimpleProperties
    {
        private string _internalName;

        public EntityOnlySimpleProperties()
        {
            this._internalName = "NoName";
        }

        public int Id { get; set; }

        /// <summary>
        /// Some comment...
        /// </summary>
        public string Name { get; set; }

        public DateTime? Date { get; set; }

        public string OtherString { get; set; }

        // Getter property - should not be included.
        public bool GetterProp => true;

        protected int ProtectedProperty { get; set; }

        private int PrivateProperty { get; set; }
    }
}
