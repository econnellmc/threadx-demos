//
// Copyright (c) 2010-2023 Antmicro
//
//  This file is licensed under the MIT License.
//  Full license text is available in 'licenses/MIT.txt'.
//
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Antmicro.Renode.Core;
using Antmicro.Renode.Core.Structure;
using Antmicro.Renode.Exceptions;
using Antmicro.Renode.Logging;
using Antmicro.Renode.Utilities;
using Antmicro.Renode.Core.Structure.Registers;
using Antmicro.Renode.Peripherals.Bus;
using Antmicro.Renode.Peripherals.GPIOPort;

namespace Antmicro.Renode.Peripherals.IRQControllers
{
    public class MPSInterruptController : BasicDoubleWordPeripheral, IKnownSize, IGPIOReceiver, INumberedGPIOOutput
    {
        public MPSInterruptController(IMachine machine) : base(machine)
        {
            // GPIO
            // USB
            USB_MUX_Output = new GPIO[2, 3];
            USB_MUX_Signal = new GPIO[6];
            USB_MUX_Input = new GPIO[2];
            GPIOInitilizer(USB_MUX_Output);
            GPIOInitilizer(USB_MUX_Signal);
            GPIOInitilizer(USB_MUX_Input);

            // Crypto
            Crypto_MUX_Output = new GPIO[1, 5];
            Crypto_MUX_Signal = new GPIO[3];
            Crypto_MUX_Input = new GPIO[1];
            GPIOInitilizer(Crypto_MUX_Output);
            GPIOInitilizer(Crypto_MUX_Signal);
            GPIOInitilizer(Crypto_MUX_Input);

            // VNOC
            VNOC_MUX_Output = new GPIO[24, 2];
            VNOC_MUX_Signal = new GPIO[24];
            VNOC_MUX_Input = new GPIO[24];
            GPIOInitilizer(VNOC_MUX_Output);
            GPIOInitilizer(VNOC_MUX_Signal);
            GPIOInitilizer(VNOC_MUX_Input);

            // HNOC
            HNOC_MUX_Output = new GPIO[22, 3];
            HNOC_MUX_Signal = new GPIO[44];
            HNOC_MUX_Input = new GPIO[22];
            GPIOInitilizer(HNOC_MUX_Output);
            GPIOInitilizer(HNOC_MUX_Signal);
            GPIOInitilizer(HNOC_MUX_Input);

            Connections = dictUpdate();
            {
                // Define CSCB Registers
                Registers.CSCB_INTEN0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, writeCallback: (_, value) => { CSCB_INTEN0 = value; update(); }, valueProviderCallback: _ => CSCB_INTEN0, name: "CSCB_INTEN0");

                Registers.CSCB_INTEN1.Define(this)
                    .WithReservedBits(11, 21)
                    .WithValueField(0, 11, writeCallback: (_, value) => { CSCB_INTEN1 = value; update(); }, valueProviderCallback: _ => CSCB_INTEN1, name: "CSCB_INTEN1");

                Registers.CSCB_INTSTATUS0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, valueProviderCallback: _ => CSCB_INTSTATUS0, name: "CSCB_INTSTATUS0");

                Registers.CSCB_INTSTATUS1.Define(this)
                    .WithReservedBits(11, 21)
                    .WithValueField(0, 11, valueProviderCallback: _ => CSCB_INTSTATUS1, name: "CSCB_INTSTATUS1");

                Registers.CSCB_INTSTATUS_MASK0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, valueProviderCallback: _ => CSCB_INTSTATUS_MASK0, name: "CSCB_INTSTATUS_MASK0");

                Registers.CSCB_INTSTATUS_MASK1.Define(this)
                    .WithReservedBits(11, 21)
                    .WithValueField(0, 11, valueProviderCallback: _ => CSCB_INTSTATUS_MASK1, name: "CSCB_INTSTATUS_MASK1");

                // Define FNOC Registers
                Registers.FNOC_DET_INTEN0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, writeCallback: (_, value) => { FNOC_DET_INTEN0 = value; update(); }, valueProviderCallback: _ => FNOC_DET_INTEN0, name: "FNOC_DET_INTEN0");

                Registers.FNOC_DET_INTEN1.Define(this)
                    .WithReservedBits(22, 10)
                    .WithValueField(0, 22, writeCallback: (_, value) => { FNOC_DET_INTEN1 = value; update(); }, valueProviderCallback: _ => FNOC_DET_INTEN1, name: "FNOC_DET_INTEN1");

                Registers.FNOC_DET_INTSTATUS0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, valueProviderCallback: _ => FNOC_DET_INTSTATUS0, name: "FNOC_DET_INTSTATUS0");

                Registers.FNOC_DET_INTSTATUS1.Define(this)
                    .WithReservedBits(22, 10)
                    .WithValueField(0, 22, valueProviderCallback: _ => FNOC_DET_INTSTATUS1, name: "FNOC_DET_INTSTATUS1");

                Registers.FNOC_DET_INTSTATUS_MASK0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, valueProviderCallback: _ => FNOC_DET_INTSTATUS_MASK0, name: "FNOC_DET_INTSTATUS_MASK0");

                Registers.FNOC_DET_INTSTATUS_MASK1.Define(this)
                    .WithReservedBits(22, 10)
                    .WithValueField(0, 22, valueProviderCallback: _ => FNOC_DET_INTSTATUS_MASK1, name: "FNOC_DET_INTSTATUS_MASK1");

                // Define VNOC Registers
                Registers.VNOC_IP_INTEN0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, writeCallback: (_, value) => { VNOC_IP_INTEN0 = value; update(); }, valueProviderCallback: _ => VNOC_IP_INTEN0, name: "VNOC_IP_INTEN0");

                Registers.VNOC_IP_INTSTATUS0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, valueProviderCallback: _ => VNOC_IP_INTSTATUS0, name: "VNOC_IP_INTSTATUS0");

                Registers.VNOC_IP_INTSTATUS_MASK0.Define(this)
                    .WithReservedBits(24, 8)
                    .WithValueField(0, 24, valueProviderCallback: _ => VNOC_IP_INTSTATUS_MASK0, name: "VNOC_IP_INTSTATUS_MASK0");

                // Define MGPIO0 Registers
                Registers.MGPIO0_INTEN.Define(this)
                    .WithReservedBits(4, 28)
                    .WithValueField(0, 4, writeCallback: (_, value) => { MGPIO0_INTEN = value; update(); }, valueProviderCallback: _ => MGPIO0_INTEN, name: "MGPIO0_INTEN");

                Registers.MGPIO0_INTSTATUS.Define(this)
                    .WithReservedBits(4, 28)
                    .WithValueField(0, 4, valueProviderCallback: _ => MGPIO0_INTSTATUS, name: "MGPIO0_INTSTATUS");

                Registers.MGPIO0_INTSTATUS_MASK.Define(this)
                    .WithReservedBits(4, 28)
                    .WithValueField(0, 4, valueProviderCallback: _ => MGPIO0_INTSTATUS_MASK, name: "MGPIO0_INTSTATUS_MASK");

                // Define MGPIO1 Registers
                Registers.MGPIO1_INTEN.Define(this)
                    .WithReservedBits(4, 28)
                    .WithValueField(0, 4, writeCallback: (_, value) => { MGPIO1_INTEN = value; update(); }, valueProviderCallback: _ => MGPIO1_INTEN, name: "MGPIO1_INTEN");

                Registers.MGPIO1_INTSTATUS.Define(this)
                    .WithReservedBits(4, 28)
                    .WithValueField(0, 4, valueProviderCallback: _ => MGPIO1_INTSTATUS, name: "MGPIO1_INTSTATUS");

                Registers.MGPIO1_INTSTATUS_MASK.Define(this)
                    .WithValueField(0, 4, valueProviderCallback: _ => MGPIO1_INTSTATUS_MASK, name: "MGPIO1_INTSTATUS_MASK");

                // Define MGPIO2 Registers
                Registers.MGPIO2_INTEN.Define(this)
                    .WithReservedBits(4, 28)
                    .WithValueField(0, 4, writeCallback: (_, value) => { MGPIO2_INTEN = value; update(); }, valueProviderCallback: _ => MGPIO2_INTEN, name: "MGPIO2_INTEN");

                Registers.MGPIO2_INTSTATUS.Define(this)
                    .WithReservedBits(4, 28)
                    .WithValueField(0, 4, valueProviderCallback: _ => MGPIO2_INTSTATUS, name: "MGPIO2_INTSTATUS");

                Registers.MGPIO2_INTSTATUS_MASK.Define(this)
                    .WithReservedBits(4, 28)
                    .WithValueField(0, 4, valueProviderCallback: _ => MGPIO2_INTSTATUS_MASK, name: "MGPIO2_INTSTATUS_MASK");

                // Define FAB Registers
                Registers.FAB_INTEN.Define(this)
                    .WithValueField(0, 32, writeCallback: (_, value) => { FAB_INTEN = value; update(); }, valueProviderCallback: _ => FAB_INTEN, name: "FAB_INTEN");

                Registers.FAB_INTSTATUS.Define(this)
                    .WithValueField(0, 32, valueProviderCallback: _ => FAB_INTSTATUS, name: "FAB_INTSTATUS");

                Registers.FAB_INTSTATUS_MASK.Define(this)
                    .WithValueField(0, 32, valueProviderCallback: _ => FAB_INTSTATUS_MASK, name: "FAB_INTSTATUS_MASK");
            }
        }

        private void dictAdd(GPIO[,] gpio, Dictionary<int, IGPIO> connections)
        {
            int temp = connections.Count;
            for (int i = 0; i < gpio.GetLength(1); i++)
            {
                for (int j = 0; j < gpio.GetLength(0); j++)
                {
                    connections[i * gpio.GetLength(0) + j + temp] = gpio[j, i];
                }
            }

        }
        private void dictAdd(GPIO[,] gpio, Dictionary<int, IGPIO> connections, int numberOfOuputs)
        {
            int temp = connections.Count;
            for (int i = 0; i < numberOfOuputs; i++)
            {
                for (int j = 0; j < gpio.GetLength(0); j++)
                {
                    connections[i * gpio.GetLength(0) + j + temp] = gpio[j, i];
                }
            }

        }

        private Dictionary<int, IGPIO> dictUpdate()
        {
            var connections = new Dictionary<int, IGPIO>();

            for (int i = 0; i < 9; i++)
            {
                connections[i] = new GPIO();
            }

            dictAdd(USB_MUX_Output, connections);
            dictAdd(Crypto_MUX_Output, connections);
            dictAdd(VNOC_MUX_Output, connections,1);
            dictAdd(HNOC_MUX_Output, connections);
            return connections;
        }

        public void OnGPIO(int number, bool value)
        {
            int firstPin = 0;
            int bitNumber = 0;

            foreach (int i in Enum.GetValues(typeof(InputBlocksFirstPin)))
            {
                if (i <= number)
                {
                    firstPin = i;
                    bitNumber = number - i;
                }
            }

            switch (firstPin) //Index from 0 -> first value is at 0,0
            {
                case (int)InputBlocksFirstPin.CSCB0:
                    BitHelper.SetBit(ref CSCB_INTSTATUS0, (byte)bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.CSCB1:
                    BitHelper.SetBit(ref CSCB_INTSTATUS1, (byte)bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.FNOC_DET0:
                    BitHelper.SetBit(ref FNOC_DET_INTSTATUS0, (byte)bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.FNOC_DET1:
                    BitHelper.SetBit(ref FNOC_DET_INTSTATUS1, (byte)bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.MGPIO0:
                    BitHelper.SetBit(ref MGPIO0_INTSTATUS, (byte)bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.MGPIO1:
                    BitHelper.SetBit(ref MGPIO1_INTSTATUS, (byte)bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.MGPIO2:
                    BitHelper.SetBit(ref MGPIO2_INTSTATUS, (byte)bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.FAB:
                    BitHelper.SetBit(ref FAB_INTSTATUS, (byte)bitNumber, value);
                    break;

                // MUX USB
                case (int)InputBlocksFirstPin.USB_MUX_Input:
                    setGPIO(USB_MUX_Input, bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.USB_MUX_Signal:
                    setGPIO(USB_MUX_Signal, bitNumber, value);
                    break;

                // MUX Crypto
                case (int)InputBlocksFirstPin.Crypto_MUX_Input:
                    setGPIO(Crypto_MUX_Input, bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.Crypto_MUX_Signal:
                    setGPIO(Crypto_MUX_Signal, bitNumber, value);
                    break;

                // MUX VNOC
                case (int)InputBlocksFirstPin.VNOC_MUX_Input:
                    setGPIO(VNOC_MUX_Input, bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.VNOC_MUX_Signal:
                    setGPIO(VNOC_MUX_Signal, bitNumber, value);
                    break;

                // MUX VNOC
                case (int)InputBlocksFirstPin.HNOC_MUX_Input:
                    setGPIO(HNOC_MUX_Input, bitNumber, value);
                    break;

                case (int)InputBlocksFirstPin.HNOC_MUX_Signal:
                    setGPIO(HNOC_MUX_Signal, bitNumber, value);
                    break;

                default:
                    break;
            }

            update();
        }
        private void setGPIO(GPIO[] gpio, int bitNumber, bool value)
        {
            if (value)
                gpio[bitNumber].Set();
            else
                gpio[bitNumber].Unset();
        }

        private void update()
        {
            Mux(USB_MUX_Output, USB_MUX_Signal, USB_MUX_Input, 3);
            Mux(Crypto_MUX_Output, Crypto_MUX_Signal, Crypto_MUX_Input, 5);
            Mux(VNOC_MUX_Output, VNOC_MUX_Signal, VNOC_MUX_Input, 2);
            Mux(HNOC_MUX_Output, HNOC_MUX_Signal, HNOC_MUX_Input, 3);

            uint temp = 0;
            for (int i = 0; i < VNOC_MUX_Output.GetLength(0); i++)
            {
                if (VNOC_MUX_Output[i, 1].IsSet)
                    temp |= (uint)1 << (i);
            }
            VNOC_IP_INTSTATUS0 = temp;

            CSCB_INTSTATUS_MASK0 = CSCB_INTEN0 & CSCB_INTSTATUS0;
            CSCB_INTSTATUS_MASK1 = CSCB_INTEN1 & CSCB_INTSTATUS1;
            FNOC_DET_INTSTATUS_MASK0 = FNOC_DET_INTEN0 & FNOC_DET_INTSTATUS0;
            FNOC_DET_INTSTATUS_MASK1 = FNOC_DET_INTEN1 & FNOC_DET_INTSTATUS1;
            VNOC_IP_INTSTATUS_MASK0 = VNOC_IP_INTEN0 & VNOC_IP_INTSTATUS0;
            MGPIO0_INTSTATUS_MASK = MGPIO0_INTEN & MGPIO0_INTSTATUS;
            MGPIO1_INTSTATUS_MASK = MGPIO1_INTEN & MGPIO1_INTSTATUS;
            MGPIO2_INTSTATUS_MASK = MGPIO2_INTEN & MGPIO2_INTSTATUS;
            FAB_INTSTATUS_MASK = FAB_INTEN & FAB_INTSTATUS;

            Connections = dictUpdate();

            SetOutput(Connections[0], CSCB_INTSTATUS_MASK0);
            SetOutput(Connections[1], CSCB_INTSTATUS_MASK1);
            SetOutput(Connections[2], FNOC_DET_INTSTATUS_MASK0);
            SetOutput(Connections[3], FNOC_DET_INTSTATUS_MASK1);
            SetOutput(Connections[4], VNOC_IP_INTSTATUS_MASK0);
            SetOutput(Connections[5], MGPIO0_INTSTATUS_MASK);
            SetOutput(Connections[6], MGPIO1_INTSTATUS_MASK);
            SetOutput(Connections[7], MGPIO2_INTSTATUS_MASK);
            SetOutput(Connections[8], FAB_INTSTATUS_MASK);

        }

        // Inputs
        // GPIO[,] Output is a 2D array of GPIO
        // Eg. GPIO[i,j] each row is a different location and each column is a different bit or GPIO
        // GPIO[] Signal is the signal into the mux
        // NoOutputs is how many differnt paths out of the mux
        // The aim is to loop through all of the columns and set the GPIO in the correct row as per the Signal
        private void Mux(GPIO[,] Output, GPIO[] Signal, GPIO[] Input, int NoOutputs)
        {
            int SignalBitSize = (int)Math.Ceiling(Math.Log(NoOutputs, 2));
            int MessageSize = Output.GetLength(0);

            for (int currBit = 0; currBit < MessageSize; currBit++)
            {
                // Get value of the signal bits
                int SignalInt = 0;
                for (int j = SignalBitSize * currBit; j < SignalBitSize + SignalBitSize * currBit; j++)
                    SignalInt = SignalInt + ((Convert.ToInt32(Signal[j].IsSet)) << (j - SignalBitSize * currBit));

                if (SignalInt >= NoOutputs)
                    SignalInt = 0;

                // Reset all GPIO
                for (int j = 0; j < NoOutputs; j++)
                    if (j != SignalInt)
                        Output[currBit, j].Unset();

                // Set the GPIO that has been chosen from signal
                if (Input[currBit].IsSet)
                    Output[currBit, SignalInt].Set();
                else
                    Output[currBit, SignalInt].Unset();
            }
        }
        public static void SetOutput(IGPIO outputPin, ulong mask)
        {
            if (mask != 0)
                outputPin.Set();
            else
                outputPin.Unset();
        }
        private void GPIOInitilizer(GPIO[,] gpio)
        {
            for (int i = 0; i < gpio.GetLength(0); i++)
            {
                for (int j = 0; j < gpio.GetLength(1); j++)
                {
                    gpio[i, j] = new GPIO();
                }
            }
        }
        private void GPIOInitilizer(GPIO[] gpio)
        {
            for (int i = 0; i < gpio.GetLength(0); i++)
            {
                gpio[i] = new GPIO();
            }
        }

        public long Size => 0x1000;
        public IReadOnlyDictionary<int, IGPIO> Connections { get; set; }

        // Mux
        private readonly GPIO[,] USB_MUX_Output;
        private readonly GPIO[] USB_MUX_Signal;
        private readonly GPIO[] USB_MUX_Input;
        private readonly GPIO[,] Crypto_MUX_Output;
        private readonly GPIO[] Crypto_MUX_Signal;
        private readonly GPIO[] Crypto_MUX_Input;
        private readonly GPIO[,] VNOC_MUX_Output;
        private readonly GPIO[] VNOC_MUX_Signal;
        private readonly GPIO[] VNOC_MUX_Input;
        private readonly GPIO[,] HNOC_MUX_Output;
        private readonly GPIO[] HNOC_MUX_Signal;
        private readonly GPIO[] HNOC_MUX_Input;

        // Enable Registers
        private ulong CSCB_INTEN0;
        private ulong CSCB_INTEN1;
        private ulong FNOC_DET_INTEN0;
        private ulong FNOC_DET_INTEN1;
        private ulong VNOC_IP_INTEN0;
        private ulong MGPIO0_INTEN;
        private ulong MGPIO1_INTEN;
        private ulong MGPIO2_INTEN;
        private ulong FAB_INTEN;

        // Status Registers
        private uint CSCB_INTSTATUS0;
        private uint CSCB_INTSTATUS1;
        private uint FNOC_DET_INTSTATUS0;
        private uint FNOC_DET_INTSTATUS1;
        private uint VNOC_IP_INTSTATUS0;
        private uint MGPIO0_INTSTATUS;
        private uint MGPIO1_INTSTATUS;
        private uint MGPIO2_INTSTATUS;
        private uint FAB_INTSTATUS;

        // Mask Registers
        private ulong CSCB_INTSTATUS_MASK0;
        private ulong CSCB_INTSTATUS_MASK1;
        private ulong FNOC_DET_INTSTATUS_MASK0;
        private ulong FNOC_DET_INTSTATUS_MASK1;
        private ulong VNOC_IP_INTSTATUS_MASK0;
        private ulong MGPIO0_INTSTATUS_MASK;
        private ulong MGPIO1_INTSTATUS_MASK;
        private ulong MGPIO2_INTSTATUS_MASK;
        private ulong FAB_INTSTATUS_MASK;

        private enum Registers : long
        {
            CSCB_INTEN0 = 0x0,
            CSCB_INTEN1 = 0x4,
            CSCB_INTSTATUS0 = 0x8,
            CSCB_INTSTATUS1 = 0xC,
            CSCB_INTSTATUS_MASK0 = 0x10,
            CSCB_INTSTATUS_MASK1 = 0x14,
            FNOC_DET_INTEN0 = 0x18,
            FNOC_DET_INTEN1 = 0x1C,
            FNOC_DET_INTSTATUS0 = 0x20,
            FNOC_DET_INTSTATUS1 = 0x24,
            FNOC_DET_INTSTATUS_MASK0 = 0x28,
            FNOC_DET_INTSTATUS_MASK1 = 0x2C,
            VNOC_IP_INTEN0 = 0x30,
            VNOC_IP_INTSTATUS0 = 0x34,
            VNOC_IP_INTSTATUS_MASK0 = 0x38,
            MGPIO0_INTEN = 0x3C,
            MGPIO0_INTSTATUS = 0x40,
            MGPIO0_INTSTATUS_MASK = 0x44,
            MGPIO1_INTEN = 0x48,
            MGPIO1_INTSTATUS = 0x4C,
            MGPIO1_INTSTATUS_MASK = 0x50,
            MGPIO2_INTEN = 0x54,
            MGPIO2_INTSTATUS = 0x58,
            MGPIO2_INTSTATUS_MASK = 0x5C,
            FAB_INTEN = 0x60,
            FAB_INTSTATUS = 0x64,
            FAB_INTSTATUS_MASK = 0x68
        }
        private enum InputBlocksFirstPin : int
        {
            CSCB0 = 0,
            CSCB1 = CSCB0 + 24,
            FNOC_DET0 = CSCB1 + 11,
            FNOC_DET1 = FNOC_DET0 + 24,
            MGPIO0 = FNOC_DET1 + 22,
            MGPIO1 = MGPIO0 + 4,
            MGPIO2 = MGPIO1 + 4,
            FAB = MGPIO2 + 4,
            USB_MUX_Input = FAB + 32,
            USB_MUX_Signal = USB_MUX_Input + 2,
            Crypto_MUX_Input = USB_MUX_Signal + 4,
            Crypto_MUX_Signal = Crypto_MUX_Input + 1,
            VNOC_MUX_Input = Crypto_MUX_Signal + 3,
            VNOC_MUX_Signal = VNOC_MUX_Input + 24,
            HNOC_MUX_Input = VNOC_MUX_Signal + 24,
            HNOC_MUX_Signal = HNOC_MUX_Input + 22,
            OUT_OF_RANGE = HNOC_MUX_Signal + 44

        }
    }
}