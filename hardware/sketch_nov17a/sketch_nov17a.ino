#include "file.h"

// Объявление кодов команд и макроса валидации для них
#define cmdRead 0x01
#define cmdWrite 0x02
#define cmdButton 0x03
#define CommandValid(cmd_) (cmd_ >= cmdRead && cmd_<= cmdButton) 

#define cmdError 0x04

// Коды ошибок чтения данных
enum ReadResult { ReadOK = 0x00, ReadError, CodeInvalid, OffsetInvalid, LenInvalid, CRCError};


int i = 0;  // переменная для счетчика имитирующего показания датчика
int led = 13;
Control control;
byte* ptrControl = (byte *)&control;

byte b[]= {0x02, 0x00, 0x04, 0x00, 0x02, 0x04,0x00, 0x00};

void printHex(byte num) {
  char hexCar[2];

  sprintf(hexCar, "%02X", num);
  Serial.print(hexCar);
}


void setup() {
  String stringOne = "Info from Arduino ";
  Serial.begin(9600);    // установим скорость обмена данными
  pinMode(led, OUTPUT);  // и режим работы 13-ого цифрового пина в качестве выхода

  control.id = 1337;
  control.code = 'b';
  control.letter = 'a';
  control.digit = 255;
  control.identifier = 9.999;
}


// Обработчик нажатия на кнопку
void OnButton(int len, byte* buf)
{
  
}

void SendError(ReadResult err)
{
  byte crc=0;
  byte code = cmdError;
  byte data = err;
  
  WriteValue(&code,sizeof(code),&crc);
  WriteValue(&data,sizeof(data),&crc);
  Serial.write(crc);  
}


// Функция чтения элемента заданной длины из входного потока Serial с подсчетом контрольной суммы
bool ReadValue(byte* value, int len, byte* crc)
{
  if (Serial.readBytes(value, len) == len)
  {
    for (int i = 0; i < len; i++)
    {
      *crc ^= value[i];
    }
    return true;
  }
  return false;
}


// Функция записи элемента заданной длины в выходной поток Serial с подсчетом контрольной суммы
void WriteValue(byte* value, int len, byte* crc)
{
  for (int i = 0; i < len; i++)
  {
    Serial.write(value[i]);
    *crc ^= value[i];
  }
}

void SendData(int offset, int len)
{
  byte crc=0;
  byte code = cmdRead;
  
  WriteValue(&code,sizeof(code),&crc);
  WriteValue((byte*)&offset,sizeof(offset),&crc);
  WriteValue((byte*)&len,sizeof(len),&crc);
  WriteValue(&ptrControl[offset], len, &crc);
  Serial.write(crc);
}
 
ReadResult ReadPacket()
{
  byte code;
  byte crc = 0;
  byte crcReaded;
  int len;
  int offset;
  byte buf[64];


  // Чтение команды
  if (!ReadValue((byte*)&code,sizeof(code),&crc)) return ReadError;

  // Валидация команды
  if (!CommandValid(code)) return CodeInvalid;

  // Расшифровка, валидация и выполнение команд
  switch (code)
  {
    // Команды чтения из структуры и запись в структуру
    case cmdRead:
    case cmdWrite:
      // Чтение смещения в структуре  
      if (!ReadValue((byte*)&offset,sizeof(offset),&crc)) return ReadError;

      // Чтeние длины извлекаемых/изменяемых данных
      if (!ReadValue((byte*)&len,sizeof(len),&crc)) return ReadError;

      // Валидация смещения (смещение + длина не должны выходить за пределы структуры)  
      if (offset + len > sizeof(Control)) return OffsetInvalid;

      // Только при записи
      if (code == cmdWrite)
      {
        // Валидация длины изменяемых данных (допустимые значения длин данных - 1,2,4 байта)
        if (len != 1 && len != 2 && len != 4) return LenInvalid;

        // Чтение изменяемых данных в буфер
        if (!ReadValue(buf,len,&crc)) return ReadError;
      }

      // Чтeние crc
      if (!ReadValue((byte*)&crcReaded,sizeof(crcReaded),&crc)) return ReadError;
      
      // Валидация crc 
      if (crc != 0) return CRCError;

      // Только при записи
      if (code == cmdWrite)
      {
        memcpy(&ptrControl[offset], buf, len); 
      }      

      // Возврат прочитанных/измененных данных
      SendData(offset, len);
      break;
    // Нажатие кнопки    
    case cmdButton:
      
      // Чтeние длины команды
      if (!ReadValue((byte*)&len,sizeof(len),&crc)) return ReadError;
      
      // Валидация длины (не более 64 байт) 
      if (len > 64) return LenInvalid;
      
      // Чтение команды в буфер
      if (!ReadValue(buf,len,&crc)) return ReadError;

      // Чтeние crc
      if (!ReadValue(&crcReaded,sizeof(crc),&crc)) return ReadError;
      
      // Валидация crc 
      if (crc != 0) return CRCError;

      // Выполнение команды по кнопке
      OnButton(len, buf);      
      
      break;
  }
  return ReadOK;
}


void loop() {

  ReadResult res;
  if(Serial.available())
  {
    res = ReadPacket();
    if (res != ReadOK)
    {
      SendError(res);
    }
  }
  digitalWrite(led, HIGH);
}
