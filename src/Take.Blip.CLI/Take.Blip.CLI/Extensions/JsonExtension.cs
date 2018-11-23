using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Take.BlipCLI.Extensions
{
    public static class JsonExtension
    {
        public static string ToJson(this object instance)
        {
            if (instance == null) return "{}";
            return JsonConvert.SerializeObject(instance);
        }
    }
}
