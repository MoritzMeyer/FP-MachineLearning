using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
    public static class FloatExtensions
    {
        #region Truncate
        /// <summary>
        /// Kürzt ein Float auf die angegebene Anzahl an Nachkommastellen.
        /// </summary>
        /// <param name="value">Der Float-Wert</param>
        /// <param name="digits">Die Anzahl an Nachkommastellen.</param>
        /// <returns>Der gekürzte Float-Wert.</returns>
        public static float Truncate(this float value, int digits)
        {
            double mult = Math.Pow(10.0, digits);
            double result = Math.Truncate(mult * value) / mult;
            return (float)result;            
        }
        #endregion
    }
}
