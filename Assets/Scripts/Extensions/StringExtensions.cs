using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Extensions
{
    public static class StringExtensions
    {
        #region IsNotNullOrEmpty
        /// <summary>
        /// Prüft ob ein String nicht null oder leer ist.
        /// </summary>
        /// <param name="value">Der zu prüfende string.</param>
        /// <returns>True, wenn der string nicht leer oder nicht null ist, false wenn nicht.</returns>
        public static bool IsNotNullOrEmpty(this string value)
        {
            return (value != null && value.Length > 0);
        }
        #endregion

        #region IsNullOrEmpty
        /// <summary>
        /// Prüft ob ein string null oder leer ist.
        /// </summary>
        /// <param name="value">Der zu prüfende string.</param>
        /// <returns>True, wenn der string leer oder null ist, false wenn nicht.</returns>
        public static bool IsNullOrEmpty(this string value)
        {
            return (value == null || value.Length < 1);
        }
        #endregion
    }
}
