using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth;

//////using PIRSensor.Lib2;
using Windows.Security.Cryptography.Certificates;

namespace PIRSensor
{
    internal class Program
    {
        // GATT device GUID
        private static Guid GUID_BLUETOOTH_GATT_SERVICE_DEVICE_INTERFACE = new Guid("6E3BB679-4372-40C8-9EAA-4509DF260CD8");

        // Shock (motion) sensor guid.
        private static Guid SHOCK_SENSOR_SERVICE_GUID = new Guid("00002b50-0000-1000-8000-00805f9b34fb");
        // Motion sensor guid.
        private static Guid MOTION_SENSOR_SERVICE_GUID = new Guid("00002b40-0000-1000-8000-00805f9b34fb");

        // Selector for all GATT devices..
        internal static string GetServiceSelector()
        {
            return string.Concat("System.Devices.InterfaceClassGuid:=\"{", GUID_BLUETOOTH_GATT_SERVICE_DEVICE_INTERFACE.ToString(), "}\" AND System.Devices.InterfaceEnabled:=System.StructuredQueryType.Boolean#True");
        }

        private static DeviceInformation dev1 = null;
        private static GattDeviceService srv1 = null;
        private static GattCharacteristic ch1 = null;

         

        // Device info dumper.
        private static void Main()
        {

            ////////// Dump device info
            ////////EnumDevices().Wait();

            ////////// Try to read from sensors.
            ////////ReadShockSensor(SHOCK_SENSOR_SERVICE_GUID).Wait();
            ////ReadShockSensor(MOTION_SENSOR_SERVICE_GUID).Wait();

            // Enable sensor notifications
            //EnableSensNotification(SHOCK_SENSOR_SERVICE_GUID).Wait();
            EnableSensNotification(MOTION_SENSOR_SERVICE_GUID).Wait();

            ////////// Test API
            ////////EnumSensors().Wait();

            while (true)
            {
                Thread.CurrentThread.Join(100);
            }
        }

        private static async Task EnableSensNotification(Guid shockSensorServiceGuid)
        {

            var gattDevices = await DeviceInformation.FindAllAsync(GetServiceSelector());
            foreach (var gattDevice in gattDevices)
            {

                try
                {

                    var gattServce = await GattDeviceService.FromIdAsync(gattDevice.Id);
                    if (gattServce == null)
                    {
                        Console.WriteLine("Failed to open device: {0}", gattDevice.Id);
                        continue;
                    }

                    Console.WriteLine("ServiceUUID={0}", gattServce.Uuid);


                    var allCharacteristics = gattServce.GetCharacteristics(shockSensorServiceGuid);
                    if (allCharacteristics == null || allCharacteristics.Count == 0)
                    {
                        continue;
                    }

                    var gattCharacteristic = allCharacteristics[0];

                    dev1 = gattDevice;
                    srv1 = gattServce;
                    ch1 = gattCharacteristic;



                    Console.WriteLine("Setting chararcteristic config to Notify....");
                    var charConfig = await gattCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
                    if (charConfig.Status == GattCommunicationStatus.Success &&
                        charConfig.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Notify)
                    {
                        await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    }

                    gattCharacteristic.ValueChanged += (sender, args) => {
                        var value = args.CharacteristicValue.ToArray();
                        var strValue = BitConverter.ToString(value);
                        Console.WriteLine("Char={0} changed at {1}...", shockSensorServiceGuid, args.Timestamp);
                        Console.WriteLine("NewValue={0}", strValue);
                    };

                }
                catch (Exception e)
                {
                    Console.WriteLine("Error occured...");
                    Console.WriteLine(e.ToString());
                }

            }
        }

        private static async Task ReadShockSensor(Guid charId)
        {

            var gattDevices = await DeviceInformation.FindAllAsync(GetServiceSelector());
            foreach (var gattDevice in gattDevices)
            {

                try
                {

                    var gattServce = await GattDeviceService.FromIdAsync(gattDevice.Id);
                    if (gattServce == null)
                    {
                        Console.WriteLine("Failed to open device: {0}", gattDevice.Id);
                        continue;
                    }


                    var allCharacteristics = gattServce.GetCharacteristics(charId);
                    if (allCharacteristics == null || allCharacteristics.Count == 0)
                    {
                        continue;
                    }

                    var gattCharacteristic = allCharacteristics[0];

                    Console.WriteLine("Setting chararcteristic config to Indicate....");
                    var charConfig = await gattCharacteristic.ReadClientCharacteristicConfigurationDescriptorAsync();
                    if (charConfig.Status == GattCommunicationStatus.Success &&
                        charConfig.ClientCharacteristicConfigurationDescriptor != GattClientCharacteristicConfigurationDescriptorValue.Indicate)
                    {
                        await gattCharacteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                    }

                    Console.WriteLine("Reading characteristic value....");
                    var charValue = await gattCharacteristic.ReadValueAsync(BluetoothCacheMode.Uncached);

                    var strValue = BitConverter.ToString(charValue.Value.ToArray());
                    Console.WriteLine("Characteristic={0} Handle={2} Value={1}", gattCharacteristic.Uuid, strValue, gattCharacteristic.AttributeHandle);


                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }


            }

        }

        // Enumerates all avaialble TokenCube sensors.
        private static async Task EnumSensors()
        {
            //////var sensorService = SensorsService.Instanse;
            //////var devices = sensorService.GetAvailableDevices();

            //////foreach (var device in devices) {
            //////    Console.WriteLine(" found device : {0}", device.MacAddress);

            //////    var sensors = device.GetSensors();
            //////    foreach (var sensor in sensors) {
            //////        switch (sensor.Kind) {
            //////            case SensorKind.MechanicalShock: {
            //////                    var mechanicalShock = (MechanicalShock)sensor.Value;
            //////                    Console.WriteLine("\tKind: {0}, Value: {1}",
            //////                        sensor.Kind,
            //////                        mechanicalShock);
            //////                }
            //////                break;

            //////            case SensorKind.Orientation: {
            //////                    var orientationValue = (Orientation)sensor.Value;
            //////                    Console.WriteLine("\tKind: {0}, Value: {1}",
            //////                        sensor.Kind,
            //////                        orientationValue);
            //////                }
            //////                break;

            //////            case SensorKind.PIRMotion:
            //////            case SensorKind.Motion: {
            //////                    var sensorValue = (bool)sensor.Value;
            //////                    Console.WriteLine("\tKind: {0}, Value: {1}", sensor.Kind, sensorValue);
            //////                }
            //////                break;

            //////            case SensorKind.Battery:
            //////            case SensorKind.Humidity:
            //////            case SensorKind.Pressure:
            //////            case SensorKind.Temperature: {
            //////                    Console.WriteLine("\tKind: {0}, Value: {1}",
            //////                        sensor.Kind,
            //////                        Convert.ToDouble(sensor.Value));
            //////                }
            //////                break;
            //////        }
            //////    }


            //////    Console.WriteLine("======================================================");
            //////}

        }

        // Helper method which enumerates all GATT devices.
        private static async Task EnumDevices()
        {
            byte[] baseGuid = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x80, 0x00, 0x00, 0x80, 0x5f, 0x9b, 0x34, 0xfb };

            var devices = await DeviceInformation.FindAllAsync(GetServiceSelector());
            foreach (var device in devices)
            {

                Console.WriteLine("devId='{0}' name='{1}'", device.Id, device.Name);

                var gattServce = await GattDeviceService.FromIdAsync(device.Id);


                if (gattServce != null)
                {
                    Console.WriteLine("ServiceUUID={0}", gattServce.Uuid);

                    for (uint i = 0; i <= 0xffff; i++)
                    {
                        baseGuid[1] = (byte)((i & 0xFF00) >> 8);
                        baseGuid[0] = (byte)(i & 0x00FF);

                        var charGuid = new Guid(baseGuid);
                        Console.WriteLine(charGuid);


                        var chars = gattServce.GetCharacteristics(charGuid);
                        if (chars == null || chars.Count == 0)
                        {
                            continue;
                        }

                        foreach (var gattCharacteristic in chars)
                        {

                            var strValue = string.Empty;
                            try
                            {
                                var value = await gattCharacteristic.ReadValueAsync();
                                strValue = BitConverter.ToString(value.Value.ToArray());
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.ToString());
                            }

                            var propValue = CharPropertyToString(gattCharacteristic.CharacteristicProperties);
                            Console.WriteLine("Char: {0} Value: {1}, Props: {2}", gattCharacteristic.UserDescription, strValue, propValue);

                            var wellKnownDesk = new List<Guid>() {
                                GattDescriptorUuids.CharacteristicAggregateFormat,
                                GattDescriptorUuids.CharacteristicExtendedProperties,
                                GattDescriptorUuids.CharacteristicPresentationFormat,
                                GattDescriptorUuids.CharacteristicUserDescription,
                                GattDescriptorUuids.ClientCharacteristicConfiguration,
                                GattDescriptorUuids.ServerCharacteristicConfiguration };

                            foreach (var d in wellKnownDesk)
                            {
                                var des = gattCharacteristic.GetDescriptors(d);
                                foreach (var gattDescriptor in des)
                                {
                                    try
                                    {
                                        var value2 = await gattDescriptor.ReadValueAsync();
                                        var strValue2 = BitConverter.ToString(value2.Value.ToArray());
                                        Console.WriteLine("  >>>> desc: {0}, value: {1}", gattDescriptor.Uuid, strValue2);

                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.ToString());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        private static string CharPropertyToString(GattCharacteristicProperties prop)
        {
            var sb = new StringBuilder();
            var strList = new List<string>();

            if (prop == GattCharacteristicProperties.None)
            {
                return "None";
            }

            if ((prop & GattCharacteristicProperties.Broadcast) == GattCharacteristicProperties.Broadcast)
            {
                strList.Add("Broadcast");
            }

            if ((prop & GattCharacteristicProperties.Read) == GattCharacteristicProperties.Read)
            {
                strList.Add("Read");
            }

            if ((prop & GattCharacteristicProperties.WriteWithoutResponse) == GattCharacteristicProperties.WriteWithoutResponse)
            {
                strList.Add("WriteWithoutResponse ");
            }

            if ((prop & GattCharacteristicProperties.Write) == GattCharacteristicProperties.Write)
            {
                strList.Add("Write");
            }

            if ((prop & GattCharacteristicProperties.Notify) == GattCharacteristicProperties.Notify)
            {
                strList.Add("Notify");
            }

            if ((prop & GattCharacteristicProperties.Indicate) == GattCharacteristicProperties.Indicate)
            {
                strList.Add("Indicate");
            }

            if ((prop & GattCharacteristicProperties.AuthenticatedSignedWrites) == GattCharacteristicProperties.AuthenticatedSignedWrites)
            {
                strList.Add("AuthenticatedSignedWrites");
            }

            if ((prop & GattCharacteristicProperties.ExtendedProperties) == GattCharacteristicProperties.ExtendedProperties)
            {
                strList.Add("ExtendedProperties");
            }

            if ((prop & GattCharacteristicProperties.ReliableWrites) == GattCharacteristicProperties.ReliableWrites)
            {
                strList.Add("ReliableWrites");
            }

            if ((prop & GattCharacteristicProperties.WritableAuxiliaries) == GattCharacteristicProperties.WritableAuxiliaries)
            {
                strList.Add("WritableAuxiliaries");
            }

            return string.Join(" | ", strList);
        }
    }
}
