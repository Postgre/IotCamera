using static Unosquare.RaspberryIO.Pi;
using Components;
using Unosquare.RaspberryIO.Gpio;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace IotCamera
{
    static class Program
    {
        private static RGBLed rgbLed;
        private static Button button;

        static void Main(string[] args)
        {
            Tag();
        }

        private static void Test() 
        {
            rgbLed = new RGBLed(
            Gpio.GetPinByBcmPinNumber(16),
            Gpio.GetPinByBcmPinNumber(20),
            Gpio.GetPinByBcmPinNumber(21));

            button = new Button(Gpio.GetPinByBcmPinNumber(4));
            button.Click += async (s, e) =>
            {
                Console.WriteLine("Button was pressed.");

                rgbLed.SetColor(255, 0, 0);

                await Task.Delay(1000);

                rgbLed.SetColor(0, 255, 0);

                await Task.Delay(1000);

                rgbLed.SetColor(0, 0, 255);

                await Task.Delay(1000);

                rgbLed.State = false;
            };

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static void Tag()
        {
            var mFRC522 = new MFRC522();

            while (true)
            {
                // Scan for cards
                var (status, TagType) = mFRC522.Request(MFRC522.PICC_REQIDL);

                // If a card is found
                if (status == MFRC522.MI_OK)
                {
                    Console.WriteLine("Card detected");
                }

                // Get the UID of the card
                var (status2, uid) = mFRC522.Anticoll();

                // If we have the UID, continue
                if (status2 == MFRC522.MI_OK)
                {
                    // Print UID
                    Console.WriteLine("Card read UID: " + (uid[0]) + "," + (uid[1]) + "," + (uid[2]) + "," + (uid[3]));

                    // This is the default key for authentication
                    var key = new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };

                    // Select the scanned tag
                    mFRC522.SelectTag(uid);

                    // Authenticate
                    var status3 = mFRC522.Auth(MFRC522.PICC_AUTHENT1A, 8, key, uid);

                    // Check if authenticated
                    if (status3 == MFRC522.MI_OK)
                    {
                        mFRC522.ReadSpi(8);
                        mFRC522.StopCrypto1();
                    }
                    else
                    {
                        Console.WriteLine("Authentication error");
                    }
                }
            }
        }
    }

    public static class GpioExtensions
    {
        public static GpioPin GetPinByNumber(this GpioController controller, int pinNumber)
        {
            return controller.Pins.FirstOrDefault(pin => pin.PinNumber == pinNumber);
        }

        public static GpioPin GetPinByBcmPinNumber(this GpioController controller, int pinNumber)
        {
            return controller.Pins.FirstOrDefault(pin => pin.BcmPinNumber == pinNumber);
        }

        public static GpioPin GetPinByWiringPiPinNumber(this GpioController controller, int pinNumber)
        {
            return controller.Pins.FirstOrDefault(pin => pin.WiringPiPinNumber == (WiringPiPin)pinNumber);
        }
    }
}
