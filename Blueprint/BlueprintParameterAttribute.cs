using System;

namespace DrakeToolbox.Blueprints
{
    public sealed class BlueprintParameterAttribute : Attribute
    {
        private string parameterHeader;

        public string ParameterHeader => parameterHeader;

        public BlueprintParameterAttribute(string parameterHeader)
        {
            this.parameterHeader = parameterHeader;
        }
    }
}