namespace Arduino
{
    public class PropertyF<T> : Property<T> where T : struct, IComparable, IEquatable<T>
    {
        TextBox textBox = new TextBox() { Size = new Size(100, 20), Location = new Point(130, 10) };
        Label label = new Label() { Size = new Size(120, 20), Location = new Point(10, 10), TextAlign = ContentAlignment.MiddleRight };



        public PropertyF(IHardware hardware, hardParam param) : base( hardware,  param)
        {
            textBox.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Return)
                {
                    e.Handled = true;
                    UserSet(textBox.Text.Replace(',', '.'));
                }
            };
            textBox.TextChanged += (s, e) => { focusTimer.Stop(); focusTimer.Start(); };

            textBox.Leave += (s, e) => { focusTimer.Stop(); updateTimer.Start(); UserSet(textBox.Text.Replace(',', '.')); };

            textBox.Enter += (s, e) => { focusTimer.Start(); updateTimer.Stop(); };
            


            label.Text = Name.ToString();
            _box.Controls.Add(textBox);
            _box.Controls.Add(label);
        }
        public override void Update()
        {
            textBox.Text = Value.ToString();
        }
    }
}
