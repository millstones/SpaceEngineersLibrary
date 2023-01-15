using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage.GameServices;
using VRageMath;

namespace IngameScript
{
    sealed class PidController
    {
        double _processVariable = 0;

        public PidController(double gainProportional, double gainIntegral, double gainDerivative, double outputMax,
            double outputMin)
        {
            this.GainDerivative = gainDerivative;
            this.GainIntegral = gainIntegral;
            this.GainProportional = gainProportional;
            this.OutputMax = outputMax;
            this.OutputMin = outputMin;
        }

        /// <summary>
        /// The controller output
        /// </summary>
        /// <param name="timeSinceLastUpdate">timespan of the elapsed time
        /// since the previous time that ControlVariable was called</param>
        /// <returns>Value of the variable that needs to be controlled</returns>
        public double ControlVariable(TimeSpan timeSinceLastUpdate)
        {
            double error = SetPoint - ProcessVariable;

            // integral term calculation
            IntegralTerm += (GainIntegral * error * timeSinceLastUpdate.TotalSeconds);
            IntegralTerm = Clamp(IntegralTerm);

            // derivative term calculation
            double dInput = _processVariable - ProcessVariableLast;
            double derivativeTerm = GainDerivative * (dInput / timeSinceLastUpdate.TotalSeconds);

            // proportional term calcullation
            double proportionalTerm = GainProportional * error;

            double output = proportionalTerm + IntegralTerm - derivativeTerm;

            output = Clamp(output);

            return output;
        }

        /// <summary>
        /// The derivative term is proportional to the rate of
        /// change of the error
        /// </summary>
        public double GainDerivative { get; set; } = 0;

        /// <summary>
        /// The integral term is proportional to both the magnitude
        /// of the error and the duration of the error
        /// </summary>
        public double GainIntegral { get; set; } = 0;

        /// <summary>
        /// The proportional term produces an output value that
        /// is proportional to the current error value
        /// </summary>
        /// <remarks>
        /// Tuning theory and industrial practice indicate that the
        /// proportional term should contribute the bulk of the output change.
        /// </remarks>
        public double GainProportional { get; set; } = 0;

        /// <summary>
        /// The max output value the control device can accept.
        /// </summary>
        public double OutputMax { get; private set; } = 0;

        /// <summary>
        /// The minimum ouput value the control device can accept.
        /// </summary>
        public double OutputMin { get; private set; } = 0;

        /// <summary>
        /// Adjustment made by considering the accumulated error over time
        /// </summary>
        /// <remarks>
        /// An alternative formulation of the integral action, is the
        /// proportional-summation-difference used in discrete-time systems
        /// </remarks>
        public double IntegralTerm { get; private set; } = 0;


        /// <summary>
        /// The current value
        /// </summary>
        public double ProcessVariable
        {
            get { return _processVariable; }
            set
            {
                ProcessVariableLast = _processVariable;
                _processVariable = value;
            }
        }

        /// <summary>
        /// The last reported value (used to calculate the rate of change)
        /// </summary>
        public double ProcessVariableLast { get; private set; } = 0;

        /// <summary>
        /// The desired value
        /// </summary>
        public double SetPoint { get; set; } = 0;

        /// <summary>
        /// Limit a variable to the set OutputMax and OutputMin properties
        /// </summary>
        /// <returns>
        /// A value that is between the OutputMax and OutputMin properties
        /// </returns>
        /// <remarks>
        /// Inspiration from http://stackoverflow.com/questions/3176602/how-to-force-a-number-to-be-in-a-range-in-c
        /// </remarks>
        double Clamp(double variableToClamp)
        {
            if (variableToClamp <= OutputMin)
            {
                return OutputMin;
            }

            if (variableToClamp >= OutputMax)
            {
                return OutputMax;
            }

            return variableToClamp;
        }
    }

    /// <summary>
    /// Discrete time PID controller class.
    /// (Whiplash141 - 11/22/2018)
    /// </summary>
    class PID
    {
        public PIDSetup Setups;

        double _timeStep = 0;
        double _inverseTimeStep = 0;
        double _errorSum = 0;
        double _lastError = 0;
        bool _firstRun = true;

        public PID(PIDSetup setup)
        {
            Setups = setup; ;
            _inverseTimeStep = 1 / _timeStep;
        }

        protected virtual double GetIntegral(double currentError, double errorSum, double timeStep)
        {
            return errorSum + currentError * timeStep;
        }
        
        public double Control(double error, double timeStep)
        {
            if (timeStep != _timeStep)
            {
                _timeStep = timeStep;
                _inverseTimeStep = 1 / _timeStep;
            }
            //Compute derivative term
            var errorDerivative = (error - _lastError) * _inverseTimeStep;

            if (_firstRun)
            {
                errorDerivative = 0;
                _firstRun = false;
            }

            //Get error sum
            _errorSum = GetIntegral(error, _errorSum, _timeStep);

            //Store this error as last error
            _lastError = error;

            //Construct output
            return Setups.kp * error + Setups.ki * _errorSum + Setups.kd * errorDerivative;
        }

        public void Reset()
        {
            _errorSum = 0;
            _lastError = 0;
            _firstRun = true;
        }
    }

    class DecayingIntegralPID : PID
    {
        readonly double _decayRatio;

        public DecayingIntegralPID(PIDSetup setup, double decayRatio) : base(setup)
        {
            _decayRatio = decayRatio;
        }

        protected override double GetIntegral(double currentError, double errorSum, double timeStep)
        {
            return errorSum = errorSum * (1.0 - _decayRatio) + currentError * timeStep;
        }
    }

    class ClampedIntegralPID : PID
    {
        readonly double _upperBound;
        readonly double _lowerBound;

        public ClampedIntegralPID(PIDSetup setup, double lowerBound,
            double upperBound) : base(setup)
        {
            _upperBound = upperBound;
            _lowerBound = lowerBound;
        }

        protected override double GetIntegral(double currentError, double errorSum, double timeStep)
        {
            errorSum = errorSum + currentError * timeStep;
            return Math.Min(_upperBound, Math.Max(errorSum, _lowerBound));
        }
    }

    class BufferedIntegralPID : PID
    {
        readonly Queue<double> _integralBuffer = new Queue<double>();
        readonly int _bufferSize;

        public BufferedIntegralPID(PIDSetup setup, int bufferSize) : base(setup)
        {
            _bufferSize = bufferSize;
        }

        protected override double GetIntegral(double currentError, double errorSum, double timeStep)
        {
            if (_integralBuffer.Count == _bufferSize)
                _integralBuffer.Dequeue();
            _integralBuffer.Enqueue(currentError * timeStep);
            return _integralBuffer.Sum();
        }
    }

    struct PIDSetup
    {
#pragma warning disable 649
        public float kp, ki, kd;
#pragma warning restore 649
    }

    class Vector3PID
    {
        public readonly PID X, Y, Z;
        public Vector3D Power;
        public Vector3PID(PIDSetup kx, PIDSetup ky, PIDSetup kz)
        {
            X = new PID(kx);
            Y = new PID(ky);
            Z = new PID(kz);
        }

        public void Control(Vector3 errors, double timeStep)
        {
            Power = new Vector3(
                X.Control(errors.X, timeStep),
                Y.Control(errors.Y, timeStep),
                Z.Control(errors.Z, timeStep));
        }

        public void Reset()
        {
            X.Reset();
            Y.Reset();
            Z.Reset();
        }
    }
}
