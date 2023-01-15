using System;
using System.Collections;

namespace IngameScript
{
    abstract class CNCMotor
    {
        public float SP { get; private set; }
        public float PV => _getPV(this);
        public bool IsWork => _task != null;

        public readonly SEMotor Motor;
        readonly  Func<CNCMotor, float> _getPV;

        TaskBase _task;

        protected CNCMotor(SEMotor motor, Func<CNCMotor, float> getPV = null)
        {
            Motor = motor;
            _getPV = getPV ?? ((m) => m.Motor.L);
        }

        IEnumerator WorkTask(bool fast)
        {
            while (Math.Abs(SP - PV) > Motor.DeadZone)
            {
                if (SP < PV)
                {
                    if (Motor.IsMinL) break;
                    Motor.MoveBackward(fast);
                }
                else
                {
                    if (Motor.IsMaxL) break;
                    Motor.MoveForward(fast);
                }


                yield return null;
            }

            Stop();
        }

        public void Start(float sp, bool fast)
        {
            SP = Clamp(sp);

            _task?.Cancel();
            _task = WorkTask(fast).ToTask().OnComplete(() => _task = null);
            _task.Run();
        }

        public void Stop()
        {
            //_task?.Cancel();
            _task = null;
            
            Motor.Stop();
        }
        
        protected abstract float Clamp(float value);
    }
}