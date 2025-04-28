namespace SimpleAVREmulator
{
    public struct RegisterFile
    {
        const int RegistersCount = 32;
        private ArraySegment<byte> _data;

        public byte R0 { get => _data[0]; set => _data[0] = value; }
        public byte R1 { get => _data[1]; set => _data[1] = value; }
        public byte R2 { get => _data[2]; set => _data[2] = value; }
        public byte R3 { get => _data[3]; set => _data[3] = value; }
        public byte R4 { get => _data[4]; set => _data[4] = value; }
        public byte R5 { get => _data[5]; set => _data[5] = value; }
        public byte R6 { get => _data[6]; set => _data[6] = value; }
        public byte R7 { get => _data[7]; set => _data[7] = value; }
        public byte R8 { get => _data[8]; set => _data[8] = value; }
        public byte R9 { get => _data[9]; set => _data[9] = value; }
        public byte R10 { get => _data[10]; set => _data[10] = value; }
        public byte R11 { get => _data[11]; set => _data[11] = value; }
        public byte R12 { get => _data[12]; set => _data[12] = value; }
        public byte R13 { get => _data[13]; set => _data[13] = value; }
        public byte R14 { get => _data[14]; set => _data[14] = value; }
        public byte R15 { get => _data[15]; set => _data[15] = value; }
        public byte R16 { get => _data[16]; set => _data[16] = value; }
        public byte R17 { get => _data[17]; set => _data[17] = value; }
        public byte R18 { get => _data[18]; set => _data[18] = value; }
        public byte R19 { get => _data[19]; set => _data[19] = value; }
        public byte R20 { get => _data[20]; set => _data[20] = value; }
        public byte R21 { get => _data[21]; set => _data[21] = value; }
        public byte R22 { get => _data[22]; set => _data[22] = value; }
        public byte R23 { get => _data[23]; set => _data[23] = value; }
        public byte R24 { get => _data[24]; set => _data[24] = value; }
        public byte R25 { get => _data[25]; set => _data[25] = value; }
        public byte R26 { get => _data[26]; set => _data[26] = value; }
        public byte R27 { get => _data[27]; set => _data[27] = value; }
        public byte R28 { get => _data[28]; set => _data[28] = value; }
        public byte R29 { get => _data[29]; set => _data[29] = value; }
        public byte R30 { get => _data[30]; set => _data[30] = value; }
        public byte R31 { get => _data[31]; set => _data[31] = value; }

        // X=R27:R26
        public ushort X
        {
            get => (ushort)((_data[27] << 8) | _data[26]);
            set
            {
                _data[26] = (byte)(value & 0xFF);
                _data[27] = (byte)((value >> 8) & 0xFF);
            }
        }

        // Y=R29:R28
        public ushort Y
        {
            get => (ushort)((_data[29] << 8) | _data[28]);
            set
            {
                _data[28] = (byte)(value & 0xFF);
                _data[29] = (byte)((value >> 8) & 0xFF);
            }
        }

        // Z=R31:R30
        public ushort Z
        {
            get => (ushort)((_data[31] << 8) | _data[30]);
            set
            {
                _data[30] = (byte)(value & 0xFF);
                _data[31] = (byte)((value >> 8) & 0xFF);
            }
        }

        public RegisterFile(ArraySegment<byte> regs)
        {
            if (regs.Count != RegistersCount)
                throw new ArgumentException();

            _data = regs;
        }

        public void Reset()
        {
            for (int i = 0; i < _data.Count; i++)
            {
                _data[i] = 0;
            }
        }

        public byte this[int i]
        {
            get => _data[i];
            set => _data[i] = value;
        }
    }
}
