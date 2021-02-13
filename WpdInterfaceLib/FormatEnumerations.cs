using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpdInterfaceLib
{
    public enum Script
    {
        superscript,
        subscript,
        none
    }

    public enum Highlight
    {
        yellow,
        green,
        cyan,
        magenta,
        blue,
        red,
        darkBlue,
        darkCyan,
        darkGreen,
        darkMagenta,
        darkRed,
        darkYellow,
        darkGray,
        lightGray,
        black,
        none
    };

    public enum UnderlineStyle
    {
        none = 0,
        singleLine = 1,
        words = 2,
        doubleLine = 3,
        dotted = 4,
        thick = 6,
        dash = 7,
        dotDash = 9,
        dotDotDash = 10,
        wave = 11,
        dottedHeavy = 20,
        dashedHeavy = 23,
        dashDotHeavy = 25,
        dashDotDotHeavy = 26,
        dashLongHeavy = 27,
        dashLong = 39,
        wavyDouble = 43,
        wavyHeavy = 55,
    };

    public enum StrikeThrough
    {
        none,
        strike,
        doubleStrike
    };

    public enum Misc
    {
        none,
        shadow,
        outline,
        outlineShadow,
        emboss,
        engrave
    };

    /// <summary>
    /// Change the caps style of text, for use with Append and AppendLine.
    /// </summary>
    public enum CapsStyle
    {
        /// <summary>
        /// No caps, make all characters are lowercase.
        /// </summary>
        none,

        /// <summary>
        /// All caps, make every character uppercase.
        /// </summary>
        caps,

        /// <summary>
        /// Small caps, make all characters capital but with a small font size.
        /// </summary>
        smallCaps
    };
}
