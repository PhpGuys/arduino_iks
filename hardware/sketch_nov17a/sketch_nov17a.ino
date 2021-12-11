int i = 0;  // переменная для счетчика имитирующего показания датчика
int led = 13; 

void setup() {
  Serial.begin(9600);    // установим скорость обмена данными
  pinMode(led, OUTPUT);  // и режим работы 13-ого цифрового пина в качестве выхода
}
void loop() {
  i = i + 1;  // чтобы мы смогли заметить что данные изменились
  String stringOne = "Info from Arduino ";
  stringOne += i;  // конкатенация
  Serial.println(stringOne);  // отправляем строку на порт

  char incomingChar;
  
  if (Serial.available() > 0)
  {
    // считываем полученное с порта значение в переменную
    incomingChar = Serial.read();  
   // в зависимости от значения переменной включаем или выключаем LED
    switch (incomingChar) 
    {
      case '1':
        digitalWrite(led, HIGH);
        break;
      case '0':
        digitalWrite(led, LOW);
        break;
    }
  }
  delay(300);
}
