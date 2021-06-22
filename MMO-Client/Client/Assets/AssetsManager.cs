using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MMO_Client.Common.Logger;

namespace MMO_Client.Client.Assets
{
    class AssetsManager
    {
        private static AssetsManager instance;
        private readonly List<VectorAsset> vectorPool = new();

        public AssetsManager()
        {
            instance = this;
            Logger.Info("Assets Manager Created", "Assets Manager");
        }

        private VectorAsset getFreeVectorAsset()
        {
            foreach(VectorAsset v in vectorPool)
            {
                if (v.IsFree)
                {
                    v.IsFree = false;
                    return v;
                }
            }

            VectorAsset vectorAsset = new() { IsFree = false };
            vectorPool.Add(vectorAsset);

            return vectorAsset;
        }
    }
}
