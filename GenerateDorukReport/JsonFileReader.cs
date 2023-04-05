using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Text;

namespace GenerateDorukReport
{
    public static class JsonFileReader
    {
        public static T Read<T>(string filePath)
        {
            var content = File.ReadAllText(filePath, Encoding.GetEncoding("iso-8859-9"));
            return JsonConvert.DeserializeObject<T>(content, new IsoDateTimeConverter { DateTimeFormat = "d.MM.yyyy HH:mm:ss" });
        }
    }
}
