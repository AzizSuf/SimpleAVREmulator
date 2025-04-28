namespace SimpleAVREmulator
{
    public struct StatusRegister
    {
        /// <summary>Carry Flag</summary>
        public bool C;

        /// <summary>Zero Flag</summary>
        public bool Z;

        /// <summary>Negative Flag</summary>
        public bool N;

        /// <summary>Two’s Complement Overflow Flag</summary>
        public bool V;

        /// <summary>Sign Bit, S = N ⊕ V</summary>
        public bool S;

        /// <summary>Half Carry Flag</summary>
        public bool H;

        /// <summary>Bit Copy Storage</summary>
        public bool T;

        /// <summary>Global Interrupt Enable</summary>
        public bool I;
    }
}
