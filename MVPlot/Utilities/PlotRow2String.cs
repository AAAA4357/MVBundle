using Google.Protobuf;
using Google.Protobuf.Collections;
using MVPlot.Managers;
using MVPlot.Proto;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace MVPlot.Utilities
{
    public class PlotRow2String : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            RepeatedField<ByteString> contents = (RepeatedField<ByteString>)values[1];
            return (PlotRowType)values[0] switch
            {
                PlotRowType.Normal => string.Format(LanguageManager.Instance["ByteString2StringType1"]!, contents[1].ToString(Encoding.Default), contents[0].ToString(Encoding.Default)),
                PlotRowType.Narrator => string.Format(LanguageManager.Instance["ByteString2StringType2"]!, contents[0].ToString(Encoding.Default)),
                PlotRowType.Bubble => string.Format(LanguageManager.Instance["ByteString2StringType3"]!, contents[0].ToString(Encoding.Default)),
                PlotRowType.Se => string.Format(LanguageManager.Instance["ByteString2StringType4"]!, contents[0].ToString(Encoding.Default)),
                PlotRowType.ShakeScreen => string.Format(LanguageManager.Instance["ByteString2StringType5"]!, contents[0].ToString(Encoding.Default).Split(',')),
                PlotRowType.ChangeBgm => string.Format(LanguageManager.Instance["ByteString2StringType6"]!, contents[0].ToString(Encoding.Default)),
                _ => null!,
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null!;
        }
    }
}
