
namespace Arduino
{
    public class PropertyH<T> : Property<T> where T : struct, IComparable, IEquatable<T>
    {
        CheckBox checkBox = new CheckBox() { Size = new Size(240, 20), Location = new Point(10, 10) };
        public PropertyH(IHardware hardware, hardParam param) : base(hardware, param)
        {
            _box.Height = 40;
            checkBox.CheckedChanged += (s, e) => UserSet(checkBox.Checked? "1":"0");

            checkBox.Text = Name.ToString();
            _box.Controls.Add(checkBox);

        }
        public override void Update()
        {
            checkBox.Checked = !(Value.ToString() == "0");
        }
    }
}
