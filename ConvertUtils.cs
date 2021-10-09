using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace project
{
    class ConvertUtils
    {
        public static string byteToHexstring(byte b)
        {
            return string.Format("{0:x2}", b);
        }

        public static byte Int2ToByte(int i)// <=255
        {
            return BitConverter.GetBytes(i)[0];
        }

        public static int ByteToInt(byte b)
        {
            return (int)b;
        }

        public static byte[] hexStringToByteArray(string s)
        {
            byte[] convertArr = new byte[s.Length / 2];
            for (int i = 0; i < convertArr.Length; i++)
            {
                convertArr[i] = Convert.ToByte(s.Substring(i * 2, 2), 16);
            }
            return convertArr;
        }

        public static float byteArrayToFloat_bigEndian(byte[] byteArray)
        {
            Array.Reverse(byteArray);
            return BitConverter.ToSingle(byteArray, 0);
        }

        public static byte[] FloatToByteArray(float f)
        {
            return BitConverter.GetBytes(f);
        }

        public static string byteArrayToString(byte[] byteArray, int arrayLength)
        {
            string str = string.Empty;
            for (int i = 0; i < arrayLength; i++)
            {
                str += byteToHexstring(byteArray[i]);
            }
            return str;
        }

        public static UInt32 ByteArrayToUInt32_bigEndian(byte[] byteArray)
        {
            Array.Reverse(byteArray);
            return BitConverter.ToUInt32(byteArray, 0);
        }

        public static UInt16 ByteArrayToUInt16_bigEndian(byte[] byteArray)
        {
            Array.Reverse(byteArray);
            return BitConverter.ToUInt16(byteArray, 0);
        }

        public static byte[] Uint32ToByteArray(UInt32 ui)
        {
            byte[] barray = BitConverter.GetBytes(ui);
            Array.Reverse(barray);
            return barray;
        }

        public static byte[] Uint16ToByteArray(UInt16 ui)
        {
            byte[] barray = BitConverter.GetBytes(ui);
            Array.Reverse(barray);
            return barray;
        }
    }
}
