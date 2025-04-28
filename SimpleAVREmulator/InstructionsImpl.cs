using System.Diagnostics;

namespace SimpleAVREmulator
{
    // Execution of instructions
    public partial class CPU
    {
        void NOP()
        {
            PC++;
        }

        void LDI(byte Rd, byte imm)
        {
            regs[Rd] = imm;
            PC++;
        }


        #region Arithmetic instructions
        void ADDApplySREG(byte RdVal, byte RrVal, byte RVal)
        {
            bool Rd7 = RdVal.GetBit(7);
            bool Rr7 = RrVal.GetBit(7);
            bool R7 = RVal.GetBit(7);
            bool Rd3 = RdVal.GetBit(3);
            bool Rr3 = RrVal.GetBit(3);
            bool R3 = RVal.GetBit(3);

            SREG.C = Rd7 & Rr7 | Rr7 & !R7 | !R7 & Rd7;
            SREG.Z = RVal == 0;
            SREG.N = RdVal.GetBit(7);
            SREG.V = Rd7 & Rr7 & R7 | !Rd7 & !Rr7 & R7;
            SREG.S = SREG.N ^ SREG.V;
            SREG.H = Rd3 & Rr3 | Rr3 & !R3 | !R3 & Rd3;
        }

        void SUBApplySREG(byte RdVal, byte RrVal, byte RVal)
        {
            bool Rd7 = RdVal.GetBit(7);
            bool Rr7 = RrVal.GetBit(7);
            bool R7 = RVal.GetBit(7);
            bool Rd3 = RdVal.GetBit(3);
            bool Rr3 = RrVal.GetBit(3);
            bool R3 = RVal.GetBit(3);

            SREG.C = !Rd7 & Rr7 | Rr7 & R7 | R7 & !Rd7;
            SREG.Z = RVal == 0;
            SREG.N = R7;
            SREG.V = Rd7 & !Rr7 & !R7 | !Rd7 & Rr7 & R7;
            SREG.S = SREG.N ^ SREG.V;
            SREG.H = !Rd3 & Rr3 | Rr3 & R3 | R3 & !Rd3;
        }

        void ADD(byte Rd, byte Rr)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[Rd] + regs[Rr]);
            }
            ADDApplySREG(regs[Rd], regs[Rr], res);
            regs[Rd] = res;
            PC++;
        }

        void ADC(byte Rd, byte Rr)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[Rd] + regs[Rr] + Convert.ToByte(SREG.C));
            }
            ADDApplySREG(regs[Rd], regs[Rr], res);
            regs[Rd] = res;
            PC++;
        }

        void SUB(byte Rd, byte Rr)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[Rd] - regs[Rr]);
            }
            SUBApplySREG(regs[Rd], regs[Rr], res);
            regs[Rd] = res;
            PC++;
        }

        void SBC(byte Rd, byte Rr)
        {
            throw new NotImplementedException();
            PC++;
        }

        void SUBI(byte r, byte K)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[r] - K);
            }
            SUBApplySREG(regs[r], K, res);

            regs[r] = res;

            PC++;
        }

        void SBCI(byte Rd, byte K)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[Rd] - K - Convert.ToByte(SREG.C));
            }

            bool Rd3 = regs[Rd].GetBit(3);
            bool K3 = K.GetBit(3);
            bool R3 = res.GetBit(3);

            bool Rd7 = regs[Rd].GetBit(7);
            bool K7 = K.GetBit(7);
            bool R7 = res.GetBit(7);

            // TODO
            SREG.H = !Rd3 & K3 | K3 & R3 | R3 & !Rd3;
            SREG.N = res.GetBit(7);
            SREG.Z = res == 0 & SREG.Z;   // ??
            SREG.V = Rd7 & !K7 & !R7 | !Rd7 & K7 & R7;
            SREG.S = SREG.N ^ SREG.V;
            SREG.C = !Rd7 & K7 | K7 & R7 | R7 & !Rd7;

            //throw new NotImplementedException();

            PC++;
        }

        void ADIW(byte Rr, byte K)
        {
            Debug.Assert(0 <= K && K <= 63);
            Debug.Assert(Rr == 24 || Rr == 26 || Rr == 28 || Rr == 30);

            ushort val = (ushort)((regs[Rr + 1] << 8) | regs[Rr]);
            val += K;

            bool R15 = val.GetBit(15);
            bool Rdh7 = regs[Rr + 1].GetBit(7);

            SREG.N = R15;
            SREG.Z = val == 0;
            SREG.V = R15 & !Rdh7;
            SREG.S = SREG.N ^ SREG.V;
            SREG.C = !R15 & Rdh7;

            regs[Rr] = (byte)(val & 0xFF);
            regs[Rr + 1] = (byte)(val >> 8);

            PC++;
        }

        void SBIW(byte Rr, byte K)
        {
            Debug.Assert(0 <= K && K <= 63);
            Debug.Assert(Rr == 24 || Rr == 26 || Rr == 28 || Rr == 30);

            ushort val = (ushort)((regs[Rr + 1] << 8) | regs[Rr]);
            val -= K;

            bool R15 = val.GetBit(15);
            bool Rdh7 = regs[Rr + 1].GetBit(7);

            SREG.N = R15;
            SREG.Z = val == 0;
            SREG.V = R15 & !Rdh7;
            SREG.S = SREG.N ^ SREG.V;
            SREG.C = !R15 & Rdh7;

            regs[Rr] = (byte)(val & 0xFF);
            regs[Rr + 1] = (byte)(val >> 8);

            PC++;
        }

        void INC(byte Rd)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[Rd] + 1);
            }

            SREG.Z = res == 0;
            SREG.N = res.GetBit(7);
            SREG.V = regs[Rd] == 0x7F;
            SREG.S = SREG.N ^ SREG.V;

            regs[Rd] = res;
            PC++;
        }

        void DEC(byte Rd)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[Rd] - 1);
            }

            SREG.Z = res == 0;
            SREG.N = res.GetBit(7);
            SREG.V = regs[Rd] == 0x80;
            SREG.S = SREG.N ^ SREG.V;

            regs[Rd] = res;
            PC++;
        }

        void COM(byte Rr)
        {
            throw new NotImplementedException();
        }

        void NEG(byte Rd)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Compare instructions
        void CP(byte Rd, byte Rr)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[Rd] - regs[Rr]);
            }
            // None of the registers are changed.
            // Only SREG modification is performed!
            SUBApplySREG(regs[Rd], regs[Rr], res);
            PC++;
        }

        void CPC(byte Rd, byte Rr)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[Rd] - regs[Rr] - Convert.ToByte(SREG.C));
            }

            // None of general purpuse registers are changed.
            // Only SREG modification is performed!

            bool Rd7 = regs[Rd].GetBit(7);
            bool Rr7 = regs[Rr].GetBit(7);
            bool R7 = res.GetBit(7);
            bool Rd3 = regs[Rd].GetBit(3);
            bool Rr3 = regs[Rr].GetBit(3);
            bool R3 = res.GetBit(3);

            SREG.H = !Rd3 & Rr3 | Rr3 & R3 | R3 & !Rd3;
            SREG.V = Rd7 & !Rr7 & !R7 | !Rd7 & Rr7 & R7;
            SREG.S = SREG.N ^ SREG.V;
            SREG.N = R7;
            SREG.C = !Rd7 & Rr7 | Rr7 & R7 | R7 & !Rd7;
            SREG.Z = res == 0;

            PC++;
        }

        void CPI(byte r, byte K)
        {
            byte res;
            unchecked
            {
                res = (byte)(regs[r] - K);
            }
            // None of the registers are changed.
            // Only SREG modification is performed!
            SUBApplySREG(regs[r], K, res);
            PC++;
        }

        void CPSE(byte Rd, byte Rr)
        {
            if (regs[Rd] == regs[Rr])
            {
                if (Is32BitInstruction(dataMemory[PC + 1]))
                {
                    PC += 3; //  Skip a two word instruction
                }
                else
                {
                    PC += 2; // Skip a one word instruction
                }
            }
            else
            {
                PC++;
            }
        }
        #endregion


        #region Logical instructions
        void LogicalApplySREG(byte res)
        {
            SREG.N = res.GetBit(7);
            SREG.V = false;
            SREG.Z = res == 0;
            SREG.S = SREG.N ^ SREG.V;
        }

        void AND(byte Rd, byte Rr)
        {
            byte res = (byte)(Rd & Rr);
            regs[Rd] = res;
            LogicalApplySREG(res);
            PC++;
        }

        void ANDI(byte Rd, byte imm)
        {
            byte res = (byte)(Rd & imm);
            regs[Rd] = res;
            LogicalApplySREG(res);
            PC++;
        }

        void OR(byte Rd, byte Rr)
        {
            byte res = (byte)(regs[Rd] | regs[Rr]);
            regs[Rd] = res;
            LogicalApplySREG(res);
            PC++;
        }

        void ORI(byte Rd, byte imm)
        {
            byte res = (byte)(regs[Rd] | regs[imm]);
            regs[Rd] = res;
            LogicalApplySREG(res);
            PC++;
        }

        void EOR(byte Rd, byte Rr)
        {
            byte res = (byte)(regs[Rd] ^ regs[Rr]);
            regs[Rd] = res;
            LogicalApplySREG(res);
            PC++;
        }

        void LSR(byte Rr)
        {
            throw new NotImplementedException();
        }

        void ASR(byte Rr)
        {
            throw new NotImplementedException();
        }
        #endregion


        void MOV(byte Rd, byte Rr)
        {
            regs[Rd] = regs[Rr];
            PC++;
        }

        void MOVW(byte Rd, byte Rr)
        {
            Debug.Assert(byte.IsEvenInteger(Rd));
            Debug.Assert(byte.IsEvenInteger(Rr));

            regs[Rd] = regs[Rr];
            regs[Rd + 1] = regs[Rr + 1];

            PC++;
        }


        #region Data instructions
        void IN(byte reg, byte A)
        {
            throw new NotImplementedException();
        }

        void OUT(byte reg, byte A)
        {
            if (A == 0x00)
            {
                Console.Write(Convert.ToChar(regs[reg]));
            }
            else
            {
                //TODO
                //throw new NotImplementedException();
            }

            PC++;
        }

        #endregion


        void POP(byte Rd)
        {
            SP++;
            regs[Rd] = dataMemory[SP];
            PC++;
        }

        void PUSH(byte Rr)
        {
            dataMemory[SP] = regs[Rr];
            SP--;
            PC++;
        }

        void RET()
        {
            SP--;
            PC = programMemory[SP];
        }

        void RETI()
        {
            SP--;
            PC = programMemory[SP];
            SREG.I = true;
        }

        void RJMP(sbyte k)
        {
            PC = (ushort)(PC + k + 1);
        }

        void JMP(uint highK)
        {
            uint lowK = programMemory[PC + 1];
            PC = (ushort)((highK << 16) | lowK);
        }

        void CALL(uint highK)
        {
            // Store return adress
            programMemory[SP] = PC;
            SP--;

            uint lowK = programMemory[PC + 1];
            PC = (ushort)((highK << 16) | lowK);
        }

        void RCALL(sbyte k)
        {
            // Store return address
            programMemory[SP] = PC;
            SP--;

            throw new NotImplementedException();
        }

        void EICALL()
        {
            throw new NotImplementedException();
        }

        void EIJMP()
        {
            throw new NotImplementedException();
        }

        void ICALL()
        {
            throw new NotImplementedException();
        }

        void IJMP()
        {
            PC = regs.Z;
        }

        void LPM()
        {
            throw new NotImplementedException();
        }

        void LPM_Reg(byte Rr)
        {
            throw new NotImplementedException();
        }


        #region Memory instructions
        // 32-bit Instruction
        void STS(byte Rr)
        {
            ushort k = programMemory[PC + 1];
            dataMemory[k] = regs[Rr];

            PC += 2;
        }

        void ST_X(byte Rr)
        {
            dataMemory[regs.X] = regs[Rr];
            PC++;
        }

        void ST_X_PostIncrement(byte Rr)
        {
            dataMemory[regs.X] = regs[Rr];
            regs.X++;
            PC++;
        }

        void ST_X_PreDecrement(byte Rr)
        {
            regs.X--;
            dataMemory[regs.X] = regs[Rr];
            PC++;
        }

        void ST_Y(byte Rr)
        {
            throw new NotImplementedException();
        }

        void ST_Y_PostIncrement(byte Rr)
        {
            throw new NotImplementedException();
        }

        void ST_Y_PreDecrement(byte Rr)
        {
            throw new NotImplementedException();
        }

        void ST_Z(byte Rr)
        {
            dataMemory[regs.Z] = regs[Rr];
            PC++;
        }

        void ST_Z_PostIncrement(byte Rr)
        {
            dataMemory[regs.Z] = regs[Rr];
            regs.Z++;
            PC++;
        }



        void LD_Z(byte Rd)
        {
            throw new NotImplementedException();
        }

        void LD_Y(byte Rd)
        {
            throw new NotImplementedException();
        }
        void LPM_Reg_ZPostIncrement(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LDS(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LDS_16(byte r, byte addr)
        {
            throw new NotImplementedException();
        }

        void LD_X(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LD_X_PostIncrement(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LD_X_PreDecrement(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LD_Y_PostIncrement(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LD_Y_PreDecrement(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LD_Z_PostIncrement(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LD_Z_PreDecrement(byte Rr)
        {
            throw new NotImplementedException();
        }


        void LAC(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LAS(byte Rr)
        {
            throw new NotImplementedException();
        }

        void LAT(byte Rr)
        {
            throw new NotImplementedException();
        }
        #endregion


        #region Conditional relative branch instructions
        void BRCS(sbyte k)
        {
            PC = (ushort)(SREG.C == true ? (PC + k + 1) : (PC + 1));
        }

        // also BRSH (Same of Higher)
        void BRCC(sbyte k)
        {
            PC = (ushort)(SREG.C == false ? (PC + k + 1) : (PC + 1));
        }

        void BREQ(sbyte k)
        {
            PC = (ushort)(SREG.Z == true ? (PC + k + 1) : (PC + 1));
        }

        void BRNE(sbyte k)
        {
            PC = (ushort)(SREG.Z == false ? (PC + k + 1) : (PC + 1));
        }

        void BRMI(sbyte k)
        {
            PC = (ushort)(SREG.N == true ? (PC + k + 1) : (PC + 1));
        }

        void BRPL(sbyte k)
        {
            PC = (ushort)(SREG.N == false ? (PC + k + 1) : (PC + 1));
        }

        void BRVS(sbyte k)
        {
            PC = (ushort)(SREG.V == true ? (PC + k + 1) : (PC + 1));
        }

        void BRVC(sbyte k)
        {
            PC = (ushort)(SREG.Z == false ? (PC + k + 1) : (PC + 1));
        }

        void BRLT(sbyte k)
        {
            PC = (ushort)(SREG.S == true ? (PC + k + 1) : (PC + 1));
        }

        void BRGE(sbyte k)
        {
            PC = (ushort)(SREG.S == false ? (PC + k + 1) : (PC + 1));
        }

        void BRHS(sbyte k)
        {
            PC = (ushort)(SREG.H == true ? (PC + k + 1) : (PC + 1));
        }

        void BRHC(sbyte k)
        {
            PC = (ushort)(SREG.H == false ? (PC + k + 1) : (PC + 1));
        }

        void BRTS(sbyte k)
        {
            PC = (ushort)(SREG.T == true ? (PC + k + 1) : (PC + 1));
        }

        void BRTC(sbyte k)
        {
            PC = (ushort)(SREG.T == false ? (PC + k + 1) : (PC + 1));
        }

        void BRIE(sbyte k)
        {
            PC = (ushort)(SREG.I == true ? (PC + k + 1) : (PC + 1));
        }

        void BRID(sbyte k)
        {
            PC = (ushort)(SREG.I == false ? (PC + k + 1) : (PC + 1));
        }
        #endregion


        #region SREG set/clear instructions
        void SEC()
        {
            SREG.C = true;
            PC++;
        }

        void CLC()
        {
            SREG.C = false;
            PC++;
        }

        void SEZ()
        {
            SREG.Z = true;
            PC++;
        }

        void CLZ()
        {
            SREG.Z = false;
            PC++;
        }

        void SEN()
        {
            SREG.N = true;
            PC++;
        }

        void CLN()
        {
            SREG.N = false;
            PC++;
        }

        void SEV()
        {
            SREG.V = true;
            PC++;
        }

        void CLV()
        {
            SREG.V = false;
            PC++;
        }

        void SES()
        {
            SREG.S = true;
            PC++;
        }

        void CLS()
        {
            SREG.S = false;
            PC++;
        }

        void SEH()
        {
            SREG.H = true;
            PC++;
        }

        void CLH()
        {
            SREG.H = false;
            PC++;
        }

        void SET()
        {
            SREG.T = true;
            PC++;
        }

        void CLT()
        {
            SREG.T = false;
            PC++;
        }

        void SEI()
        {
            SREG.I = true;
            PC++;
        }

        void CLI()
        {
            SREG.I = false;
            PC++;
        }
        #endregion


        void DES(byte Rr)
        {
            throw new NotImplementedException();
        }

        void XCH(byte Rd)
        {
            throw new NotImplementedException();
        }

        void SWAP(byte Rd)
        {
            throw new NotImplementedException();
        }

        void BLD(byte reg, byte bitNumber)
        {
            throw new NotImplementedException();

        }

        void BST(byte reg, byte bitNumber)
        {
            throw new NotImplementedException();

        }

        void SBRC(byte R, byte bitNumber)
        {
            if (regs[R].GetBit(bitNumber) == false)
            {
                if (Is32BitInstruction(dataMemory[PC + 1]))
                {
                    PC += 3; //  Skip a two word instruction
                }
                else
                {
                    PC += 2; // Skip a one word instruction
                }
            }
            PC++;
        }

        void SBRS(byte R, byte bitNumber)
        {
            if (regs[R].GetBit(bitNumber) == true)
            {
                if (Is32BitInstruction(dataMemory[PC + 1]))
                {
                    PC += 3; //  Skip a two word instruction
                }
                else
                {
                    PC += 2; // Skip a one word instruction
                }
            }
            PC++;
        }

        void CBI(byte A, byte bit)
        {
            throw new NotImplementedException();

        }

        void SBI(byte A, byte bit)
        {
            throw new NotImplementedException();

        }

        void SBIC(byte A, byte bit)
        {
            throw new NotImplementedException();

        }

        void SBIS(byte A, byte bit)
        {
            throw new NotImplementedException();

        }


        #region Multiply
        void MUL(byte Rd, byte Rr)
        {
            throw new NotImplementedException();
        }

        void MULS(byte Rd, byte Rr)
        {
            throw new NotImplementedException();
        }

        void MULSU(byte Rd, byte Rr)
        {
            throw new NotImplementedException();
        }

        void FMUL(byte Rd, byte Rr)
        {
            throw new NotImplementedException();
        }

        void FMULS(byte Rd, byte Rr)
        {
            throw new NotImplementedException();
        }

        void FMULSU(byte Rd, byte Rr)
        {
            throw new NotImplementedException();
        }
        #endregion



        #region MCU Control instructions
        void BREAK()
        {
            isRunning = false;
        }

        // We are using 'Watch Dog Reset' instruction to call display function
        void WDR()
        {
            displayCallback?.Invoke();
            PC++;
        }

        void SLEEP()
        {
            Thread.Sleep(20);
            PC++;
            //throw new NotImplementedException();
        }
        #endregion
    }
}
