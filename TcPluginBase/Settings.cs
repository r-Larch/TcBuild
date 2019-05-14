using System;
using System.Collections.Generic;


namespace TcPluginBase {
    public class Settings : Dictionary<string, string> {
        public Settings() : base(StringComparer.InvariantCultureIgnoreCase)
        {
        }

        public new string this[string key] {
            get => ContainsKey(key) ? base[key] : default;
            set => base[key] = value;
        }
    }
}
