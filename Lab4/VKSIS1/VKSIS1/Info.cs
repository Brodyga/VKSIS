using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VKSIS1
{
    static class Info
    {
        public static int MachineNumber { get; set; }
        public static int MachineNumberFromSend { get; set; }
        public static int MachineNumberToSend { get; set; }

        public static String Data { get; set; }

        public static bool Error { get; set; }
        public static bool Transfer { get; set; }
        public static bool ErrorSndRcv { get; set; }
        public static bool ErrorData { get; set; }
    }
}
