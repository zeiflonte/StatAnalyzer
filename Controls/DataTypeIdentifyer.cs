using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stats.Controls
{
    class DataTypeIdentifyer
    {
        private static float criteria;
        public float Criteria
        {
            private get
            {
                return criteria;
            }
            set
            {
                criteria = value;
            }
        }
        private List<float> data = null;
        public DataTypeIdentifyer(List<float> data)
        {
            this.data = data;
        }

        //true - дискретные данные
        //false - непрерывные
        public bool Identify()
        {
            float[] unique = data.Distinct().ToArray();

            if ( (unique.Length/(float)data.Count) < (1 - criteria) )
            {
                return true;
            }


            return false;
        }
    }
}
