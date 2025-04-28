using IntelHexFormatReader;
using IntelHexFormatReader.Model;


namespace SimpleAVREmulator
{
    internal class Program
    {
        static void Main(string[] args)
        {
            /*
            var mem = new ushort[]
            {
                0x0000, // NOP
                //0x0000, // NOP
                //0xE401, // LDI R16, 0x41('A')
                //0xB900, // OUT 0x0, R16           // 0x0 - вывод в консоль

                //0x9503, // INC R16
                //0xB900, // OUT 0x0, R16
                //0x9503, // INC R16
                //0xB900, // OUT 0x0, R16
                //0x9503, // INC R16
                //0xB900, // OUT 0x0, R16


                0x0000, // nop
                0xE401, // ldi r16, 0x41
                0xEF10, //ldi r17, 240
            //loop:
                0xB900, // out 0x0, r16
                0x9503, // inc r16
                0x9513, // inc r17
                0xF7E1, // brne loop

                0x9598, // BREAK
            };
            */

            //var mem = LoadHex(@"D:\Programming\AVR\ASM\for_emulator\for_emulator\Release\for_emulator.hex");
            //var mem = LoadHex(@"D:\Programming\AVR\C\for_emulator_c\for_emulator_c\Release\for_emulator_c.hex");

            //var cpu = new CPU(mem, 1024);
            //cpu.Run();

            int @int = 10;
            Console.WriteLine(@int);
        }

        public static ushort[] LoadHex(string hexFilePath)
        {
            var reader = new HexFileReader(hexFilePath, 1024 * 50); // TODO: size 1024

            MemoryBlock memoryRepresentation = reader.Parse();

            // opcode - 2 bytes
            var programOpcodes = new ushort[memoryRepresentation.Cells.Length / 2];

            int opcodeIndex = 0;
            int i;
            for (i = 0; i < programOpcodes.Length; i += 2)
            {
                if (!memoryRepresentation.Cells[i].Modified) break;

                byte low = memoryRepresentation.Cells[i].Value;
                byte high = memoryRepresentation.Cells[i + 1].Value;
                ushort opcode = (ushort)((high << 8) | low);
                programOpcodes[opcodeIndex] = opcode;
                opcodeIndex++;
            }

            Array.Resize(ref programOpcodes, opcodeIndex);

            return programOpcodes;
        }
    }
}
