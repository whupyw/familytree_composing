using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuoJia_Genealogy.Model
{
    public class IndividualWithFamily
    {
        public Individual Person { get; set; }
        public bool IsSpouse { get; set; }          //是否为外族配偶
        public Individual SpouseOf { get; set; }    //所属本族配偶
        public List<Individual> Children { get; set; } = new List<Individual>();
    }
}
