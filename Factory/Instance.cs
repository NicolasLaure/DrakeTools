namespace DrakeToolbox.Factory
{
    public class Instance
    {
        protected uint id;

        public uint Id => id;

        internal const string SetIdMethodName = nameof(SetId);

        private void SetId(uint id)
        {
            this.id = id;
        }
    }
}