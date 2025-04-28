namespace SimpleAVREmulator
{
    public static class BitUtils
    {
        public static bool GetBit(this byte pByte, int bitNo)
        {
            return (pByte & 1 << bitNo) != 0;
        }

        public static bool GetBit(this ushort pByte, int bitNo)
        {
            return (pByte & 1 << bitNo) != 0;
        }

        public static ushort GetBitRange(this ushort pShort, int firstIndexInclusive, int secondIndexInclusive)
        {
            if (firstIndexInclusive > secondIndexInclusive)
                throw new ArgumentException();

            if (firstIndexInclusive < 0 || firstIndexInclusive >= 16)
                throw new ArgumentException();

            if (secondIndexInclusive < 0 || secondIndexInclusive >= 16)
                throw new ArgumentException();

            ushort ones = (ushort)((1 << (secondIndexInclusive - firstIndexInclusive + 1)) - 1);
            ushort res = (ushort)((pShort >> firstIndexInclusive) & ones);

            return res;
        }
    }
}
