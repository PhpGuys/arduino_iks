#ifndef FILE_H
#define FILE_H


struct Control
{
//type:Z name:общая информация 
    unsigned int id;   //type:F name: ID min:0 max:200 def:100  
    unsigned int id2;   //type:V name: ID2 min:0 max:200 def:100
//type:Z name:ключевые параметры 
    unsigned int code;//type:S name:смещение min:0 max: 100 def:30
    signed char letter;//type:F name:угол поворота min:-15 max:39 def:3 timer: 6
signed char letter;//type:F name:угол поворота min:-15 max:39 def:3 timer: 6
signed char letter;//type:F name:у min:-15 max:39 def:3 timer: 6
signed char letter;//type:F min:-15 max:39 def:3 timer: 6
// type:B name:Старт cmd: 0x23-0x55-0x74
//type:Z name:доп. параметры 
    signed int digit;//type:H name:инверсия
    float identifier;//type:F name:погрешность min: -50.0 max: 80.0 def: 0.661
// type:B name:Завершить cmd: cmd: 0x12-0x34-0xAA
    signed int digit2;//type:H name:инверсия2
};//