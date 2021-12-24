#include "file.h"

int i = 0;  // переменная для счетчика имитирующего показания датчика
int led = 13;
Control control;
  
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
void loop() {
  
  i = i + 1;
  String stringOne = "button";
  
  byte* ptr1 = (byte*)&control.id;
      // разбиваем на байты
  byte ptr1_1 = *ptr1;
  byte ptr1_2 = *(ptr1 + 1);
  
  byte* ptr2 = (byte*)&control.code;
  byte ptr2_1 = *ptr2;
  
  byte* ptr3 = (byte*)&control.letter;
  byte ptr3_1 = *ptr3;
  
  byte* ptr4 = (byte*)&control.digit;
      // разбиваем на байты
  byte ptr4_1 = *ptr4;
  byte ptr4_2 = *(ptr4 + 1);
  
  byte* ptr5 = (byte*)&control.identifier;
      // разбиваем на байты
  byte ptr5_1 = *ptr5;
  byte ptr5_2 = *(ptr5 + 1);
  byte ptr5_3 = *(ptr5 + 2);
  byte ptr5_4 = *(ptr5 + 3);
  
  char incomingChar;
  String incomingString;
  if (Serial.available() > 0)
  {
    // считываем полученное с порта значение в переменную
    incomingChar = Serial.read();  
   // в зависимости от значения переменной включаем или выключаем LED
    switch (incomingChar) 
    {
      case '1':
        Serial.println("sending...");
        delay(200);
        //control.id
        Serial.println(ptr1_1);
        delay(200);
        Serial.println(ptr1_2);
        delay(200);
      
        //control.code
        Serial.println(ptr2_1);
        delay(200);
      
        //control.letter
        Serial.println(ptr3_1);
        delay(200);
        digitalWrite(led, HIGH);
        break;
      case '0':
        digitalWrite(led, LOW);
        break;
    }
    
  }
  
//
//  //control.digit
//  Serial.println(ptr4_1);
//  delay(200);
//  Serial.println(ptr4_2);
//  delay(200);
//  
//  //control.identifier
//  Serial.println(ptr5_1);
//  delay(200);
//  Serial.println(ptr5_2);
//  delay(200);
//  Serial.println(ptr5_3);
//  delay(200);
//  Serial.println(ptr5_4);
//  delay(200);

  
  delay(3000);
}
