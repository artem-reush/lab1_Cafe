using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace lab1_Cafe.Models
{
    public class PositionModel
    {
        public int Dish { get; set; }
        public int Count { get; set; }

        public static string AsString(PositionModel[] array)
        {
            string[] dishes;
            List<string> dishesList = new List<string>(array.Length);
            foreach(PositionModel pos in array)
            {
                dishesList.Add(pos.Dish.ToString());
            }
            dishes = dishesList.ToArray();
            return String.Join(',', dishes);
        }
    }
}
