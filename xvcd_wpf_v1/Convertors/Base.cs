using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Converters
{
    public class HumanDisplayUnit
    {
        public double Value;
        public double Base;
        public double Factor;
        public string Unit;

        public HumanDisplayUnit(double v)
        {
            Value = v;
            var vabs = Math.Abs(v);

            if (vabs == 0)
            {
                Factor = 1e0; Unit = "";
            }
            else if (vabs < 1e-15)
            {
                Factor = 1e-18; Unit = "a";
            }
            else if (vabs < 1e-12)
            {
                Factor = 1e-15; Unit = "f";
            }
            else if (vabs < 1e-9)
            {
                Factor = 1e-12; Unit = "p";
            }
            else if (vabs < 1e-6)
            {
                Factor = 1e-9; Unit = "n";
            }
            else if (vabs < 1e-3)
            {
                Factor = 1e-6; Unit = "u";
            }
            else if (vabs < 1e0)
            {
                Factor = 1e-3; Unit = "m";
            }
            else if (vabs < 1e3)
            {
                Factor = 1e0; Unit = "";
            }
            else if (vabs < 1e6)
            {
                Factor = 1e3; Unit = "k";
            }
            else if (vabs < 1e9)
            {
                Factor = 1e6; Unit = "M";
            }
            else if (vabs < 1e12)
            {
                Factor = 1e9; Unit = "G";
            }
            else
            {
                Factor = 1e12; Unit = "T";
            }

            Base = v / Factor;
        }

        public HumanDisplayUnit(string str)
        {
            var b = str.Split(new string[] { " " }, StringSplitOptions.None)[0];
            var u = str.Split(new string[] { " " }, StringSplitOptions.None)[1];

            Unit = u;
            Base = double.Parse(b);
            switch (Unit)
            {
                case "a":
                    Factor = 1e-18;
                    break;
                case "f":
                    Factor = 1e-15;
                    break;
                case "p":
                    Factor = 1e-12;
                    break;
                case "n":
                    Factor = 1e-9;
                    break;
                case "u":
                    Factor = 1e-6;
                    break;
                case "m":
                    Factor = 1e-3;
                    break;
                case "k":
                    Factor = 1e3;
                    break;
                case "M":
                    Factor = 1e6;
                    break;
                case "G":
                    Factor = 1e9;
                    break;
                case "T":
                    Factor = 1e12;
                    break;
                default:
                    Factor = 1;
                    break;
            }
            Value = Base * Factor;

        }
    }
}
