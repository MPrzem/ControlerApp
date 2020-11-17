using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iotpublisher
{
    public class Integral
    {
        float integral_memory;
        int dt;
        private float integralLimit;
        public float IntegralMemory { get { return integral_memory; } private set { } }
        public float Calculate(float start, float end)
        {
            integral_memory += (start + end) / 2 * dt;
            if (integral_memory > integralLimit)
                integral_memory = integralLimit;
            if (integral_memory < -integralLimit)
                integral_memory = -integralLimit;
            return integral_memory;
        }

        public Integral(int _dt, float _integralLimit)
        {
            dt = _dt;
            integralLimit = _integralLimit;
            integral_memory = 0;
        }

    }

    [Serializable]
    public class PID_Values
    {
        public float KP;
        public float TI;
        public int TD;
        public float HUM_expected;
    }
    public class PID : IControler
    {
        PID_Values val;
        float dt;
        private const int deviationTreshold = 2;
        float deviation;
        float deviation_prev;
        Integral integral;
        float out_val;
        public PID(float _dt, PID_Values val_,Integral _integral)
        {
            val=val_;
            dt = _dt;
            integral = _integral;
            deviation = 0;
            deviation_prev = 0;
            out_val = 0;
        }
        private float deriverate()
        {
            return (deviation - deviation_prev) / dt;
        }


        public void getSensData(SensData data)
        {
            deviation_prev = deviation;
            deviation = val.HUM_expected - data.SoilSens;
            deviation = -deviation;
            Console.WriteLine($"dev {deviation}.");
            if (deviation < 0)
                deviation = 0;
            if (deviation < deviationTreshold && deviation > -deviationTreshold)
                deviation = 0;

        }

        public float setOutput()
        {
            out_val = val.KP * (deviation + integral.Calculate(deviation_prev, deviation) / val.TI + val.TD * deriverate());
            if (out_val > 80)
                out_val = 80;
            else if (out_val < 0)
                out_val = 0;
            return out_val;
        }
    }
}
