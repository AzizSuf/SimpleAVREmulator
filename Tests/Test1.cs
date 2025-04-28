using SimpleAVREmulator;

namespace Tests
{
    [TestClass]
    public sealed class Test1
    {
        // Compare with immediate
        [TestMethod]
        public void CPI_0_0()
        {
            var prog = new ushort[] { 0x3000 }; // CPI R16, 0 // R16 - 0
            var cpu = new CPU(prog);
            cpu.DoCycle();

            // 0 - 0
            Assert.AreEqual(false, cpu.SREG.C);
            Assert.AreEqual(true, cpu.SREG.Z);
            Assert.AreEqual(false, cpu.SREG.N);
            Assert.AreEqual(false, cpu.SREG.V);
            Assert.AreEqual(false, cpu.SREG.S);
            Assert.AreEqual(false, cpu.SREG.H);
        }

        [TestMethod]
        public void CPI_0_1()
        {
            var prog = new ushort[] { 0x3001 }; // CPI R16, 0 // R16 - 1
            var cpu = new CPU(prog);
            cpu.DoCycle();

            // 0 - 1
            Assert.AreEqual(true, cpu.SREG.C);
            Assert.AreEqual(false, cpu.SREG.Z);
            Assert.AreEqual(true, cpu.SREG.N);
            Assert.AreEqual(false, cpu.SREG.V);
            Assert.AreEqual(true, cpu.SREG.S);
            Assert.AreEqual(true, cpu.SREG.H);
        }

        [TestMethod]
        public void CPI_1_0()
        {
            var prog = new ushort[] { 0x3000 }; // CPI R16, 0 // R16 - 1
            var cpu = new CPU(prog);
            cpu.regs[16] = 1;
            cpu.DoCycle();

            // 1 - 0
            Assert.AreEqual(false, cpu.SREG.C);
            Assert.AreEqual(false, cpu.SREG.Z);
            Assert.AreEqual(false, cpu.SREG.N);
            Assert.AreEqual(false, cpu.SREG.V);
            Assert.AreEqual(false, cpu.SREG.S);
            Assert.AreEqual(false, cpu.SREG.H);
        }

        [TestMethod]
        public void CPI_128_1()
        {
            var prog = new ushort[] { 0x3001 }; // CPI R16, 0 // R16 - 1
            var cpu = new CPU(prog);
            cpu.regs[16] = 128;
            cpu.DoCycle();

            // 128 - 1
            Assert.AreEqual(false, cpu.SREG.C);
            Assert.AreEqual(false, cpu.SREG.Z);
            Assert.AreEqual(false, cpu.SREG.N);
            Assert.AreEqual(true, cpu.SREG.V);
            Assert.AreEqual(true, cpu.SREG.S);
            Assert.AreEqual(true, cpu.SREG.H);
        }
    }
}
