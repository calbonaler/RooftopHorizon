using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Saruna
{
	static class Utils
	{
		enum JsonType
		{
			Null,
			Number,
			String,
			Boolean,
			Array,
			Object,
		}

		static readonly Dictionary<string, string> m_TimeZoneIds = new Dictionary<string, string> {
			{ "International Date Line West", "Dateline Standard Time" },
			{ "Midway Island", "UTC-11" },
			{ "American Samoa", "UTC-11" },
			{ "Hawaii", "Hawaiian Standard Time" },
			{ "Alaska", "Alaskan Standard Time" },
			{ "Tijuana", "Pacific Standard Time (Mexico)" },
			{ "Pacific Time (US & Canada)", "Pacific Standard Time" },
			{ "Arizona", "US Mountain Standard Time" },
			{ "Chihuahua", "Mountain Standard Time (Mexico)" },
			{ "Mazatlan", "Mountain Standard Time (Mexico)" },
			{ "Mountain Time (US & Canada)", "Mountain Standard Time" },
			{ "Guadalajara", "Central Standard Time (Mexico)" },
			{ "Mexico City", "Central Standard Time (Mexico)" },
			{ "Monterrey", "Central Standard Time (Mexico)" },
			{ "Saskatchewan", "Canada Central Standard Time" },
			{ "Central America", "Central America Standard Time" },
			{ "Central Time (US & Canada)", "Central Standard Time" },
			{ "Indiana (East)", "US Eastern Standard Time" },
			{ "Bogota", "SA Pacific Standard Time" },
			{ "Lima", "SA Pacific Standard Time" },
			{ "Quito", "SA Pacific Standard Time" },
			{ "Eastern Time (US & Canada)", "Eastern Standard Time" },
			{ "Caracas", "Venezuela Standard Time" },
			{ "Santiago", "Pacific SA Standard Time" },
			{ "La Paz", "SA Western Standard Time" },
			{ "Georgetown", "SA Western Standard Time" },
			{ "Atlantic Time (Canada)", "Atlantic Standard Time" },
			{ "Newfoundland", "Newfoundland Standard Time" },
			{ "Greenland", "Greenland Standard Time" },
			{ "Buenos Aires", "Argentina Standard Time" },
			{ "Brasilia", "E. South America Standard Time" },
			{ "Montevideo", "Montevideo Standard Time" },
			{ "Mid-Atlantic", "Mid-Atlantic Standard Time" },
			{ "Azores", "Azores Standard Time" },
			{ "Cape Verde Is.", "Cape Verde Standard Time" },
			{ "Casablanca", "Morocco Standard Time" },
			{ "Dublin", "GMT Standard Time" },
			{ "Edinburgh", "GMT Standard Time" },
			{ "Lisbon", "GMT Standard Time" },
			{ "London", "GMT Standard Time" },
			{ "Monrovia", "Greenwich Standard Time" },
			{ "UTC", "UTC" },
			{ "Amsterdam", "W. Europe Standard Time" },
			{ "Berlin", "W. Europe Standard Time" },
			{ "Bern", "W. Europe Standard Time" },
			{ "Rome", "W. Europe Standard Time" },
			{ "Stockholm", "W. Europe Standard Time" },
			{ "Vienna", "W. Europe Standard Time" },
			{ "Sarajevo", "Central European Standard Time" },
			{ "Skopje", "Central European Standard Time" },
			{ "Warsaw", "Central European Standard Time" },
			{ "Zagreb", "Central European Standard Time" },
			{ "Brussels", "Romance Standard Time" },
			{ "Copenhagen", "Romance Standard Time" },
			{ "Madrid", "Romance Standard Time" },
			{ "Paris", "Romance Standard Time" },
			{ "Belgrade", "Central Europe Standard Time" },
			{ "Bratislava", "Central Europe Standard Time" },
			{ "Budapest", "Central Europe Standard Time" },
			{ "Ljubljana", "Central Europe Standard Time" },
			{ "Prague", "Central Europe Standard Time" },
			{ "West Central Africa", "W. Central Africa Standard Time" },
			{ "Bucharest", "GTB Standard Time" },
			{ "Athens", "GTB Standard Time" },
			{ "Istanbul", "Turkey Standard Time" },
			{ "Jerusalem", "Israel Standard Time" },
			{ "Cairo", "Egypt Standard Time" },
			{ "Harare", "South Africa Standard Time" },
			{ "Pretoria", "South Africa Standard Time" },
			{ "Helsinki", "FLE Standard Time" },
			{ "Kyiv", "FLE Standard Time" },
			{ "Riga", "FLE Standard Time" },
			{ "Sofia", "FLE Standard Time" },
			{ "Tallinn", "FLE Standard Time" },
			{ "Vilnius", "FLE Standard Time" },
			{ "Minsk", "Kaliningrad Standard Time" },
			{ "Kuwait", "Arab Standard Time" },
			{ "Riyadh", "Arab Standard Time" },
			{ "Nairobi", "E. Africa Standard Time" },
			{ "Baghdad", "Arabic Standard Time" },
			{ "Tehran", "Iran Standard Time" },
			{ "Abu Dhabi", "Arabian Standard Time" },
			{ "Muscat", "Arabian Standard Time" },
			{ "Yerevan", "Caucasus Standard Time" },
			{ "Tbilisi", "Georgian Standard Time" },
			{ "Baku", "Azerbaijan Standard Time" },
			{ "Moscow", "Russian Standard Time" },
			{ "St. Petersburg", "Russian Standard Time" },
			{ "Volgograd", "Russian Standard Time" },
			{ "Kabul", "Afghanistan Standard Time" },
			{ "Tashkent", "West Asia Standard Time" },
			{ "Islamabad", "Pakistan Standard Time" },
			{ "Karachi", "Pakistan Standard Time" },
			{ "Sri Jayawardenepura", "Sri Lanka Standard Time" },
			{ "Chennai", "India Standard Time" },
			{ "Kolkata", "India Standard Time" },
			{ "Mumbai", "India Standard Time" },
			{ "New Delhi", "India Standard Time" },
			{ "Kathmandu", "Nepal Standard Time" },
			{ "Astana", "Central Asia Standard Time" },
			{ "Almaty", "Central Asia Standard Time" },
			{ "Ekaterinburg", "Ekaterinburg Standard Time" },
			{ "Dhaka", "Bangladesh Standard Time" },
			{ "Rangoon", "Myanmar Standard Time" },
			{ "Novosibirsk", "N. Central Asia Standard Time" },
			{ "Bangkok", "SE Asia Standard Time" },
			{ "Hanoi", "SE Asia Standard Time" },
			{ "Jakarta", "SE Asia Standard Time" },
			{ "Ulaanbaatar", "Ulaanbaatar Standard Time" },
			{ "Kuala Lumpur", "Singapore Standard Time" },
			{ "Singapore", "Singapore Standard Time" },
			{ "Krasnoyarsk", "North Asia Standard Time" },
			{ "Perth", "W. Australia Standard Time" },
			{ "Beijing", "China Standard Time" },
			{ "Chongqing", "China Standard Time" },
			{ "Hong Kong", "China Standard Time" },
			{ "Urumqi", "China Standard Time" },
			{ "Taipei", "Taipei Standard Time" },
			{ "Irkutsk", "North Asia East Standard Time" },
			{ "Seoul", "Korea Standard Time" },
			{ "Osaka", "Tokyo Standard Time" },
			{ "Sapporo", "Tokyo Standard Time" },
			{ "Tokyo", "Tokyo Standard Time" },
			{ "Adelaide", "Cen. Australia Standard Time" },
			{ "Darwin", "AUS Central Standard Time" },
			{ "Canberra", "AUS Eastern Standard Time" },
			{ "Melbourne", "AUS Eastern Standard Time" },
			{ "Sydney", "AUS Eastern Standard Time" },
			{ "Guam", "West Pacific Standard Time" },
			{ "Port Moresby", "West Pacific Standard Time" },
			{ "Brisbane", "E. Australia Standard Time" },
			{ "Hobart", "Tasmania Standard Time" },
			{ "Yakutsk", "Yakutsk Standard Time" },
			{ "Vladivostok", "Vladivostok Standard Time" },
			{ "Solomon Is.", "Central Pacific Standard Time" },
			{ "New Caledonia", "Central Pacific Standard Time" },
			{ "Auckland", "New Zealand Standard Time" },
			{ "Wellington", "New Zealand Standard Time" },
			{ "Fiji", "Fiji Standard Time" },
			{ "Kamchatka", "Kamchatka Standard Time" },
			{ "Magadan", "Magadan Standard Time" },
			{ "Marshall Is.", "UTC+12" },
			{ "Tokelau Is.", "UTC+12" },
			{ "Chatham Is.", "UTC+12" },
			{ "Samoa", "Samoa Standard Time" },
			{ "Nuku'alofa", "Tonga Standard Time" },
		};

		static readonly Dictionary<string, string> m_RubyOldTimeZoneKeys = new Dictionary<string, string> {
			{ "Dateline Standard Time", "Pacific/Midway" },
			{ "UTC-11", "Pacific/Midway" },
			{ "Hawaiian Standard Time", "Pacific/Honolulu" },
			{ "Alaskan Standard Time", "America/Juneau" },
			{ "Pacific Standard Time (Mexico)", "America/Tijuana" },
			{ "Pacific Standard Time", "America/Los_Angeles" },
			{ "US Mountain Standard Time", "America/Phoenix" },
			{ "Mountain Standard Time (Mexico)", "America/Chihuahua" },
			{ "Mountain Standard Time", "America/Denver" },
			{ "Central Standard Time (Mexico)", "America/Mexico_City" },
			{ "Canada Central Standard Time", "America/Regina" },
			{ "Central America Standard Time", "America/Guatemala" },
			{ "Central Standard Time", "America/Chicago" },
			{ "US Eastern Standard Time", "America/Indiana/Indianapolis" },
			{ "SA Pacific Standard Time", "America/Lima" },
			{ "Eastern Standard Time", "America/New_York" },
			{ "Venezuela Standard Time", "America/Caracas" },
			{ "Paraguay Standard Time", "America/Santiago" },
			{ "Central Brazilian Standard Time", "America/Santiago" },
			{ "Pacific SA Standard Time", "America/Santiago" },
			{ "SA Western Standard Time", "America/La_Paz" },
			{ "Atlantic Standard Time", "America/Halifax" },
			{ "Newfoundland Standard Time", "America/St_Johns" },
			{ "SA Eastern Standard Time", "America/Argentina/Buenos_Aires" },
			{ "Greenland Standard Time", "America/Godthab" },
			{ "Bahia Standard Time", "America/Argentina/Buenos_Aires" },
			{ "Argentina Standard Time", "America/Argentina/Buenos_Aires" },
			{ "E. South America Standard Time", "America/Sao_Paulo" },
			{ "Montevideo Standard Time", "America/Montevideo" },
			{ "Mid-Atlantic Standard Time", "Atlantic/South_Georgia" },
			{ "UTC-02", "Atlantic/South_Georgia" },
			{ "Azores Standard Time", "Atlantic/Azores" },
			{ "Cape Verde Standard Time", "Atlantic/Cape_Verde" },
			{ "Morocco Standard Time", "Africa/Casablanca" },
			{ "GMT Standard Time", "Europe/London" },
			{ "Greenwich Standard Time", "Africa/Monrovia" },
			{ "UTC", "Etc/UTC" },
			{ "W. Europe Standard Time", "Europe/Berlin" },
			{ "Namibia Standard Time", "Africa/Algiers" },
			{ "Central European Standard Time", "Europe/Sarajevo" },
			{ "Libya Standard Time", "Europe/Budapest" },
			{ "Romance Standard Time", "Europe/Brussels" },
			{ "Central Europe Standard Time", "Europe/Belgrade" },
			{ "W. Central Africa Standard Time", "Africa/Algiers" },
			{ "GTB Standard Time", "Europe/Athens" },
			{ "Turkey Standard Time", "Europe/Istanbul" },
			{ "Israel Standard Time", "Asia/Jerusalem" },
			{ "Egypt Standard Time", "Africa/Cairo" },
			{ "Syria Standard Time", "Europe/Helsinki" },
			{ "South Africa Standard Time", "Africa/Harare" },
			{ "FLE Standard Time", "Europe/Helsinki" },
			{ "Middle East Standard Time", "Europe/Helsinki" },
			{ "E. Europe Standard Time", "Europe/Helsinki" },
			{ "Jordan Standard Time", "Europe/Helsinki" },
			{ "Kaliningrad Standard Time", "Europe/Minsk" },
			{ "Arab Standard Time", "Asia/Kuwait" },
			{ "E. Africa Standard Time", "Africa/Nairobi" },
			{ "Arabic Standard Time", "Asia/Baghdad" },
			{ "Iran Standard Time", "Asia/Tehran" },
			{ "Arabian Standard Time", "Asia/Muscat" },
			{ "Caucasus Standard Time", "Asia/Yerevan" },
			{ "Georgian Standard Time", "Asia/Tbilisi" },
			{ "Azerbaijan Standard Time", "Asia/Baku" },
			{ "Mauritius Standard Time", "Asia/Muscat" },
			{ "Russian Standard Time", "Europe/Moscow" },
			{ "Afghanistan Standard Time", "Asia/Kabul" },
			{ "West Asia Standard Time", "Asia/Tashkent" },
			{ "Pakistan Standard Time", "Asia/Karachi" },
			{ "Sri Lanka Standard Time", "Asia/Colombo" },
			{ "India Standard Time", "Asia/Kolkata" },
			{ "Nepal Standard Time", "Asia/Kathmandu" },
			{ "Central Asia Standard Time", "Asia/Dhaka" },
			{ "Ekaterinburg Standard Time", "Asia/Yekaterinburg" },
			{ "Bangladesh Standard Time", "Asia/Dhaka" },
			{ "Myanmar Standard Time", "Asia/Rangoon" },
			{ "N. Central Asia Standard Time", "Asia/Novosibirsk" },
			{ "SE Asia Standard Time", "Asia/Bangkok" },
			{ "Ulaanbaatar Standard Time", "Asia/Ulaanbaatar" },
			{ "Singapore Standard Time", "Asia/Kuala_Lumpur" },
			{ "North Asia Standard Time", "Asia/Krasnoyarsk" },
			{ "W. Australia Standard Time", "Australia/Perth" },
			{ "China Standard Time", "Asia/Shanghai" },
			{ "Taipei Standard Time", "Asia/Taipei" },
			{ "North Asia East Standard Time", "Asia/Irkutsk" },
			{ "Korea Standard Time", "Asia/Seoul" },
			{ "Tokyo Standard Time", "Asia/Tokyo" },
			{ "Cen. Australia Standard Time", "Australia/Adelaide" },
			{ "AUS Central Standard Time", "Australia/Darwin" },
			{ "AUS Eastern Standard Time", "Australia/Melbourne" },
			{ "West Pacific Standard Time", "Pacific/Guam" },
			{ "E. Australia Standard Time", "Australia/Brisbane" },
			{ "Tasmania Standard Time", "Australia/Hobart" },
			{ "Yakutsk Standard Time", "Asia/Yakutsk" },
			{ "Vladivostok Standard Time", "Asia/Vladivostok" },
			{ "Central Pacific Standard Time", "Pacific/Guadalcanal" },
			{ "New Zealand Standard Time", "Pacific/Auckland" },
			{ "Fiji Standard Time", "Pacific/Fiji" },
			{ "Kamchatka Standard Time", "Asia/Kamchatka" },
			{ "Magadan Standard Time", "Asia/Magadan" },
			{ "UTC+12", "Pacific/Majuro" },
			{ "Samoa Standard Time", "Pacific/Apia" },
			{ "Tonga Standard Time", "Pacific/Tongatapu" },
		};

		static JsonType GetJsonType(XElement jsonObjectAsXml) { return jsonObjectAsXml == null ? JsonType.Null : (JsonType)Enum.Parse(typeof(JsonType), jsonObjectAsXml.Attribute("type").Value, true); }

		public static bool IsJsonNull(XElement jsonObjectAsXml) { return GetJsonType(jsonObjectAsXml) == JsonType.Null; }

		public static T Cast<T>(this XElement jsonObjectAsXml)
		{
			TypeCode typeCode;
			if (typeof(T).IsGenericType && typeof(T).GetGenericTypeDefinition() == typeof(Nullable<>))
				typeCode = Type.GetTypeCode(typeof(T).GetGenericArguments()[0]);
			else
				typeCode = Type.GetTypeCode(typeof(T));
			var jsonType = GetJsonType(jsonObjectAsXml);
			if (jsonType == JsonType.Number)
			{
				if (typeCode == TypeCode.Byte)
					return (T)(object)(byte)(int)jsonObjectAsXml;
				else if (typeCode == TypeCode.Decimal)
					return (T)(object)(decimal)jsonObjectAsXml;
				else if (typeCode == TypeCode.Double)
					return (T)(object)(double)jsonObjectAsXml;
				else if (typeCode == TypeCode.Int16)
					return (T)(object)(short)jsonObjectAsXml;
				else if (typeCode == TypeCode.Int32)
					return (T)(object)(int)jsonObjectAsXml;
				else if (typeCode == TypeCode.Int64)
					return (T)(object)(long)jsonObjectAsXml;
				else if (typeCode == TypeCode.SByte)
					return (T)(object)(sbyte)(int)jsonObjectAsXml;
				else if (typeCode == TypeCode.Single)
					return (T)(object)(float)jsonObjectAsXml;
				else if (typeCode == TypeCode.UInt16)
					return (T)(object)(ushort)(int)jsonObjectAsXml;
				else if (typeCode == TypeCode.UInt32)
					return (T)(object)(uint)jsonObjectAsXml;
				else if (typeCode == TypeCode.UInt64)
					return (T)(object)(ulong)jsonObjectAsXml;
			}
			else if (jsonType == JsonType.Boolean && typeCode == TypeCode.Boolean)
				return (T)(object)(bool)jsonObjectAsXml;
			else if (jsonType == JsonType.String && typeCode == TypeCode.String)
				return (T)(object)System.Net.WebUtility.HtmlDecode((string)jsonObjectAsXml);
			return default(T);
		}

		public static string GetResourceFamilyString(ResourceFamilies families)
		{
			return string.Join(",", families.ToString().Split(',').Select(s => PascalCaseToSnakeCase(s.Trim())));
		}

		public static ResourceFamilies GetResourceFamilies(string str)
		{
			return str.Split(',')
				.Select(s => (ResourceFamilies)Enum.Parse(typeof(ResourceFamilies), SnakeCaseToPascalCase(s.Trim())))
				.Aggregate((ored, val) => ored | val);
		}

		static string PascalCaseToSnakeCase(string pascal)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < pascal.Length; i++)
			{
				if (char.IsUpper(pascal, i))
				{
					if (i > 0)
						sb.Append('_');
					sb.Append(char.ToLower(pascal[i]));
				}
				else
					sb.Append(pascal[i]);
			}
			return sb.ToString();
		}

		static string SnakeCaseToPascalCase(string snake)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < snake.Length; i++)
			{
				if (i == 0)
					sb.Append(char.ToUpper(snake[i]));
				else if (snake[i] == '_')
				{
					do
						i++;
					while (snake[i] == '_');
					sb.Append(char.ToUpper(snake[i]));
				}
				else
					sb.Append(snake[i]);
			}
			return sb.ToString();
		}

		public static TimeZoneInfo GetTimeZoneInfo(string key)
		{
			string id;
			if (!string.IsNullOrEmpty(key) && m_TimeZoneIds.TryGetValue(key, out id))
				return TimeZoneInfo.FindSystemTimeZoneById(id);
			return null;
		}

		public static string GetRubyOldTimeZoneKey(this TimeZoneInfo info)
		{
			string key;
			if (info != null && m_RubyOldTimeZoneKeys.TryGetValue(info.Id, out key))
				return key;
			return string.Empty;
		}
	}
}
