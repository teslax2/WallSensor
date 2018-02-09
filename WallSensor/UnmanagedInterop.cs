using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WallSensor
{
    public delegate void Callback(int control_unit_serial, int probe_serial, StatisticalMeasurementEx values);

    class UnmanagedInterop
    {
        public Callback mInstance;

        public UnmanagedInterop() 
        {
            mInstance = new Callback(MeasurementsCallback);
        }

        public int Open() 
        {
            return McpOpen();
        }

        public int Close()
        {
            return McpClose();
        }

        public int DiscoverControlUnits(ref IntPtr serials,out short count)
        {
            return McpDiscoverControlUnits(ref serials,out count);
        }

        public int DiscoverProbes(int control_unit_serial, ref IntPtr probe_serials, out short probe_count)
        {
            return McpDiscoverProbes(control_unit_serial, ref probe_serials, out probe_count);
        }

        public int SetStatisticalMeasurementExCallback(int control_unit_serial, int probe_serial, Callback callback)
        {
            return McpSetStatisticalMeasurementExCallback(control_unit_serial, probe_serial, callback);
        }

        public int SetSamplingMode(int control_unit_serials, int probe_serials, ESamplingMode eSamplingMode)
        {
            return McpSetSamplingMode(control_unit_serials, probe_serials, eSamplingMode);
        }

        public int SwitchToManualTargetDetection(int control_unit_serial, int probe_serial, int delay, int avg_window_size) 
        {
            return McpSwitchToManualTargetDetection(control_unit_serial, probe_serial, delay, avg_window_size);
        }

        public int SwitchToAutomaticTargetDetection(int control_unit_serial, int probe_serial, int spacing_threshold) 
        {
            return McpSwitchToAutomaticTargetDetection(control_unit_serial, probe_serial, spacing_threshold);
        }

        public int SetMeasurementOutputMode(int control_unit_serial, int probe_serial, EMeasurementOutputMode mode) 
        {
            return McpSetMeasurementOutputMode(control_unit_serial, probe_serial, mode);
        }

        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int McpOpen();
        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int  McpClose();
        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int McpDiscoverControlUnits(ref IntPtr serials,out short count);
        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int  McpDiscoverProbes(int control_unit_serial,ref IntPtr probe_serials, out short probe_count);
        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int  McpSetStatisticalMeasurementExCallback(int control_unit_serial, int probe_serial, Callback callback);
        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int  McpSetSamplingMode(int control_unit_serial, int probe_serial, ESamplingMode mode);
        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int  McpSwitchToManualTargetDetection(int control_unit_serial, int probe_serial, int delay, int avg_window_size);
        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int  McpSwitchToAutomaticTargetDetection(int control_unit_serial, int probe_serial, int spacing_threshold);
        [DllImport("MCP100API", CharSet = CharSet.Auto)]
        private static extern int  McpSetMeasurementOutputMode(int control_unit_serial, int probe_serial, EMeasurementOutputMode mode);


        private void MeasurementsCallback(int control_unit_serial, int probe_serial, StatisticalMeasurementEx values)
        {
            if (values.statistics.average.count != 1)
                return;

            float[] distance = new float[1];
            Marshal.Copy(values.statistics.average.distance, distance, 0, 1);
            float[] thickness = new float[1];
            Marshal.Copy(values.statistics.average.thickness, thickness, 0, 1);

            Console.WriteLine("{0} thickness = {1}", probe_serial, thickness[0]);
            Console.WriteLine("{0} distance = {1}", probe_serial, distance[0]);
            Console.WriteLine("{0} min thickness = {1}", probe_serial, values.statistics.thickness_min);
            Console.WriteLine("{0} max thickness = {1}", probe_serial, values.statistics.thickness_max);
        }


    }

    enum ESamplingMode
    {
        /*!
        @brief Sample is measured by the internal pulsing of a probe.
        */
        FREE_RUN,
        /*!
        @brief Sample is measured after a rising edge of the encoder signal.
        */
        ENCODER
    };

    enum EMeasurementOutputMode
    {
        /**
         @brief Samples are sent unprocessed.
         */
        RAW = 1,
        /**
         @brief	Samples are collected into a single statistics.
         */
        EXTENDED_STATISTICAL,
        /**
         @brief	Distance samples are collected into a single statistics.
         */
        SURFACE_STATISTICAL
    };

    [StructLayout(LayoutKind.Sequential)]  
    public struct StatisticalMeasurementEx
    {
        /*!
        @brief Statistics of the set.
        */
        public StatisticalMeasurement statistics;
        /*!
        @brief The angle in degrees between the light source and the normal of the bottle.
        */
        public float tilt;
        /*!
        @brief Intensity of the top surface of the bottle. Intensity range is 0-800.
        */
        public float intensity;
        /**
        @brief Raw angle value.

        This is only for calibration purposes!
        */
        public float distance_diff;

        /**
        @brief Raw thickness value.

        This is only for calibration purposes!
        */
        public float uncompensated_thickness;

        /**
         @brief Compensated, but unscaled thickness value.

         This is only for calibration purposes!
        */
        public float unscaled_thickness;

    };

    [StructLayout(LayoutKind.Sequential)]  
    public struct StatisticalMeasurement
    {
        /*!
        @brief Average measures of the set.
        */
        public Measurement average;
        /*!
        @brief Minimum bottle thickness in micrometers.
        */
        public float thickness_min;
        /*!
        @brief Maximum bottle thickness in micrometers.
        */
        public float thickness_max;
        /*!
        @brief Running index of measured targets. This is reseted when Trigger 2 is active on control unit. This can be used for tracking cavity index of blow molder.
        */
        public uint cavity_index;
        /*!
        @brief Number of samples aqcuired on measurement 
        */
        public ushort sample_count;
        /*!
        @brief see \ref ERejectionStatus
        */
        public ERejectionStatus rejection_status;
        /*!
        @brief Running index of measured targets from beginning of powering up the probe. This can be used for tracking the individual bottles.
        */
        public uint reference_index;
        /*!
        @brief Mark if measurement tilt was uncompensated.
        */
        public ushort uncompensated;

    };

    [StructLayout(LayoutKind.Sequential)]  
    public class Measurement
    {

        /*!
        @brief Collected bottle thickness values in micrometers.
        */
        public IntPtr thickness;
        /*!
        @brief Collected distance values from the probe to a bottle in micrometers.
        */
        public IntPtr distance;
        /*!
        @brief Delay from trigger pulse to the first valid thickness reading.
	
        If ESamplingMode::FREE_RUN is applied, the unit is milliseconds. If ESamplingMode::ENCODER is applied, the unit is pulses.
        */
        public ushort start;
        /*!
        @brief Delay from trigger pulse to the last valid thickness reading.
	
        If ESamplingMode::FREE_RUN is applied, the unit is milliseconds. If ESamplingMode::ENCODER is applied, the unit is pulses.
        */
        public ushort end;
        /**
         @brief	Number of collected samples in both thickness and distance arrays.
         */
        public uint count;
     };

    public enum ERejectionStatus
    {
        /*!
        @brief The wall thickness was inside rejection limits.
        */
        NOT_REJECTED,
        /*!
        @brief The wall thickness was below minimum rejection limit.
        */
        MIN_REJECTION,
        /*!
        @brief The wall thickness was above maximum rejection limit.
        */
        MAX_REJECTION,
        /*!
        @brief The bottle was rejected for Quality analysis
        */
        QA_REJECTION
    };
}
