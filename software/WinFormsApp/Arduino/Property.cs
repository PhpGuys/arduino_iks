using System.Globalization;
using Timer = System.Windows.Forms.Timer;

namespace Arduino
{
    public interface IProperty
    {
        public Panel Box { get; }
       // public void HardwareSet(string x);
        public event EventHandler? PropertyChanged;
        public event EventHandler? PropertyRequest;
        public byte[] RawValue { set; get; }

        public ushort Offset { get; }
    }

    public class Property<T> : IProperty
        where T : struct, IComparable, IEquatable<T>
    {
        private T _value;

       // private IHardware _device;

        protected readonly Timer updateTimer = new();

        protected readonly Timer focusTimer = new() { Interval = 3000 };


        public static byte[] GetBytes<V>(V val, Func<V, byte[]> conversion)
        {
            return conversion(val);
        }

        public string Name {get;}
        protected Panel _box;
        public T Value => _value;

        public byte[] RawValue 
        {
            get => _value.GetBytes();
            set
            {
                _box.BackColor = SystemColors.Control;
                try
                {
                    _value = SetBytes(value);
                    Update();
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }


            }
        }
        public Property(IHardware hardware, hardParam param)
        {
           // _device = hardware;
            Offset = param.offset;
            Min = (T)Convert.ChangeType(param.min, typeof(T), CultureInfo.InvariantCulture);
            Max = (T)Convert.ChangeType(param.max, typeof(T), CultureInfo.InvariantCulture);
            Def = (T)Convert.ChangeType(param.def, typeof(T), CultureInfo.InvariantCulture); 
            _value = Def;
            _box = new Panel() { Width = 260, Height = 70};
            Name = param.name;

            updateTimer.Interval = param.timer * 1000;


            updateTimer.Tick += (sender, args) => { 
                PropertyRequest?.Invoke(this, EventArgs.Empty); 
            };

            focusTimer.Tick += (s, e) => { focusTimer.Stop(); _box.FindForm().ActiveControl = null; };

            updateTimer.Start();

            Update();
        }

        public Panel Box => _box;
        public ushort Offset { get; }

        public event EventHandler? PropertyChanged;
        public event EventHandler? PropertyRequest;

        public bool SetValue(string x)
        {
            T newValue;
            try 
            {
                newValue = (T)Convert.ChangeType(x, typeof(T), CultureInfo.InvariantCulture);
                if (newValue.CompareTo(Max) > 0) newValue = Max;
                else if (newValue.CompareTo(Min) < 0) newValue = Min;
            }
            catch (Exception)
            {
                MessageBox.Show("Некорректное значение");
                newValue = _value;
            }
            if (newValue.Equals(_value)) return false;

            _value = newValue;
            return true;
        }



        public void UserSet(string x)
        {
            if (SetValue(x))
            {
                _box.BackColor = Color.Red;
                PropertyChanged?.Invoke(this, EventArgs.Empty);
            }
            Update();
        }
        //public void HardwareSet(string x)
        //{
        //   // _box.BackColor = SystemColors.Control;
        //   // SetValue(x);
        //   // Update();
        //}

        public T Min {get; }
        public T Max {get; }
        public T Def {get; }

        public virtual void Update() { }

        public static T SetBytes(byte[] data)
        {
            switch (Type.GetTypeCode(typeof(T)))
            {
                case TypeCode.Byte: { return (T)Convert.ChangeType(data[0], typeof(T)); }// (byte) BitConverter.ToGetBytes((byte)ob); }
                case TypeCode.SByte: { return (T)Convert.ChangeType(data[0], typeof(T)); }// (byte) BitConverter.ToGetBytes((byte)ob); }
                case TypeCode.Int16: { return (T)Convert.ChangeType(BitConverter.ToUInt16(data), typeof(T)); }// return BitConverter.GetBytes((sbyte)ob); }
                case TypeCode.UInt16: { return (T)Convert.ChangeType(BitConverter.ToUInt16(data), typeof(T)); }
                case TypeCode.Single: { return (T)Convert.ChangeType(BitConverter.ToSingle(data), typeof(T)); }
                default: { return (T)Activator.CreateInstance(typeof(T)); }
            }
        }

    }


    public static class Extensions
    {
        public static byte[] GetBytes(this Object ob)
        {
            switch (Type.GetTypeCode(ob.GetType()))
            {
                case TypeCode.Byte: { return BitConverter.GetBytes((byte)ob); }
                case TypeCode.SByte: { return BitConverter.GetBytes((sbyte)ob); }
                case TypeCode.Int16: { return BitConverter.GetBytes((short)ob); }
                case TypeCode.UInt16: { return BitConverter.GetBytes((ushort)ob); }
                case TypeCode.Single: { return BitConverter.GetBytes((float)ob); }
                default: { return new byte[] { }; }
            }
        }
    }

}
