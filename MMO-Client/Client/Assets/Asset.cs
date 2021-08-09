namespace MMO_Client.Client.Assets
{
    internal class Asset
    {
        public bool IsFree { get; protected set; }
        public string ID { get; init; } = "UNDEFINED ASSET";

        public virtual void Free() =>
            IsFree = true;

        public virtual void Unfree() =>
            IsFree = false;
    }
}
