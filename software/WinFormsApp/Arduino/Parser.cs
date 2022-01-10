using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Arduino
{
    public struct hardParam
    {
        public string name;
        public Type type;
        public string controlType;
        public string min;
        public string max;
        public string def;
        public int timer;

        public int count;
        public ushort offset;
        public string cmd;
    }

    public class Parser
    {

        public List<IProperty> Properties = new();   

        public List<Control> Controls = new();

        IHardware _device;


        public Parser(IHardware device)
        {
            _device = device;
        }

        Dictionary<string, hardParam> dataTypes = new() 
        {
            { "unsigned char", new() {name ="", type = typeof(byte),  controlType = "F", min = "0", max = "255", def = "0", timer = 1 } },
            { "signed char", new() { name = "", type = typeof(sbyte),  controlType = "F", min = "-128", max = "127", def = "0", timer = 1 } },
            { "unsigned int", new () { name = "", type = typeof(ushort),  controlType = "F", min = "0", max = "65535", def = "0", timer = 1 } },
            { "signed int", new () { name = "", type = typeof(short), controlType = "F", min = "-32768", max = "32767", def = "0", timer = 1 } },
            { "float", new() { name = "", type = typeof(float),  controlType = "F", min = "-3.402823e38", max = "3.402823e38", def = "0", timer = 1 } },
        };

        Dictionary<string, Type> propTypes = new()
        {
            { "F", typeof(PropertyF<>) },
            { "S", typeof(PropertyS<>) },
            { "H", typeof(PropertyH<>) },
            { "Z", typeof(PropertyZ<>) },
            { "B", typeof(PropertyB<>) },
            { "V", typeof(PropertyV<>) },
        };

        List<string> separators = new(){ "struct", "//", ";", "\n"};
        List<string> dataParams = new() { "type:", "name:", "min:","max:","def:","timer:","cmd:"};
        List<string> keywords = new();
        
        public void Parse(string fileName)
        {

            keywords.AddRange(separators);
            keywords.AddRange(dataTypes.Keys);
            keywords.AddRange(dataParams);
            keywords = keywords.Select(s => "(" + s + ")").ToList();


            IProperty prop;

            string contents = File.ReadAllText(fileName);

            string[] tokens = Regex.Split(contents, string.Join("|", keywords)).Select(s => s.Trim( new char []{ ' ', '\t'})).Where(s => s != String.Empty).ToArray();

            try
            {
                int i = 0;
                int elementsCount = 0;
                ushort offset = 0;
                /// Поиск ключевого слова "struct" 
                while (tokens[i++] != "struct") { };
                
                /// Ждем открывающую скобку
                while (tokens[i++] != "{") { };

                /// Основной цикл парсинга
                do
                {
                    // Изначально кода в строке может не быть
                    bool dataLine = false;
                    hardParam param = dataTypes["signed int"];

                    if (dataTypes.ContainsKey(tokens[i]))
                    {
                        dataLine = true;
                        param = dataTypes[tokens[i]];
                        param.name = tokens[++i];
                        while (tokens[i++] != ";") { };
                    }
                    // Ищем комментарий
                    if (tokens[i] == "//")
                    {
                        // Обрабатываем до конца строки
                        while (tokens[i] != "\n")
                        {
                            string s = tokens[i];

                            if (dataParams.Contains(s))
                            {
                                if (s == "min:") { param.min = tokens[++i]; }
                                if (s == "max:") { param.max = tokens[++i]; }
                                if (s == "def:") { param.def = tokens[++i]; }
                                if (s == "name:") { param.name = tokens[++i]; }
                                if (s == "type:") { param.controlType = tokens[++i]; }
                                if (s == "timer:") { param.timer = int.Parse(tokens[++i]); }
                                if (s == "cmd:") { param.cmd = tokens[++i];}
                            }
                            else i++;
                        };
                    }

                    // Добавляем свойство в класс, если поле в структуре найдено
                    if (dataLine)
                    {
                        param.count = ++elementsCount;
                        param.offset = offset;
                        offset += (ushort)Marshal.SizeOf(param.type);

                        Type genericType = propTypes[param.controlType].MakeGenericType(param.type);
                        prop = (IProperty)Activator.CreateInstance(genericType, _device,param);

                        _device.RegisterProperty(prop);

                        Controls.Add(prop.Box);
                        continue;
                    }
                    else
                    {
                        if (param.controlType == "B")
                        {
                            IProperty property = new PropertyB<int>(_device, param);
                            Controls.Add(property.Box);//
                        }
                        if (param.controlType == "Z")
                        {
                            IProperty property = new PropertyZ<int>(_device, param);
                            Controls.Add(property.Box);//
                        }

                    }
                } while (tokens[i++] != "}");

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
