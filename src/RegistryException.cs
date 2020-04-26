using System;
using System.Collections.Generic;
using System.Text;

namespace AnimalCrossingFlowers
{
    public class RegistryException : Exception
    {
        public RegistryException(string dataType, string id)
            : base("Error: " + dataType + " of ID " + id + " is registered more than once") { }
    }
}
