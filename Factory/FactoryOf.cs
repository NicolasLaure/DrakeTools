using System;

namespace DrakeToolbox.Factory
{
    public sealed class FactoryOf : Attribute
    {
        public Type instanceType;

        public FactoryOf(Type instanceType)
        {
            this.instanceType = instanceType;
        }
    }
}