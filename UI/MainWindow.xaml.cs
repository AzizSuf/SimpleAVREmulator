using IntelHexFormatReader;
using IntelHexFormatReader.Model;
using SimpleAVREmulator;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int CPUVideoWidth = 200;
        const int CPUVideoHeight = 200;

        const int BitmapWidth = 200;
        const int BitmapHeigh = 200;
        WriteableBitmap writeableBitmap;

        ArraySegment<byte> displayBuffer;

        public MainWindow()
        {
            InitializeComponent();

            writeableBitmap = new WriteableBitmap(BitmapWidth, BitmapHeigh, 96, 96, PixelFormats.Gray8, null);
            emulatorDisplay.Source = writeableBitmap;

            // Every single pixel displays as 3x3
            var upscaledDisplay = new WriteableBitmap(CPUVideoWidth * 3, CPUVideoHeight * 3, 96, 96, PixelFormats.Gray8, null);

            var screen = new ScreenWindow();
            screen.Show();


            screen.emulatorUpscaledDisplay.Source = upscaledDisplay;

            var prog = LoadHex(@"D:\Programming\AVR\ASM\for_emulator\for_emulator\Release\for_emulator.hex");
            //var prog = LoadHex(@"D:\Programming\AVR\C\for_emulator_c\for_emulator_c\Release\for_emulator_c.hex");
            //displayBuffer = new byte[writeableBitmap.PixelWidth * writeableBitmap.PixelHeight];

            var cpu = new CPU(prog, BitmapWidth * BitmapHeigh);
            displayBuffer = cpu.ISRAM;

            cpu.RegisterDisplayCallback(() =>
            {
                WriteBuffer(displayBuffer);
            });

            var th = new Thread(() =>
            {
                cpu.Run();

            });
            th.Start();

        }

        public void WriteBuffer(ArraySegment<byte> buffer)
        {
            Dispatcher.Invoke(() =>
            {
                var rect = new Int32Rect(0, 0, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight);
                // TODO: Сейчас происходт копировние массива !!!
                writeableBitmap.WritePixels(rect, buffer.ToArray(), writeableBitmap.PixelWidth, 0);
            });
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

        static WriteableBitmap PixelPerfectUpscale(WriteableBitmap src, int factor)
        {
            var wb = new WriteableBitmap(200, 200, 96, 96, PixelFormats.Gray8, null);


            return wb;
        }

    }
}
