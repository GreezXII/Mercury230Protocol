using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mercury230Protocol
{
    enum RequestTypes : byte
    {
        TestConnection = 0x00,  // Тестирование канала связи
        OpenConnection = 0x01,  // Запрос на открытие канала связи
        CloseConnection = 0x02, // Запрос на закрытие канала связи
        ReadSettings = 0x08,    // Запрос на чтение параметров
        ReadJournal = 0x04,     // Запрос на чтение массивов времени (журналов)
        ReadArray = 0x05,       // Запрос на чтение массивов энергии в пределах 12 месяцев
        WriteSettings = 0x03,   // Запрос на запись параметров
    }

    enum Settings : byte
    {
        SerialNumberAndReleaseDate = 0x00,  // Серийный номер и дата выпуска
        SoftwareVersion = 0x03,             // Версия ПО
        Location = 0x0B                     // Местоположение
    }

    enum Journals : byte
    {
        OnOff = 0x01,                     // Время включения и выключения прибора
        Phase1OnOff = 0x03,               // Время включения и выключения фазы 1
        Phase2OnOff = 0x04,               // Время включения и выключения фазы 2
        Phase3OnOff = 0x05,               // Время включения и выключения фазы 3
        RatesScheduleCorrection = 0x07,   // Время коррекции тарифного расписания
        HolidayScheduleCorrection = 0x08, // Время коррекции расписания праздничных дней
        OpeningClosing = 0x12,            // Время вскрытия и закрытия прибора
        Phase1CurrentOnOff = 0x17,        // Время включения и отключения тока фазы 1
        Phase2CurrentOnOff = 0x18,        // Время включения и отключения тока фазы 2
        Phase3CurrentOnOff = 0x19         // Время включения и отключения тока фазы 3
    }

    enum MeterAccessLevels : byte  // Уровень доступа к счётчику
    {
        User = 0x01,
        Admin = 0x02
    }

    enum Rates : byte  // Тарифы
    {
        Sum,    // Сумма тарифов
        Rate1,  // Тариф 1
        Rate2, // Тариф 2
        Rate3,  // Тариф 3
        Losses  // Тариф 4
    }

    enum Months : byte  // Месяцы
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

    enum DataArrays : byte    // Массивы энергии в пределах 12 месяцев
    {
        FromReset,      // От сброса
        CurrentYear,    // Текущий год
        PastYear,       // Прошедший год
        Month,          // За месяц
        CurrentDay,     // За текущие сутки
        PastDay,        // За прошедшие сутки
        PerPhase        // По фазам
    }
}
