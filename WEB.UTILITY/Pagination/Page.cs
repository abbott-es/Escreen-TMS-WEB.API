using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WEB.UTILITY.Pagination
{
    public struct Page
    {
        public Page(int number, int size)
        {
            Number = number;
            Size = size;
        }

        public int Number { get; }
        public int Size { get; }
    }
}
