using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercury230Protocol
{
    enum RequestTypes : byte
    {
        TestConnection = 0x00,        // Тестирование канала связи
        OpenConnection = 0x01,  // Запрос на открытие канала связи
        CloseConnection = 0x02, // Запрос на закрытие канала связи
        ReadArray = 0x05        // Запрос на чтение массивов в пределах 12 месяцев
    }

    enum MeterAccessLevel : byte
    {
        User = 0x01,
        Admin = 0x02
    }

    enum Rates : byte
    {
        Sum,
        First,
        Second,
        Third,
        Losses
    }

    enum Months : byte
    {
        None,
        January,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }

    enum DataArrays : byte
    {
        FromReset,
        CurrentYear,
        PastYear,
        Month,
        CurrentDay,
        PastDay,
        PerPhase
    }
}
