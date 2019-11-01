using System;
using System.Text;
using Newtonsoft.Json;

namespace G9Common.JsonHelper
{
    public static class G9JsonHelper
    {
        /// <summary>
        ///     Convert json to type
        /// </summary>
        /// <typeparam name="TResult">Specify type for convert</typeparam>
        /// <param name="json">json data</param>
        /// <returns>Converted type from json</returns>
        public static TResult FromJson<TResult>(this string json)
        {
            return JsonConvert.DeserializeObject<TResult>(json);
        }

        /// <summary>
        ///     Convert byte to type
        /// </summary>
        /// <typeparam name="TResult">Specify type for convert</typeparam>
        /// <param name="byteData">byte[] data</param>
        /// <returns>Converted type from byte[]</returns>
        public static TResult FromJson<TResult>(this byte[] byteData)
        {
            return typeof(TResult) == typeof(byte[])
                ? (TResult) (object) byteData
                : JsonConvert.DeserializeObject<TResult>(Encoding.UTF8.GetString(byteData));
        }

        /// <summary>
        ///     Convert a type to json
        /// </summary>
        /// <param name="objectItem">Specify type</param>
        /// <returns>Converted json from type</returns>
        public static string ToJson(this object objectItem)
        {
            return JsonConvert.SerializeObject(objectItem);
        }
    }
}