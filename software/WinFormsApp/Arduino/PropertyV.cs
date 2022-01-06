namespace Arduino
{
    public class PropertyV<T> : Property<T> where T : struct, IComparable, IEquatable<T>
    {
        Label label = new Label() { Size = new Size(120, 20), Location = new Point(10, 10), TextAlign = ContentAlignment.MiddleRight };
        Label label2 = new Label() { Size = new Size(80, 20), Location = new Point(130, 10), Font = new Font("Segoe UI", 10, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

        public PropertyV(IHardware hardware, hardParam param) : base(hardware, param)
        {
            _box.Height = 40;
            label.Text = Name.ToString();
            _box.Controls.Add(label2);
            _box.Controls.Add(label);
        }
        public override void Update()
        {
            label2.Text = Value.ToString();
        }
    }
}
