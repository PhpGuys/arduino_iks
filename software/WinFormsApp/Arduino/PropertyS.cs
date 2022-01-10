namespace Arduino
{
    public class PropertyS<T> : Property<T> where T : struct, IComparable, IEquatable<T>
    {
        TrackBar trackBar = new TrackBar() { Size = new Size(240, 45), Location = new Point(10, 32) };
        Label label = new Label() { Size = new Size(120, 20), Location = new Point(10, 10), TextAlign = ContentAlignment.MiddleRight };
        Label label2 = new Label() { Size = new Size(80, 20), Location = new Point(130, 10), Font = new Font("Segoe UI", 10, FontStyle.Bold),  TextAlign = ContentAlignment.MiddleCenter };


        public PropertyS(IHardware hardware, hardParam param) : base(hardware, param)
        {
            _box.Height = 80;

           
            trackBar.Maximum = int.Parse(param.max);
            trackBar.Minimum = int.Parse(param.min);
            trackBar.Value = int.Parse(Value.ToString());

            bool clicked = false;
            trackBar.ValueChanged += (s, e) =>
            {
                label2.Text = trackBar.Value.ToString();
                focusTimer.Stop(); 
                focusTimer.Start(); 
            };

            trackBar.MouseWheel += (s, e) => { ((HandledMouseEventArgs)e).Handled = true; };

            trackBar.MouseDown += (s, e) => clicked = true;
            trackBar.KeyDown += (s, e) => { ((KeyEventArgs)e).Handled = true; };


            trackBar.MouseUp += (s, e) =>
            {
                if (!clicked)   return;
                clicked = false;
                UserSet(trackBar.Value.ToString());
            };


            trackBar.Leave += (s, e) => { focusTimer.Stop(); updateTimer.Start(); };

            trackBar.Enter += (s, e) => { focusTimer.Start(); updateTimer.Stop(); };

            label.Text = Name.ToString();
            _box.Controls.Add(label2);
            _box.Controls.Add(label); 
            _box.Controls.Add(trackBar);
        }

        public override void Update()
        {
            try
            {
                trackBar.Value = int.Parse(Value.ToString());
            }
            catch (Exception) { }

            label2.Text = Value.ToString();
        }
    }
}
