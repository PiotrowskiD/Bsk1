using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSK1.Services
{
    class Utils
    {

        public static String BytesToB64(byte[] input) {
            String output = Convert.ToBase64String(input);
            return output.Replace("/", "_");
        }

    }
}
