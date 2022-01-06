#include "file.h"
int i = 0;  // переменная для счетчика имитирующего показания датчика
int led = 13;
Control control;

byte b[]= {0x02, 0x00, 0x04, 0x00, 0x02, 0x04,0x00, 0x00};
byte buf[64];
void printHex(byte num) {
  char hexCar[2];

  sprintf(hexCar, "%02X", num);
  Serial.print(hexCar);
}


typedef struct {
  int offset;
  int len;
  byte* addr;
}field;

field fields[7];





void setup() {
  String stringOne = "Info from Arduino ";
  Serial.begin(9600);    // установим скорость обмена данными
  pinMode(led, OUTPUT);  // и режим работы 13-ого цифрового пина в качестве выхода

  control.id = 1337;
  control.code = 'b';
  control.letter = 'a';
  control.digit = 255;
  control.identifier = 9.999;

fields[0]= {0, sizeof(control.id), (byte*)&control.id};
fields[1]= {2, sizeof(control.id2), (byte*)&control.id2};
fields[2]= {4, sizeof(control.code), (byte*)&control.code};
fields[3]= {6, sizeof(control.letter), (byte*)&control.letter};
fields[4]= {7, sizeof(control.digit), (byte*)&control.digit};
fields[5]= {9, sizeof(control.identifier), (byte*)&control.identifier};
fields[6]= {13, sizeof(control.digit2), (byte*)&control.digit2};

 
}

void SendField(int index)
{
  byte data = 0x02;
  byte crc = data;
  Serial.write(data);

  Serial.write(0);
  data = fields[index].offset;
  crc ^= data;
  Serial.write(data);

  Serial.write(0);
  data = fields[index].len;
  crc ^= data;
  Serial.write(data);

  for(int i=0; i<fields[index].len; i++)
  {
    data = fields[index].addr[i];
    crc ^= data;
    Serial.write(data);
  }
  Serial.write(crc);
}

int findByOffset(byte offset)
{
  for(int i=0;i<7;i++)
  {
    if (fields[i].offset == offset) return i;
  }
  return 0;
}

void OnButton()
{
  while (Serial.available() < 2){} 
  Serial.read();
  byte len = Serial.read();
  while (Serial.available() < len+1){}
  for (int i=0; i< len; i++)
  {
    Serial.read();
  }  
  Serial.read();
}


void LoadField()
{
  byte offset;
  byte len;
  byte data = 0x02;
  byte crc = data;
  while (Serial.available() < 4){}
  
  Serial.read();
  offset = Serial.read();
  crc ^= offset;
  
  Serial.read();
  len = Serial.read();
  crc ^= len;
  
  while (Serial.available() < len+1){}
  for (int i=0; i< len; i++)
  {
    buf[i] = Serial.read();
    crc ^= buf[i];
  }

  if (crc == Serial.read())
  {
    byte* ptr = fields[findByOffset(offset)].addr; 
    for (int i=0; i< len; i++)
    {
      ptr[i] = buf[i];
    }    
  }
}


void loop() {

byte code;
byte crc;
byte offset;

 if (Serial.available() > 0) {  //Данные пришли
        // считываем код
        code = Serial.read();

         /// Запрос всех полей 
        if (code == 0x01)
        {
            while (Serial.available() <= 0){}
            crc = Serial.read();
            if (crc == 0x01)
            {
               for (int i = 0; i < 7; i ++)
               {
                   SendField(i); 
               }
            }
        }
         /// Изменение одного поля 
        if (code == 0x02)
        {
            LoadField();
        }
         /// Обновление поля 
        if (code == 0x03)
        {
          crc = 0x03;
          while (Serial.available() < 3){}
          Serial.read();
          offset = Serial.read();
          crc ^= offset;
          if (crc == Serial.read())
          {
            SendField(findByOffset(offset));
          }
        }
         /// Кнопка 
        if (code == 0x04)
        {
            OnButton();
        }

        
     }
     digitalWrite(led, HIGH);
}
