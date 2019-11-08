#if NETSTANDARD2_1
using System;
#endif
using G9Common.HelperClass;
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

        #region FromJson

        public static TResult FromJson<TResult>(this string json)
        {
            return JsonConvert.DeserializeObject<TResult>(json);
        }

        #endregion

        /// <summary>
        ///     Convert byte to type
        /// </summary>
        /// <typeparam name="TResult">Specify type for convert</typeparam>
        /// <param name="byteData">byte[] data</param>
        /// <param name="encoding">Specify encoding</param>
        /// <returns>Converted type from byte[]</returns>

        #region FromJson

        public static TResult FromJson<TResult>(this byte[] byteData, G9Encoding encoding)
        {
            return typeof(TResult) == typeof(byte[])
                ? (TResult) (object) byteData
                : JsonConvert.DeserializeObject<TResult>(encoding.GetString(byteData));
        }

        #endregion

#if NETSTANDARD2_1
        /// <summary>
        ///     Convert ReadOnlyMemory byte to type
        /// </summary>
        /// <typeparam name="TResult">Specify type for convert</typeparam>
        /// <param name="byteData">ReadOnlyMemory byte data</param>
        /// <param name="encoding">Specify encoding</param>
        /// <returns>Converted type from byte[]</returns>
#region FromJson
        public static TResult FromJson<TResult>(this ReadOnlyMemory<byte> byteData, G9Encoding encoding)
        {
            return typeof(TResult) == typeof(byte[])
                ? (TResult) (object) byteData.ToArray()
                : JsonConvert.DeserializeObject<TResult>(encoding.GetString(byteData));
        }
#endregion
#endif

        /// <summary>
        ///     Convert a type to json
        /// </summary>
        /// <param name="objectItem">Specify type</param>
        /// <returns>Converted json from type</returns>

        #region ToJson

        public static string ToJson(this object objectItem)
        {
            return JsonConvert.SerializeObject(objectItem);
        }

        #endregion
    }
}