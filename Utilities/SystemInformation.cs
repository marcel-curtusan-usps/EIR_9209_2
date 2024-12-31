using System;
using System.Linq;
using System.Management;
namespace EIR_9209_2.Utilities
{
    public class SystemInformation : ISystemInformation
    {
        private static ManagementObjectSearcher baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
        private static ManagementObjectSearcher motherboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_MotherboardDevice");
        private static ManagementObjectSearcher computersystemSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_ComputerSystemProduct");
        private static ManagementObjectSearcher networkSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled = TRUE");

        static public string GetIPAddress()
        {
            try
            {
                foreach (ManagementObject queryObj in networkSearcher.Get().Cast<ManagementObject>())
                {
                    string[] ipAddresses = (string[])queryObj["IPAddress"];
                    if (ipAddresses != null && ipAddresses.Length > 0)
                    {
                        return ipAddresses[0]; // Return the first IP address found
                    }
                }
                return "";
            }
            catch (Exception e)
            {
                return "";
            }
        }
        static public string Availability
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in motherboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return GetAvailability(int.Parse(queryObj["Availability"].ToString()));
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }
        static public bool HostingBoard
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        if (queryObj["HostingBoard"].ToString() == "True")
                        { return true; }
                        else
                        { return false; }
                    }
                    return false;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }

        static public string InstallDate
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return ConvertToDateTime(queryObj["InstallDate"].ToString());
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string Manufacturer
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["Manufacturer"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string Model
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["Model"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string PartNumber
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["PartNumber"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string PNPDeviceID
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in motherboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["PNPDeviceID"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string PrimaryBusType
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in motherboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["PrimaryBusType"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string Product
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["Product"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public bool Removable
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        if (queryObj["Removable"].ToString() == "True")
                        { return true; }
                        else
                        { return false; }
                    }
                    return false;
                }
                catch (Exception e)
                {
          
                    return false;
                }
            }
        }

        static public bool Replaceable
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        if (queryObj["Replaceable"].ToString() == "True")
                        { return true; }
                        else
                        { return false; }
                    }
                    return false;
                }
                catch (Exception e)
                {
                 
                    return false;
                }
            }
        }

        static public string RevisionNumber
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in motherboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["RevisionNumber"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string SecondaryBusType
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in motherboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["SecondaryBusType"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string SerialNumber
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["SerialNumber"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
               
                    return "";
                }
            }
        }
        static public string IdentifyingNumber
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in computersystemSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["IdentifyingNumber"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        static public string Status
        {
            get
            {
                try
                {
                    foreach (ManagementObject querObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return querObj["Status"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
               
                    return "";
                }
            }
        }

        static public string SystemName
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in motherboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["SystemName"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                   
                    return "";
                }
            }
        }

        static public string Version
        {
            get
            {
                try
                {
                    foreach (ManagementObject queryObj in baseboardSearcher.Get().Cast<ManagementObject>())
                    {
                        return queryObj["Version"].ToString();
                    }
                    return "";
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        private static string GetAvailability(int availability)
        {
            switch (availability)
            {
                case 1: return "Other";
                case 2: return "Unknown";
                case 3: return "Running or Full Power";
                case 4: return "Warning";
                case 5: return "In Test";
                case 6: return "Not Applicable";
                case 7: return "Power Off";
                case 8: return "Off Line";
                case 9: return "Off Duty";
                case 10: return "Degraded";
                case 11: return "Not Installed";
                case 12: return "Install Error";
                case 13: return "Power Save - Unknown";
                case 14: return "Power Save - Low Power Mode";
                case 15: return "Power Save - Standby";
                case 16: return "Power Cycle";
                case 17: return "Power Save - Warning";
                default: return "Unknown";
            }
        }

        private static string ConvertToDateTime(string unconvertedTime)
        {
            string convertedTime = "";
            int year = int.Parse(unconvertedTime.Substring(0, 4));
            int month = int.Parse(unconvertedTime.Substring(4, 2));
            int date = int.Parse(unconvertedTime.Substring(6, 2));
            int hours = int.Parse(unconvertedTime.Substring(8, 2));
            int minutes = int.Parse(unconvertedTime.Substring(10, 2));
            int seconds = int.Parse(unconvertedTime.Substring(12, 2));
            string meridian = "AM";
            if (hours > 12)
            {
                hours -= 12;
                meridian = "PM";
            }
            convertedTime = date.ToString() + "/" + month.ToString() + "/" + year.ToString() + " " +
            hours.ToString() + ":" + minutes.ToString() + ":" + seconds.ToString() + " " + meridian;
            return convertedTime;
        }
    }
}
