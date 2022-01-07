namespace Arduino
{
    public class PropertyB<T> : Property<T> where T : struct, IComparable, IEquatable<T>
    {
        Button button = new Button() { Size = new Size(180, 40), Location = new Point(40, 10), Font = new Font("Segoe UI", 14) };
        public PropertyB(IHardware hardware, hardParam param) : base(hardware, param)
        {
            _box.Height = 60;
            _box.Controls.Add(button);
            button.Click += (s, e) => hardware.SendCommand(param.cmd);
        }
        public override void Update()
        {
            button.Text = Name;
        }
    }
}
