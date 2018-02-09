using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WallSensor
{
    class Program
    {
        private static UnmanagedInterop sensor = new UnmanagedInterop();

        static void Main(string[] args)
        {
            try
                {
                int status = sensor.Open();

	            if (status != 0)
	            {
		            Console.WriteLine("Error in opening the API: ", status);
                    return;
	            }
                short control_unit_count = 0;
	            Console.WriteLine("Discovering control units...");
                IntPtr control_unit_serials = new IntPtr();
                sensor.DiscoverControlUnits(ref control_unit_serials,out control_unit_count);
                int[] result = new int[control_unit_count];
                System.Runtime.InteropServices.Marshal.Copy(control_unit_serials, result, 0, control_unit_count);
	            Console.WriteLine("Found control unit serials {0}", control_unit_count);
                for (int cu = 0; cu < control_unit_count; cu++)
                {

                    if (status == 0)
                    {
                        short probe_count = 0;
                        IntPtr probe_serials = new IntPtr();

                        Console.WriteLine("Requesting probes for control unit: {0}", control_unit_serials);

                        sensor.DiscoverProbes(result[cu], ref probe_serials, out probe_count);
                        int[] probeResult = new int[probe_count];
                        System.Runtime.InteropServices.Marshal.Copy(probe_serials, probeResult, 0, probe_count);

                        Console.WriteLine("Found {0} probe serials for control unit:{1}", probe_count, control_unit_serials);

                        for (int p = 0; p < probe_count; p++)
                        {

                            Console.WriteLine("Settings up probe {0}..", probeResult[p]);

                            status = sensor.SetStatisticalMeasurementExCallback(result[cu], probeResult[p], sensor.mInstance);
                            if (status != 0)
                            {
                                Console.WriteLine("Error in setting the stat. measurement callback of {0}: {1}", probe_serials, status);
                                return;
                            }

                            status = sensor.SetSamplingMode(result[cu], probeResult[p], ESamplingMode.FREE_RUN);
                            if (status != 0)
                            {
                                Console.WriteLine("Error in setting the triggering mode of {0}: {1}", probe_serials, status);
                                return;
                            }

                            status = sensor.SwitchToManualTargetDetection(result[cu], probeResult[p], -1, 300);
                            if (status != 0)
                            {
                                Console.WriteLine("Error in switching {0} to manual detection mode: {1}", probe_serials, status);
                                return;
                            }

                            status = sensor.SetMeasurementOutputMode(result[cu], probeResult[p], EMeasurementOutputMode.EXTENDED_STATISTICAL);
                            if (status != 0)
                            {
                                Console.WriteLine("Error in setting the measurement mode of {0}: {1}", probe_serials, status);
                                return;
                            }

                            Console.WriteLine("Probe {0} set up!", probe_serials);
                        }

                    }
                    else
                    {
                        Console.WriteLine("Error in initializing control unit: {0}", status);
                    }
                }  
      

                Console.WriteLine("Press Enter key to close the application...");
                Console.ReadKey();

                status = sensor.Close();
                if (status != 0)
                {
                    Console.WriteLine("Error in closing the API: {0}", status);
                    return;
                }

                return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    
                }
            }
    }
}
