using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Whiskey.Utility.Collections
{
    public static class ShuffleExt
    {
        private static Random rand = new Random();
        public static IList<T> Shuffle<T>(this IList<T> list)
        {

            var n = list.Count;
            var number = 0;
            while (n > 1)
            {
                n--;
                number = rand.Next(0, list.Count);
                var itemInNumber = list[number];
                var itemToSwap = list[n];
                var temp = itemInNumber;
                list[number] = list[n];
                list[n] = temp;
            }
            return list;
        }
    }
}
