#include "file.h"

// Объявление кодов команд и макроса валидации для них
#define cmdRead 0x01
#define cmdWrite 0x02
#define cmdButton 0x03

#define CommandValid(cmd_) (cmd_ >= cmdRead && cmd_<= cmdButton)

#define cmdError 0x04

#define markerStart 0xF8
#define markerStop 0xF9
#define markerEsc 0xFA


// Коды ошибок чтения данных
enum ReadResult { ReadOK = 0x00, ReadError, CodeInvalid, OffsetInvalid, LenInvalid, CRCError};


int i = 0;  // переменная для счетчика имитирующего показания датчика
int led = 13;
Control control;
byte* ptrControl = (byte *)&control;

int cTimeOut = -1;

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


// Запись в порт с экранированием
void WriteEscaped(byte b)
{
    if (b == markerStart || b == markerStop || b == markerEsc)
    {
      Serial.write(markerEsc);
    }
    Serial.write(b); 
}



// Функция записи элемента заданной длины в выходной поток Serial с подсчетом контрольной суммы и экранированием
void WriteValue(byte* value, int len, byte* crc)
{
  for (int i = 0; i < len; i++)
  {
    WriteEscaped(value[i]);
    *crc ^= value[i];
  }
}

void SendError(ReadResult err)
{
  byte crc = 0;
  byte code = cmdError;
  byte data = err;

  Serial.write((byte)markerStart);
  WriteValue(&code, sizeof(code), &crc);
  WriteValue(&data, sizeof(data), &crc);
  WriteEscaped(crc);
  Serial.write((byte)markerStop);
  cTimeOut = -1;
}

void SendData(int offset, int len)
{
  byte crc = 0;
  byte code = cmdRead;

  Serial.write((byte)markerStart);
  WriteValue(&code, sizeof(code), &crc);
  WriteValue((byte*)&offset, sizeof(offset), &crc);
  WriteValue((byte*)&len, sizeof(len), &crc);
  WriteValue(&ptrControl[offset], len, &crc);
  WriteEscaped(crc);
  Serial.write((byte)markerStop);
}


void ParseByte(byte b)
{
  static bool _escaped = false;
  static byte RecievedBytes[64];
  static byte pos = 0;
  byte crc = 0;

  if(_escaped)
  {
    RecievedBytes[pos++] = b;
    _escaped = false;
  }
  else
  {
    switch (b)
    {
      case markerStart:
        if(pos > 0) 
        {
          pos = 0; SendError(ReadError); // Неожиданный старт пакета
        } else 
        {
          cTimeOut = 0; // Начало отчёта. Ожидаем байты в порту до таймоута
        }
      break;
      
      case markerStop:
        if(pos < 2)
        {
          pos = 0; SendError(ReadError); // Неожиданный конец пакета
        }

        // Проверка CRC
        crc = 0; // "s_of_len_data1_esc_s_data2_crc_e";
        for (int i = 0; i <pos; i++) crc ^= RecievedBytes[i];
        if (crc !=0)
        {
          pos = 0; SendError(CRCError); // Ошибка контрольной суммы
        }
        else
        {
          ProcessPacket(RecievedBytes, pos);
          pos = 0;
          cTimeOut = -1; // Не ожидаем таймаута для байтов из порта
        }
      break;
      
      case markerEsc:
        if(pos == 0)
        {
          SendError(ReadError); // Неожиданный Esc-символ
        }
        else
        {
          _escaped = true;
        }
      break;
      
      default: RecievedBytes[pos++] = b;
    }
  }
  if (pos > 32)
  {
    pos = 0;
    SendError(LenInvalid); // Слишком длинный пакет
  }  
}

void ProcessPacket(byte* RecievedBytes, byte packetLen)
{
  unsigned int offset, len;
  
  byte code = RecievedBytes[0];

  byte prefixLen = sizeof(code) + sizeof(offset) + sizeof(len);

  switch(code)
  {
    case cmdRead:
    case cmdWrite:
    {
      if (packetLen < prefixLen)
      {
        // Слишком короткий пакет
        SendError(ReadError); return; 
      }
      offset = (unsigned int)(RecievedBytes[1] + (RecievedBytes[2] << 8));
      len = (unsigned int)(RecievedBytes[3] + (RecievedBytes[4] << 8));

      // Валидация смещения (смещение + длина не должны выходить за пределы структуры)
      if (offset + len > sizeof(Control))
      {
        SendError(OffsetInvalid); return; 
      }

      // Только при записи
      if (code == cmdWrite)
      {
        // Валидация длины изменяемых данных (допустимые значения длин данных - 1,2,4 байта)
        if (len != 1 && len != 2 && len != 4) 
        {
          SendError(LenInvalid); return;
        }

        // К этому моменту мы точно знаем должную длину пакета
        if (packetLen != sizeof(code) + sizeof(offset) + sizeof(len) + len + 1)
        {
           SendError(LenInvalid); return;         
        }
        memcpy(&ptrControl[offset], RecievedBytes+prefixLen, len);
      }
      // Возврат прочитанных/измененных данных
      SendData(offset, len);
    }
    break;

    // Нажатие кнопки
    case cmdButton:
    {
      if (packetLen < sizeof(code) + sizeof(len))
      {
        // Слишком короткий пакет
        SendError(ReadError); return; 
      }
      // Чтeние длины команды
      len = (unsigned int)(RecievedBytes[3] + (RecievedBytes[4] << 8));     

      // Валидация длины (не более 32 байт)
      if (len > 32)
      {
        SendError(LenInvalid); return; 
      }

      // Выполнение команды по кнопке
      OnButton(len, RecievedBytes);
    }
    break;
    default:SendError(CodeInvalid); return; 
    
  }
}

void loop() {
  if (Serial.available())
  {
    ParseByte(Serial.read());
  } else if (cTimeOut != -1)
  {
    if (++cTimeOut > 20)
    {
      SendError(ReadError);
      cTimeOut = -1;
    }
    delay(1);
  }
}
