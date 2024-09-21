using ExpenseVoyage.Models;
using Newtonsoft.Json;

namespace ExpenseVoyage.SessionExtention
{
	public class Sessionhelper
	{
		public static void SetSession(HttpContext context, string key, string value)
		{
			context.Session.SetString(key, value);
		}

		public static string GetSession(HttpContext context, string key)
		{
			return context.Session.GetString(key);
		}

		public static void ClearSession(HttpContext context)
		{
			context.Session.Clear();
		}

		public static Users GetUserFromSession(HttpContext context)
		{
			var userJson = context.Session.GetString("userSession");
			if (string.IsNullOrEmpty(userJson))
			{
				return null;
			}
			return JsonConvert.DeserializeObject<Users>(userJson);
		}
	}
}
