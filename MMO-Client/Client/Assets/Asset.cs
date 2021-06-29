namespace MMO_Client.Client.Assets
{
    class Asset
    {
        public bool IsFree { get; protected set; } = false;
        public string ID { get; init; } = "UNDEFINED ASSET";

        public virtual void Free() =>
            IsFree = true;

        public virtual void Unfree() =>
            IsFree = false;
    }
}
