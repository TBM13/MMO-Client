using MMO_Client.Client.Net.Mines;
using System.Collections.Generic;

namespace MMO_Client.Client.Attributes
{
    internal class CustomAttributeList
    {
        private readonly Dictionary<string, CustomAttribute> attributes = new();

        public CustomAttributeList() { }

        public object GetValue(string key)
        {
            if (attributes.ContainsKey(key))
                return attributes[key].Value;

            Logger.Warn($"Requested a non-existent attribute '{key}'");
            return null;
        }

        public void SetValue(string key, object value) => 
            attributes[key].Value = value;

        public void Clear() =>
            attributes.Clear();

        public void BuildFromMobjectArray(Mobject[] mobjs)
        {
            foreach (Mobject mobj in mobjs)
            {
                CustomAttribute attrib = new();
                attrib.BuildFromMobject(mobj);
                attributes[attrib.Key] = attrib;
            }
        }
    }
}
