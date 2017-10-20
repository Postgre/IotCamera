using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Unosquare.RaspberryIO.Gpio;
using static Unosquare.RaspberryIO.Pi;
using Unosquare.RaspberryIO.Native;

namespace IotCamera
{
    static class Program
    {
        private static ulong last_interrupt_time = 0;
        private static GpioPin led;
        private static GpioPin button;

        async static Task Main(string[] args)
        {
            // Initialize
            // LED
            led = Gpio.Pin21;
            led.PinMode = GpioPinDriveMode.Output;

            // Button
            button = Gpio.Pin22;
            button.InputPullMode = GpioPinResistorPullMode.PullDown;
            button.PinMode = GpioPinDriveMode.Input;

            Console.WriteLine("Calibrating...");

            await Task.Delay(60 * 1000);

            Console.WriteLine("Ready!");

            button.RegisterInterruptCallback(EdgeDetection.RisingAndFallingEdges, OnButtonClick);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static async void OnButtonClick()
        {
            if (!button.Read())
            {
                ulong interrupt_time = WiringPi.millis();
                if (interrupt_time - last_interrupt_time > 500)
                {
                    Console.WriteLine("Detected something...");
                    last_interrupt_time = interrupt_time;
                    if (Camera.IsBusy)
                    {
                        Console.WriteLine("Camera is busy.");
                        return;
                    }
                    led.Write(true);
                    //Console.WriteLine($"Capturing image...");
                    var result = await Camera.CaptureImageJpegAsync(3280, 2464, default);
                    await File.WriteAllBytesAsync($"image-{DateTime.UtcNow.Ticks}.jpg", result);
                    led.Write(false);
                    //Console.WriteLine($"Image captured.");
                }
            }
        }

        private static async Task RunBlink()
        {
            var led = Gpio.Pin21;
            led.PinMode = GpioPinDriveMode.Output;
            var ledState = false;

            while (true)
            {
                ledState = !ledState;
                led.Write(ledState);
                await Task.Delay(1000);
            }
        }
    }
}
