using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts
{
    public class CoolDown
    {
        public float currentTime;
        public float startingTime;

        public CoolDown(float current, float start) 
        {
            this.currentTime = current;
            this.startingTime = start;
        }
    }
}
