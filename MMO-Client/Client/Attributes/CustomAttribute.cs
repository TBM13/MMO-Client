using MMO_Client.Client.Net.Mines;
using System;

namespace MMO_Client.Client.Attributes
{
    internal class CustomAttribute : IMobjectable, IMobjectBuildable
    {
        public const string SystemPrefix = "system_";

        private readonly bool deserialize;

        /// <param name="deserialize">Should we deserialize the value? Set this to false if you want to work with the raw string value sent by the server.</param>
        public CustomAttribute(string rawKey = null, object value = null, bool deserialize = true)
        {
            RawKey = rawKey;
            Value = value;
            this.deserialize = deserialize;
        }

        public string RawKey { get; private set; }
        public object Value { get; set; }

        public string Key
        {
            get
            {
                if (RawKey == "system_packs")
                    return RawKey;

                return ReadOnly ? RawKey[SystemPrefix.Length..] : RawKey;
            }
        }

        public bool ReadOnly
        {
            get => RawKey.IndexOf(SystemPrefix) == 0;
        }

        public string Serialize(object obj)
        {
            Type type = obj.GetType();
            if (type == typeof(int))
                return obj.ToString();

            if (type == typeof(bool))
                return obj.ToString().ToLowerInvariant();

            if (type == typeof(float))
                return obj.ToString().Replace(',', '.');

            if (type == typeof(string))
                return "\"" + obj + "\"";

            Logger.Error($"Couldn't serialize {obj} ({obj.GetType()}");
            return "";
        }

        public object Deserialize(string s)
        {
            if (s.Length == 0)
                return s;
            
            if (s[0] == '"')
                return s[1..];

            if (s == "true")
                return true;
            if (s == "false")
                return false;

            if (s.Contains('.'))
            {
                if (float.TryParse(s.Replace('.', ','), out float f))
                    return f;
            }

            if (int.TryParse(s, out int i))
                return i;

            return s;
        }

        public override string ToString()
        {
            object obj = Value;
            if (obj.GetType() == typeof(string))
                obj = "\"" + ((string)obj).Substring(0, 97) + "...\"";

            return Key + "=" + obj.ToString();
        }

        public void BuildFromMobject(Mobject mobj)
        {
            RawKey = mobj.Strings["type"];

            if (deserialize)
                Value = Deserialize(mobj.Strings["data"]);
            else
                Value = mobj.Strings["data"];
        }

        public Mobject ToMobject()
        {
            Mobject mobj = new();
            mobj.Strings["type"] = RawKey;

            if (deserialize)
                mobj.Strings["data"] = Serialize(Value);
            else
                mobj.Strings["data"] = (string)Value;

            return mobj;
        }

        public bool Equals(CustomAttribute customAttribute) =>
            customAttribute != null && Key == customAttribute.Key && Value == customAttribute.Value;
    }
}
