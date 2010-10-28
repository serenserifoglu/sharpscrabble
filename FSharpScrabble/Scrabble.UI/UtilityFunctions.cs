using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Scrabble.UI
{
    public sealed class UtilityFunctions
    {
        /// <summary>
        /// Go up visual tree until you find a DE of type T
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="de"></param>
        /// <returns></returns>
        public static T GetAncestorOfType<T>(DependencyObject de)
            where T : DependencyObject
        {
            do{
                if(de is T) { return (T)de; }
                de = VisualTreeHelper.GetParent(de);
            }while(de != null);

            return (T)de; //null
        }
    }
}
