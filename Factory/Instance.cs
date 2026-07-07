namespace DrakeToolbox.Factory
{
    public abstract class Instance
    {
        protected uint id = 0u;
        protected uint ownerId = 0u;

        public uint Id => id;
        public uint OwnerId => ownerId;

        internal const string SetIdsMethodName = nameof(SetIds);

        private void SetIds(uint id, uint ownerId)
        {
            this.id = id;
            this.ownerId = ownerId;
        }
    }
}