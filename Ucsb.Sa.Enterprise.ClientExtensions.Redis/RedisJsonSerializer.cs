using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Ucsb.Sa.Enterprise.ClientExtensions.Redis
{
	public class RedisJsonSerializer
	{
		private static JsonSerializerSettings _settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };

		public static byte[] Serialize(object data)
		{
			return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data, _settings));
		}

		public static object Deserialize(byte[] data)
		{
			if (data == null)
			{
				return null;
			}
			return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), _settings);
		}
	}
}
