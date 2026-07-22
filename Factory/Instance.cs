using DrakeToolbox.Flow;

namespace DrakeToolbox.Factory
{
    public abstract class Instance : IInitable
    {
        protected uint id = 0u;
        protected uint ownerId = 0u;
        protected uint clientId = 0u;

        public uint Id => id;
        public uint OwnerId => ownerId;

        internal const string SetIdsMethodName = nameof(SetIds);

        private void SetIds(uint id, uint ownerId, uint clientId)
        {
            this.id = id;
            this.ownerId = ownerId;
            this.clientId = clientId;
        }

        public abstract void Init();
        public abstract void LateInit();
    }
}