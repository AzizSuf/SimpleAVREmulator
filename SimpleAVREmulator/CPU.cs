using System.Diagnostics;

namespace SimpleAVREmulator
{
    public partial class CPU
    {
        const int RegistersCount = 32;
        const int IORegistersCount = 64;
        const int ExtIORegistersCount = 160;
        const int IRAMStartAdress = RegistersCount + IORegistersCount + ExtIORegistersCount;

        public StatusRegister SREG = new();

        // Program Counter (word(16-bit) adressing)
        ushort PC;

        ushort[] programMemory;

        // 32 Registers:     0x0000 - 0x001F
        // 64 I/O Registers: 0x0020 - 0x005F
        // 160 Ext. I/o Reg: 0x0060 - 0x00FF
        // Internal SRAM:    0x0100 - 0x0100+ISRAM_SIZE
        byte[] dataMemory;

        public RegisterFile regs;
        public ArraySegment<byte> ISRAM;

        // Stack Pointer Low and High parts
        byte SPL;
        byte SPH;

        // Stack Pointer
        ushort SP
        {
            set
            {
                SPL = (byte)(value & 0xFF);
                SPH = (byte)(value >> 8);
            }

            get
            {
                return (ushort)((SPH << 8) | SPL);
            }
        }

        long cycleCounter;
        
        bool isRunning;
        Action? displayCallback;
        Stopwatch stopwatch;


        public CPU(ushort[] programMem, int isramSize)
        {
            programMemory = programMem;
            dataMemory = new byte[RegistersCount + IORegistersCount + ExtIORegistersCount + isramSize];

            regs = new RegisterFile(new ArraySegment<byte>(dataMemory, 0, RegistersCount));
            ISRAM = new ArraySegment<byte>(dataMemory, IRAMStartAdress, isramSize);

            isRunning = true;
            stopwatch = new Stopwatch();
        }

        public void Run()
        {
            while (isRunning)
            {
                DoCycle();
            }
        }

        public void DoCycle()
        {
            stopwatch.Restart();
            ushort opcode = programMemory[PC];
            DecodeAndExecute(opcode);
            stopwatch.Stop();
            //Debug.WriteLine(stopwatch.Elapsed.TotalNanoseconds);
            //Debug.WriteLine($"0x{opcode:X}");
            //Debug.WriteLine($"PC: 0x{PC:X}");
        }

        public void RegisterDisplayCallback(Action displayCallback)
        {
            this.displayCallback = displayCallback;
        }
    }
}
