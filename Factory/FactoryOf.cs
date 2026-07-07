using System;

namespace DrakeToolbox.Factory
{
    public sealed class FactoryOf : Attribute
    {
        public Type[] instanceTypes;

        public FactoryOf(params Type[] instanceTypes)
        {
            this.instanceTypes = instanceTypes;
        }
    }
}