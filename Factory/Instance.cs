namespace DrakeToolbox.Factory
{
    public abstract class Instance
    {
        protected uint id;
        protected uint ownerId;

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