using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WindowsPhoneUtils.Extensions
{
    public static class ColorExtensions
    {

        public static Color Lerp(this Color colour, Color to, float amount)
        {
            float sr = colour.R, sg = colour.G, sb = colour.B;
            float er = to.R, eg = to.G, eb = to.B;

            byte r = (byte)sr.Lerp(er, amount),
                 g = (byte)sg.Lerp(eg, amount),
                 b = (byte)sb.Lerp(eb, amount);

            return Color.FromArgb(colour.A, r, g, b);
        }

        public static float Lerp(this float start, float end, float amount)
        {
            float difference = end - start;
            float adjusted = difference * amount;
            return start + adjusted;
        }

    }
}
