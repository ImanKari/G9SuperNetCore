using System;
using System.Linq;
using G9LogManagement;

namespace G9Common.RandomNumberWithoutDuplicates
{
    public class G9RandomNumberWithoutDuplicates
    {
        #region Fields And Properties

        /// <summary>
        ///     یک شی از کلاس رندوم برای ساخت اعداد تکراری
        /// </summary>
        private readonly Random _random;

        /// <summary>
        ///     فیلد برای نگهداری آخرین عدد رندوم
        /// </summary>
        private int _randomInt;

        /// <summary>
        ///     فیلد برای نگهداری آخرین عدد رندوم اعشاری
        /// </summary>
        private float _randomFloat;

        #endregion

        #region Methods

        /// <summary>
        ///     سازنده کلاس
        /// </summary>

        #region public G9RandomNumberWithoutDuplicates()

        public G9RandomNumberWithoutDuplicates()
        {
            _random = new Random();
        }

        #endregion

        /// <summary>
        ///     تابع یک عدد رندوم تولید می کند و این قابلیت را دارد تا در حلقه ها عدد تکراری بر نگرداند
        ///     عدد بعدی همیشه متفاوت خواهد بود
        ///     امکان تکرار دو عدد وجود ندارد
        /// </summary>
        /// <param name="minValue">بازه اول عدد رندوم</param>
        /// <param name="maxValue">بازه دوم عدد رندوم</param>
        /// <returns>یک عدد غیر تکراری بین دو بازه بر می گرداند</returns>

        #region public int Next(int minValue, int maxValue)

        public int Next(int minValue, int maxValue)
        {
            int randomNumbers;
            do
            {
                randomNumbers = _random.Next(minValue, maxValue);
            } while (_randomInt == randomNumbers);

            _randomInt = randomNumbers;
            return _randomInt;
        }

        #endregion

        /// <summary>
        ///     تابع یک عدد رندوم تولید می کند و این قابلیت را دارد تا در حلقه ها عدد تکراری بر نگرداند
        ///     عدد بعدی همیشه متفاوت خواهد بود
        ///     امکان تکرار دو عدد وجود ندارد
        ///     همچنین اعدادی که به صورت پارامز وارد می شوند نباید در عدد رندوم وجود داشته باشند
        /// </summary>
        /// <param name="minValue">بازه اول عدد رندوم</param>
        /// <param name="maxValue">بازه دوم عدد رندوم</param>
        /// <returns>یک عدد غیر تکراری بین دو بازه بر می گرداند</returns>

        #region public int Next(int minValue, int maxValue)

        public int NextWithoutParamsNumbers(int minValue, int maxValue, params int[] Numbers)
        {
            int randomNumbers;
            do
            {
                randomNumbers = _random.Next(minValue, maxValue);
            } while (_randomInt == randomNumbers && Numbers.Count(s => s == randomNumbers) == 0);

            _randomInt = randomNumbers;
            return _randomInt;
        }

        #endregion

        /// <summary>
        ///     تابع یک عدد دابل بین 0 تا 1 به صورت اعشاری بر می گرداند
        ///     عدد بعدی همیشه متفاوت خواهد بود
        ///     امکان تکرار دو عدد وجود ندارد
        /// </summary>
        /// <returns>یک عدد اعشاری بین 0 تا 1</returns>

        #region public double NextDouble()

        public float NextDouble()
        {
            float randomNumbers;
            do
            {
                randomNumbers = (float)_random.NextDouble();
            } while (_randomFloat == randomNumbers);

            _randomFloat = randomNumbers;
            return _randomFloat;
        }

        #endregion

        #endregion
    }
}