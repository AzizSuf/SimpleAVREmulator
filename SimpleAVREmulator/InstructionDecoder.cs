using System.Diagnostics;

// The following are equivalent instructions (opcodes are equal).
// The right side shows the implementation:
//   BCLR == separate instructions for each flag in the SREG
//   CBR == ANDI Rd, ($FF - K)
//   CLR == XOR Rd, Rd
//   LSL == ADD, Rd, Rd
//   ROL == ADC Rd,Rd
//   SBR == ORI Rd, K
//   SER == LDI, Rd, 0xFF
//   TST == AND Rd, Rd

namespace SimpleAVREmulator
{
    public partial class CPU
    {
        public void DecodeAndExecute(ushort opcode)
        {
            // opcode == nb1:nb2:nb3:nb4
            byte nb1 = (byte)opcode.GetBitRange(12, 15);

            switch (nb1)
            {
                case 0b0000: FirstNibble_0000(opcode); return;
                case 0b0001: FirstNibble_0001(opcode); return;
                case 0b0010: FirstNibble_0010(opcode); return;
                case 0b0011: FirstNibble_0011(opcode); return;
                case 0b0100: FirstNibble_0100(opcode); return;
                case 0b0101: FirstNibble_0101(opcode); return;
                case 0b0110: FirstNibble_0110(opcode); return;
                case 0b0111: FirstNibble_0111(opcode); return;
                case 0b1000: FirstNibble_1000(opcode); return;
                case 0b1001: FirstNibble_1001(opcode); return;
                case 0b1010: FirstNibble_1010(opcode); return;
                case 0b1011: FirstNibble_1011(opcode); return;
                case 0b1100: FirstNibble_1100(opcode); return;
                case 0b1101: FirstNibble_1101(opcode); return;
                case 0b1110: FirstNibble_1110(opcode); return;
                case 0b1111: FirstNibble_1111(opcode); return;
            }
        }

        /// <summary>
        /// 0000 0000 0000 0000  NOP
        /// 0000 01rd dddd rrrr  CPC Rd,Rr
        /// 0000 10rd dddd rrrr  SBC Rd,Rr
        /// 0000 11rd dddd rrrr  ADD Rd,Rr
        /// 
        /// 0000 0001 dddd rrrr  MOVW Rd+1:Rd,Rr+1:Rr
        /// 0000 0010 dddd rrrr  MULS – Multiply Signed
        /// 
        /// 0000 0011 0ddd 0rrr  MULSU – Multiply Signed with Unsigned
        /// 0000 0011 0ddd 1rrr  FMUL – Fractional Multiply Unsigned
        /// 0000 0011 1ddd 0rrr  FMULS – Fractional Multiply Signed
        /// 0000 0011 1ddd 1rrr  FMULSU – Fractional Multiply Signed with Unsigned
        /// </summary>
        private void FirstNibble_0000(ushort opcode)
        {
            if (opcode == 0x0000)
            {
                NOP(); return;
            }

            // todo rename
            // Multiply ins
            ushort co = (ushort)((opcode.GetBitRange(7, 7) << 1) | opcode.GetBitRange(3, 3));
            byte r = (byte)opcode.GetBitRange(0, 2);
            byte d = (byte)opcode.GetBitRange(4, 6);

            ushort nb2 = opcode.GetBitRange(8, 11);
            switch (nb2)
            {
                case 0b0001: MOVW((byte)(opcode.GetBitRange(4, 7) * 2), (byte)(opcode.GetBitRange(0, 3) * 2)); return;
                case 0b0010: MULS(d, r); return;
                case 0b0011:
                    switch (co)
                    {
                        case 0b00: MULSU(d, r); return;
                        case 0b01: FMUL(d, r); return;
                        case 0b10: FMULS(d, r); return;
                        case 0b11: FMULSU(d, r); return;
                    }
                    break;
            }

            ushort insType = opcode.GetBitRange(10, 11);
            var (Rd, Rr) = ExtractTwoRegisters(opcode);
            switch (insType)
            {
                case 0b01: CPC(Rd, Rr); return;
                case 0b10: SBC(Rd, Rr); return;
                case 0b11: ADD(Rd, Rr); return;
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// 0001 00rd dddd rrrr  CPSE Rd,Rr
        /// 0001 01rd dddd rrrr  CP Rd,Rr
        /// 0001 10rd dddd rrrr  SUB Rd,Rr
        /// 0001 11rd dddd rrrr  ADC Rd,Rr
        /// </summary>
        private void FirstNibble_0001(ushort opcode)
        {
            byte insType = (byte)opcode.GetBitRange(10, 11);

            var (Rd, Rr) = ExtractTwoRegisters(opcode);
            switch (insType)
            {
                case 0b00: CPSE(Rd, Rr); return;
                case 0b01: CP(Rd, Rr); return;
                case 0b10: SUB(Rd, Rr); return;
                case 0b11: ADC(Rd, Rr); return;
            }
        }

        /// <summary>
        /// 0010 00rd dddd rrrr  AND Rd,Rr
        /// 0010 01rd dddd rrrr  EOR Rd,Rr
        /// 0010 10rd dddd rrrr  OR Rd,Rr
        /// 0010 11rd dddd rrrr  MOV Rd,Rr
        /// </summary>
        private void FirstNibble_0010(ushort opcode)
        {
            byte insType = (byte)opcode.GetBitRange(10, 11);

            var (Rd, Rr) = ExtractTwoRegisters(opcode);
            switch (insType)
            {
                case 0b00: AND(Rd, Rr); return;
                case 0b01: EOR(Rd, Rr); return;
                case 0b10: OR(Rd, Rr); return;
                case 0b11: MOV(Rd, Rr); return;
            }
        }

        /// <summary>
        /// 0011 KKKK dddd KKKK  CPI Rd,K  16<=d<=31
        /// </summary>
        private void FirstNibble_0011(ushort opcode)
        {
            var (Rd, imm) = ExtractRegisterAndImmediate(opcode);
            CPI(Rd, imm);
        }

        /// <summary>
        /// 0100 KKKK dddd KKKK  SBCI Rd,K      d=16..31
        /// </summary>
        private void FirstNibble_0100(ushort opcode)
        {
            var (Rd, imm) = ExtractRegisterAndImmediate(opcode);
            SBCI(Rd, imm);
        }

        /// <summary>
        /// 0101 KKKK dddd KKKK  SUBI Rd,K
        /// </summary>
        private void FirstNibble_0101(ushort opcode)
        {
            var (Rd, imm) = ExtractRegisterAndImmediate(opcode);
            SUBI(Rd, imm);
        }

        /// <summary>
        /// 0110 KKKK dddd KKKK  ORI Rd,K
        /// </summary>
        private void FirstNibble_0110(ushort opcode)
        {
            var (Rd, imm) = ExtractRegisterAndImmediate(opcode);
            ORI(Rd, imm);
        }

        /// <summary>
        /// 0111 KKKK dddd KKKK  ANDI Rd,K         16<=d<=31
        /// </summary>
        private void FirstNibble_0111(ushort opcode)
        {
            var (Rd, imm) = ExtractRegisterAndImmediate(opcode);
            ANDI(Rd, imm);
        }

        /// <summary>
        ///          ***_      ****  
        /// (i) 1000 001r rrrr 1000  ST Y, Rr
        /// (i) 1000 001r rrrr 0000  ST Z, Rr
        /// (i) 1000 000d dddd 1000  LD Rd, Y
        /// (i) 1000 000d dddd 0000  LD Rd, Z
        /// </summary>
        private void FirstNibble_1000(ushort opcode)
        {
            byte nb2_first_3bit = (byte)opcode.GetBitRange(9, 11);
            byte reg = (byte)opcode.GetBitRange(4, 8);

            byte nb4 = (byte)opcode.GetBitRange(0, 3);

            switch (nb2_first_3bit)
            {
                case 0b000:
                    switch (nb4)
                    {
                        case 0b0000: LD_Z(reg); break;
                        case 0b1000: LD_Y(reg); break;
                    }
                    break;

                case 0b001:
                    switch (nb4)
                    {
                        case 0b0000: ST_Z(reg); break;
                        case 0b1000: ST_Y(reg); break;
                    }
                    break;
            }
        }

        /// <summary>
        /// For every SREG flag (set or clear):
        ///           _***
        /// 1001 0100 0000 1000  SEC – Set Carry Flag
        /// 1001 0100 1000 1000  CLC – Clear Carry Flag
        /// 1001 0100 0001 1000  SEZ – Set Zero Flag
        /// 1001 0100 1001 1000  CLZ – Clear Zero Flag
        /// 1001 0100 0010 1000  SEN – Set Negative Flag
        /// 1001 0100 1010 1000  CLN – Clear Negative Flag
        /// 1001 0100 0011 1000  SEV – Set Overflow Flag
        /// 1001 0100 1011 1000  CLV – Clear Overflow Flag
        /// 1001 0100 0100 1000  SES – Set Signed Flag
        /// 1001 0100 1100 1000  CLS – Clear Signed Flag
        /// 1001 0100 0101 1000  SEH – Set Half Carry Flag
        /// 1001 0100 1101 1000  CLH – Clear Half Carry Flag
        /// 1001 0100 0110 1000  SET – Set T Flag
        /// 1001 0100 1110 1000  CLT – Clear T Flag
        /// 1001 0100 0111 1000  SEI – Set Global Interrupt Flag
        /// 1001 0100 1111 1000  CLI – Clear Global Interrupt Flag
        /// 1001 0100 1sss 1000  BCLR s – Bit Clear in SREG
        /// 1001 0100 0sss 1000  BSET s – Bit Set in SREG
        /// 
        /// 1001 010d dddd 0101  ASR d  – Arithmetic Shift Right
        /// 
        /// 1001 010k kkkk 111k  CALL k – Long Call to a Subroutine
        /// kkkk kkkk kkkk kkkk
        ///
        /// 1001 1000 AAAA Abbb  CBI A, b – Clear Bit in I/O Register
        /// 1001 010d dddd 0000  COM – One’s Complement
        /// 1001 010d dddd 1010  DEC – Decrement
        /// 1001 0100 KKKK 1011  DES – Data Encryption Standard
        /// 1001 0101 0001 1001  EICALL – Extended Indirect Call to Subroutine
        /// 1001 0100 0001 1001  EIJMP – Extended Indirect Jump
        /// 1001 0101 0000 1001  ICALL – Indirect Call to Subroutine
        /// 1001 0100 0000 1001  IJMP – Indirect Jump
        /// 1001 010d dddd 0011  INC – Increment
        ///
        /// 1001 010k kkkk 110k  JMP k – Jump
        /// kkkk kkkk kkkk kkkk
        ///
        /// 1001 001r rrrr 0110  LAC – Load and Clear
        /// 1001 001r rrrr 0101  LAS – Load and Set
        /// 1001 001r rrrr 0111  LAT – Load and Toggle
        ///
        /// 32 Bit Instructions:
        /// 1001 000d dddd 0000  LDS – Load Direct from Data Space
        /// kkkk kkkk kkkk kkkk
        /// 1001 001d dddd 0000  STS – Store Direct to Data Space
        /// kkkk kkkk kkkk kkkk
        /// 
        /// ST – Store Indirect From Register to Data Space using Index X
        /// (i)   1001 001r rrrr 1100  (X) ← Rr X: Unchanged
        /// (ii)  1001 001r rrrr 1101  (X) ← Rr, X ← X+1 X: Post incremented
        /// (iii) 1001 001r rrrr 1110   X ← X - 1, (X) ← Rr X: Pre decremented
        /// 
        /// LD – Load Indirect from Data Space to Register using Index X
        /// (i)   1001 000d dddd 1100   Rd ← (X) X: Unchanged
        /// (ii)  1001 000d dddd 1101   Rd ← (X) X ← X + 1 X: Post incremented
        /// (iii) 1001 000d dddd 1110   X ← X - 1 Rd ← (X) X: Pre decremented
        /// 
        /// LPM – Load Program Memory
        /// (i)   1001 0101 1100 1000  LPM - R0 ← (Z), R0 implied
        /// (ii)  1001 000d dddd 0100  LPM Rd ← (Z)              
        /// (iii) 1001 000d dddd 0101  LPM  Rd ← (Z) Z ← Z + 1   
        /// 
        /// SPM – Store Program Memory
        /// TODO:
        /// 
        /// 1001 010d dddd 0110  LSR – Logical Shift Right
        /// 
        /// 1001 11rd dddd rrrr  MUL – Multiply Unsigned
        /// 1001 010d dddd 0001  NEG – Two’s Complement
        /// 
        /// 1001 000d dddd 1111  POP – Pop Register from Stack
        /// 1001 001d dddd 1111  PUSH – Push Register on Stack
        ///
        /// 1001 0101 0000 1000  RET – Return from Subroutine
        /// 1001 0101 0001 1000  RETI – Return from Interrupt
        /// 
        /// 1001 1001 AAAA Abbb  SBIC – Skip if Bit in I/O Register is Cleared
        /// 1001 1011 AAAA Abbb  SBIS – Skip if Bit in I/O Register is Set
        /// 
        /// 1001 0110 KKdd KKKK  ADIW – Add Immediate to Word
        /// 1001 010d dddd 0010  SWAP – Swap Nibbles
        /// 1001 0100 KKKK 1011  DES – Data Encryption Standard
        /// 
        /// 1001 0101 1001 1000  BREAK – Break
        /// 1001 0101 1000 1000  SLEEP
        /// 1001 0101 1010 1000  WDR - Watchdog Reset
        /// 
        // ELPM TODO
        /// </summary>
        private void FirstNibble_1001(ushort opcode)
        {
            byte clearOrSet = (byte)opcode.GetBitRange(7, 7);
            byte branchType = (byte)opcode.GetBitRange(4, 6);
            byte secondNibble = (byte)opcode.GetBitRange(8, 11);

            // opcode == nb1:nb2:nb3:nb4

            byte nb2_first_2bit = (byte)opcode.GetBitRange(10, 11);
            if (nb2_first_2bit == 0b11)
            {
                var (Rd, Rr) = ExtractTwoRegisters(opcode);
                MUL(Rd, Rr);
                return;
            }

            byte nb4 = (byte)opcode.GetBitRange(0, 3);
            byte nb2 = (byte)opcode.GetBitRange(8, 11);

            {
                byte nb2_first_3bit = (byte)opcode.GetBitRange(9, 11);
                byte reg = (byte)opcode.GetBitRange(4, 8);

                if (nb4 == 0b1111)
                {
                    switch (nb2_first_3bit)
                    {
                        case 0b000: POP(reg); return;
                        case 0b001: PUSH(reg); return;
                    }
                }

                if (nb2 == 0b0100 && nb4 == 0b1011)
                {
                    DES((byte)opcode.GetBitRange(4, 7)); return;
                }

                switch (nb2_first_3bit)
                {
                    case 0b000:
                        switch (nb4)
                        {
                            case 0b0100: LPM_Reg(reg); return;
                            case 0b0101: LPM_Reg_ZPostIncrement(reg); return;

                            case 0b0000: LDS(reg); return;
                            case 0b1100: LD_X(reg); return;
                            case 0b1101: LD_X_PostIncrement(reg); return;
                            case 0b1110: LD_X_PreDecrement(reg); return;
                            case 0b1001: LD_Y_PostIncrement(reg); return;
                            case 0b1010: LD_Y_PreDecrement(reg); return;
                            case 0b0001: LD_Z_PostIncrement(reg); return;
                            case 0b0010: LD_Z_PreDecrement(reg); return;
                        }
                        break;

                    case 0b001:
                        switch (nb4)
                        {
                            case 0b0110: LAC(reg); return;
                            case 0b0101: LAS(reg); return;
                            case 0b0111: LAT(reg); return;
                            case 0b0000: STS(reg); return;

                            case 0b1100: ST_X(reg); return;
                            case 0b1101: ST_X_PostIncrement(reg); return;
                            case 0b1110: ST_X_PreDecrement(reg); return;

                            case 0b1001: ST_Y_PostIncrement(reg); return;
                            case 0b1010: ST_Y_PreDecrement(reg); return;

                            case 0b0001: ST_Z_PostIncrement(reg); return;

                            case 0b0100: XCH(reg); return;

                                // TOOD:
                                // STD
                                //case: 0b
                        }
                        break;

                    case 0b010:
                        switch (nb4)
                        {
                            case 0b0000: COM(reg); return;
                            case 0b0001: NEG(reg); return;
                            case 0b0010: SWAP(reg); return;
                            case 0b0011: INC(reg); return;
                            case 0b0110: LSR(reg); return;
                            case 0b0101: ASR(reg); return;
                            case 0b1010: DEC(reg); return;

                            // 0b111x, x - unspecified 
                            case byte n when (n >> 1) == 0b111:
                                {
                                    //1001 010k kkkk 111k - CALL
                                    byte k = (byte)(opcode.GetBitRange(4, 8) | opcode.GetBitRange(0, 0));
                                    CALL(k); return;
                                }

                            // 1001 010k kkkk 110k JMP k – Jump
                            case byte n when (n >> 1) == 0b110:
                                {
                                    byte k = (byte)(opcode.GetBitRange(4, 8) | opcode.GetBitRange(0, 0));
                                    JMP(k); return;
                                }

                                //case 0b0111: 
                                // TODO: ROR
                        }
                        break;
                }
            }

            // Specific instructions
            switch (opcode)
            {
                case 0b1001_0101_0000_1000: RET(); return;
                case 0b1001_0101_0001_1000: RETI(); return;
                case 0b1001_0101_1001_1000: BREAK(); return;
                case 0b1001_0101_0001_1001: EICALL(); return;
                case 0b1001_0100_0001_1001: EIJMP(); return;
                case 0b1001_0101_0000_1001: ICALL(); return;
                case 0b1001_0100_0000_1001: IJMP(); return;
                case 0b1001_0101_1100_1000: LPM(); return;
                case 0b1001_0101_1000_1000: SLEEP(); return;
                case 0b1001_0101_1010_1000: WDR(); return;
            }

            switch (secondNibble)
            {
                case 0b0100: SREGModifyInstructions(); return;

                case 0b1000: CBI((byte)opcode.GetBitRange(3, 7), (byte)opcode.GetBitRange(0, 2)); return;
                case 0b1010: SBI((byte)opcode.GetBitRange(3, 7), (byte)opcode.GetBitRange(0, 2)); return;

                case 0b1001: SBIC((byte)opcode.GetBitRange(3, 7), (byte)opcode.GetBitRange(0, 2)); return;
                case 0b1011: SBIS((byte)opcode.GetBitRange(3, 7), (byte)opcode.GetBitRange(0, 2)); return;

                // check for 011
                case byte nb when (nb >> 1) == 0b011:
                    {
                        byte twoBitValue = (byte)opcode.GetBitRange(4, 5);

                        // The two bit value from the opcode corresponds to registers R24, R26, R28, R30
                        byte reg = (byte)(24 + 2 * twoBitValue);

                        byte K = (byte)((opcode.GetBitRange(6, 7) << 4) | opcode.GetBitRange(0, 3));

                        switch (nb & 1)
                        {
                            case 0: ADIW(reg, K); return;
                            case 1: SBIW(reg, K); return;
                        }
                        break;
                    }

                default:
                    {
                        //return new Instructions.InvalidInstruction(opcode);
                        throw new NotImplementedException($"{secondNibble}");
                    }
            }

            // Processes instructions that set or clear specific flags in the SREG(Status Register).
            void SREGModifyInstructions()
            {
                switch (clearOrSet)
                {
                    // If bit setted
                    case 0b0:
                        switch (branchType)
                        {
                            case 0b000: SEC(); return;
                            case 0b001: SEZ(); return;
                            case 0b010: SEN(); return;
                            case 0b011: SEV(); return;
                            case 0b100: SES(); return;
                            case 0b101: SEH(); return;
                            case 0b110: SET(); return;
                            case 0b111: SEI(); return;
                        }
                        break;

                    // If bit cleared
                    case 0b1:
                        switch (branchType)
                        {
                            case 0b000: CLC(); return;
                            case 0b001: CLZ(); return;
                            case 0b010: CLN(); return;
                            case 0b011: CLV(); return;
                            case 0b100: CLS(); return;
                            case 0b101: CLH(); return;
                            case 0b110: CLT(); return;
                            case 0b111: CLI(); return;
                        }
                        break;

                    default: throw new UnreachableException();
                }

                throw new NotImplementedException();
            }
            ;

            // TODO
            throw new NotImplementedException();
        }

        /// <summary>
        ///      _
        /// 1010 0kkk dddd kkkk  LDS (16-bit) – Load Direct from Data Space
        /// </summary>
        private void FirstNibble_1010(ushort opcode)
        {
            if (opcode.GetBitRange(11, 11) == 0)
            {
                // A 7 - bit address must be supplied. The address given in the instruction is coded to a data space address as follows:
                // ADDR[7:0] = (~INST[8], INST[8], INST[10], INST[9], INST[3], INST[2], INST[1], INST[0])
                ushort bits_0_3 = opcode.GetBitRange(0, 3);
                ushort bits_9_10 = opcode.GetBitRange(9, 10);
                ushort bit_8 = opcode.GetBitRange(8, 8);

                byte addr = (byte)((~bit_8 << 6) | (bit_8 << 6) | (bits_9_10 << 4) | bits_0_3);

                LDS_16((byte)opcode.GetBitRange(4, 7), addr); return;
            }
            throw new UnreachableException();
        }

        /// <summary>
        /// 1011 0AAd dddd AAAA  IN Rd,A
        /// 1011 1AAr rrrr AAAA  OUT A,Rr
        /// </summary>
        private void FirstNibble_1011(ushort opcode)
        {
            byte Alow = (byte)opcode.GetBitRange(0, 3);
            byte Ahigh = (byte)opcode.GetBitRange(9, 10);
            byte A = (byte)((Ahigh << 5) | Alow);

            byte reg = (byte)opcode.GetBitRange(4, 8);

            if (opcode.GetBit(11) == false)
            {
                IN(reg, A); return;
            }
            else
            {
                OUT(reg, A); return;
            }
        }

        /// <summary>
        /// 1100 kkkk kkkk kkkk RJMP k
        /// </summary>
        private void FirstNibble_1100(ushort opcode)
        {
            unchecked // todo
            {
                ushort k = (ushort)(opcode & 0x0FFF);
                RJMP((sbyte)k); // TODO
            }
        }

        /// <summary>
        /// 1101 kkkk kkkk kkkk  RCALL k
        /// </summary>
        private void FirstNibble_1101(ushort opcode)
        {
            ushort k = opcode.GetBitRange(0, 11);
            RCALL((sbyte)k);
        }

        /// <summary>
        /// 1110 KKKK dddd KKKK  LDI Rd,K d = 16..31
        /// 1110 1111 dddd 1111  SER – Set all Bits in Register
        /// </summary>
        private void FirstNibble_1110(ushort opcode)
        {
            var (Rd, imm) = ExtractRegisterAndImmediate(opcode);
            LDI(Rd, imm);
        }

        /// <summary>
        /// For every SREG flag (set or clear):
        /// 1111 00kk kkkk k000  BRCS k - Branch if Carry Set
        /// 1111 01kk kkkk k000  BRCC k - Branch if Carry Cleared | BRSH - Branch if Same or Higher (Unsigned)
        /// 1111 00kk kkkk k001  BREQ k - Branch if Equal
        /// 1111 01kk kkkk k001  BRNE k - Branch if Not Equal
        /// 1111 00kk kkkk k010  BRMI k – Branch if Minus
        /// 1111 01kk kkkk k010  BRPL k – Branch if Plus
        /// 1111 00kk kkkk k011  BRVS k – Branch if Overflow Set
        /// 1111 01kk kkkk k011  BRVC k – Branch if Overflow Cleared
        /// 1111 00kk kkkk k100  BRLT k – Branch if Less Than (Signed)
        /// 1111 01kk kkkk k100  BRGE k – Branch if Greater or Equal (Signed)
        /// 1111 00kk kkkk k101  BRHS k – Branch if Half Carry Flag is Set
        /// 1111 01kk kkkk k101  BRHC k – Branch if Half Carry Flag is Cleared
        /// 1111 00kk kkkk k110  BRTS k – Branch if the T Flag is Set
        /// 1111 01kk kkkk k110  BRTC k – Branch if the T Flag is Cleared
        /// 1111 00kk kkkk k111  BRIE k – Branch if Global Interrupt is Enabled
        /// 1111 01kk kkkk k111  BRID k – Branch if Global Interrupt is Disabled
        ///
        /// 1111 100d dddd 0bbb  BLD – Bit Load from the T Flag in SREG to a Bit in Register
        /// 1111 101d dddd 0bbb  BST – Bit Store from Bit in Register to T Flag in SREG
        /// 
        /// 1111 111r rrrr 0bbb  SBRS – Skip if Bit in Register is Set
        /// 1111 110r rrrr 0bbb  SBRC – Skip if Bit in Register is Cleared
        /// </summary>
        private void FirstNibble_1111(ushort opcode)
        {
            #region Conditional branch instructions
            // Jump displasment -64:+63
            ushort disp = opcode.GetBitRange(3, 9);

            // Signed 7-bit value
            // The 6th bit (starting from zero) is the sign bit, extend the sign to 8 bits
            byte k_2compliment = (disp.GetBit(6)) ? (byte)(disp | 0x80) : (byte)disp;

            sbyte k;
            unchecked
            {
                k = (sbyte)(k_2compliment & 0xFF);
            }

            byte clearedOrSet = (byte)opcode.GetBitRange(10, 11);
            byte branchType = (byte)opcode.GetBitRange(0, 2);

            switch (clearedOrSet) // TODO
            {
                // If bit setted
                case 0b00:
                    switch (branchType)
                    {
                        case 0b000: BRCS(k); return;
                        case 0b001: BREQ(k); return;
                        case 0b010: BRMI(k); return;
                        case 0b011: BRVS(k); return;
                        case 0b100: BRLT(k); return;
                        case 0b101: BRHS(k); return;
                        case 0b110: BRTS(k); return;
                        case 0b111: BRIE(k); return;
                    }
                    break;

                // If bit cleared
                case 0b01:
                    switch (branchType)
                    {
                        case 0b000: BRCC(k); return;
                        case 0b001: BRNE(k); return;
                        case 0b010: BRPL(k); return;
                        case 0b011: BRVC(k); return;
                        case 0b100: BRGE(k); return;
                        case 0b101: BRHC(k); return;
                        case 0b110: BRTC(k); return;
                        case 0b111: BRID(k); return;
                    }
                    break;
            }
            #endregion

            byte storeOrLoad = (byte)opcode.GetBitRange(9, 9);
            byte reg = (byte)opcode.GetBitRange(4, 8);
            byte bitNumber = (byte)opcode.GetBitRange(0, 2);

            if (opcode.GetBitRange(3, 3) == 0)
            {
                switch (clearedOrSet)
                {
                    case 0b10:
                        switch (storeOrLoad)
                        {
                            case 0: BLD(reg, bitNumber); return;
                            case 1: BST(reg, bitNumber); return;
                        }
                        break;

                    case 0b11:
                        switch (storeOrLoad)
                        {
                            case 0: SBRC(reg, bitNumber); return;
                            case 1: SBRS(reg, bitNumber); return;
                        }
                        break;
                }
            }

            throw new UnreachableException();
        }

        /// <summary>
        /// xxxx xxrd dddd rrrr
        /// x: Operation
        /// d: Destination register
        /// r: Source register
        /// </summary>
        private static (byte Rd, byte Rr) ExtractTwoRegisters(ushort opcode)
        {
            byte d = (byte)((opcode >> 4) & 0b11111);
            byte r = (byte)(((opcode >> 5) & (1 << 4)) | (opcode & 0x0F));
            return (d, r);
        }

        /// <summary>
        /// xxxx KKKK dddd KKKK
        /// x: Operation
        /// d: Destination register
        /// K: Constant data
        /// </summary>
        private static (byte Rd, byte imm) ExtractRegisterAndImmediate(ushort opcode)
        {
            byte Rd = (byte)(((opcode >> 4) & 0x0F) + 16);
            byte imm = (byte)(((opcode >> 4) & (0b1111 << 4)) | (opcode & 0x0F));
            return (Rd, imm);
        }

        // Firs words of 32-bit instructions
        // 1001 010 'k kkkk' 11'1k'  CALL – Long Call to a Subroutine
        // 1001 010 'k kkkk' 11'0k'  JMP – Jump

        // 1001 00 '0ddddd' 0000  LDS – Load Direct from Data Space
        // 1001 00 '1ddddd' 0000  STS – Store Direct to Data Space
        private static bool Is32BitInstruction(ushort opcode)
        {

            return (opcode & 0xFC0F) == 0x9000 || // LDS, STS
                   (opcode & 0xFE0C) == 0x940C;   // CALL, JMP

            /// из simuide
            //uint16_t o = m_progMem[pc] & 0xfc0f;
            //return  o == 0x9200 || // STS ! Store Direct to Data Space
            //        o == 0x9000 || // LDS Load Direct from Data Space
            //        o == 0x940c || // JMP Long Jump
            //        o == 0x940d || // JMP Long Jump
            //        o == 0x940e || // CALL Long Call to sub
            //        o == 0x940f;   // CALL Long Call to sub

        }
    }
}
