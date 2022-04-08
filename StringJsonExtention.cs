using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedClassProject
{
    public static class StringJsonExtention
    {
        public static T ConvertJsonTo<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
