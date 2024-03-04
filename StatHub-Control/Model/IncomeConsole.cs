using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StatHub.Control.Model
{
    internal class IncomeConsole
    {
        public int Id;
        public string Path;
        public DateTime DateTime;
        public double IncomeDay;
        public double IncomeTotal;

        public IncomeConsole(int Id)
        {
            this.Id = Id;
        }
    }
}
