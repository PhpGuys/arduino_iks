namespace Arduino
{
    public class PropertyZ<T> : Property<T> where T : struct, IComparable, IEquatable<T>
    {
        Label label = new Label() { Dock = DockStyle.Fill, Location = new Point(10, 10), Font = new Font("Segoe UI", 14,FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };
        public PropertyZ(IHardware hardware, hardParam param) : base(hardware, param)
        {
            _box.Height = 40;
            _box.Controls.Add(label);
        }
        public override void Update()
        {
            label.Text = Name;
        }
    }
}
