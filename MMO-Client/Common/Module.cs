﻿namespace MMO_Client
{
    internal abstract class Module
    {
        public abstract string Name { get; }

        public Module() { }

        public abstract void Initialize();
        public abstract void Terminate();
    }
}
